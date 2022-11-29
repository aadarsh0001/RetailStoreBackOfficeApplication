using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyRetailStore.ViewModels;

namespace MyRetailStore.Views.Windows
{
    /// <summary>
    /// Interaction logic for CreatePurchaseInvoiceView.xaml
    /// </summary>
    public partial class CreatePurchaseInvoiceView : Window
    {
        public CreatePurchaseInvoiceView()
        {
            InitializeComponent();

        }
        public MessageBoxResult result;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            result = MessageBox.Show("Are you sure want to close this window ?", "Comfirmation Required", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                
            }


        }
    }
}
