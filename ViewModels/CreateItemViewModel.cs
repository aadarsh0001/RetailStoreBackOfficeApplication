using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MyRetailStore.Commands;
using MyRetailStore.Models;

namespace MyRetailStore.ViewModels
{
  public  class CreateItemViewModel : BaseViewModel
    {
        public CreateItemViewModel()
        {
            // --------------------- Implementing Icommands for interaction between business and presentation layer 
            
            SaveUpdateCommand = new RelayCommand(new Action<object>(UpdateItemDetails));   
            JumpToUpdateCategory = new DelegateCommand<object>((obj) => { jumpCategoryUpdate(obj); });
            AddNewItem = new RelayCommand(new Action<object>(JumpToAddItem));
            SaveCommand = new RelayCommand(new Action<object>(AddItems));

           //-------------------------- Declaring collection for storing data 

            UCategory = new ObservableCollection<InventoryModel>();        
            UItemName = new ObservableCollection<InventoryModel>();
            USubCategory = new ObservableCollection<InventoryModel>();
            USuppliers = new ObservableCollection<InventoryModel>();
            Items = new ObservableCollection<InventoryModel>();

            //-------------------------Checking for active and inactive entries 
         
            IEnumerable MyActiveItems = null;
            IEnumerable MyInactiveItems = null;
            GetItemInfo(out MyActiveItems, out MyInactiveItems);
            Active.Clear();
            if (MyActiveItems != null)
            {
                foreach (InventoryModel item in MyActiveItems)
                {
                    Active.Add(item);
                }
            }
            if (Active != null)
            {
                Items = Active;
                this._view = new ListCollectionView(this.Active);


            }
            this._view = new ListCollectionView(this._Items);
            this._view.Filter = Filter;


        }


        //-------------------------------------------- Method for filtering data based on list 

