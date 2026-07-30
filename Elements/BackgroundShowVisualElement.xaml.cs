using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MultiTools.Elements;

public sealed partial class BackgroundShowVisualElement : StackPanel
{
    public BackgroundShowVisualElement()
    {
        InitializeComponent();
    }
    
    public static readonly DependencyProperty ShowDragAndDropProperty = DependencyProperty.Register(nameof(ShowDragAndDrop), typeof(Visibility), typeof(BackgroundShowVisualElement), new PropertyMetadata(Microsoft.UI.Xaml.Visibility.Collapsed ,OnShowDragAndDropChanged));

    private static void OnShowDragAndDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var element = d as BackgroundShowVisualElement;
        element.FontIconDragAndDrop.Visibility = (Microsoft.UI.Xaml.Visibility)e.NewValue;
        element.TextBlockDragAndDrop.Visibility = (Microsoft.UI.Xaml.Visibility)e.NewValue;
    }
    
    public Visibility ShowDragAndDrop
    {
        get => (Visibility)GetValue(ShowDragAndDropProperty);
        set => SetValue(ShowDragAndDropProperty, value);
    }
    
    public static readonly DependencyProperty ShowBusyRingProperty = DependencyProperty.Register(nameof(ShowBusyRing),typeof(Visibility),typeof(BackgroundShowVisualElement),new PropertyMetadata(Microsoft.UI.Xaml.Visibility.Collapsed,OnShowBusyRingChanged));

    private static void OnShowBusyRingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var element = d as BackgroundShowVisualElement;
        element.ProgressRingBusy.Visibility = (Microsoft.UI.Xaml.Visibility)e.NewValue;
    }

    public Visibility ShowBusyRing
    {
        get => (Visibility)GetValue(ShowBusyRingProperty);
        set => SetValue(ShowBusyRingProperty, value);
    }
}