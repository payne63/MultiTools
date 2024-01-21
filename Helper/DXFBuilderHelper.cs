using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvitechTools.Models;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Inventor;
using SplittableDataGridSAmple.Base;

namespace SplittableDataGridSAmple.Helper;
internal class DXFBuilderHelper
{
    private static Application InventorApp;
    private static Inventor.Sheet sheet;

    public static DrawingDocument Build(string PartPath, string templatePath = @"C:\Users\Public\Documents\Autodesk\Inventor 2019\Templates\Metric\ISO.idw")
    {
        InventorApp = InventorManagerHelper.GetActualInventorApp();
        if (InventorApp == null) { throw new Exception("application inventor fermer"); }
        var partDocument = InventorApp.Documents.Open(PartPath, true) as PartDocument;
        var dataSheetMetal = FlatCheck(partDocument);
        if (dataSheetMetal == null )
        {
            throw new Exception("piece non laser");
        }

        var drawingDoc = InventorApp.Documents.Add(DocumentTypeEnum.kDrawingDocumentObject, templatePath, true) as Inventor.DrawingDocument;

        sheet = drawingDoc.Sheets[1];
        sheet.Orientation = PageOrientationTypeEnum.kPortraitPageOrientation;
        sheet.Size = DrawingSheetSizeEnum.kA4DrawingSheetSize;

        var zoom = DefineZoom(dataSheetMetal);

        
        (DrawingView frontView, DrawingView sideView) = CreateView(sheet, partDocument,zoom, dataSheetMetal.TurnView);
        if (dataSheetMetal.TurnView)
        {
            dataSheetMetal.TurnSwap();
        }

        var intentsFrontView = GetIntents(frontView);
        CreateHorizontalDimension(frontView, intentsFrontView);
        CreateVerticalDimension(frontView, intentsFrontView);

        var intentsSideView = GetIntents(sideView);
        CreateHorizontalDimension(sideView, intentsSideView);

        var transientGeometry = InventorApp.TransientGeometry;
        sheet.DrawingNotes.GeneralNotes.AddFitted(transientGeometry.CreatePoint2d(4,5),dataSheetMetal.ToString());
        partDocument.Close();
        return drawingDoc;
    }

    private static List<GeometryIntent> GetIntents(DrawingView view)
    {
        var intents = new List<GeometryIntent>();
        foreach (DrawingCurve drawingCurve in view.DrawingCurves)
        {
            switch (drawingCurve.ProjectedCurveType)
            {
                case Curve2dTypeEnum.kCircleCurve2d:
                case Curve2dTypeEnum.kCircularArcCurve2d:
                case Curve2dTypeEnum.kEllipseFullCurve2d:
                case Curve2dTypeEnum.kEllipticalArcCurve2d:
                    AddIntent(intents, drawingCurve, PointIntentEnum.kCircularTopPointIntent, true);
                    AddIntent(intents, drawingCurve, PointIntentEnum.kCircularBottomPointIntent, true);
                    AddIntent(intents, drawingCurve, PointIntentEnum.kCircularLeftPointIntent, true);
                    AddIntent(intents, drawingCurve, PointIntentEnum.kCircularRightPointIntent, true);
                    AddIntent(intents, drawingCurve, PointIntentEnum.kEndPointIntent, false);
                    AddIntent(intents, drawingCurve, PointIntentEnum.kStartPointIntent, false);
                    break;
                case Curve2dTypeEnum.kLineCurve2d:
                case Curve2dTypeEnum.kLineSegmentCurve2d:
                    AddIntent(intents, drawingCurve, PointIntentEnum.kEndPointIntent, false);
                    AddIntent(intents, drawingCurve, PointIntentEnum.kStartPointIntent, false);
                    break;
            }
        }
        return intents;
    }

    private static void AddIntent(List<GeometryIntent> intents, DrawingCurve drawingCurve, PointIntentEnum pointIntentEnum, bool onLineCheck)
    {
        var intent = sheet.CreateGeometryIntent(drawingCurve, pointIntentEnum);
        if (intent == null) return;
        if (onLineCheck)
        {
            if (IntentIsOnCurve(intent))
            {
                intents.Add(intent);
            }
        }
        else
        {
            intents.Add(intent);
        }
    }

