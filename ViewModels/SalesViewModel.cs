using MyRetailStore.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyRetailStore.Models;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MyRetailStore.ViewModels
{
    class SalesViewModel : BaseViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SalesViewModel()
        {

            /*
             * Handdling NUll Value Exception 
             * for all these following list and observeable collection
             */

            SalesItemList = new List<SalesModel>();
            ListInvoice = new List<PurchaseModel>();
            SalesRecordsList = new List<SalesModel>();
            SelectedCustomer_Name = new SalesModel();
            SelectedSalesInvoice = new SalesModel();
            SelectedSalesItem = new SalesModel();
            UItemName = new ObservableCollection<InventoryModel>();
            USubCategory = new ObservableCollection<InventoryModel>();
            UCategory = new ObservableCollection<InventoryModel>();
            UCustomers = new List<SalesModel>();
            SelectedCustomer_Name = new SalesModel();
            SelectedSalesInvoice = new SalesModel();
            SelectedSalesItem = new SalesModel();
            UItemName = new ObservableCollection<InventoryModel>();
            USubCategory = new ObservableCollection<InventoryModel>();
            UCategory = new ObservableCollection<InventoryModel>();
            UCustomers = new List<SalesModel>();
            TempCurrentItemHolder = new List<SalesModel>();
            TempSalesItemCollection = new List<SalesModel>();


            /*
            * Intially Getting values from Various Methods.
            * These Methods are Following
            */

            AllInOneTotalValueHandlingFunc();
            UpdateAddSalesItemCommand = new RelayCommand(new Action<object>(UpdateJumpToAddSalesItem));
            UpdateSalesQuantity = new RelayCommand(new Action<object>(UpdateItemQuantity));
            JumpToUpdateSalesItem = new RelayCommand(new Action<object>(JumpToUpdateSelectedRecord));
            RemoveSalesItem = new RelayCommand(new Action<object>(RemoveSelectedRecord));
            SaveInvoiceItemCommand = new RelayCommand(new Action<object>(SaveAllRecordsToDBFinally));
            RemoveSalesInvoice = new RelayCommand(new Action<object>(RemoveSelectedInvoice));
            JumpToUpdateSalesRecordCommand = new RelayCommand(new Action<object>(JumpToUpdateSalesRecord));
            AddSalesItemCommand = new RelayCommand(new Action<object>(JumpToAddSalesItem));
            AddSalesItem = new RelayCommand(new Action<object>(AddSalesItemMethod));
            SalesItemList = GetSalesItemsList();
            SalesRecordsList = GetSalesRecords();
            this._view = new ListCollectionView(this._SalesRecordsList);
            this._view.Filter = Filter;
            SalesInvoiceCommand = SalesInvoiceCommand = new RelayCommand(new Action<object>(JumpToSalesInvoice));
        }


        private List<PurchaseModel> _listInvoice;

        public List<PurchaseModel> ListInvoice
        {
            get
            {
                return _listInvoice;
            }
            set
            {
                _listInvoice = value;
                OnPropertyChanged(nameof(ListInvoice));
            }
        }

        /// <summary>
        /// Handling Total Purchase Value After Deletion of
        /// invoice number and item
        /// </summary>
        /// 
        private void AllInOneTotalValueHandlingFunc()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                conn.Open();

                SqlCommand GetPurchaseItemCMD = new SqlCommand("select su_invoicenumber from R_salesRECORD", conn);

                using (SqlDataReader reader = GetPurchaseItemCMD.ExecuteReader())
                {

                    ListInvoice.Clear();
                    while (reader.Read())
                    {
                        ListInvoice.Add(new PurchaseModel
                        {

                            InvoiceNumber = (int)reader["su_invoicenumber"],

                        });
                    }
                }

                //-----------------------------------------------

                SqlCommand query = new SqlCommand("UpdateTotalInPurchaseRecords", conn);
                query.CommandType = CommandType.StoredProcedure;
                SqlParameter pInvoiceNumber = new SqlParameter("@invoiceNumber", SqlDbType.Int);
                SqlParameter pPurchaseTotalValue = new SqlParameter("@totalPurchaseValue", SqlDbType.Int);

                foreach (var item in ListInvoice)
                {
                    pInvoiceNumber.Value = item.InvoiceNumber;
                    pPurchaseTotalValue.Value = 0;

                    query.Parameters.Add(pInvoiceNumber);
                    query.Parameters.Add(pPurchaseTotalValue);

                    query.ExecuteNonQuery();
                    query.Parameters.Clear();
                }

                SqlCommand query2 = new SqlCommand("update R_salesRECORD set sU_TOTALsalesVALUE = 0 where sU_TOTALsalesVALUE is null;", conn);
                query2.ExecuteNonQuery();

                conn.Close();


            }

        }

        bool IsJumpedInUpdate;

        // Jumping to Update Add Sales Item
        public void UpdateJumpToAddSalesItem(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            Window AddSalesWindow = new Views.Windows.AddSalesItemView();

            if (LCustomer_Name == string.Empty && LCustomer_Number == string.Empty)
            {
                MessageBox.Show("Please enter custome name and number", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            if (LCustomer_Name == string.Empty)
            {
                MessageBox.Show("Please enter custome name", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            if (LCustomer_Number == string.Empty)
            {
                MessageBox.Show("Please enter customer Number", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }



            LCustomer_Name = SelectedSalesInvoice.CustomerName;
            LCustomer_Number = SelectedSalesInvoice.CustomerNumber;

            isItemBasedOnFirstClick = true;
            UItemNameList();
            UCategoryList();

            IsJumpedInUpdate = true;
            AddSalesWindow.DataContext = this;
            AddSalesWindow.Show();
        }

        private ICommand _UpdateAddSalesItemCommand;
        public ICommand UpdateAddSalesItemCommand
        {
            get { return _UpdateAddSalesItemCommand; }
            set { _UpdateAddSalesItemCommand = value; }
        }

        int ItemInSalesItemList;
        int LastQunatity;
        int ExistingQuantity;

        /// <summary>
        /// Updating Quantity of Existing Item as well temporary
        /// created Items..
        /// All Actions Happens on Some Conditons.
        /// AS well as updating stock in hand and total sale value
        /// </summary>
        
        private void UpdateItemQuantity(object obj)
        {
            AllInOneTotalValueHandlingFunc();
            foreach (var item in SalesItemList)
            {

                if (SelectedSalesItem.ItemCode == item.ItemCode)
                {
                    ItemInSalesItemList = 1;
                }
            }

            //updating Quantity of Existing Item
            if (ItemInSalesItemList == 1)
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        if (conn == null)
                        {
                            throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                        }

                        conn.Open();

                        SqlCommand ExistingQuantityCommandOfItem = new SqlCommand("select sI_salesQUANTITY from R_salesINVOICE where sI_INVOICENUMBER ='" + SelectedSalesInvoice.InvoiceNumber + "' AND" +
                            " sI_ITEMCODE = '" + SelectedSalesItem.ItemCode + "'", conn);
                        LastQunatity = Convert.ToInt32(ExistingQuantityCommandOfItem.ExecuteScalar());
                        LastQunatity = SelectedSalesItem.SalesQuantity - LastQunatity;

                        //getting existing stock in hand of item
                        SqlCommand ExistingQuantityCommand = new SqlCommand("select CI_STOCKINHAND  from R_CREATEITEM  where CI_ITEMCODE='" + SelectedSalesItem.ItemCode + "'", conn);
                        ExistingQuantity = Convert.ToInt32(ExistingQuantityCommand.ExecuteScalar());

                        /*
                         * updating sales quantity and total sales values
                         * of item
                         */
                        SqlCommand UpdateQuantityCommand = new SqlCommand("UPDATE R_SALESINVOICE SET" +
                            " SI_SALESQUANTITY = '" + SelectedSalesItem.SalesQuantity + "' ,SI_TOTALSALESVALUE = '" + SelectedSalesItem.SalesQuantity * SelectedSalesItem.SalesPricePerUnit + "' WHERE" +
                            " SI_INVOICENUMBER = '" + SelectedSalesInvoice.InvoiceNumber + "' AND SI_ITEMCODE = '" + SelectedSalesItem.ItemCode + "'", conn);
                        UpdateQuantityCommand.ExecuteNonQuery();

                        SqlCommand MinusQuantityCommand = new SqlCommand("update R_CREATEITEM set CI_STOCKINHAND = '" + (ExistingQuantity - LastQunatity) + "' where CI_ITEMCODE='" + SelectedSalesItem.ItemCode + "'", conn);
                        MinusQuantityCommand.ExecuteNonQuery();

                        MessageBox.Show("Sales quantity updated successfully ", "Quantity updated", MessageBoxButton.OK);


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                    finally
                    {
                        AllInOneTotalValueHandlingFunc();

                        SqlCommand TotalPriceByInvoiceCommand = new SqlCommand("select sum(si_totalSalesvalue) as TotalByInvoice from r_Salesinvoice where si_invoicenumber ='" + SelectedSalesInvoice.InvoiceNumber + "'", conn);

                        LTotalByInvoice = Convert.ToInt32(TotalPriceByInvoiceCommand.ExecuteScalar());
                        foreach (var item in TempSalesItemCollection)
                        {
                            LTotalByInvoice += item.TotalSalesValueByItemName;
                            OnPropertyChanged("");
                            OnPropertyChanged(nameof(LTotalByInvoice));
                        }
                        SqlCommand UpdateQuantityCommand = new SqlCommand("UPDATE R_SALESRECORD SET SU_TOTALSALESVALUE = '" + LTotalByInvoice + "' WHERE SU_INVOICENUMBER = '" + SelectedSalesInvoice.InvoiceNumber + "'", conn);
                        UpdateQuantityCommand.ExecuteNonQuery();

                        TotalSalesValueOfInvoice = LTotalByInvoice;
                        SelectedSalesInvoice.TotalSalesValueOfInvoice = LTotalByInvoice;
                        OnPropertyChanged(nameof(SelectedSalesInvoice));
                        SalesItemList.AddRange(TempSalesItemCollection);
                        SalesRecordsList = GetSalesRecords();

                        SalesItemList.AddRange(TempSalesItemCollection);
                        this._view2 = new ListCollectionView(this._SalesItemList);
                        OnPropertyChanged(nameof(View2));

                        conn.Close();
                    }
            }

            /*
             * Updating sales Quantity for temporary Added
             * Item name
             */
            else
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        var result = from r in TempSalesItemCollection where r.ItemCode == SelectedSalesItem.ItemCode select r;

                        result.First().SalesQuantity = SelectedSalesItem.SalesQuantity;

                        SqlCommand TotalPriceByInvoiceCommand = new SqlCommand("select SI_TOTALSALESVALUE from R_SALESINVOICE where SI_INVOICENUMBER='" + SelectedSalesItem.InvoiceNumber + "'", conn);
                        conn.Open();
                        LTotalByInvoice = Convert.ToInt32(TotalPriceByInvoiceCommand.ExecuteScalar());

                        foreach (var item in TempSalesItemCollection)
                        {
                            item.TotalSalesValueByItemName = item.SalesPricePerUnit * item.SalesQuantity;

                            LTotalByInvoice += item.TotalSalesValueByItemName;
                            TotalSalesValueByItemName = item.TotalSalesValueByItemName;
                            OnPropertyChanged("");
                            OnPropertyChanged(nameof(TotalSalesValueByItemName));
                            OnPropertyChanged(nameof(LTotalByInvoice));

                        }

                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                    }
                    finally
                    {
                        AllInOneTotalValueHandlingFunc();
                        SalesItemList.AddRange(TempSalesItemCollection);

                        SelectedSalesInvoice.TotalSalesValueOfInvoice = LTotalByInvoice;
                        SalesRecordsList = GetSalesRecords();

                        SalesItemList = GetSalesItemsList();
                        SalesItemList.AddRange(TempCurrentItemHolder);
                        this._view2 = new ListCollectionView(this._SalesItemList);
                        OnPropertyChanged(nameof(View2));
                        OnPropertyChanged("TotalSalesValueByItemName");
                        conn.Close();
                    }

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

        private ICommand _UpdateSalesQuantity;
        public ICommand UpdateSalesQuantity
        {
            get { return _UpdateSalesQuantity; }
            set { _UpdateSalesQuantity = value; }
        }

        // jumping to update selected record UI
        private void JumpToUpdateSelectedRecord(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            Window UpdateSalesItem = new Views.Windows.UpdateSalesItemView();
            UpdateSalesItem.DataContext = this;
            OnPropertyChanged(nameof(SelectedSalesItem));
            UpdateSalesItem.Show();
        }

        private ICommand _JumpToUpdateSalesItem;
        public ICommand JumpToUpdateSalesItem
        {
            get { return _JumpToUpdateSalesItem; }
            set { _JumpToUpdateSalesItem = value; }
        }

        private SalesModel _SelectedSalesItem2;
        public SalesModel SelectedSalesItem2
        {
            get { return _SelectedSalesItem2; }
            set
            {
                _SelectedSalesItem2 = value;
                OnPropertyChanged(nameof(SelectedSalesItem2));
            }
        }

        bool RemovalStatusForTemp;
        int RemovalStatus;
        int TbyItem;
        int TbyInvoice;
        int TotalQuantityByItem;

        /// <summary>
        /// Removing Selected Items
        /// from DB.
        /// Also Clearing All Items Related To this invoice number from DB,
        /// Adjusting Total sales Value, Stock In Hand etc
        /// </summary>
         
        private void RemoveSelectedRecord(object obj)
        {
            AllInOneTotalValueHandlingFunc();
            MessageBoxResult RequestConfirm = MessageBox.Show("Do you want to remove record", "Confirm removal", MessageBoxButton.YesNo);

            if (RequestConfirm == MessageBoxResult.Yes)
            {
                SelectedSalesItem2 = SelectedSalesItem;
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        if (conn == null)
                        {
                            throw new Exception("Connection String is Null. Set the value of Connection String in Retail->Properties-?Settings.settings");
                        }

                        foreach (var item in SalesItemList)
                        {

                            if (SelectedSalesItem.ItemCode == item.ItemCode)
                            {
                                TbyItem = SelectedSalesItem.TotalSalesValueByItemName;
                                TbyInvoice = SelectedSalesInvoice.TotalSalesValueOfInvoice;
                                LTotalByInvoice = TbyInvoice - TbyItem;
                                TotalQuantityByItem = SelectedSalesItem.SalesQuantity;
                                conn.Open();

                                //getting existing stock in hand for item from DB
                                SqlCommand ExistingQuantityCommand = new SqlCommand("select CI_STOCKINHAND  from R_CREATEITEM  where CI_ITEMCODE='" + SelectedSalesItem.ItemCode + "'", conn);
                                ExistingQuantity = Convert.ToInt32(ExistingQuantityCommand.ExecuteScalar());

                                if (TotalQuantityByItem > ExistingQuantity)
                                {
                                    MessageBox.Show("Insufficient quantity", "Operation cancelled", MessageBoxButton.OK);
                                    return;
                                }
                                else
                                {
                                    //Deleting Selected Record based on item code and invoice number
                                    SqlCommand UDeleteItemCommand = new SqlCommand("DELETE FROM R_SALESINVOICE  WHERE SI_ITEMCODE ='" + SelectedSalesItem.ItemCode + "' and SI_INVOICENUMBER='" + SelectedSalesInvoice.InvoiceNumber + "'", conn);
                                    UDeleteItemCommand.ExecuteNonQuery();

                                    //updating total purchase value
                                    SqlCommand UUpdateTotalCommand = new SqlCommand("UPDATE R_SALESRECORD SET SU_TOTALSalesVALUE='" + LTotalByInvoice + "' WHERE SU_INVOICENUMBER='" + SelectedSalesInvoice.InvoiceNumber + "'", conn);
                                    UUpdateTotalCommand.ExecuteNonQuery();

                                    //updating stock in hand
                                    SqlCommand MinsusQuantityCommand = new SqlCommand("update R_CREATEITEM set CI_STOCKINHAND = '" + (ExistingQuantity + TotalQuantityByItem) + "' where CI_ITEMCODE='" + SelectedSalesItem.ItemCode + "'", conn);
                                    MinsusQuantityCommand.ExecuteNonQuery();

                                    RemovalStatus = 1;
                                }


                            }

                        }
                        //Getting Fresh Data From DB after Removal
                        if (RemovalStatus == 1)
                        {
                            SqlCommand GetPurchaseItemCMD = new SqlCommand("select a.* ,(b.ci_stockinhand + a.sI_SalesQUANTITY) TotalQuantity from R_SalesINVOICE a left join R_CREATEITEM b on" +
                            " sI_ITEMCODE = ci_itemcode where a.sI_INVOICENUMBER = '" + SelectedSalesInvoice.InvoiceNumber + "'", conn);

                            using (SqlDataReader reader = GetPurchaseItemCMD.ExecuteReader())
                            {

                                SalesItemList.Clear();
                                while (reader.Read())
                                {
                                    SalesItemList.Add(new SalesModel
                                    {
                                        Itemname = reader["sI_ITEMNAME"].ToString(),
                                        ItemCode = (int)reader["sI_ITEMCODE"],
                                        SalesPricePerUnit = (int)reader["sI_SalesPRICEUNIT"],
                                        SalesQuantity = (int)reader["sI_SalesQUANTITY"],
                                        TotalSalesValueByItemName = (int)reader["sI_TOTALSalesVALUE"],
                                        StockInHand = (int)reader["TotalQuantity"]

                                    });
                                }
                            }

                            this._view2 = new ListCollectionView(this._SalesItemList);
                            RemovalStatus = 0;
                            OnPropertyChanged(nameof(View2));
                        }


                        SalesModel Myitem = new();
                        TbyInvoice = 0;

                        foreach (SalesModel item in TempSalesItemCollection)
                        {

                            TbyInvoice += item.TotalSalesValueByItemName;

                            if (SelectedSalesItem2.ItemCode == item.ItemCode)
                            {

                                Myitem = SelectedSalesItem2;
                                RemovalStatusForTemp = true;
                            }

                        }

                        // Removing Temporary Added 
                        if (RemovalStatusForTemp)
                        {
                            TempSalesItemCollection.Remove(Myitem);
                            TbyItem = SelectedSalesItem2.TotalSalesValueByItemName;

                            LTotalByInvoice = TbyInvoice - TbyItem;
                            SalesItemList.Clear();
                            SalesItemList.AddRange(TempSalesItemCollection);
                            this._view2 = new ListCollectionView(this._SalesItemList);
                            OnPropertyChanged(nameof(View2));
                            OnPropertyChanged(nameof(LTotalByInvoice));
                        }



                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        AllInOneTotalValueHandlingFunc();

                    }
                    /*
                     * handling  total sales value in finally block
                     */
                    finally
                    {

                        TempCollectionLength = TempSalesItemCollection.Count;
                        if (TempCollectionLength == 0)
                        {
                            OnClickAction = false;
                        }
                        conn.Close();
                        OnPropertyChanged("");
                        AllInOneTotalValueHandlingFunc();
                    }
            }
            else
            {
                return;
            }
        }

        private ICommand _RemoveSalesItem;
        public ICommand RemoveSalesItem
        {
            get { return _RemoveSalesItem; }
            set { _RemoveSalesItem = value; }
        }
        int removedInvoice;

        /// <summary>
        /// Removing Selected Invoice Number
        /// from DB.
        /// Also Clearing All Items Related To this invoice number from DB,
        /// Adjusting Total sales Value, Stock In Hand etc
        /// </summary>
        
        private void RemoveSelectedInvoice(object obj)
        {
            MessageBoxResult RequestConfirm = MessageBox.Show("Do You Want to Remove Invoice", "Confirm Removal", MessageBoxButton.YesNo);

            if (RequestConfirm == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        if (conn == null)
                        {
                            throw new Exception("Connection String is Null. Set the value of Connection String in Retail->Properties-?Settings.settings");
                        }

                        removedInvoice = SelectedSalesInvoice.InvoiceNumber;
                        conn.Open();

                        //Deleting sales records based on invoice number
                        SqlCommand UCustomerCommand = new SqlCommand("DELETE FROM R_SALESRECORD  WHERE SU_INVOICENUMBER ='" + SelectedSalesInvoice.InvoiceNumber + "'", conn);
                        UCustomerCommand.ExecuteNonQuery();

                        //Deleting all Records From SalesInvoice Table Related To that Invoice Number
                        SqlCommand UDeleteItemCommand = new SqlCommand("DELETE FROM R_SALESINVOICE  WHERE sI_INVOICENUMBER='" + removedInvoice + "'", conn);
                        UDeleteItemCommand.ExecuteNonQuery();


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }

                    /*
                      * handling  total sales value in finally block
                      */

                    finally
                    {
                        AllInOneTotalValueHandlingFunc();

                        SalesRecordsList = GetSalesRecords();
                        this._view = new ListCollectionView(this._SalesRecordsList);
                        OnPropertyChanged(nameof(View));
                        this._view.Filter = Filter;
                        OnPropertyChanged(nameof(View.Filter));

                        conn.Close();
                    }
            }
            else
            {
                return;
            }
        }

        private ICommand _RemoveSalesInvoice;
        public ICommand RemoveSalesInvoice
        {
            get { return _RemoveSalesInvoice; }
            set { _RemoveSalesInvoice = value; }
        }

        // Jumping to update sales Record
        private void JumpToUpdateSalesRecord(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            Window UpdateSalesRecordWindow = new Views.Windows.UpdateSalesInvoiceView();

            UpdateSalesRecordWindow.DataContext = this;

            GetSalesItemsList();
            this._view2 = new ListCollectionView(this._SalesItemList);

            UpdateSalesRecordWindow.Show();
        }

        /// <summary>
        /// Getting Fresh Sales Item From DB.
        /// Returning All Records In single List
        /// </summary>
        /// <returns></returns>
        private List<SalesModel> GetSalesItemsList()
        {
            AllInOneTotalValueHandlingFunc();

            List<SalesModel> listOfSalesItems = new();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    conn.Open();

                    /*
                     * selecting Invoice Number, stock in hand
                     */
                    SqlCommand UItemNameCommand = new SqlCommand("SELECT * FROM R_SALESINVOICE WHERE SI_INVOICENUMBER ='" + SelectedSalesInvoice.InvoiceNumber + "'", conn);

                    SqlCommand GetSalesItemCMD = new SqlCommand("select a.* ,(b.ci_stockinhand ) TotalQuantity from R_SALESINVOICE a left join R_CREATEITEM b on" +
                       " SI_ITEMCODE = ci_itemcode where a.SI_INVOICENUMBER = '" + SelectedSalesInvoice.InvoiceNumber + "'", conn);

                    using (SqlDataReader reader = GetSalesItemCMD.ExecuteReader()) // reading data row by row
                    {

                        SalesItemList.Clear();
                        while (reader.Read())
                        {
                            SalesItemList.Add(new SalesModel
                            {
                                Itemname = reader["SI_ITEMNAME"].ToString(),
                                ItemCode = (int)reader["SI_ITEMCODE"],
                                SalesPricePerUnit = (int)reader["SI_SALESPRICEUNIT"],
                                SalesQuantity = (int)reader["SI_SALESQUANTITY"],
                                TotalSalesValueByItemName = (int)reader["SI_TOTALSALESVALUE"],
                                StockInHand = (int)reader["TotalQuantity"]
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                    AllInOneTotalValueHandlingFunc();

                }
                finally
                {
                    conn.Close();
                    AllInOneTotalValueHandlingFunc();


                }

            return listOfSalesItems;
        }

        private ICommand _JumpToUpdateSalesRecordCommand;
        public ICommand JumpToUpdateSalesRecordCommand
        {
            get { return _JumpToUpdateSalesRecordCommand; }
            set { _JumpToUpdateSalesRecordCommand = value; }
        }


        private ICommand _SaveInvoiceItemCommand;
        public ICommand SaveInvoiceItemCommand
        {
            get { return _SaveInvoiceItemCommand; }
            set { _SaveInvoiceItemCommand = value; }
        }


        private SalesModel _SelectedSalesInvoice;
        public SalesModel SelectedSalesInvoice
        {
            get { return _SelectedSalesInvoice; }
            set
            {
                _SelectedSalesInvoice = value;

                OnPropertyChanged(nameof(SelectedSalesInvoice));

            }
        }
        bool IsDbInitiated = false;

        /// <summary>
        /// Saving All Data To DB from Temporary Holded 
        /// observable Collection
        /// </summary>
        /// <param name="obj"></param>
        /// 
        private void SaveAllRecordsToDBFinally(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in Retail store->Properties-?Settings.settings");
                    }

                    IsDbInitiated = true;
                    CreateTempInvoiceNumber();

                    SqlCommand query = new SqlCommand("ADDSALESITEM", conn);
                    conn.Open();
                    query.CommandType = CommandType.StoredProcedure;
                    SqlParameter pInvoiceNumber = new SqlParameter("@sSI_INVOICENUMBER", SqlDbType.Int);
                    SqlParameter pSalesQuantity = new SqlParameter("@sSI_SALESQUANTITY", SqlDbType.Int);
                    SqlParameter pSalesPriceUnit = new SqlParameter("@sSI_SALESPRICEPERUNIT", SqlDbType.Int);
                    SqlParameter pItemcode = new SqlParameter("@sSI_ITEMCODE", SqlDbType.Int);
                    SqlParameter pItemName = new SqlParameter("@sSI_ITEMNAME", SqlDbType.VarChar);

                    /*
                     * adding data to DB from Temporary Observable Collection
                     */
                    foreach (var item in TempSalesItemCollection)
                    {
                        pInvoiceNumber.Value = item.InvoiceNumber;
                        pSalesQuantity.Value = item.SalesQuantity;
                        pSalesPriceUnit.Value = item.SalesPricePerUnit;
                        pItemcode.Value = item.ItemCode;
                        pItemName.Value = item.Itemname;

                        query.Parameters.Add(pInvoiceNumber);
                        query.Parameters.Add(pSalesQuantity);
                        query.Parameters.Add(pItemName);
                        query.Parameters.Add(pSalesPriceUnit);
                        query.Parameters.Add(pItemcode);

                        query.ExecuteNonQuery();
                        query.Parameters.Clear();

                        

                        //getting stock in hand
                        SqlCommand StockInHandCommand = new SqlCommand("select CI_stockinhand from R_CREATEITEM where CI_ITEMCODE='" + item.ItemCode + "'", conn);
                        StockInHand = Convert.ToInt32(StockInHandCommand.ExecuteScalar());

                        //updating stock in hand 
                        SqlCommand MinusQuantityCommand = new SqlCommand("update R_CREATEITEM set CI_STOCKINHAND = '" + (item.StockInHand - item.SalesQuantity) + "' where CI_ITEMCODE='" + item.ItemCode + "'", conn);
                        MinusQuantityCommand.ExecuteNonQuery();

                    }


                    MessageBox.Show("All sales's added successfully", "Item added", MessageBoxButton.OK);

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Try again", MessageBoxButton.OK, MessageBoxImage.Information);

                }

                finally
                {
                    AllInOneTotalValueHandlingFunc();

                    TempSalesItemCollection.Clear();
                    SqlCommand UCustomerCommand = new SqlCommand("UPDATE R_SALESRECORD SET SU_TOTALSALESVALUE='" + LTotalByInvoice + "' WHERE SU_INVOICENUMBER='" + LInvoice_no + "'", conn);
                    UCustomerCommand.ExecuteNonQuery();
                    OnPropertyChanged(nameof(View));
                    SalesRecordsList = GetSalesRecords();
                    this._view = new ListCollectionView(this._SalesRecordsList);
                    View.Refresh();

                    OnPropertyChanged("");
                    conn.Close();

                }


        }

        /// <summary>
        /// This Method Creating Temporary invoice
        /// number and saving DB only if when user click on Save Buttin on UI
        /// </summary>

        private void CreateTempInvoiceNumber()
        {
            AllInOneTotalValueHandlingFunc();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                    }

                    conn.Open();

                    /*
                     * Whenever User Click On Save Button Only After That
                     * Invoice Get Generated and Saved Into DB
                     */

                    if (IsDbInitiated)
                    {
                        SqlCommand query = new SqlCommand("CREATESALESRECORDS", conn);
                        query.CommandType = CommandType.StoredProcedure;
                        SqlParameter pTotalSalesValue = new SqlParameter("@uSU_TOTALSALESVALUE", SqlDbType.Int);
                        SqlParameter pCustomerName = new SqlParameter("@uSU_CUSTOMERNAME", SqlDbType.VarChar);
                        SqlParameter pCustomerNumber = new SqlParameter("@uSU_CUSTOMERNUMBER", SqlDbType.VarChar);
                        SqlParameter pInvoiceNumber = new SqlParameter("@uSU_INVOICENUMBER", SqlDbType.Int);

                        pInvoiceNumber.Value = LInvoice_no;
                        pTotalSalesValue.Value = LTotalByInvoice;
                        pCustomerName.Value = LCustomer_Name;
                        pCustomerNumber.Value = LCustomer_Number;

                        query.Parameters.Add(pInvoiceNumber);
                        query.Parameters.Add(pTotalSalesValue);
                        query.Parameters.Add(pCustomerName);
                        query.Parameters.Add(pCustomerNumber);

                        query.ExecuteNonQuery();
                        IsDbInitiated = false;
                    }

                    // Temporary Creating Invoice Number
                    else
                    {
                        SqlCommand UInvoiceCodeCommand = new SqlCommand("SELECT max(su_invoicenumber) FROM R_SALESRECORD", conn);

                        var IsInvoiceNumberNull = UInvoiceCodeCommand.ExecuteScalar();


                        if (IsInvoiceNumberNull == DBNull.Value)
                        {
                            InvoiceNumber = 1000;
                        }
                        else
                        {
                            InvoiceNumber = Convert.ToInt32(UInvoiceCodeCommand.ExecuteScalar());
                        }
                        LInvoice_no = InvoiceNumber + 1;

                        DateTime Purchase_d = DateTime.Today;
                        LSales_dt = Purchase_d.ToString("dd-MMM-yyyy").Split()[0];
                        LCustomer_Name = string.Empty;
                        LCustomer_Number = string.Empty;

                    }
                }

                catch
                {
                    //
                }

                finally
                {
                    conn.Close();
                    AllInOneTotalValueHandlingFunc();

                }
        }



        //Jumping to sales invoice
        private void JumpToSalesInvoice(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            Window SalesInvoice = new Views.Windows.CreateSalesInvoiceView();
            TempSalesItemCollection = new List<SalesModel>();
            CreateTempInvoiceNumber();

            SalesItemList = GetSalesItemsList();
            SalesRecordsList = GetSalesRecords();

            this._view2 = new ListCollectionView(this._SalesItemList);
            View2.Refresh();
            this._view = new ListCollectionView(this._SalesRecordsList);
            View.Refresh();
            this._view.Filter = Filter;

            SalesInvoice.DataContext = this;
            OnPropertyChanged(nameof(SalesRecordsList));
            OnPropertyChanged(nameof(SalesItemList));
            OnPropertyChanged(nameof(View));
            OnPropertyChanged(nameof(View2));
            OnPropertyChanged(nameof(View.Filter));


            SalesInvoice.Show();

        }

        /// <summary>
        /// Selected Purchase Item values Comes From
        /// these Methods...
        /// these properties binded to selectedItem in Datagrid
        /// </summary>

        private SalesModel _SelectedSalesItem;
        public SalesModel SelectedSalesItem
        {
            get { return _SelectedSalesItem; }
            set
            {
                _SelectedSalesItem = value;
                OnPropertyChanged(nameof(SelectedSalesItem));
            }
        }
        private SalesModel _SelectedCustomer_Name;
        public SalesModel SelectedCustomer_Name
        {
            get { return _SelectedCustomer_Name; }
            set
            {
                _SelectedCustomer_Name = value;

                OnPropertyChanged(nameof(SelectedCustomer_Name));

            }
        }

        private SalesModel _SelectedCustomer_Number;
        public SalesModel SelectedCustomer_Number
        {
            get { return _SelectedCustomer_Number; }
            set
            {
                _SelectedCustomer_Number = value;

                OnPropertyChanged(nameof(SelectedCustomer_Number));

            }
        }

        private DateTime _DisplayDate;

        public DateTime DisplayDate
        {
            get { return _DisplayDate = DateTime.Today; }
            set { _DisplayDate = value; }
        }

        bool isItemBasedOnFirstClick = false;
        //Jumping to add sales Item UI
        public void JumpToAddSalesItem(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            Window AddSalesWindow = new Views.Windows.AddSalesItemView();
            if (LCustomer_Name == string.Empty && LCustomer_Number == string.Empty)
            {
                MessageBox.Show("Please enter custome name and number", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            if (LCustomer_Name == string.Empty)
            {
                MessageBox.Show("Please enter custome name", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            if (LCustomer_Number == string.Empty)
            {
                MessageBox.Show("Please enter customer Number", "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            isItemBasedOnFirstClick = true;
            UItemNameList();
            UCategoryList();

            AddSalesWindow.DataContext = this;
            AddSalesWindow.Show();
        }


        public List<SalesModel> TempSalesItemCollection;

        public List<SalesModel> TempCurrentItemHolder;

        int Count;
        int TempSalesByItemNameHolder;

        int SalesItemLength;
        int TempCollectionLength;

        /// <summary>
        /// Disabling Combobox After Selecting supplier and 
        /// adding atleast one item in purchase invoice.
        /// </summary>
        /// 

        private bool _DisableComboAfterChange = true;
        public bool DisableComboAfterChange
        {
            get { return _DisableComboAfterChange; }

            set
            {
                _DisableComboAfterChange = value;
                OnPropertyChanged(nameof(DisableComboAfterChange));
            }
        }


        /// <summary>
        /// Adding Purchase item To DB Based On So many 
        /// Different Conditons..
        /// Initially Storing All Those Data into Temporary observable Collection
        /// </summary>
        /// 
        private void AddSalesItemMethod(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                try
                {

                    if (SalesQuantity == 0 || Itemname == string.Empty)  //purchase Quantity should be greater than zero
                    {
                        if (SalesQuantity == 0)
                            MessageBox.Show("Sales quantity must be greater than zero", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                        else
                            MessageBox.Show("Item name should be selected", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                        return;
                    }

                    foreach (var item in SalesItemList) //existing item can't add one more time
                    {
                        if (ItemCode == item.ItemCode)
                        {
                            MessageBox.Show("Sales item already exists. Please update existing item", "Existing item", MessageBoxButton.OK);
                            TbyItem = 1;
                            return;

                        }
                    }

                    foreach (var item in TempSalesItemCollection) //existing item can't add one more time
                    {
                        if (ItemCode == item.ItemCode)
                        {
                            MessageBox.Show("Sales item already exists. Please update existing item", "Existing item", MessageBoxButton.OK);
                            TbyItem = 1;
                            return;

                        }
                    }

                    if (IsJumpedInUpdate)
                    {
                        LInvoice_no = SelectedSalesInvoice.InvoiceNumber;
                    }

                    if (SalesQuantity > StockInHand)
                    {
                        MessageBox.Show("Insufficient quantity for sale", "Less quantity", MessageBoxButton.OK);
                        return;
                    }

                    //Adding these all records for temporary base in observable collection
                    TempSalesItemCollection.Add(new SalesModel
                    {
                        InvoiceNumber = LInvoice_no,
                        ItemCode = this.ItemCode,
                        Itemname = this.Itemname,
                        SalesPricePerUnit = this.SalesPricePerUnit,
                        SalesQuantity = this.SalesQuantity,
                        StockInHand = this.StockInHand,
                        TotalSalesValueByItemName = this.SalesQuantity * this.SalesPricePerUnit
                    });

                    

                    foreach (var item in TempSalesItemCollection)
                    {
                        TempCurrentItemHolder.Clear();
                        TempCurrentItemHolder.Add(item);
                    }

                    SalesItemList.AddRange(TempCurrentItemHolder);
                    this._view2 = new ListCollectionView(this._SalesItemList);
                    OnPropertyChanged(nameof(View2));

                    foreach (var item in TempSalesItemCollection)
                    {
                        TempSalesByItemNameHolder = item.TotalSalesValueByItemName;
                    }

                    TempCollectionLength = TempSalesItemCollection.Count;
                    SalesItemLength = SalesItemList.Count;

                    Itemcategory = string.Empty;
                    SubCategory = string.Empty;
                    Itemname = string.Empty;
                    SalesQuantity = 0;
                    SalesPricePerUnit = 0;


                    if (TempCollectionLength > 0 || SalesItemLength > 0)
                    {
                        DisableComboAfterChange = false;
                        OnClickAction = true;
                    }
                    MessageBox.Show("Sales item added successfully", "Sold successfully", MessageBoxButton.OK);


                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    TempSalesItemCollection.Clear();
                    AllInOneTotalValueHandlingFunc();

                }

                finally
                {
                    //updating customer name 
                    SqlCommand UCustomerCommand = new SqlCommand("UPDATE R_SALESRECORD SET SU_CUSTOMERNAME='" + LCustomer_Name + "' , SU_CUSTOMERNUMBER ='" + LCustomer_Number + "' WHERE SU_INVOICENUMBER='" + LInvoice_no + "'", conn);
                    conn.Open();
                    UCustomerCommand.ExecuteNonQuery();

                    if (TbyItem != 1)
                    {
                        if (Count == 0)
                        {
                            LTotalByInvoice = 0;
                            foreach (var item in SalesItemList)
                            {
                                LTotalByInvoice += item.TotalSalesValueByItemName; //totalling total purchase value of added and previous item of db

                            }
                            Count = 1;
                        }
                        else
                        {
                            LTotalByInvoice += TempSalesByItemNameHolder; //totalling total purchase value of added and previous item of db
                        }
                    }
                    conn.Close();
                    AllInOneTotalValueHandlingFunc();


                    OnPropertyChanged("");
                }


            }


        }

        private bool _OnClickAction = false;
        public bool OnClickAction
        {
            get { return _OnClickAction; }

            set
            {
                _OnClickAction = value;
                OnPropertyChanged(nameof(OnClickAction));
            }
        }

        /// <summary>
        /// To Access Unique/Distinct Data from DB
        /// Here, we Load Unique Item name/category/sub category/suppliers
        /// </summary>
        /// 
        private List<SalesModel> _UCustomers;
        public List<SalesModel> UCustomers
        {
            get { return _UCustomers; }
            set
            {
                _UCustomers = value;
                OnPropertyChanged(nameof(UCustomers));
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

        #region
        /// <summary>
        /// Methods to load distnict data of various
        /// elements.
        /// </summary>
        #endregion

        /// <summary>
        /// getting  category list
        /// </summary>
        public void UCategoryList()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {

                    SqlCommand UCategoryCommand = new SqlCommand("SELECT DISTINCT CI_ITEMCATEGORY  FROM R_CREATEITEM WHERE  CI_STATUS=1", conn);
                    conn.Open();
                    using (SqlDataReader reader = UCategoryCommand.ExecuteReader()) //reading row of record using executereader
                    {

                        UCategory.Clear();
                        while (reader.Read())
                        {
                            UCategory.Add(new InventoryModel { Itemcategory = reader["CI_ITEMCATEGORY"].ToString() }); // adding Unique Category From DB
                        }
                    }


                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {

                    conn.Close();
                }
            }
        }

        /// <summary>
        /// getting sub category list
        /// </summary>
        public void USubCategoryList()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {

                    SqlCommand USubCategoryCommand = new SqlCommand("SELECT DISTINCT CI_ITEMSUBCAT  FROM R_CREATEITEM WHERE CI_ITEMCATEGORY='" + Itemcategory + "' AND CI_STATUS=1", conn);
                    conn.Open();
                    using (SqlDataReader reader = USubCategoryCommand.ExecuteReader()) //reading row of record using executereader
                    {

                        USubCategory.Clear();
                        while (reader.Read())
                        {
                            USubCategory.Add(new InventoryModel { SubCategory = reader["CI_ITEMSUBCAT"].ToString() }); // adding Unique sub category From DB
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


        /// <summary>
        /// getting item list
        /// </summary>
        public void UItemNameList()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {
                    if(isItemBasedOnFirstClick)
                    {
                        SqlCommand UItemNameCommand = new SqlCommand("SELECT DISTINCT CI_ITEMNAME  FROM R_CREATEITEM WHERE CI_STATUS=1", conn);
                        conn.Open();
                        using (SqlDataReader reader = UItemNameCommand.ExecuteReader()) //reading row of record using executereader
                        {

                            UItemName.Clear();
                            while (reader.Read())
                            {
                                UItemName.Add(new InventoryModel { Itemname = reader["CI_ITEMNAME"].ToString() }); // adding Unique Item Name From DB
                            }
                        }
                        isItemBasedOnFirstClick = false;
                    }
                    else
                    {
                        SqlCommand UItemNameCommand = new SqlCommand("SELECT DISTINCT CI_ITEMNAME  FROM R_CREATEITEM WHERE" +
                        " CI_ITEMCATEGORY='" + Itemcategory + "' AND CI_ITEMSUBCAT='" + SubCategory + "' AND CI_STATUS=1", conn);
                        conn.Open();
                        using (SqlDataReader reader = UItemNameCommand.ExecuteReader()) //reading row of record using executereader
                        {

                            UItemName.Clear();
                            while (reader.Read())
                            {
                                UItemName.Add(new InventoryModel { Itemname = reader["CI_ITEMNAME"].ToString() }); // adding Unique Item Name From DB
                            }
                        }

                    }


                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {

                    conn.Close();
                }
            }
        }

        /// <summary>
        /// getting sales price per unit, stock in hand and 
        /// item code for particular item 
        /// </summary>
        public void USalesPricePerUnitAndCodeForItemName()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {
                    //getting sales price per unit
                    SqlCommand SalesPriceUnitCommand = new SqlCommand("select CI_SALESPRICEUNIT from R_CREATEITEM where CI_ITEMNAME='" + Itemname + "'", conn);
                    conn.Open();
                    SalesPricePerUnit = Convert.ToInt32(SalesPriceUnitCommand.ExecuteScalar());

                    /*
                     * executescalar returns first column of the firs row.
                     * It's only return single value or single row
                     */

                    SqlCommand ItemCodeCommand = new SqlCommand("select CI_ITEMCODE from R_CREATEITEM where CI_ITEMNAME='" + Itemname + "'", conn);
                    ItemCode = Convert.ToInt32(ItemCodeCommand.ExecuteScalar());

                    //getting stock in hand of item 
                    SqlCommand StockInHandCommand = new SqlCommand("select CI_STOCKINHAND from R_CREATEITEM where CI_ITEMNAME='" + Itemname + "'", conn);
                    StockInHand = Convert.ToInt32(StockInHandCommand.ExecuteScalar());
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

        /// <summary>
        /// getting stock in hand for certain item
        /// </summary>
        public void UStockInHandForItemName()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {

                    //getting stock in hand for item
                    SqlCommand SalesPriceUnitCommand = new SqlCommand("select ci_stockinhand from R_CREATEITEM where CI_ITEMNAME='" + Itemname + "'", conn);
                    conn.Open();
                    StockInHand = Convert.ToInt32(SalesPriceUnitCommand.ExecuteScalar());

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
            CustomerName = string.Empty;
            CustomerNumber = string.Empty;
            Itemname = string.Empty;
            SalesPricePerUnit = 0;
            SalesQuantity = 0;
            OnPropertyChanged("");
        }


        /// <summary>
        /// Properties for Extra Work in Purchase
        /// Same Properties Added in PurchaseModel.cs
        /// </summary>

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


        private string _ItemCategory;
        public string Itemcategory
        {
            get
            { return _ItemCategory; }
            set
            {
                _ItemCategory = value;
                USubCategoryList();
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
                UItemNameList();

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

        private string _Itemname;
        public string Itemname
        {
            get
            { return _Itemname; }
            set
            {
                _Itemname = value;
                OnPropertyChanged(nameof(Itemname));
                USalesPricePerUnitAndCodeForItemName();
                UStockInHandForItemName();

            }
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

        private ICommand _AddSalesItemCommand;
        public ICommand AddSalesItemCommand
        {
            get { return _AddSalesItemCommand; }
            set { _AddSalesItemCommand = value; }
        }

        private ICommand _AddSalesItem;
        public ICommand AddSalesItem
        {
            get { return _AddSalesItem; }
            set { _AddSalesItem = value; }
        }


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

        /// <summary>
        /// store data in list to show on datagrid
        /// </summary>

        private List<SalesModel> _SalesRecordsList;
        public List<SalesModel> SalesRecordsList
        {
            get { return _SalesRecordsList; }
            set
            {
                _SalesRecordsList = value;
                OnPropertyChanged(nameof(SalesRecordsList));
            }
        }


        /// <summary>
        /// Accessing All Records from  DB and storing
        /// into single list for different purposes
        /// </summary>
        private List<SalesModel> GetSalesRecords()
        {
            AllInOneTotalValueHandlingFunc();

            List<SalesModel> listOfSalesInvoice = new();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in Retail->Properties-?Settings.settings");
                    }

                    SqlCommand query = new SqlCommand("VIEWSALESRECORDS", conn);

                    query.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)  //adding data into list one by one using datarow functionalities
                    {
                        SalesModel m = new SalesModel();
                        m.InvoiceNumber = (int)row["SU_INVOICENUMBER"];
                        DateTime OnlyDate = (DateTime)row["SU_SALESDATE"];
                        m.TotalSalesValueOfInvoice = (int)row["SU_TOTALSALESVALUE"];
                        m.CustomerName = row["SU_CUSTOMERNAME"].ToString();
                        m.CustomerNumber = row["SU_CUSTOMERNUMBER"].ToString();
                        m.SalesDate = OnlyDate.ToString("dd-MMM-yyyy").Split()[0];


                        listOfSalesInvoice.Add(new SalesModel
                        {
                            InvoiceNumber = m.InvoiceNumber,
                            SalesDate = m.SalesDate,
                            TotalSalesValueOfInvoice = m.TotalSalesValueOfInvoice,
                            CustomerName = m.CustomerName,
                            CustomerNumber = m.CustomerNumber

                        });
                    }
                    return listOfSalesInvoice;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                    AllInOneTotalValueHandlingFunc();

                }

            return listOfSalesInvoice;

        }

        private List<SalesModel> _SalesItemList;
        public List<SalesModel> SalesItemList
        {
            get { return _SalesItemList; }
            set
            {
                _SalesItemList = value;
                OnPropertyChanged(nameof(SalesItemList));
            }
        }

        public Visibility DateTimeVisibility { get; set; } = Visibility.Hidden;

        /// <summary>
        /// searching text using search  by
        /// </summary>

        private string _TextSearch;
        public string TextSearch
        {
            get { return _TextSearch; }
            set
            {
                _TextSearch = value;
                OnPropertyChanged("TextSearch");
                View.Refresh();

            }
        }

        private DateTime _TextSearch2;
        public DateTime TextSearch2
        {
            get { return _TextSearch2; }
            set
            {
                _TextSearch2 = value;
                _TextSearch = _TextSearch2.ToString("dd-MMM-yyyy").Split()[0];
                View.Refresh();
                OnPropertyChanged("TextSearch");
                OnPropertyChanged("TextSearch2");
            }
        }

        //Filtering Datagrid based on Supplier Name, Invoice Number and Purchase date
        private bool Filter(object item)
        {
            if (String.IsNullOrEmpty(TextSearch))
                return true;
            else if (FilteredList == "Invoice Number")
                return ((item as SalesModel).InvoiceNumber.ToString().IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Customer")
                return ((item as SalesModel).CustomerName.ToString().IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Sales Date")
                return ((item as SalesModel).SalesDate.ToString().IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else
                return false;
        }

        private string _FilteredList;
        public string FilteredList
        {
            get
            { return _FilteredList; }
            set
            {
                TextSearch = string.Empty;
                _FilteredList = value;
                if (_FilteredList == "Sales Date")
                {
                    DateTimeVisibility = Visibility.Visible;
                    OnPropertyChanged("DateTimeVisibility");
                }
                else
                    DateTimeVisibility = Visibility.Hidden;
                OnPropertyChanged("DateTimeVisibility");
                OnPropertyChanged(nameof(FilteredList));
            }
        }

        /// <summary>
        /// Listcollectionview used for Filtering datagrid based on search condition
        /// view and view2 for two different UI's 
        /// </summary>
        private ListCollectionView _view2;
        public ICollectionView View2
        {
            get { return this._view2; }
        }

        private ListCollectionView _view;
        public ICollectionView View
        {
            get { return this._view; }
        }


        private void CreateInvoiceNumber()
        {
            AllInOneTotalValueHandlingFunc();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in Retail->Properties-?Settings.settings");
                    }
                    TempSalesItemCollection.Clear();
                    DisableComboAfterChange = true;
                    SqlCommand query = new SqlCommand("CREATESALESRECORDS", conn);
                    conn.Open();
                    query.CommandType = CommandType.StoredProcedure;
                    query.ExecuteNonQuery();

                    SqlCommand UCategoryCommand = new SqlCommand("SELECT TOP 1 * FROM R_SALESRECORD ORDER BY SU_INVOICENUMBER DESC", conn);

                    SqlDataReader reader = UCategoryCommand.ExecuteReader();

                    while (reader.Read())
                    {

                        Invoice_no = reader.GetInt32(0);
                        TotalByInvoice = reader.GetInt32(2);
                        DateTime Sales_d = reader.GetDateTime(1);
                        Sales_dt = Sales_d.ToString("dd-MMM-yyyy").Split()[0];
                        Customer_Name = reader.GetString(3);

                    }
                    reader.Close();

                    //UCustomersList();

                }


                catch
                {
                    //
                }

                finally
                {
                    conn.Close();
                    AllInOneTotalValueHandlingFunc();


                }
        }


        /// <summary>
        /// Store last values of data
        /// L stands for Last Here
        /// storing Last invoice/customer/total/purchase date
        /// </summary>
        private int Invoice_no;

        public int LInvoice_no
        {
            get { return Invoice_no; }
            set
            {
                Invoice_no = value;
                OnPropertyChanged(nameof(Invoice_no));
            }
        }

        private string Customer_Name;

        public string LCustomer_Name
        {
            get { return Customer_Name; }
            set
            {
                Customer_Name = value;
                OnPropertyChanged(nameof(Customer_Name));
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


        private int TotalByInvoice;

        public int LTotalByInvoice
        {
            get { return TotalByInvoice; }
            set
            {
                TotalByInvoice = value;
                OnPropertyChanged(nameof(TotalByInvoice));
            }
        }

        private string Sales_dt;

        public string LSales_dt
        {
            get { return Sales_dt; }
            set
            {
                Sales_dt = value;
                OnPropertyChanged(nameof(Sales_dt));
            }
        }



        private ICommand _SalesInvoiceCommand;
        public ICommand SalesInvoiceCommand
        {
            get { return _SalesInvoiceCommand; }
            set { _SalesInvoiceCommand = value; }
        }

    }
}
