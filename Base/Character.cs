// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Base
{
    public class Character : INotifyPropertyChanged
    {
        public ObservableCollection<Character> Childrens { get; set; }
        string _Name;
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged(); } }
        string _Age;
        public string Age { get => _Age; set { _Age = value; OnPropertyChanged(); } }
        bool _Work;
        public bool Work { get => _Work; set { _Work = value; OnPropertyChanged(); } }
        public string MoreInfoName => $"this is {Name}";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void AddChildren(Character character)
        {
            Childrens ??= new();
            Childrens.Add(character);
        }
    }
}