        private bool Filter(object item)
        {
            if (String.IsNullOrEmpty(TextSearch))
                return true;
            else if (FilteredList == "Category")
                return ((item as InventoryModel).Itemcategory.IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Sub Category")
                return ((item as InventoryModel).SubCategory.IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Item Name")
                return ((item as InventoryModel).Itemname.IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Supplier")
                return ((item as InventoryModel).SupplierName.IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else
                return false;
        }

        //------------------------------------------------ Property for viewing list collections 

        private ListCollectionView _view;
        public ICollectionView View
        {
            get { return this._view; }
        }

        //------------------------------------------------ Property for searching via text 
        private string _TextSearch;
        public string TextSearch
        {
            get { return _TextSearch; }
            set
            {
                if (FilteredList != null)   
                {
                    _TextSearch = value;
                    OnPropertyChanged("TextSearch");
                    View.Refresh();
                }
                else
                    View.Refresh();
            }
        }

        //------------------------------------------------ Property resulted filter list
        private string _FilteredList;
        public string FilteredList
        {
            get
            { return _FilteredList; }
            set
            {
                TextSearch = string.Empty;

                _FilteredList = value;
                OnPropertyChanged(nameof(FilteredList));
            }
        }

        //------------------------------------------------ Property for viewing checkbox inactive or active 

        private bool _IsSelectedBox;
        public bool IsSelectedBox
        {
            get
            { return _IsSelectedBox; }
            set
            {

                if (_IsSelectedBox == value) return;
                IEnumerable MyActiveItems = null;
                IEnumerable MyInactiveItems = null;
                GetItemInfo(out MyActiveItems, out MyInactiveItems);
                Active.Clear();
                Inactive.Clear();
                if (MyActiveItems != null)
                {
                    foreach (InventoryModel item in MyActiveItems)
                    {
                        Active.Add(item);

                    }
                }

                if (MyInactiveItems != null)
                {
                    foreach (InventoryModel item in MyInactiveItems)
                    {
                        Inactive.Add(item);
                    }
                }

                _IsSelectedBox = value;

                _view = _IsSelectedBox ? new ListCollectionView(Inactive) : new ListCollectionView(Active);
                this._view.Filter = Filter;


                OnPropertyChanged(nameof(IsSelectedBox));
                OnPropertyChanged(nameof(View));

            }

        }

        //--------------------------------------------------------------Method for updating details 
        public void UpdateItemDetails(object obj)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))

                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                    }
                    else
                    {
                        SqlCommand query = new SqlCommand("UPDATEITEM", conn);
                        conn.Open();
                        query.CommandType = CommandType.StoredProcedure;
                        SqlParameter pStatus = new SqlParameter("@ci_STATUS", SqlDbType.Int);
                        SqlParameter pId = new SqlParameter("@ci_ITEMCODE", SqlDbType.Int);
                       
                        SqlParameter pItemName = new SqlParameter("@ci_ITEMNAME", SqlDbType.VarChar);
                        SqlParameter pPurchasePriceUnit = new SqlParameter("@ci_PURCHASEPRICEUNIT", SqlDbType.Int);
                        SqlParameter pSupplierName = new SqlParameter("@ci_SUPPLIERNAME", SqlDbType.VarChar);
                        SqlParameter pStockInHand = new SqlParameter("@ci_STOCKINHAND", SqlDbType.Int);
                        SqlParameter pSalePriceUnit = new SqlParameter("@ci_SALESPRICEUNIT", SqlDbType.Int);

                        pId.Value = SelectedItems.ItemCode;
                        pStatus.Value = SelectedItems.isStatus == "Inactive" ? 0 : (object)1;
                        
                        pItemName.Value = SelectedItems.Itemname;
                        pPurchasePriceUnit.Value = SelectedItems.PurchasePricePerUnit;
                        pSupplierName.Value = SelectedItems.SupplierName;
                        pStockInHand.Value = SelectedItems.StockInHand;
                        pSalePriceUnit.Value = SelectedItems.SalesPricePerUnit;



                        query.Parameters.Add(pItemName);
                        query.Parameters.Add(pPurchasePriceUnit);
                        query.Parameters.Add(pSupplierName);
                        query.Parameters.Add(pStockInHand);
                        query.Parameters.Add(pSalePriceUnit);
                        query.Parameters.Add(pStatus);
                        query.Parameters.Add(pId);

                       


                        query.ExecuteNonQuery();
                        MessageBox.Show("Category updated successfully", "Category updated", System.Windows.MessageBoxButton.OK);

                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                HotReload("a");
                
            }
        }

        //----------------------------------------------------------- Property for saving updated collection 

        private ICommand _SaveUpdateCommand;

        public ICommand SaveUpdateCommand
        {
            get { return _SaveUpdateCommand; }
            set { _SaveUpdateCommand = value; }
        }


        //----------------------------------------------------------Method to jump on category update page 
        private void jumpCategoryUpdate(object obj)
        {
            MyDropDownList();
            Window window = new Views.Windows.UpdateItemDetailsView();
            window.DataContext = this;
            window.Show();

        }

        //---------------------------------------------------------- Property for jumpcategory page 

        private ICommand _JumpToUpdateCategory;
        public ICommand JumpToUpdateCategory
        {
            get { return _JumpToUpdateCategory; }
            set { _JumpToUpdateCategory = value; }
        }

        //------------------------------------------------ Property for checking duplicacy

        private ObservableCollection<InventoryModel> _UItemName;
        public ObservableCollection<InventoryModel> UItemName
        {
            get { return _UItemName; }
            set
            {
                _UItemName = value;
                OnPropertyChanged(nameof(UItemName));
            }
        }
        bool Canexist = false;                 //------------------- Variable fo boolean type for duplicacy

        //-----------------------------------------------Method  for adding items 
        public void AddItems(object obj)
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                try
                {



                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                    }
                    conn.Open();
                    SqlCommand UItemNameCommand = new SqlCommand("select distinct(ci_itemname) from r_createitem", conn);



