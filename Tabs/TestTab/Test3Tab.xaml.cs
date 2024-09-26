// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using SplittableDataGridSAmple.Base;
using System.Collections.ObjectModel;
//using System.Text.Json;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using MsgReader.Outlook;
using SplittableDataGridSAmple.Helper;
using System.Threading.Tasks;
using SplittableDataGridSAmple.Interfaces;
using AvitechTools.Models;
using Inventor;
using DocumentFormat.OpenXml.Math;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs.TestTab;

public sealed partial class Test3Tab : TabViewItem, IInitTab
{
    InventorManagerHelper inventorManager;
    public Test3Tab()
    {
        this.InitializeComponent();
    }
    public void InitTabAsync()
    {

    }

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0)
            {
                var storageFile = items[0] as StorageFile;
                if (storageFile != null)
                {
                    Trace.WriteLine($"storage file: {storageFile.Path}");
                    readMSG(new FileInfo(storageFile.Path));
                }
            }
        }
    }
    public void readMSG(FileInfo fileInfo)
    {
        try
        {
            if (fileInfo.Extension.ToLower().Equals(".msg"))
            {

                using (var msg = new MsgReader.Outlook.Storage.Message(fileInfo.FullName))
                {
                    var contact = msg.Contact;
                    if (contact != null)
                    {
                        Trace.WriteLine(contact.SurName);//Prenom
                        Trace.WriteLine(contact.GivenName);//Nom
                        Trace.WriteLine(contact.Function);//Poste

                        Trace.WriteLine(contact.CellularTelephoneNumber);//portable
                        Trace.WriteLine(contact.Email1EmailAddress);
                        Trace.WriteLine(contact.BusinessTelephoneNumber);
                        Trace.WriteLine(contact.BusinessTelephoneNumber2);
                        Trace.WriteLine(contact.HomeTelephoneNumber);
                        Trace.WriteLine(contact.HomeTelephoneNumber2);
                        Trace.WriteLine(contact.PrimaryTelephoneNumber);

                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Trace.WriteLine($"{fileInfo.Name} Unable to convert  msg file: {ex.Message}.");
        }
    }
    private void TabViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void Button_Click_Dxf(object sender, RoutedEventArgs e)
    {
        var partFile = @"E:\testPlan\P641-051-03.ipt";

        var documentPart = InventorManagerHelper.GetActualInventorApp()?.Documents.Open(partFile);

        var appI = InventorManagerHelper.GetActualInventorApp();
        if (appI == null) { return; }
        if (appI.ActiveDocument is Inventor.PartDocument part)
        {
            
            if (part.ComponentDefinition is SheetMetalComponentDefinition sheetMetalComponentDefinition)
            {
                if (sheetMetalComponentDefinition.HasFlatPattern)
                {
                    sheetMetalComponentDefinition.FlatPattern.Edit();
                }
                else
                {
                    sheetMetalComponentDefinition.Unfold();
                }
                var thickness = (double)sheetMetalComponentDefinition.Thickness.Value * 10;
                var range = sheetMetalComponentDefinition.RangeBox;
                var width = (range.MaxPoint.X - range.MinPoint.X) * 10;
                var height = (range.MaxPoint.Y - range.MinPoint.Y) * 10;
                Trace.WriteLine(width);
                Trace.WriteLine(height);
                var dxfFormat = "FLAT PATTERN DXF?AcadVersion=2000&OuterProfileLayer=IV_INTERIOR_PROFILES";
                //sheetMetalComponentDefinition.DataIO.WriteDataToFile(dxfFormat, $"E:\\testPlan\\P641-050-01 {thickness}mm.dxf");
            }
            var templatePath = @"C:\Users\Public\Documents\Autodesk\Inventor 2019\Templates\Metric\ISO.idw";
            var drawingDoc = appI.Documents.Add(DocumentTypeEnum.kDrawingDocumentObject, templatePath) as Inventor.DrawingDocument;
            var sheet = drawingDoc.Sheets[1];
            sheet.Orientation = PageOrientationTypeEnum.kPortraitPageOrientation;
            sheet.Size = DrawingSheetSizeEnum.kA4DrawingSheetSize;

            var transientGeometry = appI.TransientGeometry;
            var position = transientGeometry.CreatePoint2d(10d, 20d);
            var nameValueMap = appI.TransientObjects.CreateNameValueMap();
            nameValueMap.Add("SheetMetalFoldedModel", false);
            var view = sheet.DrawingViews.AddBaseView(documentPart, position, 1d, ViewOrientationTypeEnum.kDefaultViewOrientation, DrawingViewStyleEnum.kHiddenLineRemovedDrawingViewStyle, AdditionalOptions: nameValueMap);
            //var drawingCurveOrientation = transientGeometry.CreateLine2d(position, transientGeometry.CreateUnitVector2d(10d, 0d)) ;

            var position2 = transientGeometry.CreatePoint2d(15d, 20d);
            var view2 = sheet.DrawingViews.AddProjectedView(view, position2, DrawingViewStyleEnum.kHiddenLineDrawingViewStyle);

            var intents = new List<GeometryIntent>();

            foreach (DrawingCurve drawingCurve in view.DrawingCurves)
            {
                switch (drawingCurve.ProjectedCurveType)
                {
                    case Curve2dTypeEnum.kCircleCurve2d:
                    case Curve2dTypeEnum.kCircularArcCurve2d:
                    case Curve2dTypeEnum.kEllipseFullCurve2d:
                    case Curve2dTypeEnum.kEllipticalArcCurve2d:
                        AddIntent(drawingCurve, PointIntentEnum.kCircularTopPointIntent, true);
                        AddIntent(drawingCurve, PointIntentEnum.kCircularBottomPointIntent, true);
                        AddIntent(drawingCurve, PointIntentEnum.kCircularLeftPointIntent, true);
                        AddIntent(drawingCurve, PointIntentEnum.kCircularRightPointIntent, true);
                        AddIntent(drawingCurve, PointIntentEnum.kEndPointIntent, false);
                        AddIntent(drawingCurve, PointIntentEnum.kStartPointIntent, false);
                        break;
                    case Curve2dTypeEnum.kLineCurve2d:
                    case Curve2dTypeEnum.kLineSegmentCurve2d:
                        AddIntent(drawingCurve, PointIntentEnum.kEndPointIntent, false);
                        AddIntent(drawingCurve, PointIntentEnum.kStartPointIntent, false);
                        break;
                }
            }

            var orderedIntentsInX = intents.Where(x => x.PointOnSheet != null).OrderBy(x => x.PointOnSheet.X).ToList();
            CreateHorizontalDimension(orderedIntentsInX.First(), orderedIntentsInX.Last(), 1.2d - 0.6d);

            var orderedIntentsInY = intents.Where(x => x.PointOnSheet != null).OrderBy(x => x.PointOnSheet.Y).ToList();
            CreateVerticalDimension(orderedIntentsInY.Last(), orderedIntentsInY.First(), 1.2d - 0.6d);

            documentPart.Close();
            return;

            void AddIntent(DrawingCurve drawingCurve, PointIntentEnum kCircularTopPointIntent, bool onLineCheck)
            {
                var intent = sheet.CreateGeometryIntent(drawingCurve, kCircularTopPointIntent);
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

            bool IntentIsOnCurve(GeometryIntent intent)
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

            void CreateHorizontalDimension(GeometryIntent pointLeft, GeometryIntent pointRight, double distanceFromView)
            {
                var textX = pointLeft.PointOnSheet.X + (pointRight.PointOnSheet.X - pointLeft.PointOnSheet.X) / 2;
                var textY = view.Position.Y + view.Height / 2 + distanceFromView;
                var pointText = appI.TransientGeometry.CreatePoint2d(textX, textY);
                sheet.DrawingDimensions.GeneralDimensions.AddLinear(pointText, pointLeft, pointRight, DimensionTypeEnum.kHorizontalDimensionType);
            }

            void CreateVerticalDimension(GeometryIntent pointLeft, GeometryIntent pointRight, double distanceFromView)
            {

                var textX = view.Position.X - view.Width / 2 - distanceFromView;
                var textY = pointLeft.PointOnSheet.Y + (pointRight.PointOnSheet.Y - pointLeft.PointOnSheet.Y) / 2;
                var pointText = appI.TransientGeometry.CreatePoint2d(textX, textY);
                sheet.DrawingDimensions.GeneralDimensions.AddLinear(pointText, pointLeft, pointRight, DimensionTypeEnum.kVerticalDimensionType);
            }
        }

    }

    private async void Button_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            var drawingDocument = DXFBuilderHelper.BuildTrueSheetMetal(null,@"E:\testPlan\P641-110.ipt");
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Erreur",
                Content = ex.Message,
                PrimaryButtonText = "Fermer",
                DefaultButton = ContentDialogButton.Primary,
            };
            _ = await dialog.ShowAsync();
        }
    }
}