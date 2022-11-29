using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRetailStore.Models
{
    public class PurchaseModel : InventoryModel
    {

        /// <summary>
        /// Purchase Properties to access data from db or bind data to xaml UI
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

        private string _PurchaseDate;
        public string PurchaseDate
        {
            get
            { return _PurchaseDate; }
            set
            {
                _PurchaseDate = value;
                OnPropertyChanged(nameof(PurchaseDate));
            }
        }

        private int _TotalPurchaseValueByItemName;
        public int TotalPurchaseValueByItemName
        {
            get
            { return _TotalPurchaseValueByItemName; }
            set
            {
                _TotalPurchaseValueByItemName = value;
                OnPropertyChanged(nameof(TotalPurchaseValueByItemName));
            }
        }

        private int _PurchaseQuantity;
        public int PurchaseQuantity
        {
            get
            { return _PurchaseQuantity; }
            set
            {
                _PurchaseQuantity = value;
                OnPropertyChanged(nameof(PurchaseQuantity));
            }
        }

        private int _StockInHand;
        public int StockInHand
        {
            get
            { return _StockInHand; }
            set
            {
                _StockInHand = value;
                OnPropertyChanged(nameof(StockInHand));
            }
        }


        private int _TotalPurchaseValueOfInvoice;
        public int TotalPurchaseValueOfInvoice
        {
            get
            { return _TotalPurchaseValueOfInvoice; }
            set
            {
                _TotalPurchaseValueOfInvoice = value;
                OnPropertyChanged(nameof(TotalPurchaseValueOfInvoice));
            }
        }




    }
}
