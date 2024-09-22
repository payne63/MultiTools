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
using SplittableDataGridSAmple.Helper;

namespace SplittableDataGridSAmple.Tabs;

public sealed partial class ParameterTab : TabViewItem, Interfaces.IInitTab
{
    public ObservableCollection<Base.User> Users { get => MainWindow.Instance.UsersName;  set { MainWindow.Instance.UsersName = value; } }

    public ParameterTab()
    {
        this.InitializeComponent();
    }
    private async void LoadUsers()
    {
        Users.Clear();
        foreach (var user in await JsonHelper.LoadArray<Base.User>(MainWindow.UsersDataPath))
        {
            Users.Add(user);
        }
    }
    private async void Button_Click_SaveUsers(object sender, RoutedEventArgs e)
    {
        await JsonHelper.SaveArray<Base.User>(Users.ToArray(), MainWindow.UsersDataPath);
    }

    private void Button_Click_NewUser(object sender, RoutedEventArgs e)
    {
        Users.Insert(0,new Base.User { Name = "", MailAdress = "", DocumentBuyPath = "popo.doc" });
    }
    private void Button_Click_RemoveUser(object sender, RoutedEventArgs e)
    {
        var selectedUser = ListViewUsers.SelectedItem  as Base.User;
        if (selectedUser != null)
        {
            TeachingTip.IsOpen = true;
        }
    }
    private void TeachingTip_ActionButtonClick(TeachingTip sender, object args)
    {
        Users.Remove(ListViewUsers.SelectedItem as Base.User);
        TeachingTip.IsOpen=false;
    }
    private void TeachingTip_CloseButtonClick(TeachingTip sender, object args)  => TeachingTip.IsOpen=false;

    public void InitTabAsync()
    {
        if (Users.Count == 0) LoadUsers();
    }
}