                    using (SqlDataReader reader = UItemNameCommand.ExecuteReader())
                    {
                        //  UItemName.Clear();



                        while (reader.Read())
                        {
                            UItemName.Add(new InventoryModel { Itemname = reader["ci_itemname"].ToString() });



                        }
                    }



                    foreach (var item in UItemName)
                    {
                        if (item.Itemname == Itemname)
                        {
                            Canexist = true;

                        }

                    }
                    if (Itemcategory == string.Empty || SubCategory == string.Empty || Itemname == string.Empty || SupplierName == string.Empty || PurchasePricePerUnit == 0 || StockInHand == 0 || SalesPricePerUnit == 0)
                    {
                        MessageBox.Show("Please fill all details", "Datails missing", MessageBoxButton.OK);
                      
                        return;
                    }
                    if (Canexist)
                    {
                        MessageBox.Show("Item name already exists", "item name exists", MessageBoxButton.OK);
                                               
                        return;
                    }
                    else
                    {
                        SqlCommand query = new SqlCommand("ADDNEWITEM", conn);



                        query.CommandType = CommandType.StoredProcedure;
                        SqlParameter pCategory = new SqlParameter("@ci_CATEGORY", SqlDbType.VarChar);
                        SqlParameter pSubCategory = new SqlParameter("@ci_SUBCATEGORY", SqlDbType.VarChar);
                        SqlParameter pItemName = new SqlParameter("@ci_ITEMNAME", SqlDbType.VarChar);
                        SqlParameter pSupplierName = new SqlParameter("@ci_SUPPLIERNAME", SqlDbType.VarChar);
                        SqlParameter pPurchasePriceUnit = new SqlParameter("@ci_PURCHASEPRICEUNIT", SqlDbType.Int);
                        SqlParameter pStockInHand = new SqlParameter("@ci_STOCKINHAND", SqlDbType.Int);
                        SqlParameter pSalePriceUnit = new SqlParameter("@ci_SALESPRICEUNIT", SqlDbType.Int);


                        pCategory.Value = Itemcategory;
                        pSubCategory.Value = SubCategory;
                        pItemName.Value = Itemname;
                        pSupplierName.Value = SupplierName;
                        pPurchasePriceUnit.Value = PurchasePricePerUnit;
                        pStockInHand.Value = StockInHand;
                        pSalePriceUnit.Value = SalesPricePerUnit;


                        query.Parameters.Add(pCategory);
                        query.Parameters.Add(pSubCategory);
                        query.Parameters.Add(pItemName);
                        query.Parameters.Add(pSupplierName);
                        query.Parameters.Add(pPurchasePriceUnit);
                        query.Parameters.Add(pStockInHand);
                        query.Parameters.Add(pSalePriceUnit);



                        query.ExecuteNonQuery();

                        MessageBox.Show("Item created successfully", "Item added", MessageBoxButton.OK);

                    }

