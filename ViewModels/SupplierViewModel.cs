using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyRetailStore.Commands;
using MyRetailStore.Models;

namespace MyRetailStore.ViewModels
{
    class SupplierViewModel:BaseViewModel
    {
        //--------------------------------------Constructor
        public SupplierViewModel()
        {
            SaveSupplierCommand = new RelayCommand(new Action<object>(AddSupplier));
            AddSupplierCommand = new RelayCommand(new Action<object>(JumpToAddSupplier));
            SaveUpdateCommand = new RelayCommand(new Action<object>(UpdateSupplier));
            JumpToUpdateSupplier = new DelegateCommand<object>((obj) => { jumpSupplierUpdate(obj); });
            ObservableCollection<SupplierModel> _ = new ObservableCollection<SupplierModel>();
            Supplier = new ObservableCollection<SupplierModel>();
            USuppliers = new ObservableCollection<InventoryModel>();
            IEnumerable MyActiveSuppliers = null;
            IEnumerable MyInactiveSuppliers = null;
            GetSupplierInfo(out MyActiveSuppliers, out MyInactiveSuppliers);
            Active.Clear();
            if (MyActiveSuppliers != null)
            {

                foreach (SupplierModel item in MyActiveSuppliers)
                {
                    Active.Add(item);
                }
            }
            if (Active != null)
            {
                Supplier = Active;
            }
        }


        //-----------------------------------Properties


        ObservableCollection<SupplierModel> Active = new();
        ObservableCollection<SupplierModel> Inactive = new();

        //------------------------------Property for check box

        private bool _IsSelectedBox;
        public bool IsSelectedBox
        {
            get
            { return _IsSelectedBox; }
            set
            {

                if (_IsSelectedBox == value) return;
                IEnumerable MyActiveSuppliers = null;
                IEnumerable MyInactiveSuppliers = null;
                GetSupplierInfo(out MyActiveSuppliers, out MyInactiveSuppliers);
                Active.Clear();
                Inactive.Clear();
                if (MyActiveSuppliers != null)
                {
                    foreach (SupplierModel item in MyActiveSuppliers)
                    {
                        Active.Add(item);

                    }
                }

                if (MyInactiveSuppliers != null)
                {
                    foreach (SupplierModel item in MyInactiveSuppliers)
                    {
                        Inactive.Add(item);
                    }
                }

                _IsSelectedBox = value;

                Supplier = _IsSelectedBox ? Inactive : Active;

                OnPropertyChanged(nameof(IsSelectedBox));
                OnPropertyChanged(nameof(Supplier));

            }

        }

        //------------------------------ Method  for clear input box

        private void ClearInputControls()
        {
            Name = string.Empty;
            MobileNumber = string.Empty;
            Address = string.Empty;
            Email = string.Empty;
            OnPropertyChanged("");
        }

        //------------------------------ Method  for refreshing grid

        public void HotReload(string ShowList)
        {
            IEnumerable MyActiveSuppliers = null;
            IEnumerable MyInactiveSuppliers = null;
            GetSupplierInfo(out MyActiveSuppliers, out MyInactiveSuppliers);
            Active.Clear();
            Inactive.Clear();
            foreach (SupplierModel item in MyActiveSuppliers)
            {
                Active.Add(item);

            }

            foreach (SupplierModel item in MyInactiveSuppliers)
            {
                Inactive.Add(item);
            }
            if (ShowList.ToUpper() == "A")
            {
                Supplier = Active;

            }
            else if (ShowList.ToUpper() == "IA")
            {
                Supplier = Inactive;
            }
           
        }

        //------------------------------ Properties

        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public int Status { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        //------------------------------ Method for getting suppliers information

        public void GetSupplierInfo(out IEnumerable MyActiveSuppliers, out IEnumerable MyInactiveSuppliers)
        {
            IEnumerable ActiveSuppliers = null;
            IEnumerable InactiveSuppliers = null;

            List<SupplierModel> listOfSupplier = new();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                SqlCommand query = new SqlCommand("VIEWSUPPLIER", conn);

                query.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);

                foreach (DataRow row in dataTable.Rows)
                {
                    SupplierModel m = new SupplierModel();
                    m.SNo = (int)row["S_ID"];
                    m.SupplierName = row["S_NAME"].ToString();
                    m.Mobile = row["S_MOBILE"].ToString();
                    m.Status = (int)row["S_STATUS"];
                    m.Email = row["S_EMAILID"].ToString();
                    m.Address = row["S_ADDRESS"].ToString();
                    if (m.Status == 0)
                    {
                        m.isStatus = "Inactive";
                    }
                    else
                    {
                        m.isStatus = "Active";



                    }
                    listOfSupplier.Add(new SupplierModel
                    {
                        SNo = m.SNo,
                        SupplierName = m.SupplierName,
                        Mobile = m.Mobile,
                        Status = m.Status,
                        isStatus = m.isStatus,

                        Email = m.Email,
                        Address = m.Address

                    });

                    ActiveSuppliers = from s in listOfSupplier
                                      where s.isStatus == "Active"
                                      select s;

                    InactiveSuppliers = from s in listOfSupplier
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
                    MyActiveSuppliers = ActiveSuppliers;
                    MyInactiveSuppliers = InactiveSuppliers;
                    conn.Close();
                }
            }
        }