    private static bool IntentIsOnCurve(GeometryIntent intent)
    {
        var geometry = intent.Geometry as DrawingCurve;
        var sp = intent.PointOnSheet;
        var pts = new double[2] { sp.X, sp.Y };
        double[] gp = new double[] { };
        double[] md = new double[] { };
        double[] pm = new double[] { };
        SolutionNatureEnum[] st = new SolutionNatureEnum[] { };
        try
        {
            geometry.Evaluator2D.GetParamAtPoint(ref pts, ref gp, ref md, ref pm, ref st);
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    private static double DefineZoom(DataSheetMetal dataSheetMetal)
    {
        var maxWidth = 120d;
        var maxHeight = 180d;
        var inverseZoom = 1d;
        while (true)
        {
            if (dataSheetMetal.Height/inverseZoom <maxHeight && dataSheetMetal.Width/inverseZoom < maxWidth)
            {
                return 1/inverseZoom;
            }
            inverseZoom +=1;
        }
    }

    private static (DrawingView frontView, DrawingView sideView) CreateView(Inventor.Sheet sheet, PartDocument partDocument, double zoom,bool turnView)
    {
        var transientGeometry = InventorApp.TransientGeometry;
        var positionFrontView = transientGeometry.CreatePoint2d(21d/2, (29.7d-4d)/2+4);
        var nameValueMap = InventorApp.TransientObjects.CreateNameValueMap();
        nameValueMap.Add("SheetMetalFoldedModel", false);
        var FrontView = sheet.DrawingViews.AddBaseView(partDocument as _Document, positionFrontView, zoom, ViewOrientationTypeEnum.kDefaultViewOrientation, DrawingViewStyleEnum.kHiddenLineRemovedDrawingViewStyle, AdditionalOptions: nameValueMap);
        if (turnView)
        {
            FrontView.RotateByAngle(Math.PI/2);
        }
        var positionSideView = transientGeometry.CreatePoint2d(17d, (29.7d - 4d) / 2 + 4);
        var sideView = sheet.DrawingViews.AddProjectedView(FrontView, positionSideView, DrawingViewStyleEnum.kHiddenLineDrawingViewStyle);

        return (FrontView, sideView);
    }

    private static void CreateHorizontalDimension(DrawingView view, List<GeometryIntent> intents)
    {
        var orderedIntentsInX = intents.Where(x => x.PointOnSheet != null).OrderBy(x => x.PointOnSheet.X).ToList();
        var distanceFromView = 1.2d - 0.6d;
        var pointLeft = orderedIntentsInX.First();
        var pointRight = orderedIntentsInX.Last();
        var textX = pointLeft.PointOnSheet.X + (pointRight.PointOnSheet.X - pointLeft.PointOnSheet.X) / 2;
        var textY = view.Position.Y + view.Height / 2 + distanceFromView;
        var pointText = InventorApp.TransientGeometry.CreatePoint2d(textX, textY);
        sheet.DrawingDimensions.GeneralDimensions.AddLinear(pointText, pointLeft, pointRight, DimensionTypeEnum.kHorizontalDimensionType);
    }

    private static void CreateVerticalDimension(DrawingView view, List<GeometryIntent> intents)
    {
        var orderedIntentsInY = intents.Where(x => x.PointOnSheet != null).OrderBy(x => x.PointOnSheet.Y).ToList();
        var distanceFromView = 1.2d - 0.6d;
        var pointLeft = orderedIntentsInY.Last();
        var pointRight = orderedIntentsInY.First();
        var textX = view.Position.X - view.Width / 2 - distanceFromView;
        var textY = pointLeft.PointOnSheet.Y + (pointRight.PointOnSheet.Y - pointLeft.PointOnSheet.Y) / 2;
        var pointText = InventorApp.TransientGeometry.CreatePoint2d(textX, textY);
        sheet.DrawingDimensions.GeneralDimensions.AddLinear(pointText, pointLeft, pointRight, DimensionTypeEnum.kVerticalDimensionType);
    }


    private static DataSheetMetal FlatCheck(PartDocument partDocument)
    {
        if (partDocument == null) { throw new Exception(); }
        if (partDocument.ComponentDefinition is SheetMetalComponentDefinition sheetMetalComponentDefinition)
        {
            var mustSave = (LaserDescriptionCheck(partDocument) ,UnfoldLaserCheck(sheetMetalComponentDefinition));
            var thickness = (double)sheetMetalComponentDefinition.Thickness.Value * 10;
            var range = sheetMetalComponentDefinition.RangeBox;
            var width = Math.Round ((range.MaxPoint.X - range.MinPoint.X) * 10,2);
            var height = Math.Round((range.MaxPoint.Y - range.MinPoint.Y) * 10,2);
            var dataSheetMetal = new DataSheetMetal(partDocument.DisplayName,
                thickness,
                height,
                width);
            if (mustSave.Item1 || mustSave.Item2)
            {
                sheetMetalComponentDefinition.FlatPattern.ExitEdit();
                partDocument.Save();
            }
            return dataSheetMetal;
        }
        return null;
    }

    private static bool LaserDescriptionCheck(PartDocument partDocument)
    {
        var description = (string)partDocument.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value;
        if (description.IndexOf("laser", StringComparison.OrdinalIgnoreCase) == -1)
        {
            partDocument.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value = description + " LASER";
            return true;
        }
        return false;
    }
    private static bool UnfoldLaserCheck(SheetMetalComponentDefinition sheetMetalComponentDefinition)
    {
        if (sheetMetalComponentDefinition.HasFlatPattern)
        {
            sheetMetalComponentDefinition.FlatPattern.Edit();
        }
        else
        {
            sheetMetalComponentDefinition.Unfold();
            return true;
        }
        return false;
    }

    private static void SaveSimpleDxf(SheetMetalComponentDefinition sheetMetalComponentDefinition)
    {
        var dxfFormat = "FLAT PATTERN DXF?AcadVersion=2000&OuterProfileLayer=IV_INTERIOR_PROFILES"; var thickness = (double)sheetMetalComponentDefinition.Thickness.Value * 10;
        sheetMetalComponentDefinition.DataIO.WriteDataToFile(dxfFormat, $"E:\\testPlan\\P641-050-01 {thickness}mm.dxf");
    }
}
