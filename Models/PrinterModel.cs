using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Management;

namespace MultiTools.Models
{
    public class PrinterModel
    {
        public static ObservableCollection<PrinterModel> PrinterModels;

        public string Name { get; private set; }

        public static PrinterModel NullPrinterModel => new PrinterModel("null printer");

        private PrinterModel(string name)
        {
            Name = name;
        }
         public static ObservableCollection<PrinterModel> GetSystemPrinter()
        {
            PrinterModels = new ObservableCollection<PrinterModel>();
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                var status = printer.GetPropertyValue("Status");
                var isDefault = printer.GetPropertyValue("Default");
                var isNetworkPrinter = printer.GetPropertyValue("Network");

                //Console.WriteLine("{0} (Status: {1}, Default: {2}, Network: {3}",name, status, isDefault, isNetworkPrinter);
                PrinterModels.Add(new PrinterModel((string)name));
            }
            return PrinterModels;
        }
    }
}
