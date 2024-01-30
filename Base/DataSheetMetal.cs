using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base;
internal class DataSheetMetal
{
    public string Name;
    public double Thickness;
    public double Height;
    public double Width;

    public bool TurnView =>  Width > Height;

    public void TurnSwap() => (Width, Height) = (Height,Width);
    public DataSheetMetal(string name, double thickness, double height, double width)
    {
        Name = name;
        Thickness = Math.Round(thickness, 1);
        Height = height;
        Width = width;
    }
    public override string ToString() => $"Epaisseur :{Thickness}mm , Hauteur :{Height}mm , Largeur :{Width}mm";
}
