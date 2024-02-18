using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace SplittableDataGridSAmple.Base;

public sealed class PressAndHoldButton : Button
{
    public delegate void PointerEventHandler(object? sender, PointerPressEventArgs e);
    public event PointerEventHandler PointerPressPreview = delegate { };

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        PointerPressPreview(this, new PointerPressEventArgs { PointerPositionRelative = e.GetCurrentPoint(this).Position.X} ) ;
        base.OnPointerPressed(e);
    }
}

public class PointerPressEventArgs : EventArgs
{
    public double PointerPositionRelative;
}
