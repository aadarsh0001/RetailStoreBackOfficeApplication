using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRetailStore.Models
{
    public class SalesModel : InventoryModel
    {
        /// <summary>
        /// Sales Properties to access data from db or bind data to xaml UI
        /// </summary>
        /// 

        private int _InvoiceNumber;
        public int InvoiceNumber
        {
            get
            { return _InvoiceNumber; }
            set
            {
                _InvoiceNumber = value;
                OnPropertyChanged(nameof(InvoiceNumber));
            }
        }

        private string _SalesDate;
        public string SalesDate
        {
            get
            { return _SalesDate; }
            set
            {
                _SalesDate = value;
                OnPropertyChanged(nameof(SalesDate));
            }
        }
        private string _CustomerName;
        public string CustomerName
        {
            get
            { return _CustomerName; }
            set
            {
                _CustomerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }
        private string _CustomerNumber;
        public string CustomerNumber
        {
            get
            { return _CustomerNumber; }
            set
            {
                _CustomerNumber = value;
                OnPropertyChanged(nameof(CustomerNumber));
            }
        }

        private string Customer_Number;

        public string LCustomer_Number
        {
            get { return Customer_Number; }
            set
            {
                Customer_Number = value;
                OnPropertyChanged(nameof(Customer_Number));
            }
        }

        private int _TotalSalesValueByItemName;
        public int TotalSalesValueByItemName
        {
            get
            { return _TotalSalesValueByItemName; }
            set
            {
                _TotalSalesValueByItemName = value;
                OnPropertyChanged(nameof(TotalSalesValueByItemName));
            }
        }

        private int _SalesQuantity;
        public int SalesQuantity
        {
            get
            { return _SalesQuantity; }
            set
            {
                _SalesQuantity = value;
                OnPropertyChanged(nameof(SalesQuantity));
            }
        }

        private int _TotalSalesValueOfInvoice;
        public int TotalSalesValueOfInvoice
        {
            get
            { return _TotalSalesValueOfInvoice; }
            set
            {
                _TotalSalesValueOfInvoice = value;
                OnPropertyChanged(nameof(TotalSalesValueOfInvoice));
            }
        }



    }
}
