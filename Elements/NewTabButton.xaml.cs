using DocumentFormat.OpenXml.Drawing;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SplittableDataGridSAmple.Tabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core.AnimationMetrics;

namespace SplittableDataGridSAmple.Elements;

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
            var tabExisting = MainWindow.tabViewStaticRef.TabItems.FirstOrDefault( x => x.GetType() == tabType);
            if (tabExisting != null  )
            {
                MainWindow.tabViewStaticRef.SelectedItem = tabExisting;
                return;
            }
            var selectedTabItem = MainWindow.tabViewStaticRef.SelectedItem as TabViewItem;
            MainWindow.tabViewStaticRef.TabItems.Add(_tabViewItemToLoad);
            ((Interfaces.IInitTab)_tabViewItemToLoad).InitTabAsync();
            MainWindow.tabViewStaticRef.SelectedItem = _tabViewItemToLoad;
            //MainWindow.tabViewRef.TabItems.Remove(selectedTabItem);
        };
        this.isBetaVersion = isBetaVersion;
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        ClickMethod.Invoke(this, e);
    }

    private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        StoryboardMoveLeft.Children[0].SetValue(DoubleAnimation.FromProperty, Translation.X);
        StoryboardMoveLeft.Children[0].SetValue(DoubleAnimation.ToProperty, 20);
        StoryboardMoveLeft.Begin();
    }

    private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        StoryboardMoveLeft.Children[0].SetValue(DoubleAnimation.FromProperty, Translation.X);
        StoryboardMoveLeft.Children[0].SetValue(DoubleAnimation.ToProperty, 0);
        StoryboardMoveLeft.Begin();
    }
}
