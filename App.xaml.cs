using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MyRetailStore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string UserId { get; internal set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Window loginWindow = new Views.LoginView();
            loginWindow.DataContext = new ViewModels.LoginViewModel();
            loginWindow.Show();

            

            //if (((ViewModels.LoginViewModel)loginWindow.DataContext).IsLoginSuccessfull == true)
            //{
            //    loginWindow.Hide();
            //    Window mainwindow = new Views.MainView();
            //    mainwindow.DataContext = new ViewModels.MainViewModel();
            //    mainwindow.Show();
            //    loginWindow.Close();
            //}


            
        }
    }
}
