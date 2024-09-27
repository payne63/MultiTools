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
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;
using System.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using System.Diagnostics;
using MultiTools.Base;
using static MultiTools.Base.DataIBase;

namespace MultiTools.Elements
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

        public delegate void MoveDataHandler(DataIQT dataIQT,DataIBase.CategoryType fromCategoryType, DataIBase.CategoryType toCategoryType);
        public event MoveDataHandler MoveData;

        public delegate void SelectionHandler(DataIBase.CategoryType fromCategoryType);
        public event SelectionHandler Selection;

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

        public readonly DataIBase.CategoryType category;
        public int CountElement => Datas.Count;

        private ObservableCollection<DataIQT> _Datas;
        public ObservableCollection<DataIQT> Datas { get { return _Datas; } set { _Datas = value; OnPropertyChanged(); } }

        private DataGridQT()
        {
            this.InitializeComponent();
        }

        public DataGridQT(DataIBase.CategoryType categoryType, ObservableCollection<DataIQT> datas) : this()
        {
            if (datas == null) { throw new Exception("erreur"); }
            category = categoryType;
            Datas = datas;
            IsVisible  = datas.Count == 0 ? false: true;
            Title = categoryType.ToString();
            Datas.CollectionChanged += (sender,e) => { OnPropertyChanged(nameof(CountElement)); };
            Datas.CollectionChanged += (sender, e) => { IsVisible = Datas.Count == 0 ? false : true; };
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
                Selection.Invoke(category);
            }
        }

        public void RemoveSelection() => dataGrid.SelectedItem = null;

        private void dataGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null) { dataGrid.SelectedItem = null; }
        }

        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0) return;
            if (e.RemovedItems.Count == 0) return;
            if (e.RemovedItems[0].ToString() == e.AddedItems[0].ToString()) return;

            MoveData.Invoke(((FrameworkElement)sender).DataContext as DataIQT, (DataIBase.CategoryType)e.RemovedItems.First(),(DataIBase.CategoryType)e.AddedItems.First());
 
        }
    }
}