        //------------------------------ Property for suppliers selection

        private SupplierModel _SelectedSupplier;

        public SupplierModel SelectedSupplier
        {
            get { return _SelectedSupplier; }
            set
            {
                _SelectedSupplier = value;
                OnPropertyChanged(nameof(SelectedSupplier));

            }
        }

        //------------------------------ Method for navigate to update supplier UI

        private void jumpSupplierUpdate(object obj)
        {
            Window window = new Views.Windows.UpdateSupplierView();
            window.DataContext = this;
            window.Show();

        }

        //------------------------------ Property for update supplier UI

        private ICommand _JumpToUpdateSupplier;
        public ICommand JumpToUpdateSupplier
        {
            get { return _JumpToUpdateSupplier; }
            set { _JumpToUpdateSupplier = value; }
        }

        //------------------------------ Property for add supplier UI

        private ICommand _AddSupplierCommand;
        public ICommand AddSupplierCommand
        {
            get { return _AddSupplierCommand; }
            set { _AddSupplierCommand = value; }
        }

        //------------------------------ Method for update supplier UI

        private void JumpToAddSupplier(object obj)
        {
            Window AddSupplier = new Views.Windows.AddSuplierView
            {
                 
            DataContext = this
            };
            ClearInputControls();
            AddSupplier.Show();
        }

        //------------------------------ Property for save supplier button

        private ICommand _SaveSupplierCommand;
        public ICommand SaveSupplierCommand
        {
            get { return _SaveSupplierCommand; }
            set { _SaveSupplierCommand = value; }
        }

        //------------------------------ Property for supplier information


        private ObservableCollection<SupplierModel> _Supplier;

        public ObservableCollection<SupplierModel> Supplier
        {
            get { return _Supplier; }
            set
            {
                _Supplier = value;
                OnPropertyChanged(nameof(Supplier));
            }
        }

        //------------------------------ Property to check duplicate supplier

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

        //------------------------ Variable for duplicacy

        bool Canexist = false;
        string checkduplicacy;

        //------------------------ Method for add supplier

