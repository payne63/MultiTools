using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.FileProperties;
using ABI.Microsoft.UI.Xaml.Media.Imaging;
using MultiTools.Base;
using BitmapImage = Microsoft.UI.Xaml.Media.Imaging.BitmapImage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiTools.Elements;
public sealed partial class ToolTipInventor : ToolTip
{
    public FileInfo FileInfoData
    {
        get
        {
            return (FileInfo)GetValue(FileInfoDataProperty);}
        set
        {
            SetValue(FileInfoDataProperty, value);
        } 
    }
    
    public static readonly DependencyProperty FileInfoDataProperty = DependencyProperty.Register("FileInfoData", typeof(FileInfo), typeof(ToolTipInventor), new PropertyMetadata(null, OnFileInfoDataChanged));

    private static async void OnFileInfoDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as ToolTipInventor;
        if (control == null) return;
        control.DataContext = e.NewValue as FileInfo;
        control.UpdateToolTip();
    }

    private async void UpdateToolTip()
    {
        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(FileInfoData.FullName);
        var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
        var bitmapImage = new BitmapImage();
        bitmapImage.SetSource(iconThumbnail);
        
        ImageThumbNail.Source = bitmapImage;
        PartNumber.Text = FileInfoData.Name;
    }

    public ToolTipInventor()
    {
        this.InitializeComponent();
    }
}
