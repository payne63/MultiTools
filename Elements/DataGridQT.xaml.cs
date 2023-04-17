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
using SplittableDataGridSAmple.Base;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;
using System.Data;
using static SplittableDataGridSAmple.DataIBase;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Elements
{

    public sealed partial class DataGridQT : StackPanel, INotifyPropertyChanged
    {

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public static List<DataGridQT> dataGridCollection { get; private set; } = new();

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { _Title = value; OnPropertyChanged(); }
        }

        private bool _IsVisible = true;
        public bool IsVisible
        {
            get { return _IsVisible; }
            set { _IsVisible = value; OnPropertyChanged(); }
        }

        //public CategoryType CategorySelected { get; set; }

        private CategoryType category;
        public int QtElement { get => Datas.Count; }

        private ObservableCollection<DataIQT> _Datas;
        public ObservableCollection<DataIQT> Datas { get { return _Datas; } set { _Datas = value; OnPropertyChanged(); } }

        private DataGridQT()
        {
            this.InitializeComponent();
        }
        public DataGridQT(CategoryType categoryType) : this()
        {
            category = categoryType;
            dataGridCollection.Add(this);
        }

        public static void ClearAllData()
        {
            foreach (var dataGrid in dataGridCollection)
            {
                dataGrid.Datas.Clear();
            }
        }

        private void dataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
        {
            if (e.Column.Tag.ToString() == "Name")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
                {
                    dataGrid.ItemsSource = new ObservableCollection<DataIQT>(from item in Datas orderby item.NameFile ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else
                {
                    dataGrid.ItemsSource = new ObservableCollection<DataIQT>(from item in Datas orderby item.NameFile descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
            }
            if (e.Column.Tag.ToString() == "Description")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
                {
                    dataGrid.ItemsSource = new ObservableCollection<DataIQT>(from item in Datas orderby item.Description ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else
                {
                    dataGrid.ItemsSource = new ObservableCollection<DataIQT>(from item in Datas orderby item.Description descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
            }

            if (e.Column.Tag.ToString() == "Category")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
                {
                    dataGrid.ItemsSource = new ObservableCollection<DataIQT>(from item in Datas orderby item.Category.ToString() ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else
                {
                    dataGrid.ItemsSource = new ObservableCollection<DataIQT>(from item in Datas orderby item.Category.ToString() descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
            }

            // Remove sorting indicators from other columns
            foreach (var dgColumn in dataGrid.Columns)
            {
                if (dgColumn.Tag.ToString() != e.Column.Tag.ToString()) dgColumn.SortDirection = null;
            }

        }

        private async void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var dataIQT = e.AddedItems[0] as DataIQT;
                await dataIQT.UpdateThumbnail();
            }
            if (e.RemovedItems.Count > 0) return;
            foreach (var dataGridQT in dataGridCollection.Where(x => x.category != category))
            {
                dataGridQT.dataGrid.SelectedItem = null;
            }
        }

        private void Button_Click_ChangeCategory(object sender, RoutedEventArgs e)
        {

        }

        private void dataGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null) { dataGrid.SelectedItem = null; }
        }

        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0) return;
            if (e.RemovedItems.Count == 0) return;
            if (e.RemovedItems[0].ToString() == e.AddedItems[0].ToString()) return;
            var dataIQT = ((FrameworkElement)sender).DataContext as DataIQT;
            var newDateGridQT =  dataGridCollection.Where(x => x.category == (CategoryType)e.AddedItems[0]).FirstOrDefault();
            var oldDateGridQT =  dataGridCollection.Where(x => x.category == (CategoryType)e.RemovedItems[0]).FirstOrDefault();
            newDateGridQT.Datas.Add(dataIQT);
            oldDateGridQT.Datas.Remove(dataIQT);
            newDateGridQT.OnPropertyChanged(nameof(QtElement));
            oldDateGridQT.OnPropertyChanged(nameof(QtElement));
            dataIQT.Category = (CategoryType)e.AddedItems[0];
        }
    }
}
