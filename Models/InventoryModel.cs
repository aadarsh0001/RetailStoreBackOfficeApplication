using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyRetailStore.ViewModels;

namespace MyRetailStore.Models
{
  public  class InventoryModel : SupplierModel
    {
        private int _ItemCode;
        public int ItemCode
        {
            get
            { return _ItemCode; }
            set
            {
                _ItemCode = value;
                OnPropertyChanged(nameof(ItemCode));
            }
        }

        private string _ItemCategory;
        public string Itemcategory
        {
            get
            { return _ItemCategory; }
            set
            {
                _ItemCategory = value;
                OnPropertyChanged(nameof(Itemcategory));
            }
        }

        private string _SubCategory;
        public string SubCategory
        {
            get
            { return _SubCategory; }
            set
            {
                _SubCategory = value;
                OnPropertyChanged(nameof(SubCategory));
            }
        }

        private string _Itemname;
        public string Itemname
        {
            get
            { return _Itemname; }
            set
            {
                _Itemname = value;
                OnPropertyChanged(nameof(Itemname));
            }
        }

        private int _PurchasePricePerUnit;
        public int PurchasePricePerUnit
        {
            get
            { return _PurchasePricePerUnit; }
            set
            {
                _PurchasePricePerUnit =value;
                OnPropertyChanged(nameof(PurchasePricePerUnit));
            }
        }

        private int _SalesPricePerUnit;
        public int SalesPricePerUnit
        {
            get
            { return _SalesPricePerUnit; }
            set
            {
                _SalesPricePerUnit = value;
                OnPropertyChanged(nameof(SalesPricePerUnit));
            }
        }

        private int _StockHand;
        public int StockInHand
        {
            get
            { return _StockHand; }
            set
            {
                _StockHand = value;
                OnPropertyChanged(nameof(StockInHand));
            }
        }
       
    }
}
