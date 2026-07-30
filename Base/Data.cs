using System;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace MultiTools.Base
{
    public class Data : NotifyPropertyChangedBase
    {
        public string Code { get { return NameFile.Substring(0, NameFile.LastIndexOf('.')); } }
        public string NameFile { get; set; } = String.Empty;
        public string NewNameFile { get; set; } = String.Empty;
        public string FullPathName { get; set; } = String.Empty;
        public string NewFullPathName { get; set; } = String.Empty;
        public string Author { get; set; }
        public string Compagny { get; set; }
        public string Project { get; set; }
        public List<string> ReferenceDocument { get; set; } = new List<string>();
        public Inventor.DocumentTypeEnum DocumentType { get; set; }
        //public stdole.IPictureDisp thumbnail { get; set; }
        public delegate void IsSelectedEventHandler(Data data);
        public event IsSelectedEventHandler IsSelectedClick;

        public string IconBase => DocumentType switch
        {
            Inventor.DocumentTypeEnum.kPartDocumentObject => "ms-appx:///Images/part.png",
            Inventor.DocumentTypeEnum.kAssemblyDocumentObject => "ms-appx:///Images/assembly.png",
            Inventor.DocumentTypeEnum.kDrawingDocumentObject => "ms-appx:///Images/drawing.png",
            _ => throw new Exception("type de document inventor non défini")
        };

        private bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }

            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    OnPropertyChanged("IsSelected");
                    if (m_isSelected)
                    {
                        Trace.WriteLine($" file selected : {this.Code}");
                        IsSelectedClick?.Invoke(this);
                    }
                }
            }
        }


        private ObservableCollection<Data> m_children;
        public ObservableCollection<Data> Children
        {
            get
            {
                if (m_children == null) // pour pas crash si aucun enfant.
                {
                    m_children = new ObservableCollection<Data>();
                }
                return m_children;
            }
            set
            {
                m_children = value;
            }
        }

        private bool m_isExpanded;
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                if (m_isExpanded != value)
                {
                    m_isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        
        // public event PropertyChangedEventHandler PropertyChanged;
        //
        // private void NotifyPropertyChanged(string propertyName)
        // {
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }
    }
}