                    ClearInputControls();
                    HotReload("a");
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message, "Try again", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                finally
                {
                   

                    conn.Close();
                }
            }
            
        }

        //-------------------------------------------------------------------- Property for unique catagory 

        private ObservableCollection<InventoryModel> _UCategory;
        public ObservableCollection<InventoryModel> UCategory
        {
            get { return _UCategory; }
            set
            {
                _UCategory = value;
                OnPropertyChanged(nameof(UCategory));
            }
        }

        private ObservableCollection<InventoryModel> _USubCategory;
        public ObservableCollection<InventoryModel> USubCategory
        {
            get { return _USubCategory; }
            set
            {
                _USubCategory = value;
                OnPropertyChanged(nameof(USubCategory));
            }
        }

        private ObservableCollection<InventoryModel> _USuppliers;
        public ObservableCollection<InventoryModel> USuppliers
        {
            get { return _USuppliers; }
            set
            {
                _USuppliers = value;
                OnPropertyChanged(nameof(USuppliers));
            }
        }

       
        public void MyDropDownList()
        {
           

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {
                
                SqlCommand query = new SqlCommand();
                conn.Open();
                    UCategory.Clear();
                    USubCategory.Clear();
                    USuppliers.Clear();
                
                    SqlCommand UCategoryCommand = new SqlCommand("SELECT DISTINCT CC_CATEGORY FROM R_CREATECATEGORY WHERE CC_STATUS=1", conn);
                  
                    using (SqlDataReader reader = UCategoryCommand.ExecuteReader())   // Reading row one by one 
                    {
                        
                        UCategory.Clear();
                        while (reader.Read())
                        {
                           
                            UCategory.Add(new InventoryModel { Itemcategory = reader["CC_CATEGORY"].ToString() });
                        }
                       
                    }

                    SqlCommand USubCategoryCommand = new SqlCommand("SELECT DISTINCT CC_SUBCATEGORY FROM R_CREATECATEGORY WHERE CC_STATUS=1", conn);

                    using (SqlDataReader reader = USubCategoryCommand.ExecuteReader())
                    {
                        USubCategory.Clear();

                        while (reader.Read())
                        {
                            USubCategory.Add(new InventoryModel { SubCategory = reader["CC_SUBCATEGORY"].ToString() });

                        }
                    }

                    SqlCommand USupplierCommand = new SqlCommand("SELECT DISTINCT S_NAME FROM R_SUPPLIERS WHERE S_STATUS=1", conn);

                    using (SqlDataReader reader = USupplierCommand.ExecuteReader())
                    {

                        USuppliers.Clear();
                        while (reader.Read())
                        {
                            USuppliers.Add(new InventoryModel { SupplierName = reader["S_NAME"].ToString() });
                        }
                    }

                }
                catch (SqlException ex)
                {
                    throw ex;
                }
                finally
                {
                   
                    conn.Close();
                }
            }
        }

        private void ClearInputControls()
        {
            Itemcategory = string.Empty;
            SubCategory = string.Empty;
            SupplierName = string.Empty;
            Itemname = string.Empty;
            SalesPricePerUnit = 0 ;
            PurchasePricePerUnit = 0;
            StockInHand = 0;

            OnPropertyChanged("");
        }

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

        string CatRelatedSub;

        private string _ItemCategory;
        public string Itemcategory
        {
            get
            { return _ItemCategory; }
            set
            {
                _ItemCategory = value;
                OnPropertyChanged(nameof(Itemcategory));
                CatRelatedSub = Itemcategory;
                CatRelatedToSub();

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
                _Itemname = _SubCategory + "-" + _ItemCategory;   // itemanme concatenate using subcatagory and catagory
                OnPropertyChanged("");
                OnPropertyChanged(nameof(SubCategory));
            }
        }


        public void CatRelatedToSub()

        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {

                    SqlCommand query = new SqlCommand();
                    conn.Open();

                    USubCategory.Clear();

                    SqlCommand USubCategoryCommand = new SqlCommand("SELECT DISTINCT CC_SUBCATEGORY FROM R_CREATECATEGORY WHERE CC_STATUS=1 AND CC_CATEGORY='" + CatRelatedSub + "'", conn);

                    using (SqlDataReader reader = USubCategoryCommand.ExecuteReader())
                    {
                        USubCategory.Clear();

                        while (reader.Read())
                        {
                            USubCategory.Add(new InventoryModel { SubCategory = reader["CC_SUBCATEGORY"].ToString() });

                        }
                    }

                }
                catch (SqlException ex)
                {
                    throw ex;
                }
                finally
                {

                    conn.Close();
                }
            }
        }


        private string _SupplierName;
        public string SupplierName
        {
            get
            { return _SupplierName; }
            set
            {
                _SupplierName = value;
                OnPropertyChanged(nameof(SupplierName));
            }
        }




        private string _Itemname ;
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
                _PurchasePricePerUnit = value;
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

        //------------------------------------------------ for refreshing data in collection 
        public void HotReload(string ShowList)
        {
            IEnumerable MyActiveItems = null;
            IEnumerable MyInactiveItems = null;
            GetItemInfo(out MyActiveItems, out MyInactiveItems);
            Active.Clear();
            Inactive.Clear();
            foreach (InventoryModel item in MyActiveItems)
            {
                Active.Add(item);

            }

            foreach (InventoryModel item in MyInactiveItems)
            {
                Inactive.Add(item);
            }
            if (ShowList.ToUpper() == "A")
            {
                Items = Active;

            }
            else if (ShowList.ToUpper() == "IA")
            {
                Items = Inactive;
            }
            else
            {
                {
                    Items = Active;
                }
                OnPropertyChanged(nameof(Items));
            }
        }


        public void GetItemInfo(out IEnumerable MyActiveItems, out IEnumerable MyInactiveItems)
        {
            IEnumerable ActiveItems = null;
            IEnumerable InactiveItems = null;


            List<InventoryModel> listOfItems = new();
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                SqlCommand query = new SqlCommand("VIEWINVENTORYITEMS", conn);

                query.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                foreach (DataRow row in dataTable.Rows)
                {

                    InventoryModel m = new InventoryModel();
                    m.ItemCode = (int)row["CI_ITEMCODE"];
                    m.Itemcategory = row["CI_ITEMCATEGORY"].ToString();
                    m.SubCategory = row["CI_ITEMSUBCAT"].ToString();
                    m.SupplierName = row["CI_SUPPLIERNAME"].ToString();
                    m.Itemname = row["CI_ITEMNAME"].ToString();
                    m.StockInHand = (int)row["CI_STOCKINHAND"];
                    m.PurchasePricePerUnit = (int)row["CI_PURCHASEPRICEUNIT"];
                    m.SalesPricePerUnit = (int)row["CI_SALESPRICEUNIT"];
                    m.Status = (int)row["CI_STATUS"];
                    if (m.Status == 0)
                    {
                        m.isStatus = "Inactive";
                    }
                    else
                    {
                        m.isStatus = "Active";
                    }
                    listOfItems.Add(new InventoryModel
                    {
                        ItemCode = m.ItemCode,
                        Itemcategory = m.Itemcategory,
                        SubCategory = m.SubCategory,
                        SupplierName = m.SupplierName,
                        Itemname = m.Itemname,
                        isStatus = m.isStatus,
                        Status = m.Status,
                        StockInHand = m.StockInHand,
                        PurchasePricePerUnit = m.PurchasePricePerUnit,
                        SalesPricePerUnit = m.SalesPricePerUnit,
  
                    });

                    ActiveItems = from s in listOfItems
                                  where s.isStatus == "Active"
                                  select s;

                    InactiveItems = from s in listOfItems
                                    where s.isStatus == "Inactive"
                                    select s;


                }

                try
                {
                    conn.Open();
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
                finally
                {
                    MyActiveItems = ActiveItems;
                    MyInactiveItems = InactiveItems;
                    conn.Close();
                }
            }
        }


        private ObservableCollection<InventoryModel> _Items;

        public ObservableCollection<InventoryModel> Items
        {
            get { return _Items; }
            set
            {
                _Items = value;
                OnPropertyChanged(nameof(Items));
            }
        }


        private InventoryModel _SelectedItems;

        public InventoryModel SelectedItems
        {
            get { return _SelectedItems; }
            set
            {
                _SelectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));

            }
        }

        ObservableCollection<InventoryModel> Active = new();
        ObservableCollection<InventoryModel> Inactive = new();


        private ICommand _AddNewItem;
        public ICommand AddNewItem
        {
            get { return _AddNewItem; }
            set { _AddNewItem = value; }
        }
        
        private ICommand _SaveCommand;
        public ICommand SaveCommand
        {
            get { return _SaveCommand; }
            set { _SaveCommand = value; }
        }
        private void JumpToAddItem(object obj)
        {
            Window addItem = new Views.Windows.AddNewItemView();
            MyDropDownList();
            addItem.DataContext = this;
            addItem.Show();
        }

    }

   
}
