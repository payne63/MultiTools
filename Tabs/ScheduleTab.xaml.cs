using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.VisualStudio.Shell.Interop;
using SplittableDataGridSAmple.Base;
using SplittableDataGridSAmple.Interfaces;
using Windows.System;
using Windows.UI.Core;

namespace SplittableDataGridSAmple.Tabs;
public sealed partial class ScheduleTab : TabViewItem, IInitTab
{

    public ObservableCollection<TaskBase> TaskBases
    {
        get; set;
    }

    public double MouseDownLocationX;

    public ScheduleTab()
    {
        this.InitializeComponent();
        InitCanvasHeader();
    }


    public void InitTab()
    {

        TaskBases = new()
        {
            new TaskBase("rest", new DateTime(2024,3,20)) ,
            new TaskBase("ppo", new DateTime(2024,3,20)) ,
            new TaskBase("cvwef", new DateTime(2024,3,20)) ,
        };
        TaskBases.First().Children.Add(new TaskBase("children", new DateTime(2024, 3, 22)));
        TaskBases.First().IsExpanded = true;
    }

    private void InitCanvasHeader()
    {
        CanvasHeader.Children.Clear();
        var cultureFrench = CultureInfo.CreateSpecificCulture("fr-FR");
        var lineColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100));
        var lineColorLight = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 100, 100, 100));
        var step = 20;
        var count = 0;
        var maxHeight = 75;
        foreach (var day in GetDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31)))
        {
            count++;
            if (day.Day == 1)
            {
                var monthText = new TextBlock { Text = day.ToString("MMMM yyyy", cultureFrench).ToUpper() };
                Canvas.SetLeft(monthText, step * count + step * 15);
                Canvas.SetTop(monthText, 0);
                Canvas.SetZIndex(monthText, -1);
                CanvasHeader.Children.Add(monthText);
                var line = new Line { X1 = count * step, Y1 = 0, X2 = count * step, Y2 = 20, StrokeThickness = 3, Stroke = lineColor };
                var lineTwo = new Line { X1 = count * step, Y1 = 40, X2 = count * step, Y2 = maxHeight, StrokeThickness = 3, Stroke = lineColor };
                CanvasHeader.Children.Add(line);
                CanvasHeader.Children.Add(lineTwo);
            }

            if (day.ToString("dddd", cultureFrench)[..1].StartsWith("l"))
            {
                var week = cultureFrench.Calendar.GetWeekOfYear(day, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                var weekText = new TextBlock { Text = "S" + week.ToString() };
                Canvas.SetLeft(weekText, step * count + step + 40);
                Canvas.SetTop(weekText, 20);
                Canvas.SetZIndex(weekText, -1);
                CanvasHeader.Children.Add(weekText);
                var line = new Line { X1 = count * step, Y1 = 20, X2 = count * step, Y2 = maxHeight, StrokeThickness = 2, Stroke = lineColor };
                CanvasHeader.Children.Add(line);
            }

            var dayText = new TextBlock { Text = day.ToString("dd") };
            Canvas.SetLeft(dayText, step * count + 2);
            Canvas.SetTop(dayText, 40);
            Canvas.SetZIndex(dayText, -1);
            CanvasHeader.Children.Add(dayText);
            var dayName = new TextBlock { Text = day.ToString("dddd", cultureFrench)[..1].ToUpper() };
            Canvas.SetLeft(dayName, step * count + 6);
            Canvas.SetTop(dayName, 55);
            Canvas.SetZIndex(dayName, -1);
            CanvasHeader.Children.Add(dayName);
            var line2 = new Line { X1 = count * step, Y1 = 40, X2 = count * step, Y2 = maxHeight, StrokeThickness = 2, Stroke = lineColorLight };
            CanvasHeader.Children.Add(line2);
            var lineVertical = new Line { X1 = count * step, Y1 = 20, X2 = count * step + step, Y2 = 20, StrokeThickness = 2, Stroke = lineColor };
            CanvasHeader.Children.Add(lineVertical);
        }
    }

    public void GetHolidays(object sender, RoutedEventArgs e)
    {
        HttpClient client = new HttpClient();
        var responce = client.GetStringAsync("https://calendrier.api.gouv.fr/jours-feries/metropole.json").Result;
        //var Sunrise = JsonSerializer.Deserialize<SunriseSunsetDto>(responce.ToString()).Results.Sunrise;
        var datas = responce.Replace("{", "").Replace("}", "").Split(", ");
        var holidays = datas.Select(x =>
        {
            var split = x.Split(": ");
            var date = DateTime.Parse(split[0].Replace('"', (char)0));
            var description = split[1].Replace('"', (char)0);
            return new TaskBase(description, date);
        }
        ).ToList();
        Trace.WriteLine(responce);
    }
    private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            if (scrollViewer.Name == nameof(ScrollViewerSchedule))
            {
                ScrollViewerHeader.ScrollToHorizontalOffset(ScrollViewerSchedule.HorizontalOffset);
                ScrollViewerTask.ScrollToVerticalOffset(ScrollViewerSchedule.VerticalOffset);
            }
            if (scrollViewer.Name == nameof(ScrollViewerTask))
            {
                ScrollViewerSchedule.ScrollToVerticalOffset(ScrollViewerTask.VerticalOffset);
            }
        }
    }

    private void Button_Click_UpdateHeader(object sender, RoutedEventArgs e)
    {
        var stopwatch = Stopwatch.StartNew();

        InitCanvasHeader();
        stopwatch.Stop();
        Trace.WriteLine($"stopwatch {stopwatch.Elapsed}");
        var offset = but.ActualOffset;
        Trace.WriteLine(offset);
        InitTab();
    }



    public static IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("endDate must be greater than or equal to startDate");

        while (startDate <= endDate)
        {
            yield return startDate;
            startDate = startDate.AddDays(1);
        }
    }

    private void ScrollViewerSchedule_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var wheelData = e.GetCurrentPoint((Canvas)sender).Properties.MouseWheelDelta;
        //ScrollViewerSchedule.ScrollToHorizontalOffset(wheelData + ScrollViewerSchedule.HorizontalOffset);
        ScrollViewerSchedule.ChangeView(wheelData + ScrollViewerSchedule.HorizontalOffset, null, null);
    }


    private void Button_PointerPressed(object sender, PointerPressEventArgs e)
    {
        MouseDownLocationX = (int)e.PointerPositionRelative;
        if (IsKeyDown(VirtualKey.Control))
        {
            if (sender is PressAndHoldButton bt)
            {
                (bt.DataContext as TaskBase).StartUpdateTimeSpan();
            }
        }
    }

    private void Button_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Button bt)
        {
            var taskBase = bt.DataContext as TaskBase;
            if (e.GetCurrentPoint(bt).Properties.IsLeftButtonPressed && IsKeyDown(VirtualKey.Control))
            {
                var delta = e.GetCurrentPoint(bt).Position.X - MouseDownLocationX;
                taskBase.UpdateTimeSpan(delta);
                return;
            }
            if (e.GetCurrentPoint(bt).Properties.IsLeftButtonPressed)
            {
                var delta = e.GetCurrentPoint(bt).Position.X - MouseDownLocationX;
                taskBase.UpdateStartTime(delta);
                return;
            }
        }
    }

    private void Button_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        MouseDownLocationX = 0;
    }
    static bool IsKeyDown(VirtualKey key)
    {
        return InputKeyboardSource.GetKeyStateForCurrentThread(key).HasFlag(CoreVirtualKeyStates.Down);
    }

    private void ListViewItem_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        MouseDownLocationX = (int)e.GetCurrentPoint(this).Position.X;
    }

    private void ListViewItem_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            var delta = e.GetCurrentPoint(this).Position.X - MouseDownLocationX;
            foreach (var taskBase in TaskBases)
            {
                taskBase.UpdateStartTime(delta);
            }
        }
    }

    private void TreeViewTask_Collapsed(TreeView sender, TreeViewCollapsedEventArgs args)
    {
        args.Node.IsExpanded = true;
    }
}
