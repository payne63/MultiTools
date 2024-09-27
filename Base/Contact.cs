using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MultiTools.Base {
    public class Contact : INotifyPropertyChanged {


        private bool _IsMale = true;
        public bool IsMale { get => _IsMale; set { _IsMale = value; OnPropertyChanged(); } }

        string _Name;
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged(); } }

        string _PreName;
        public string PreName { get => _PreName; set { _PreName = value; OnPropertyChanged(); } }

        string _PhoneNumber;
        public string PhoneNumber { get => _PhoneNumber; set { _PhoneNumber = value; OnPropertyChanged(); } }
        string _PhoneNumber2;
        public string PhoneNumber2 { get => _PhoneNumber2; set { _PhoneNumber2 = value; OnPropertyChanged(); } }

        string _Mail;
        public string Mail { get => _Mail; set { _Mail = value; OnPropertyChanged(); } }

        private Company _Company;
        public Company Company { get { return _Company; } set { _Company = value; OnPropertyChanged(); } }

        string _JobDecription;
        public string JobDescription { get => _JobDecription; set { _JobDecription = value; OnPropertyChanged(); } }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Contact CreateDeepCopy() {
            var newContact = new Contact();
            newContact.Name = _Name;
            newContact.PreName = _PreName;
            newContact.JobDescription = _JobDecription;
            newContact.Mail = _Mail;
            newContact.PhoneNumber = _PhoneNumber;
            newContact.PhoneNumber2 = _PhoneNumber2;
            newContact.Company = _Company;
            newContact.IsMale = _IsMale;

            return newContact;
        }
        public void CopyData(Contact contact) {
            this.Name = contact._Name;
            this.PreName = contact._PreName;
            this.JobDescription = contact._JobDecription;
            this.Mail = contact._Mail;
            this.PhoneNumber = contact._PhoneNumber;
            this.PhoneNumber2 = contact._PhoneNumber2;
            this.Company = contact._Company;
            this.IsMale= contact._IsMale;
        }
    }
}
