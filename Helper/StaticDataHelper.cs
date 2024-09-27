using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiTools.Base;

namespace MultiTools.Helper
{
    public static class StaticDataHelper
    {
        private static List<Contact> _Allcontacts;
        private static List<Company> _AllCompany;

        public async static Task<List<Contact>> GetAllcontacts() 
        { 
                if (_Allcontacts == null) await LoadContacts();
                return _Allcontacts;
        }

        public async static Task<List<Company>> GetAllCompany()
        {
            if (_AllCompany == null) await LoadCompanys();
            return _AllCompany;
        }

        private static async Task LoadContacts()
        {
            _Allcontacts = new List<Contact>();
            foreach (var contact in await JsonHelper.LoadArray<Base.Contact>(MainWindow.ContactsDataPath))
            {
                _Allcontacts.Add(contact);
            }
        }

        public static async void SaveContacts()
            => await JsonHelper.SaveArray<Base.Contact>(_Allcontacts.ToArray(), MainWindow.ContactsDataPath);


        private static async Task LoadCompanys()
        {
            _AllCompany = new List<Company>();
            foreach (var compagny in await JsonHelper.LoadArray<Base.Company>(MainWindow.CompanyDataPath))
            {
                _AllCompany.Add(compagny);
            }
        }

        public static async void SaveCompanys()
            => await JsonHelper.SaveArray<Base.Company>(_AllCompany.ToArray(), MainWindow.CompanyDataPath);

    }


}