        private void AddSupplier(object obj)
        {
            
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (Name == string.Empty || MobileNumber == string.Empty || Email == string.Empty)
                {
                    MessageBox.Show("Please fill all details", "Datails missing", MessageBoxButton.OK);                  
                    return;

                }

                if (MobileNumber.Length > 10)
                {
                    MessageBox.Show("Mobile number length is exceeding", "Limit exceeding", MessageBoxButton.OK);
                    return;

                }

                else
                {
                    try
                    {


                        if (conn == null)
                        {

                            throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                        }


                   
                        conn.Open();
                        SqlCommand USupplierCommand = new SqlCommand("SELECT DISTINCT S_NAME, S_MOBILE, S_EMAILID FROM R_SUPPLIERS WHERE S_STATUS=1", conn);

                        using (SqlDataReader reader = USupplierCommand.ExecuteReader())
                        {

                            USuppliers.Clear();
                            while (reader.Read())
                            {
                                USuppliers.Add(new InventoryModel { SupplierName = reader["S_NAME"].ToString(), Mobile = reader["S_MOBILE"].ToString(), Email = reader["S_EMAILID"].ToString() });
                                
                            }
                        }

                        foreach(var supplier in USuppliers)
                        {

                            if(supplier.SupplierName==Name||supplier.Mobile==MobileNumber||supplier.Email==Email)
                            {
                                Canexist = true;
                                if (supplier.SupplierName==Name)
                                {
                                    checkduplicacy = "Supplier name";
                                }                               
                                if(supplier.Mobile==MobileNumber)
                                { checkduplicacy = "Mobile number"; }
                                if(supplier.Email==Email)
                                { checkduplicacy = "Email";}
                                if(supplier.SupplierName == Name && supplier.Mobile == MobileNumber && supplier.Email == Email)
                                { checkduplicacy = "Values"; }
                                if(supplier.SupplierName == Name && supplier.Mobile == MobileNumber)
                                { checkduplicacy = "Name and mobile number"; }
                                if(supplier.SupplierName == Name &&supplier.Email == Email)
                                { checkduplicacy = "Name and email"; }
                                if(supplier.Mobile == MobileNumber && supplier.Email == Email)
                                { checkduplicacy = "Mobile number and email"; }
                               
                                
                            }
                        }
                        if(Canexist)
                        {
                            MessageBox.Show(checkduplicacy+" already exists", "Exisiting Details", MessageBoxButton.OK);
                            Canexist = false;
                            return;
                        }

                        SqlCommand query = new SqlCommand("ADDSUPPLIER", conn);
                        query.CommandType = CommandType.StoredProcedure;
                        SqlParameter pName = new SqlParameter("@sS_NAME", SqlDbType.VarChar);
                        SqlParameter pMobileNumber = new SqlParameter("@sS_MOBILE", SqlDbType.VarChar);
                        SqlParameter pAddress = new SqlParameter("@sS_ADDRESS", SqlDbType.VarChar);
                        SqlParameter pEmail = new SqlParameter("@sS_EMAIL", SqlDbType.VarChar);

                        pName.Value = Name;
                        pMobileNumber.Value = MobileNumber;
                        pAddress.Value = Address;
                        Regex regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                        Match match = regex.Match(Email);
                        if (match.Success)
                        {
                            pEmail.Value = Email;
                        }
                        else
                        {
                            MessageBox.Show("Please enter valid email ID", "Invalid email", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            pName.Value = Name;
                            pMobileNumber.Value = MobileNumber;
                            pAddress.Value = Address;
                            Email = string.Empty;
                            OnPropertyChanged("");
                            return;

                        }

                        query.Parameters.Add(pName);
                        query.Parameters.Add(pMobileNumber);
                        query.Parameters.Add(pEmail);
                        query.Parameters.Add(pAddress);

                        query.ExecuteNonQuery();

                        MessageBox.Show("Supplier added successfully", "Supplier added", System.Windows.MessageBoxButton.OK);
                        ClearInputControls();
                        HotReload("a");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                    finally
                    {
                        conn.Close();                    
                        
                        
                    }
                }
            }
        }

        //------------------------------ Property to save update updated supplier


        private ICommand _SaveUpdateCommand;
        public ICommand SaveUpdateCommand
        {
            get { return _SaveUpdateCommand; }
            set { _SaveUpdateCommand = value; }
        }

        //------------------------------ Method to update supplier
        private void UpdateSupplier(object obj)
        {
            if (SelectedSupplier.SupplierName== string.Empty || SelectedSupplier.Email == string.Empty || SelectedSupplier.Mobile == string.Empty)
            {
                MessageBox.Show("Please fill all details", "Datails missing", MessageBoxButton.OK);

                HotReload("a");
                return;

            }
            else
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    {
                        if (conn == null)
                        {
                            throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                        }

                        SqlCommand query = new SqlCommand("UPDATESUPPLIERS", conn);
                        conn.Open();
                        query.CommandType = CommandType.StoredProcedure;
                        SqlParameter pMobileNumber = new SqlParameter("@sS_MOBILE", SqlDbType.VarChar);
                        SqlParameter pStatus = new SqlParameter("@sS_STATUS", SqlDbType.Int);
                        SqlParameter pId = new SqlParameter("@sS_ID", SqlDbType.Int);
                        SqlParameter pName = new SqlParameter("@sS_NAME", SqlDbType.VarChar);
                        SqlParameter pEmail = new SqlParameter("@sS_EMAIL", SqlDbType.VarChar);
                        SqlParameter pAddress = new SqlParameter("@sS_ADDRESS", SqlDbType.VarChar);

                        pMobileNumber.Value = SelectedSupplier.Mobile;
                        pStatus.Value = SelectedSupplier.isStatus == "Inactive" ? 0 : (object)1;
                        pId.Value = SelectedSupplier.SNo;
                        pAddress.Value = SelectedSupplier.Address;

                        Regex regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                        Match match = regex.Match(SelectedSupplier.Email);
                        if (match.Success)
                        {
                            pEmail.Value = SelectedSupplier.Email;
                        }
                        else
                        {
                            MessageBox.Show("Please enter valid email ID", "Invalid email", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                        }

                        pName.Value = SelectedSupplier.SupplierName;

                        query.Parameters.Add(pName);
                        query.Parameters.Add(pEmail);
                        query.Parameters.Add(pAddress);
                        query.Parameters.Add(pMobileNumber);
                        query.Parameters.Add(pStatus);
                        query.Parameters.Add(pId);


                        query.ExecuteNonQuery();
                        MessageBox.Show("Supplier updated successfully", "Supplier updated", System.Windows.MessageBoxButton.OK);
                        IsSelectedBox = false;
                    }
                    HotReload("a");
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }

                finally
                {
                    IsSelectedBox = false;
                    HotReload("a");
                }
               
            }
           
        }
    }

}
