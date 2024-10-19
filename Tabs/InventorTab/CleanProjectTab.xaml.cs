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
using MultiTools.Base;
using MultiTools.Helper;

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
        OrphansPart.CollectionChanged += (sender, args) =>
        {
            OnPropertyChanged(nameof(DragAndDropVisibility));
        };
    }

    private async Task DoTheJob()
    {
        IsInterfaceEnabled = false;

        var listOfPartAndAssembly = await PartsAndAssemblyFindInRoot();
        foreach (var item in listOfPartAndAssembly)
        {
            await Task.Run(() =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    OrphansPart.Add(new DataIClean(item, _mainAssemblyStorageFile.Path));
                });
            });
        }

        Trace.WriteLine("fin");
        try
        {
            await getDataIClean();
        }
        catch (System.Exception ex)
        {
            IsInterfaceEnabled = true;
            OpenSimpleMessage(XamlRoot, $"Erreur!!{ex.Message} ");
            return;
        }

        foreach (var dataIClean in _allParts)
        {
            if (listOfPartAndAssembly.All(x => x != dataIClean.FullPathName))
            {
                OrphansPart.Add(dataIClean);
            }
            //if (groups.All(x => x.FullPathName != file))
            //{
            //    OrphansPart.Add(new DataIQT(file, 0));
            //}
        }

        IsInterfaceEnabled = true;
    }

    private async Task getDataIClean()
    {
        await RecursiveGetDataIClean(_mainAssemblyStorageFile.Path);
    }

    private async Task RecursiveGetDataIClean(string path)
    {
        I.Document doc = null;
        await Task.Run((() =>
        {
            doc = InventorHelper2.GetDocument(path);
        }));

        if (doc is I.AssemblyDocument assemblyDoc)
        {
            _allParts.Add(new DataIClean(path, _mainAssemblyStorageFile.Path));
            foreach (I.ComponentOccurrence occurrence in assemblyDoc.ComponentDefinition.Occurrences)
            {
                I.ComponentDefinition compDef = occurrence.Definition;
                if (compDef is I.PartComponentDefinition partCompDef)
                {
                    _allParts.Add(new DataIClean(((I.PartDocument)(partCompDef.Document)).FullFileName,
                        _mainAssemblyStorageFile.Path));
                }
                else if (compDef is I.AssemblyComponentDefinition assemblyComponent)
                {
                    await RecursiveGetDataIClean(((I.AssemblyDocument)(assemblyComponent.Document)).FullFileName);
                }
            }
        }

        doc.Close();
    }

    public async Task<List<string>> PartsAndAssemblyFindInRoot()
    {
        var listOfPartAndAssembly = new List<string>();
        if (_mainAssemblyStorageFile == null) return listOfPartAndAssembly;

        Task.Run((() =>
        {
            listOfPartAndAssembly = Directory.GetFiles(Path.GetDirectoryName(_mainAssemblyStorageFile.Path), "*.*",
                    System.IO.SearchOption.AllDirectories)
                .Where(path => path.EndsWith(".ipt") || path.EndsWith(".iam"))
                .Where(path => !path.Contains("OldVersions"))
                .ToList();
        }));
        return listOfPartAndAssembly;
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

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
            await DoTheJob2(_mainAssemblyStorageFile);
        }
    }


    private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => OrphansPart.Clear();

    private async void Button_Click_SelectFiles(object sender, RoutedEventArgs e)
    {
        var file = await GetFileOpenPicker(".ipt", ".iam");
        if (file == null) return;
        _mainAssemblyStorageFile = file;
        // await DoTheJob();
        await DoTheJob2(file);
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIdwModel = ((FrameworkElement)sender).DataContext as DataIClean;
        OrphansPart.Remove(contextIdwModel!);
    }

    private void Button_Click_Clean(object sender, RoutedEventArgs e)
    {
        if (OrphansPart.Count == 0) return;
        var oldPartDirectory = Path.GetDirectoryName(_mainAssemblyStorageFile.Path) + @"\AutoOLD";
        if (!Directory.Exists(oldPartDirectory))
        {
            Directory.CreateDirectory(oldPartDirectory);
        }

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
            }
        }

        OrphansPart.Clear();
    }

    private async Task DoTheJob2(StorageFile storageFileSelect)
    {
        IsInterfaceEnabled = false;
        var directory = Path.GetDirectoryName(storageFileSelect.Path);
        List<string> files = new();
        foreach (var filter in new[] { "*.ipt", "*.iam" })
        {
            files.AddRange(Directory.GetFiles(directory, filter, SearchOption.AllDirectories));
        }

        foreach (var file in files.Where(path => !path.Contains("OldVersions")))
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

    private async Task RecursiveGetChildIClean(string pathFile, string pathMainFile, List<DataIClean> fallowedList)
    {
        I.Document? document = null;
        await Task.Run(() =>
        {
            document = InventorHelper2.GetDocument(pathFile);
        });
        if (document == null) return;

        fallowedList.Add(new DataIClean(pathFile, pathMainFile));

        if (document is I.PartDocument) return;

        if (document is I.AssemblyDocument assemblyDocument)
        {
            foreach (I.ComponentOccurrence occurrence in assemblyDocument.ComponentDefinition.Occurrences)
            {
                if (occurrence.Definition is I.PartComponentDefinition partComponentDef)
                {
                    fallowedList.Add(new DataIClean(((I.PartDocument)(partComponentDef.Document)).FullFileName,
                        pathMainFile));
                }

                if (occurrence.Definition is I.AssemblyComponentDefinition assemblyComponentDefinition)
                {
                    await RecursiveGetChildIClean(
                        (assemblyComponentDefinition.Document as I.AssemblyDocument).FullFileName, pathMainFile,
                        fallowedList);
                }
            }
        }

        document.Close();
    }
}