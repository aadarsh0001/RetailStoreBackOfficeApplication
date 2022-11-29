using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MyRetailStore.Commands;
using MyRetailStore.Models;

namespace MyRetailStore.ViewModels
{
    class ItemCategoryViewModel : BaseViewModel
    {
        public ItemCategoryViewModel()
        {
            //---------------------------------------------------------------- commands for business and presentation layer 

            SaveCommand = new RelayCommand(new Action<object>(AddCategory));
            AddCategoryCommand = new RelayCommand(new Action<object>(JumpToAddCategory));
            UCategory = new ObservableCollection<InventoryModel>();

            SaveUpdateCommand = new RelayCommand(new Action<object>(UpdateCategory));
            JumpToUpdateCategory = new DelegateCommand<object>((obj) => { jumpCategoryUpdate(obj); });
            CategoryInfo = new ObservableCollection<InventoryModel>();

            //------------------------------------------------ Property for viewing active and inactive items 
            IEnumerable MyActiveCategory = null;
            IEnumerable MyInactiveCategory = null;
            GetCategoryInfo(out MyActiveCategory, out MyInactiveCategory);
            Active.Clear();
            if (MyActiveCategory != null)
            {
                foreach (InventoryModel item in MyActiveCategory)
                {
                    Active.Add(item);
                }
            }
            if (Active != null)
            {
                CategoryInfo = Active;
                this._view = new ListCollectionView(this.Active);
            }

            this._view = new ListCollectionView(this._CategoryInfo);
            this._view.Filter = Filter;

        }
        //------------------------------------------------ Property for filtering record 
        private bool Filter(object item)
        {
            if (String.IsNullOrEmpty(TextSearch))
                return true;
            else if (FilteredList == "Category")
                return ((item as InventoryModel).Itemcategory.IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Sub Category")
                return ((item as InventoryModel).SubCategory.IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else
                return false;
        }

        private ListCollectionView _view;
        public ICollectionView View
        {
            get { return this._view; }
        }

        //------------------------------------------------ Property for filtering  collections 

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

        //------------------------------------------------ Property for text search 
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

        private bool _IsSelectedBox;
        public bool IsSelectedBox
        {
            get
            { return _IsSelectedBox; }
            set
            {

                if (_IsSelectedBox == value) return;
                IEnumerable MyActiveCategory = null;
                IEnumerable MyInactiveCategory = null;
                GetCategoryInfo(out MyActiveCategory, out MyInactiveCategory); ;
                Active.Clear();
                Inactive.Clear();
                if (MyActiveCategory != null)
                {
                    foreach (InventoryModel item in MyActiveCategory)
                    {
                        Active.Add(item);

                    }
                }

                if (MyInactiveCategory != null)
                {
                    foreach (InventoryModel item in MyInactiveCategory)
                    {
                        Inactive.Add(item);
                    }
                }

                _IsSelectedBox = value;
                this._view = _IsSelectedBox ? new ListCollectionView(Inactive) : new ListCollectionView(Active);
                this._view.Filter = Filter;

                OnPropertyChanged(nameof(IsSelectedBox));
                OnPropertyChanged(nameof(View));
               

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
                   
                    SqlCommand UCategoryCommand = new SqlCommand("SELECT DISTINCT CC_CATEGORY FROM R_CREATECATEGORY WHERE CC_STATUS=1", conn);

                    using (SqlDataReader reader = UCategoryCommand.ExecuteReader())
                    {

                        UCategory.Clear();
                        while (reader.Read())
                        {

                            UCategory.Add(new InventoryModel { Itemcategory = reader["CC_CATEGORY"].ToString() });
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


        private InventoryModel _SelectedCategory;
        public InventoryModel SelectedCategory
        {
            get { return _SelectedCategory; }
            set
            {
                _SelectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));

            }
        }
        private void ClearInputControls()
        {
            Itemcategory = string.Empty;
            SubCategory = string.Empty;
            
            OnPropertyChanged("");
        }

        public void HotReload(string ShowList)
        {
            IEnumerable MyActiveCategory = null;
            IEnumerable MyInactiveCategory = null;
            GetCategoryInfo(out MyActiveCategory, out MyInactiveCategory); ;
            Active.Clear();
            Inactive.Clear();
            foreach (InventoryModel item in MyActiveCategory)
            {
                Active.Add(item);

            }

            foreach (InventoryModel item in MyInactiveCategory)
            {
                Inactive.Add(item);
            }
            if (ShowList.ToUpper() == "A")
            {
                CategoryInfo = Active;

            }
            else if (ShowList.ToUpper() == "IA")
            {
                CategoryInfo = Inactive;
            }
            else
            {
                {
                    CategoryInfo = Active;
                }
                OnPropertyChanged(nameof(CategoryInfo));
            }
        }

        private ICommand uSaveCommand;
        public ICommand SaveCommand
        {
            get { return uSaveCommand; }
            set { uSaveCommand = value; }
        }

        private ICommand _SaveUpdateCommand;

        public ICommand SaveUpdateCommand
        {
            get { return _SaveUpdateCommand; }
            set { _SaveUpdateCommand = value; }
        }

        private ICommand _AddCategoryCommand;
        public ICommand AddCategoryCommand
        {
            get { return _AddCategoryCommand; }
            set { _AddCategoryCommand = value; }
        }
        private ICommand _JumpToUpdateCategory;
        public ICommand JumpToUpdateCategory
        {
            get { return _JumpToUpdateCategory; }
            set { _JumpToUpdateCategory = value; }
        }

        private void JumpToAddCategory(object obj)
        {
            Window addCategory = new Views.Windows.AddCategoryView();
            MyDropDownList();
            addCategory.DataContext = this;
            addCategory.Show();
        }

       

       
      

        ObservableCollection<InventoryModel> Active = new();
        ObservableCollection<InventoryModel> Inactive = new();
        private ObservableCollection<InventoryModel> _CategoryInfo;

        public ObservableCollection<InventoryModel> CategoryInfo
        {
            get { return _CategoryInfo; }
            set
            {
                _CategoryInfo = value;
                OnPropertyChanged(nameof(CategoryInfo));
            }
        }


        public void AddCategory(object obj)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                    }

                    SqlCommand query = new SqlCommand("ADDCATEGORY", conn);
                    conn.Open();
                    query.CommandType = CommandType.StoredProcedure;
                    SqlParameter pCategory = new SqlParameter("@cc_CATEGORY", SqlDbType.VarChar);
                    SqlParameter pSubCategory = new SqlParameter("@cc_SUBCATEGORY", SqlDbType.VarChar);

                    pCategory.Value = Itemcategory;
                    pSubCategory.Value = SubCategory;
                
                    query.Parameters.Add(pCategory);
                    query.Parameters.Add(pSubCategory);
                    
                    query.ExecuteNonQuery();
                }
                ClearInputControls();
                HotReload("a");
                MyDropDownList();
                MessageBox.Show("Category added successfully", "Category added", MessageBoxButton.OK);

            }
            catch
            {
                MessageBox.Show("Please try again", "Try again", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }


        public void GetCategoryInfo(out IEnumerable MyActiveCategory, out IEnumerable MyInactiveCategory)
        {
            IEnumerable ActiveCategory = null;
            IEnumerable InactiveCategory = null;

            List<InventoryModel> listOfCategory = new();
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                SqlCommand query = new SqlCommand("VIEWINVENTORYCATEGORY", conn);

                query.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                foreach (DataRow row in dataTable.Rows)
                {
                    InventoryModel m = new InventoryModel();
                    m.ItemCode = (int)row["CC_CATEGORYCODE"];
                    m.Itemcategory = row["CC_CATEGORY"].ToString();
                    m.SubCategory = row["CC_SUBCATEGORY"].ToString();
                    m.Status = (int)row["CC_STATUS"];
                    if (m.Status == 0)
                    {
                        m.isStatus = "Inactive";
                    }
                    else
                    {
                        m.isStatus = "Active";
                    }

                    listOfCategory.Add(new InventoryModel
                    {
                        ItemCode = m.ItemCode,
                        Itemcategory = m.Itemcategory,
                        SubCategory = m.SubCategory,
                        isStatus = m.isStatus,
                        Status = m.Status
                    });

                    ActiveCategory = from s in listOfCategory
                                  where s.isStatus == "Active"
                                  select s;

                    InactiveCategory = from s in listOfCategory
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

                    MyActiveCategory = ActiveCategory;
                    MyInactiveCategory = InactiveCategory;
                    conn.Close();
                }
            }
        }



        public void UpdateCategory(object obj)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))

                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in RetailDBN->Properties-?Settings.settings");
                    }
                    else
                    {
                        SqlCommand query = new SqlCommand("UPDATECATEGORY", conn);
                        conn.Open();
                        query.CommandType = CommandType.StoredProcedure;
                        SqlParameter pStatus = new SqlParameter("@cc_STATUS", SqlDbType.Int);
                        SqlParameter pId = new SqlParameter("@cc_CATEGORYCODE", SqlDbType.Int);
                        SqlParameter pCategory = new SqlParameter("@cc_CATEGORY", SqlDbType.VarChar);
                        SqlParameter pSubCategory = new SqlParameter("@cc_SUBCATEGORY", SqlDbType.VarChar);


                        pId.Value = SelectedCategory.ItemCode;
                        pCategory.Value = SelectedCategory.Itemcategory;
                        pSubCategory.Value = SelectedCategory.SubCategory;                      
                        pStatus.Value = SelectedCategory.isStatus == "Inactive" ? 0 : (object)1;


                        query.Parameters.Add(pCategory);
                        query.Parameters.Add(pSubCategory);
                        query.Parameters.Add(pStatus);
                        query.Parameters.Add(pId);
                     
                        query.ExecuteNonQuery();

                    }
                    MessageBox.Show("Category updated successfully", "Category updated", MessageBoxButton.OK);
                    HotReload("a");

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        private void jumpCategoryUpdate(object obj)
        {
            Window window = new Views.Windows.UpdateCategoryView();
            window.DataContext = this;
            window.Show();

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

        private int _Status;
        public int Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

    }


}
