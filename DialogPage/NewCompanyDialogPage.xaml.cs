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
using System.Collections.ObjectModel;
using SplittableDataGridSAmple.Base;
using Microsoft.UI.Xaml.Documents;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.DialogPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewCompanyDialogPage : Page
    {

        private Company _companyContext;
        public Company companyContext
        {
            get { return _companyContext; }
            set { _companyContext = value; }
        }
        public NewCompanyDialogPage()
        {
            this.InitializeComponent();
        }
        public NewCompanyDialogPage(Company company )
        {
            this.InitializeComponent();
            //this.companyContext = company;
            this.companyContext = company.CreateDeepCopy();
            CheckCustomer.IsChecked = companyContext.Type.HasFlag(Company.TypeEnum.Client)? true : false;
            CheckSupplier.IsChecked = companyContext.Type.HasFlag(Company.TypeEnum.Fournisseur) ? true : false;
            CheckIntern.IsChecked = companyContext.Type.HasFlag(Company.TypeEnum.Interne) ? true : false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _companyContext.Type |= CheckCustomer.IsChecked == true ? Company.TypeEnum.Client : Company.TypeEnum.Aucun;
            _companyContext.Type |= CheckSupplier.IsChecked == true ? Company.TypeEnum.Fournisseur : Company.TypeEnum.Aucun;
            _companyContext.Type |= CheckIntern.IsChecked == true ? Company.TypeEnum.Interne : Company.TypeEnum.Aucun;
        }
    }
}
