using RtfPipe.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiTools.Base
{
    public class Company : INotifyPropertyChanged
    {
        string _Name;
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged(); } }

        [Flags]
        public enum TypeEnum { Aucun=0, Client=1, Fournisseur=2, Interne=4 }
        TypeEnum _Type;
        public TypeEnum Type { get => _Type; set { _Type = value; OnPropertyChanged(); } }
       

        string _Address;
        public string Address { get => _Address; set { _Address = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Company CreateDeepCopy()
        {
            var newCompany = new Company();
            newCompany.Name = _Name;
            newCompany.Address = _Address;
            newCompany.Type = _Type;

            return newCompany;
        }

        public void CopyData(Company contact)
        {
            this.Name = contact._Name;
            this.Type = contact.Type;
            this.Address = contact.Address;
        }
    }
}
