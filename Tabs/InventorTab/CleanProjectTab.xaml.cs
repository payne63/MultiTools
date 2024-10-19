using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using CommunityToolkit.WinUI.UI.Controls;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using MultiTools.Interfaces;
using System.Reflection;
using I = Inventor;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MultiTools.Base;
using MultiTools.Helper;
using MultiTools.Models;

namespace MultiTools.Tabs.InventorTab;

public sealed partial class CleanProjectTab : Interfaces.IInitTab, INotifyPropertyChanged
{
    private StorageFile? _mainAssemblyStorageFile;

    private readonly HashSet<DataIClean> _allParts = new();

    private ObservableCollection<DataIClean> _orpheansPart = new();

    public ObservableCollection<DataIClean> OrphansPart
    {
        get => _orpheansPart;
        set => _orpheansPart = value;
    }

    public Visibility DragAndDropVisibility => OrphansPart.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public CleanProjectTab()
    {
        this.InitializeComponent();
    }

    public async void InitTabAsync()
    {
        OrphansPart.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(DragAndDropVisibility));
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e) =>
        e.AcceptedOperation = DataPackageOperation.Move;

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            OpenSimpleMessage(XamlRoot, "Format non compatible");
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        _mainAssemblyStorageFile = items.FirstOrDefault() as StorageFile;

        if (_mainAssemblyStorageFile != null)
        {
            await CollectAndFill(_mainAssemblyStorageFile);
        }
    }


    private void Button_Click_RemoveData(object sender, RoutedEventArgs e)
    {
        OrphansPart.Clear();
        _mainAssemblyStorageFile = null;
    }

    private async void Button_Click_SelectFiles(object sender, RoutedEventArgs e)
    {
        var file = await GetFileOpenPicker(".ipt", ".iam");
        if (file == null) return;
        _mainAssemblyStorageFile = file;
        await CollectAndFill(file);
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIdwModel = ((FrameworkElement)sender).DataContext as DataIClean;
        OrphansPart.Remove(contextIdwModel!);
    }

    private async void Button_Click_Clean(object sender, RoutedEventArgs e)
    {
        if (OrphansPart.Count == 0) return;
        if (OrphansPart.All(x => x.IsInMainAssembly))
        {
            await OpenSimpleMessage(XamlRoot, "aucun fichier à déplacer");
            return;
        }
        var oldPartDirectory = Path.GetDirectoryName(_mainAssemblyStorageFile.Path) + @"\AutoOLD";
        if (!Directory.Exists(oldPartDirectory))
        {
            Directory.CreateDirectory(oldPartDirectory);
        }

        List<CleanHistoryModel> historyFileMoved = new();
        foreach (var dataIClean in OrphansPart)
        {
            if (dataIClean.IsInMainAssembly == false)
            {
                var newPath = oldPartDirectory
                              + dataIClean.RelativeFolderPath
                              + @"\"
                              + dataIClean.NameFile;
                var directoryMove = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(directoryMove))
                {
                    Directory.CreateDirectory(directoryMove);
                }

                File.Move(dataIClean.FullPathName, newPath);
                historyFileMoved.Add(new CleanHistoryModel(dataIClean.FullPathName, newPath));
            }
        }

        var jsonString = JsonSerializer.Serialize<List<CleanHistoryModel>>(historyFileMoved,
            new JsonSerializerOptions { WriteIndented = true });
        var historyFileName = DateTime.Now.ToString("yy-MM-dd à HH\\hmm") + ".json";
        await File.WriteAllTextAsync(oldPartDirectory + $"\\Historique_Du_{historyFileName}", jsonString);


        var report = new StringBuilder();
        report.AppendLine("Les fichiers ont ete déplacés:");
        report.AppendLine($"(fichier d'historique, {historyFileName})");
        await OpenSimpleMessage(XamlRoot, report.ToString());

        OrphansPart.Clear();
    }

    private async Task CollectAndFill(StorageFile storageFileSelect)
    {
        IsInterfaceEnabled = false;
        var directory = Path.GetDirectoryName(storageFileSelect.Path);
        List<string> files = new();
        foreach (var filter in new[] { "*.ipt", "*.iam" })
        {
            files.AddRange(Directory.GetFiles(directory, filter, SearchOption.AllDirectories));
        }

        foreach (var file in files.Where(path => !path.Contains("OldVersions") && !path.Contains("AutoOLD")))
        {
            var dataIClean = await Task.Run(() => new DataIClean(file, storageFileSelect.Path));
            OrphansPart.Add(dataIClean);
        }

        await foreach (var childString in GetChildPathFromBom(storageFileSelect.Path))
        {
            var matchPart = OrphansPart.FirstOrDefault(o => o.FullPathName == childString);
            if (matchPart != null)
            {
                matchPart.IsInMainAssembly = true;
            }
        }
        
        var sortableList = new List<DataIClean>(OrphansPart);
        sortableList.Sort(((a, b) => string.Compare(a.PartNumber, b.PartNumber, StringComparison.Ordinal) ));
        sortableList.Sort((DataIClean a, DataIClean b) => a.IsInMainAssembly.CompareTo(b.IsInMainAssembly));
        for (int i = 0; i < sortableList.Count; i++)
        {
            OrphansPart.Move(OrphansPart.IndexOf(sortableList[i]), i);
        }
        
        IsInterfaceEnabled = true;
    }

    private async IAsyncEnumerable<string> GetChildPathFromBom(string pathFile)
    {
        I.ApprenticeServerDocument? document = null;
        await Task.Run(() =>
        {
            document = ApprenticeHelper.GetApprenticeDocument(pathFile);
        });
        if (document == null) yield break;
        yield return pathFile; // ajoute le fichier d'origine
        if (document.DocumentType == I.DocumentTypeEnum.kAssemblyDocumentObject)
        {
            var assemblyDocument = document.ComponentDefinition as I.AssemblyComponentDefinition;
            var bom = assemblyDocument.BOM;
            foreach (I.BOMRow bomRow in bom.BOMViews[1].BOMRows)
            {
                var FullDocumentName = ((I.ApprenticeServerDocument)(bomRow.ComponentDefinitions[1]).Document)
                    .FullDocumentName;
                yield return FullDocumentName;
            }
        }
    }

    private async void ButtonBase_OnClick_Restore(object sender, RoutedEventArgs e)
    {
        var jsonFileRestoration = await GetFileOpenPicker(".json");
        if (jsonFileRestoration == null) return;
        
        var file = await File.ReadAllTextAsync(jsonFileRestoration.Path);

        var listRestore = JsonSerializer.Deserialize<List<CleanHistoryModel>>(file);
        if (listRestore == null) OpenSimpleMessage(XamlRoot, "erreur sur le fichier json");

        var isExeption = false;
        foreach (var move in listRestore)
        {
            try
            {
                File.Move(move.To, move.From);
            }
            catch (Exception exception)
            {
                await OpenSimpleMessage(XamlRoot,$"erreur sur le fichier {Path.GetFileName(move.To)} : {exception.Message}");
                isExeption = true;
            }
        }

        if (isExeption == false)
        {
            File.Delete(jsonFileRestoration.Path);
            OpenSimpleMessage(XamlRoot, "fichiers restorés");
        }

    }
    
    private void MyListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        // Créer un menu contextuel (MenuFlyout)
        MenuFlyout flyout = new MenuFlyout();

        // Ajouter des éléments au menu
        MenuFlyoutItem editItem = new MenuFlyoutItem { Text = "Edit" };
        // editItem.Click += (s, args) => EditItem(sender, e);
        
        MenuFlyoutItem deleteItem = new MenuFlyoutItem { Text = "Delete" };
        // deleteItem.Click += (s, args) => DeleteItem(sender, e);
        deleteItem.Click += (_,_)=> Button_Click_Remove(sender, e);
        
        // Ajouter les éléments au MenuFlyout
        flyout.Items.Add(editItem);
        flyout.Items.Add(deleteItem);

        // Obtenir l'élément cliqué
        ListViewItem clickedItem = (sender as FrameworkElement)?.DataContext as ListViewItem;

        // Afficher le menu contextuel à la position du clic droit
        flyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
    }
    
}