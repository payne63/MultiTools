using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base
{
    public class User : INotifyPropertyChanged
    {
        string _Name;
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged(); } }

        string _MailAdress;
        public string MailAdress { get => _MailAdress; set { _MailAdress = value; OnPropertyChanged(); } }

        string _DocumentBuyPath;
        public string DocumentBuyPath { get => _DocumentBuyPath; set { _DocumentBuyPath = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
