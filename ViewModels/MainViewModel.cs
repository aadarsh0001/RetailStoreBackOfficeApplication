using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyRetailStore.Commands;
using MyRetailStore.Models;
using MyRetailStore.ViewModels;

namespace MyRetailStore.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            //---------------------------------------set dashboard as a primary window-----
            ShiftView("DashBoard");
            //--------------------------------------------------------------------- Navigation
            NavigateCommand = new RelayCommand(new Action<object>(ShiftView));

        }


        public FrameworkElement ViewArea { get; set; }
        public string MyProperty { get; set; }
        public void ShiftView(object commandParameter)
        {
            string parameter = commandParameter.ToString();
            switch (parameter)
            {
                //-----------------------------------------------------------Views Navigation
                case "LogoutApp":
                    ProcessStartInfo Info = new ProcessStartInfo();
                    // Info.Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + System.Reflection.Assembly.GetEntryAssembly().Location + "\"";
                    Info.WindowStyle = ProcessWindowStyle.Hidden;
                    Info.CreateNoWindow = true;
                    string fileName = "MyRetailStore.exe";
                    string path = Path.Combine(Environment.CurrentDirectory, fileName);
                    Process.Start(path);
                    Process.GetCurrentProcess().Kill();
                    break;

                case "addNewItem":
                    Window newItem = new Views.Windows.AddNewItemView();
                    newItem.Show();
                    break;

                case "addNewCategory":
                    Window newCategory = new Views.Windows.AddCategoryView();
                    newCategory.Show();
                    break;

                case "RetailStore":
                    ViewArea = new Views.UserControlView.AddStoreView();
                    ViewArea.DataContext = new StoreViewModel();
                    break;

                case "ItemCategory":
                    ViewArea = new Views.UserControlView.ItemCategoryView();
                    ViewArea.DataContext = new ItemCategoryViewModel();
                    break;

                case "CreateItem":
                    ViewArea = new Views.UserControlView.CreateItemView();
                    ViewArea.DataContext = new CreateItemViewModel();
                    break;

                case "DashBoard":
                    ViewArea = new Views.UserControlView.DashboardView();
                    ViewArea.DataContext = new DashboardViewModel();
                    break;

                case "CloseApp":
                    Application.Current.Shutdown();
                    break;

                case "SupplierManagement":
                    ViewArea = new Views.UserControlView.SupplierManagementView();
                    ViewArea.DataContext = new SupplierViewModel();
                    break;

                case "SalesRecordManagement":
                    ViewArea = new Views.UserControlView.SalesRecord();
                    ViewArea.DataContext = new SalesViewModel();

                    break;

                case "PurchaseRecordManagement":
                    ViewArea = new Views.UserControlView.PurchaseRecord();
                    ViewArea.DataContext = new PurchaseViewModel();
                    break;

                case "UserManagement":
                    ViewArea = new Views.UserControlView.UserManagementView();
                    ViewArea.DataContext = new UserManagementViewModel();
                    break;

                //-------------------------------------------------------------Return Navigation


                case "returnToManageSupplier":
                    ViewArea = new Views.UserControlView.SupplierManagementView();
                    break;

                case "ReturnToManageUser":
                    ViewArea = new Views.UserControlView.UserManagementView();
                    break;

                //------------------------------------------------------------------Update Navigation
                case "UpdateUserCommand":
                    Window updateUser = new Views.Windows.UpdateUserView();
                    updateUser.Show();
                    break;


            }
            OnPropertyChanged(nameof(ViewArea));
        }

        private ICommand _NavigateCommand;

        public ICommand NavigateCommand
        {
            get { return _NavigateCommand; }
            set { _NavigateCommand = value; }
        }

    }
}
