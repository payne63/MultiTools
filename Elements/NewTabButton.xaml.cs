using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SplittableDataGridSAmple.Tabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace SplittableDataGridSAmple.Elements
{
    public sealed partial class NewTabButton : Button
    {
        //public Symbol SymbolImage { get; set; }
        public string GlyphsIconTab { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public RoutedEventHandler ClickMethod { get; set; }
        private TabViewItem _tabViewItemToLoad;

        public bool isBetaVersion { get; set; }

        public NewTabButton(Type tabType, string description = null, bool isBetaVersion = false)
        {
            this.InitializeComponent();

            _tabViewItemToLoad = (TabViewItem)Activator.CreateInstance(tabType);
            Title = _tabViewItemToLoad.Header.ToString();
            var s = _tabViewItemToLoad.IconSource as FontIconSource;
            GlyphsIconTab = s != null ? s.Glyph : "&#xE700;";
            Description = description;
            ClickMethod += delegate
            {
                var selectedTabItem = MainWindow.tabViewRef.SelectedItem as TabViewItem;
                MainWindow.tabViewRef.TabItems.Add(_tabViewItemToLoad);
                ((Interfaces.IInitTab)_tabViewItemToLoad).InitTab();
                MainWindow.tabViewRef.SelectedItem = _tabViewItemToLoad;
                MainWindow.tabViewRef.TabItems.Remove(selectedTabItem);
            };
            this.isBetaVersion = isBetaVersion;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClickMethod.Invoke(this, e);
        }
    }
}
