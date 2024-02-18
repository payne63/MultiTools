using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace SplittableDataGridSAmple.Base;

public class TaskBase : INotifyPropertyChanged
{
    #region InotifyPropertyChange
    public event PropertyChangedEventHandler PropertyChanged;
    internal void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    public TaskBase(string description, DateTime date)
    {
        Description = description;
        Date = date;
        Children.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
            {
                this.IsExpanded = true;
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (TaskBase addItem in e.NewItems)
                    {
                        addItem.LeftOffset = LeftOffset + 15;
                    }
                }

            };
    }

    private SolidColorBrush colorBrush = new(Windows.UI.Color.FromArgb(255, 200, 20, 80));

    public SolidColorBrush ColorBrush
    {
        get => colorBrush;
        set
        {
            colorBrush = value;
            NotifyPropertyChanged();
        }
    }

    private double leftOffset = 0;
    public double LeftOffset
    {
        get => leftOffset;
        set
        {
            leftOffset = value; 
            NotifyPropertyChanged();
        }
    }

    public static readonly int BlocSize = 20;

    private int temporaryActualDay;

    private ObservableCollection<TaskBase> m_children;
    public ObservableCollection<TaskBase> Children
    {
        get
        {
            m_children ??= new ObservableCollection<TaskBase>();
            return m_children;
        }
        set => m_children = value;
    }
    private bool m_isExpanded;
    public bool IsExpanded
    {
        get => m_isExpanded;
        set
        {
            if (m_isExpanded != value)
            {
                m_isExpanded = value;
                NotifyPropertyChanged();
            }
        }
    }

    public string Description
    {
        get; private set;
    }
    public DateTime Date
    {
        get; private set;
    }

    private int countDay = 2;
    public int CountDay
    {
        get => countDay;
        set
        {

            countDay = value < 1 ? 1 : value;
            Width = value * BlocSize;
            NotifyPropertyChanged();
        }
    }

    public int width = 40;
    public int Width
    {
        get => width;
        set
        {
            width = value < 20 ? 20 : value;
            NotifyPropertyChanged();
        }
    }
    private double leftPos = 100;

    public void UpdateStartTime(double deltaMouse)
    {
        LeftPos += deltaMouse;
        RecursiveUpdateStartTime(this, deltaMouse);
    }

    private static void RecursiveUpdateStartTime(TaskBase parent, double deltaMouse)
    {
        if (parent.Children.Count > 0)
        {
            foreach (TaskBase child in parent.Children)
            {
                child.LeftPos += deltaMouse;
                RecursiveUpdateStartTime(child, deltaMouse);
            }
        }
    }

    public void StartUpdateTimeSpan() => temporaryActualDay = countDay;
    public void UpdateTimeSpan(double deltaMouse)
    {
        var dayExtend = (int)(deltaMouse / 20);
        CountDay = temporaryActualDay + dayExtend;
    }


    public double LeftPos
    {
        get => leftPos;
        set
        {
            leftPos = ((int)value / 20) * 20 ;
            NotifyPropertyChanged();
        }
    }

}

