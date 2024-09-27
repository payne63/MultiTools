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
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using Windows.Storage;
using MsgReader.Outlook;
using System.Text.Json;
using MultiTools.Base;
using System.Collections.ObjectModel;
using MultiTools.Helper;

namespace MultiTools.Tabs.TestTab;

public sealed partial class Test2Tab : TabViewItem
{
        
    public Test2Tab()
    {
        this.InitializeComponent();
            
        Brush b = new SolidColorBrush(Colors.Chocolate);
        canvas.Children.Add(new Rectangle { Fill = b, Height = 100, Width = 100, Translation = new System.Numerics.Vector3(10, 10, 0) });
    }
    private void Button_Click_New_Mail(object sender, RoutedEventArgs e)
    {
        var outlookHelper = new OutlookHelper();
        outlookHelper.ShowNewMailITem("Test Body", @"C:\Users\Florent\source\repos\SplittableDataGridSAmple\test.json");
    }
        

        

        

        
}