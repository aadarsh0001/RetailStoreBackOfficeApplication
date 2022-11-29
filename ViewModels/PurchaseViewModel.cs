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
    class PurchaseViewModel : BaseViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PurchaseViewModel()
        {
            /*
             * Handdling NUll Value Exception 
             * for all these following list and observeable collection
             */

            ListInvoice = new List<PurchaseModel>();
            PurchaseItemList = new List<PurchaseModel>();
            PurchaseRecordsList = new List<PurchaseModel>();
            SelectedSupplier_Name = new PurchaseModel();
            SelectedPurchaseInvoice = new PurchaseModel();
            SelectedPurchaseItem = new PurchaseModel();
            UItemName = new ObservableCollection<InventoryModel>();
            USubCategory = new ObservableCollection<InventoryModel>();
            UCategory = new ObservableCollection<InventoryModel>();
            USuppliers = new List<PurchaseModel>();
            TempCurrentItemHolder = new List<PurchaseModel>();
            TempPurchaseItemCollection = new List<PurchaseModel>();

            /*
             * Intially Getting values from Various Methods.
             * These Methods are Following
             */

            AllInOneTotalValueHandlingFunc();
            UpdateAddPurchaseItemCommand = new RelayCommand(new Action<object>(UpdateJumpToAddPurchaseItem));
            UpdatePurchaseQuantity = new RelayCommand(new Action<object>(UpdateItemQuantity));
            JumpToUpdatePurchaseItem = new RelayCommand(new Action<object>(JumpToUpdateSelectedRecord));
            RemovePurchaseItem = new RelayCommand(new Action<object>(RemoveSelectedRecord));
            SaveInvoiceItemCommand = new RelayCommand(new Action<object>(SaveAllRecordsToDBFinally));
            RemovePurchaseInvoice = new RelayCommand(new Action<object>(RemoveSelectedInvoice));
            JumpToUpdatePurchaseRecordCommand = new RelayCommand(new Action<object>(JumpToUpdatePurchaseRecord));
            AddPurchaseItemCommand = new RelayCommand(new Action<object>(JumpToAddPurchaseItem));
            AddPurchaseItem = new RelayCommand(new Action<object>(AddPurchaseItemMethod));
            PurchaseItemList = GetPurchaseItemsList();
            PurchaseRecordsList = GetPurchaseRecords();
            this._view = new ListCollectionView(this._PurchaseRecordsList);
            this._view.Filter = Filter;
            PurchaseInvoiceCommand = PurchaseInvoiceCommand = new RelayCommand(new Action<object>(JumpToPurchaseInvoice));
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
        private void AllInOneTotalValueHandlingFunc()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                conn.Open();

                SqlCommand GetPurchaseItemCMD = new SqlCommand("select Pu_invoicenumber from R_PURCHASERECORD", conn);

                using (SqlDataReader reader = GetPurchaseItemCMD.ExecuteReader())
                {

                    ListInvoice.Clear();
                    while (reader.Read())
                    {
                        ListInvoice.Add(new PurchaseModel
                        {

                            InvoiceNumber = (int)reader["Pu_invoicenumber"],

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

                SqlCommand query2 = new SqlCommand("update R_PURCHASERECORD set PU_TOTALPURCHASEVALUE = 0 where PU_TOTALPURCHASEVALUE is null;", conn);
                query2.ExecuteNonQuery();

                conn.Close();


            }

        }

        bool IsJumpedInUpdate;

        //Jumping to add purchse item
        public void UpdateJumpToAddPurchaseItem(object obj)
        {

            Window AddPurchaseWindow = new Views.Windows.AddPurchaseItemView();

            if (LSupplier_Name == "NA")
            {
                MessageBox.Show("Please select supplier", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }
            AllInOneTotalValueHandlingFunc();


            LSupplier_Name = SelectedPurchaseInvoice.SupplierName;

            UItemNameListBasecOnSupplierName();
            UCategoryList();
            IsJumpedInUpdate = true;
            AddPurchaseWindow.DataContext = this;
            AddPurchaseWindow.Show();
        }

        private ICommand _UpdateAddPurchaseItemCommand;
        public ICommand UpdateAddPurchaseItemCommand
        {
            get { return _UpdateAddPurchaseItemCommand; }
            set { _UpdateAddPurchaseItemCommand = value; }
        }

        int ItemInPurchaseItemList;
        int LastQunatity;
        int ExistingQuantity;

        /// <summary>
        /// Updating Quantity of Existing Item as well temporary
        /// created Items..
        /// All Actions Happens on Some Conditons.
        /// AS well as updating stock in hand and total purchase value
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateItemQuantity(object obj)
        {

            foreach (var item in PurchaseItemList)
            {

                if (SelectedPurchaseItem.ItemCode == item.ItemCode)
                {

                    ItemInPurchaseItemList = 1;
                }
            }

            //updating Quantity of Existing Items
            if (ItemInPurchaseItemList == 1)
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        if (conn == null)
                        {
                            throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                        }

                        conn.Open();


                        SqlCommand ExistingQuantityCommandOfItem = new SqlCommand("select PI_PURCHASEQUANTITY from R_PURCHASEINVOICE where PI_INVOICENUMBER ='" + SelectedPurchaseInvoice.InvoiceNumber + "' AND" +
                            " PI_ITEMCODE = '" + SelectedPurchaseItem.ItemCode + "'", conn);
                        LastQunatity = Convert.ToInt32(ExistingQuantityCommandOfItem.ExecuteScalar());
                        LastQunatity = SelectedPurchaseItem.PurchaseQuantity - LastQunatity;

                        //getting existing amount of quantity of stock in hand
                        SqlCommand ExistingQuantityCommand = new SqlCommand("select CI_STOCKINHAND  from R_CREATEITEM  where CI_ITEMCODE='" + SelectedPurchaseItem.ItemCode + "'", conn);
                        ExistingQuantity = Convert.ToInt32(ExistingQuantityCommand.ExecuteScalar());

                        //updating existing purchase quantity and total purchase value
                        SqlCommand UpdateQuantityCommand = new SqlCommand("UPDATE R_PURCHASEINVOICE SET " +
                            "PI_PURCHASEQUANTITY = '" + SelectedPurchaseItem.PurchaseQuantity + "' ,PI_TOTALPURCHASEVALUE = '" + SelectedPurchaseItem.PurchaseQuantity * SelectedPurchaseItem.PurchasePricePerUnit + "' WHERE" +
                            " PI_INVOICENUMBER = '" + SelectedPurchaseInvoice.InvoiceNumber + "' AND PI_ITEMCODE = '" + SelectedPurchaseItem.ItemCode + "'", conn);
                        UpdateQuantityCommand.ExecuteNonQuery();

                        SqlCommand PlusQuantityCommand = new SqlCommand("update R_CREATEITEM set CI_STOCKINHAND = '" + (ExistingQuantity + LastQunatity) + "' where CI_ITEMCODE='" + SelectedPurchaseItem.ItemCode + "'", conn);

                        PlusQuantityCommand.ExecuteNonQuery();


                        MessageBox.Show("Purchase quantity updated successfully", "Quantity updated", MessageBoxButton.OK);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                    finally
                    {

                        SqlCommand TotalPriceByInvoiceCommand = new SqlCommand("select sum(pi_totalpurchasevalue) as TotalByInvoice from r_purchaseinvoice where" +
                            " pi_invoicenumber ='" + SelectedPurchaseInvoice.InvoiceNumber + "'", conn);

                        LTotalByInvoice = Convert.ToInt32(TotalPriceByInvoiceCommand.ExecuteScalar());
                        foreach (var item in TempPurchaseItemCollection)
                        {
                            LTotalByInvoice += item.TotalPurchaseValueByItemName;
                            OnPropertyChanged("");
                            OnPropertyChanged(nameof(LTotalByInvoice));
                        }
                        SqlCommand UpdateQuantityCommand = new SqlCommand("UPDATE R_PURCHASERECORD SET PU_TOTALPURCHASEVALUE = '" + LTotalByInvoice + "' WHERE PU_INVOICENUMBER = '" + SelectedPurchaseInvoice.InvoiceNumber + "'", conn);
                        UpdateQuantityCommand.ExecuteNonQuery();

                        TotalPurchaseValueOfInvoice = LTotalByInvoice;
                        SelectedPurchaseInvoice.TotalPurchaseValueOfInvoice = LTotalByInvoice;
                        OnPropertyChanged(nameof(SelectedPurchaseInvoice));
                        PurchaseItemList.AddRange(TempPurchaseItemCollection);
                        PurchaseRecordsList = GetPurchaseRecords();

                        PurchaseItemList.AddRange(TempPurchaseItemCollection);
                        this._view2 = new ListCollectionView(this._PurchaseItemList);
                        OnPropertyChanged(nameof(View2));
                        AllInOneTotalValueHandlingFunc();

                        conn.Close();
                    }
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        var result = from r in TempPurchaseItemCollection where r.ItemCode == SelectedPurchaseItem.ItemCode select r;

                        result.First().PurchaseQuantity = SelectedPurchaseItem.PurchaseQuantity;

                        SqlCommand TotalPriceByInvoiceCommand = new SqlCommand("select PI_TOTALPURCHASEVALUE from R_PURCHASEINVOICE where " +
                            "PI_INVOICENUMBER='" + SelectedPurchaseItem.InvoiceNumber + "'", conn);
                        conn.Open();
                        LTotalByInvoice = Convert.ToInt32(TotalPriceByInvoiceCommand.ExecuteScalar());

                        foreach (var item in TempPurchaseItemCollection)
                        {
                            item.TotalPurchaseValueByItemName = item.PurchasePricePerUnit * item.PurchaseQuantity;

                            LTotalByInvoice += item.TotalPurchaseValueByItemName;
                            TotalPurchaseValueByItemName = item.TotalPurchaseValueByItemName;
                            OnPropertyChanged("");
                            OnPropertyChanged(nameof(TotalPurchaseValueByItemName));
                            OnPropertyChanged(nameof(LTotalByInvoice));

                        }

                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                    }
                    finally
                    {
                        PurchaseItemList.AddRange(TempPurchaseItemCollection);

                        SelectedPurchaseInvoice.TotalPurchaseValueOfInvoice = LTotalByInvoice;
                        PurchaseRecordsList = GetPurchaseRecords();

                        PurchaseItemList = GetPurchaseItemsList();
                        PurchaseItemList.AddRange(TempCurrentItemHolder);
                        this._view2 = new ListCollectionView(this._PurchaseItemList);
                        OnPropertyChanged(nameof(View2));
                        OnPropertyChanged("TotalPurchaseValueByItemName");
                        AllInOneTotalValueHandlingFunc();

                        conn.Close();
                    }

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

        private ICommand _UpdatePurchaseQuantity;
        public ICommand UpdatePurchaseQuantity
        {
            get { return _UpdatePurchaseQuantity; }
            set { _UpdatePurchaseQuantity = value; }
        }

        private PurchaseModel _SelectedPurchaseItem;
        public PurchaseModel SelectedPurchaseItem
        {
            get { return _SelectedPurchaseItem; }
            set
            {
                _SelectedPurchaseItem = value;
                OnPropertyChanged(nameof(SelectedPurchaseItem));
            }
        }

        //Jumping to Update Purchase Item UI
        private void JumpToUpdateSelectedRecord(object obj)
        {

            Window UpdatePurchaseItem = new Views.Windows.UpdatePurchaseItemView();
            UpdatePurchaseItem.DataContext = this;
            OnPropertyChanged(nameof(this.SelectedPurchaseItem));
            UpdatePurchaseItem.Show();
        }

        private ICommand _JumpToUpdatePurchaseItem;
        public ICommand JumpToUpdatePurchaseItem
        {
            get
            {
                return _JumpToUpdatePurchaseItem;
            }
            set
            {
                _JumpToUpdatePurchaseItem = value;
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
        /// Adjusting Total Purchase Value, Stock In Hand etc
        /// </summary>
        /// <param name="obj"></param>
        private void RemoveSelectedRecord(object obj)
        {
            AllInOneTotalValueHandlingFunc();

            //confirmation required to remove Items
            MessageBoxResult RequestConfirm = MessageBox.Show("Do you want to remove record", "Confirm removal", MessageBoxButton.YesNo);

            if (RequestConfirm == MessageBoxResult.Yes)
            {
                SelectedPurchaseItem2 = SelectedPurchaseItem;
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        if (conn == null)
                        {
                            throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                        }

                        foreach (var item in PurchaseItemList)
                        {

                            if (SelectedPurchaseItem.ItemCode == item.ItemCode)
                            {
                                TbyItem = SelectedPurchaseItem.TotalPurchaseValueByItemName;
                                TbyInvoice = SelectedPurchaseInvoice.TotalPurchaseValueOfInvoice;
                                LTotalByInvoice = TbyInvoice - TbyItem;
                                TotalQuantityByItem = SelectedPurchaseItem.PurchaseQuantity;

                                conn.Open();

                                SqlCommand ExistingQuantityCommand = new SqlCommand("select CI_STOCKINHAND  from R_CREATEITEM  where CI_ITEMCODE='" + SelectedPurchaseItem.ItemCode + "'", conn);
                                ExistingQuantity = Convert.ToInt32(ExistingQuantityCommand.ExecuteScalar());

                                if (TotalQuantityByItem > ExistingQuantity)
                                {
                                    MessageBox.Show("Insufficient quantity", "Operation cancelled", MessageBoxButton.OK);
                                    return;
                                }
                                else
                                {
                                    //Removing Items
                                    SqlCommand UDeleteItemCommand = new SqlCommand("DELETE FROM R_PURCHASEINVOICE  WHERE PI_ITEMCODE ='" + SelectedPurchaseItem.ItemCode + "' and PI_INVOICENUMBER='" + SelectedPurchaseInvoice.InvoiceNumber + "'", conn);
                                    UDeleteItemCommand.ExecuteNonQuery();

                                    //updating total purchase value
                                    SqlCommand UUpdateTotalCommand = new SqlCommand("UPDATE R_PURCHASERECORD SET PU_TOTALPURCHASEVALUE='" + LTotalByInvoice + "' WHERE PU_INVOICENUMBER='" + SelectedPurchaseInvoice.InvoiceNumber + "'", conn);
                                    UUpdateTotalCommand.ExecuteNonQuery();

                                    //updating stock in hand
                                    SqlCommand PlusQuantityCommand = new SqlCommand("update R_CREATEITEM set CI_STOCKINHAND = '" + (ExistingQuantity - TotalQuantityByItem) + "' where CI_ITEMCODE='" + SelectedPurchaseItem.ItemCode + "'", conn);
                                    PlusQuantityCommand.ExecuteNonQuery();

                                    RemovalStatus = 1;
                                }
                            }

                        }

                        if (RemovalStatus == 1)
                        {
                            SqlCommand GetPurchaseItemCMD = new SqlCommand("select a.* ,(b.ci_stockinhand + a.PI_PURCHASEQUANTITY) TotalQuantity from R_PURCHASEINVOICE a left join R_CREATEITEM b on" +
                            " PI_ITEMCODE = ci_itemcode where a.PI_INVOICENUMBER = '" + SelectedPurchaseInvoice.InvoiceNumber + "'", conn);

                            using (SqlDataReader reader = GetPurchaseItemCMD.ExecuteReader())
                            {

                                PurchaseItemList.Clear();
                                while (reader.Read())
                                {
                                    PurchaseItemList.Add(new PurchaseModel
                                    {
                                        Itemname = reader["PI_ITEMNAME"].ToString(),
                                        ItemCode = (int)reader["PI_ITEMCODE"],
                                        PurchasePricePerUnit = (int)reader["PI_PURCHASEPRICEUNIT"],
                                        PurchaseQuantity = (int)reader["PI_PURCHASEQUANTITY"],
                                        TotalPurchaseValueByItemName = (int)reader["PI_TOTALPURCHASEVALUE"],
                                        StockInHand = (int)reader["TotalQuantity"]

                                    });
                                }
                            }

                            this._view2 = new ListCollectionView(this._PurchaseItemList);
                            RemovalStatus = 0;
                            OnPropertyChanged(nameof(View2));
                        }

                        PurchaseModel Myitem = new();
                        TbyInvoice = 0;

                        foreach (PurchaseModel item in TempPurchaseItemCollection)
                        {

                            TbyInvoice += item.TotalPurchaseValueByItemName;

                            if (SelectedPurchaseItem2.ItemCode == item.ItemCode)
                            {
                                Myitem = SelectedPurchaseItem2;
                                RemovalStatusForTemp = true;
                            }

                        }


                        if (RemovalStatusForTemp) //removing temp Added Items
                        {
                            TempPurchaseItemCollection.Remove(Myitem);
                            TbyItem = SelectedPurchaseItem2.TotalPurchaseValueByItemName;

                            LTotalByInvoice = TbyInvoice - TbyItem;
                            PurchaseItemList.Clear();
                            PurchaseItemList.AddRange(TempPurchaseItemCollection);
                            this._view2 = new ListCollectionView(this._PurchaseItemList);
                            RemovalStatusForTemp = false;
                            OnPropertyChanged(nameof(View2));
                            OnPropertyChanged(nameof(LTotalByInvoice));
                        }



                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        AllInOneTotalValueHandlingFunc();


                    }
                    finally
                    {
                        TempCollectionLength = TempPurchaseItemCollection.Count;
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

        private ICommand _RemovePurchaseItem;
        public ICommand RemovePurchaseItem
        {
            get { return _RemovePurchaseItem; }
            set { _RemovePurchaseItem = value; }
        }

        int removedInvoice;
        private void RemoveSelectedInvoice(object obj)
        {
            MessageBoxResult RequestConfirm = MessageBox.Show("Do you want to remove invoice", "Confirm removal", MessageBoxButton.YesNo);

            if (RequestConfirm == MessageBoxResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                    try
                    {
                        if (conn == null)
                        {
                            throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                        }

                        removedInvoice = SelectedPurchaseInvoice.InvoiceNumber;
                        conn.Open();

                        //Deleting sales records based on invoice number
                        SqlCommand USupplierCommand = new SqlCommand("DELETE FROM R_PURCHASERECORD  WHERE PU_INVOICENUMBER ='" + SelectedPurchaseInvoice.InvoiceNumber + "'", conn);
                        USupplierCommand.ExecuteNonQuery();

                        //Deleting all Records From PurchaseInvoice Table Related To that Invoice Number
                        SqlCommand UDeleteItemCommand = new SqlCommand("DELETE FROM R_PURCHASEINVOICE  WHERE PI_INVOICENUMBER='" + removedInvoice + "'", conn);
                        UDeleteItemCommand.ExecuteNonQuery();


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                    finally
                    {

                        PurchaseRecordsList = GetPurchaseRecords();
                        this._view = new ListCollectionView(this._PurchaseRecordsList);
                        OnPropertyChanged(nameof(View));
                        this._view.Filter = Filter;
                        OnPropertyChanged(nameof(View.Filter));
                        AllInOneTotalValueHandlingFunc();

                        conn.Close();
                    }
            }
            else
            {
                return;
            }
        }

        private ICommand _RemovePurchaseInvoice;
        public ICommand RemovePurchaseInvoice
        {
            get { return _RemovePurchaseInvoice; }
            set { _RemovePurchaseInvoice = value; }
        }

        /// <summary>
        /// Returing Purchase Item List Based On ceratain Invoice Number
        ///
        /// </summary>
        /// <returns></returns>
        private List<PurchaseModel> GetPurchaseItemsList()
        {
            List<PurchaseModel> listOfPurchaseItems = new();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    conn.Open();

                    PurchaseItemList.Clear();

                    SqlCommand GetPurchaseItemCMD = new SqlCommand("select a.* ,(b.ci_stockinhand) TotalQuantity from R_PURCHASEINVOICE a left join R_CREATEITEM b on" +
                        " PI_ITEMCODE = ci_itemcode where a.PI_INVOICENUMBER = '" + SelectedPurchaseInvoice.InvoiceNumber + "'", conn);

                    using (SqlDataReader reader = GetPurchaseItemCMD.ExecuteReader()) //reading data one by one from sqldatareader
                    {

                        PurchaseItemList.Clear();
                        while (reader.Read())
                        {
                            PurchaseItemList.Add(new PurchaseModel
                            {
                                Itemname = reader["PI_ITEMNAME"].ToString(),
                                ItemCode = (int)reader["PI_ITEMCODE"],
                                PurchasePricePerUnit = (int)reader["PI_PURCHASEPRICEUNIT"],
                                PurchaseQuantity = (int)reader["PI_PURCHASEQUANTITY"],
                                TotalPurchaseValueByItemName = (int)reader["PI_TOTALPURCHASEVALUE"],
                                StockInHand = (int)reader["TotalQuantity"]

                            });
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
                    AllInOneTotalValueHandlingFunc();

                }

            return listOfPurchaseItems;
        }

        private ICommand _JumpToUpdatePurchaseRecordCommand;
        public ICommand JumpToUpdatePurchaseRecordCommand
        {
            get { return _JumpToUpdatePurchaseRecordCommand; }
            set { _JumpToUpdatePurchaseRecordCommand = value; }
        }


        private ICommand _SaveInvoiceItemCommand;
        public ICommand SaveInvoiceItemCommand
        {
            get { return _SaveInvoiceItemCommand; }
            set
            {
                AllInOneTotalValueHandlingFunc();
                _SaveInvoiceItemCommand = value;
            }
        }


        private PurchaseModel _SelectedPurchaseInvoice;
        public PurchaseModel SelectedPurchaseInvoice
        {
            get
            { return _SelectedPurchaseInvoice; }
            set
            {
                _SelectedPurchaseInvoice = value;
                OnPropertyChanged(nameof(SelectedPurchaseInvoice));

            }
        }

        bool IsDbInitiated = false;

        /// <summary>
        /// Saving All Data To DB from Temporary Holded 
        /// observable Collection
        /// </summary>
        /// <param name="obj"></param>
        private void SaveAllRecordsToDBFinally(object obj)
        {
            AllInOneTotalValueHandlingFunc();


            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                    }

                    IsDbInitiated = true;
                    CreateTempInvoiceNumber();

                    SqlCommand query = new SqlCommand("ADDPURCHASEITEM", conn);
                    conn.Open();
                    query.CommandType = CommandType.StoredProcedure;
                    SqlParameter pInvoiceNumber = new SqlParameter("@pPI_INVOICENUMBER", SqlDbType.Int);
                    SqlParameter pPurchaseQuantity = new SqlParameter("@pPI_PURCHASEQUANTITY", SqlDbType.Int);
                    SqlParameter pPurchasePriceUnit = new SqlParameter("@pPI_PURCHASEPRICEPERUNIT", SqlDbType.Int);
                    SqlParameter pItemcode = new SqlParameter("@pPI_ITEMCODE", SqlDbType.Int);
                    SqlParameter pItemName = new SqlParameter("@pPI_ITEMNAME", SqlDbType.VarChar);

                    foreach (var item in TempPurchaseItemCollection)
                    {
                        pInvoiceNumber.Value = item.InvoiceNumber;
                        pPurchaseQuantity.Value = item.PurchaseQuantity;
                        pPurchasePriceUnit.Value = item.PurchasePricePerUnit;
                        pItemcode.Value = item.ItemCode;
                        pItemName.Value = item.Itemname;

                        query.Parameters.Add(pInvoiceNumber);
                        query.Parameters.Add(pPurchaseQuantity);
                        query.Parameters.Add(pItemName);
                        query.Parameters.Add(pPurchasePriceUnit);
                        query.Parameters.Add(pItemcode);

                        query.ExecuteNonQuery();
                        query.Parameters.Clear();

                        SqlCommand StockInHandCommand = new SqlCommand("select CI_stockinhand from R_CREATEITEM where CI_ITEMCODE='" + item.ItemCode + "'", conn);
                        StockInHand = Convert.ToInt32(StockInHandCommand.ExecuteScalar());

                        SqlCommand PlusQuantityCommand = new SqlCommand("update R_CREATEITEM set CI_STOCKINHAND = '" + (item.StockInHand + item.PurchaseQuantity) + "' where CI_ITEMCODE='" + item.ItemCode + "'", conn);
                        PlusQuantityCommand.ExecuteNonQuery();

                    }


                    MessageBox.Show("All purchase's added successfully", "Item added", MessageBoxButton.OK);

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Try again", MessageBoxButton.OK, MessageBoxImage.Information);

                }

                finally
                {
                    TempPurchaseItemCollection.Clear();
                    SqlCommand USupplierCommand = new SqlCommand("UPDATE R_PURCHASERECORD SET PU_TOTALPURCHASEVALUE='" + LTotalByInvoice + "' WHERE PU_INVOICENUMBER='" + LInvoice_no + "'", conn);
                    USupplierCommand.ExecuteNonQuery();
                    OnPropertyChanged(nameof(View));
                    PurchaseRecordsList = GetPurchaseRecords();
                    this._view = new ListCollectionView(this._PurchaseRecordsList);
                    View.Refresh();

                    OnPropertyChanged("");
                    AllInOneTotalValueHandlingFunc();

                    conn.Close();

                }


        }

        /// <summary>
        /// This Method Creating Temporary invoice
        /// number and saving DB only if when user click on Save Buttin on UI
        /// </summary>
        private void CreateTempInvoiceNumber()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in  Retail Store->Properties-?Settings.settings");
                    }
                    DisableComboAfterChange = true;

                    conn.Open();

                    /*
                     * Whenever User Click On Save Button Only After That
                     * Invoice Get Generated and Saved Into DB
                     */
                    if (IsDbInitiated)
                    {
                        SqlCommand query = new SqlCommand("CREATEPURCHASERECORDS", conn);
                        query.CommandType = CommandType.StoredProcedure;
                        SqlParameter pTotalPurchaseValue = new SqlParameter("@uPU_TOTALPURCHASEVALUE", SqlDbType.Int);
                        SqlParameter pSupplierName = new SqlParameter("@uPU_SUPPLIERNAME", SqlDbType.VarChar);
                        SqlParameter pInvoiceNumber = new SqlParameter("@uPU_INVOICENUMBER", SqlDbType.Int);

                        pInvoiceNumber.Value = LInvoice_no;
                        pTotalPurchaseValue.Value = LTotalByInvoice;
                        pSupplierName.Value = LSupplier_Name;

                        query.Parameters.Add(pInvoiceNumber);
                        query.Parameters.Add(pTotalPurchaseValue);
                        query.Parameters.Add(pSupplierName);

                        query.ExecuteNonQuery();
                        IsDbInitiated = false;
                    }

                    // Temporary Creating Invoice Number
                    else
                    {
                        SqlCommand UInvoiceCodeCommand = new SqlCommand("SELECT max(pu_invoicenumber) FROM R_PURCHASERECORD", conn);

                        var IsInvoiceNumberNull = UInvoiceCodeCommand.ExecuteScalar();


                        if (IsInvoiceNumberNull == DBNull.Value)
                        {
                            InvoiceNumber = 1000;
                        }
                        else
                        {
                            InvoiceNumber = Convert.ToInt32(UInvoiceCodeCommand.ExecuteScalar());
                        }
                        LTotalByInvoice = 0;
                        LInvoice_no = InvoiceNumber + 1;

                        DateTime Purchase_d = DateTime.Today;
                        LPurchase_dt = Purchase_d.ToString("dd-MMM-yyyy").Split()[0];
                        LSupplier_Name = "NA";

                        USuppliersList();

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

        //Jumping To Update Purchase Record UI
        private void JumpToUpdatePurchaseRecord(object obj)
        {
            Window UpdatePurchaseRecordWindow = new Views.Windows.UpdatePurchaseInvoiceView();
            AllInOneTotalValueHandlingFunc();

            UpdatePurchaseRecordWindow.DataContext = this;

            GetPurchaseItemsList();

            this._view2 = new ListCollectionView(this._PurchaseItemList);

            UpdatePurchaseRecordWindow.Show();
        }

        //Jumping to purchase invoice UI
        private void JumpToPurchaseInvoice(object obj)
        {

            Window PurchaseInvoice = new Views.Windows.CreatePurchaseInvoiceView();
            TempPurchaseItemCollection.Clear();
            PurchaseItemList = GetPurchaseItemsList();
            PurchaseRecordsList = GetPurchaseRecords();
            this._view2 = new ListCollectionView(this._PurchaseItemList);
            View2.Refresh();
            this._view = new ListCollectionView(this._PurchaseRecordsList);
            View.Refresh();
            CreateTempInvoiceNumber();

            PurchaseItemList = GetPurchaseItemsList();
            PurchaseRecordsList = GetPurchaseRecords();

            this._view2 = new ListCollectionView(this._PurchaseItemList);
            View2.Refresh();
            this._view = new ListCollectionView(this._PurchaseRecordsList);
            View.Refresh();
            this._view.Filter = Filter;

            PurchaseInvoice.DataContext = this;

            OnPropertyChanged(nameof(PurchaseRecordsList));
            OnPropertyChanged(nameof(PurchaseItemList));
            OnPropertyChanged(nameof(View));
            OnPropertyChanged(nameof(View2));
            OnPropertyChanged(nameof(View.Filter));

            PurchaseInvoice.Show();

        }

        /// <summary>
        /// Selected Purchase Item values Comes From
        /// these Methods...
        /// these properties binded to selectedItem in Datagrid
        /// </summary>

        private PurchaseModel _SelectedPurchaseItem2;
        public PurchaseModel SelectedPurchaseItem2
        {
            get { return _SelectedPurchaseItem2; }
            set
            {
                _SelectedPurchaseItem2 = value;
                OnPropertyChanged(nameof(SelectedPurchaseItem2));
            }
        }

        private PurchaseModel _SelectedSupplier_Name;
        public PurchaseModel SelectedSupplier_Name
        {
            get { return _SelectedSupplier_Name; }
            set
            {
                _SelectedSupplier_Name = value;

                OnPropertyChanged(nameof(SelectedSupplier_Name));

            }
        }

        //opening add purchase item UI
        public void JumpToAddPurchaseItem(object obj)
        {

            Window AddPurchaseWindow = new Views.Windows.AddPurchaseItemView();

            if (LSupplier_Name == "NA")
            {
                MessageBox.Show("Please select supplier.", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            UItemNameListBasecOnSupplierName();
            UCategoryList();
            AllInOneTotalValueHandlingFunc();

            AddPurchaseWindow.DataContext = this;
            AddPurchaseWindow.Show();
        }


        public List<PurchaseModel> TempPurchaseItemCollection;

        public List<PurchaseModel> TempCurrentItemHolder;

        int Count;
        int TempPurchaseByItemNameHolder;

        int PurchaseItemLength;
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
        /// <param name="obj"></param>
        private void AddPurchaseItemMethod(object obj)
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                try
                {

                    if (PurchaseQuantity == 0 || Itemname == string.Empty) //purchase Quantity should be greater than zero
                    {
                        if (PurchaseQuantity == 0)
                            MessageBox.Show("Purchase quantity must be greater than zero", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                        else
                            MessageBox.Show("Item name should be selected", "Alert", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                        return;
                    }

                    foreach (var item in PurchaseItemList) //existing item can't add one more time
                    {
                        if (ItemCode == item.ItemCode)
                        {
                            MessageBox.Show("Purchase item already exists. Please update existing item", "Existing item", MessageBoxButton.OK);
                            TbyItem = 1;
                            return;

                        }
                    }

                    foreach (var item in TempPurchaseItemCollection) //existing item can't add one more time
                    {
                        if (ItemCode == item.ItemCode)
                        {
                            MessageBox.Show("Purchase item already exists. Please update existing item", "Existing item", MessageBoxButton.OK);
                            TbyItem = 1;
                            return;

                        }
                    }

                    if (IsJumpedInUpdate)
                    {
                        LInvoice_no = SelectedPurchaseInvoice.InvoiceNumber;
                    }

                    //Adding these all records for temporary base in observable collection
                    TempPurchaseItemCollection.Add(new PurchaseModel
                    {
                        InvoiceNumber = LInvoice_no,
                        ItemCode = this.ItemCode,
                        Itemname = this.Itemname,
                        PurchasePricePerUnit = this.PurchasePricePerUnit,
                        PurchaseQuantity = this.PurchaseQuantity,
                        StockInHand = this.StockInHand,
                        TotalPurchaseValueByItemName = this.PurchaseQuantity * this.PurchasePricePerUnit
                    });

                    SqlCommand USupplierCommand = new SqlCommand("UPDATE R_PURCHASERECORD SET PU_SUPPLIERNAME='" + LSupplier_Name + "' WHERE PU_INVOICENUMBER='" + LInvoice_no + "'", conn);
                    conn.Open();
                    USupplierCommand.ExecuteNonQuery();




                    foreach (var item in TempPurchaseItemCollection)
                    {
                        TempCurrentItemHolder.Clear();
                        TempCurrentItemHolder.Add(item);
                    }

                    PurchaseItemList.AddRange(TempCurrentItemHolder);
                    this._view2 = new ListCollectionView(this._PurchaseItemList);
                    OnPropertyChanged(nameof(View2));

                    foreach (var item in TempPurchaseItemCollection)
                    {
                        TempPurchaseByItemNameHolder = item.TotalPurchaseValueByItemName;
                    }

                    TempCollectionLength = TempPurchaseItemCollection.Count;
                    PurchaseItemLength = PurchaseItemList.Count;

                    Itemcategory = string.Empty;
                    SubCategory = string.Empty;
                    Itemname = string.Empty;
                    PurchaseQuantity = 0;
                    PurchasePricePerUnit = 0;

                    if (TempCollectionLength > 0 || PurchaseItemLength > 0)
                    {
                        DisableComboAfterChange = false;
                        OnClickAction = true;
                    }
                    MessageBox.Show("Purchase item added successfully", "Purchased successfully", MessageBoxButton.OK);


                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                }

                finally
                {
                    if (TbyItem != 1)
                    {
                        if (Count == 0)
                        {
                            LTotalByInvoice = 0;
                            foreach (var item in PurchaseItemList)
                            {
                                LTotalByInvoice += item.TotalPurchaseValueByItemName; //totalling total purchase value of added and previous item of db

                            }
                            Count = 1;
                        }
                        else
                        {
                            LTotalByInvoice += TempPurchaseByItemNameHolder;
                        }
                    }
                    conn.Close();
                    UItemNameListBasecOnSupplierName();
                    OnPropertyChanged("");
                    AllInOneTotalValueHandlingFunc();

                }


            }


        }



        /// <summary>
        /// To Access Unique/Distinct Data from DB
        /// Here, we Load Unique Item name/category/sub category/suppliers
        /// </summary>

        private List<PurchaseModel> _USuppliers;
        public List<PurchaseModel> USuppliers
        {
            get { return _USuppliers; }
            set
            {
                _USuppliers = value;
                OnPropertyChanged(nameof(USuppliers));
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

        /// <summary>
        /// Methods to load distnict data of various
        /// elements.
        /// </summary>
        public void USuppliersList()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {
                    if (Count == 1)
                        return;
                    SqlCommand USupplierCommand = new SqlCommand("SELECT DISTINCT S_NAME FROM R_SUPPLIERS WHERE S_STATUS=1", conn);
                    conn.Open();
                    using (SqlDataReader reader = USupplierCommand.ExecuteReader()) //reading row of record using executereader
                    {

                        USuppliers.Clear();
                        while (reader.Read())
                        {
                            USuppliers.Add(new PurchaseModel { SupplierName = reader["S_NAME"].ToString() }); // adding Unique Suppliers From DB
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

                    SqlCommand UCategoryCommand = new SqlCommand("SELECT DISTINCT CI_ITEMCATEGORY  FROM R_CREATEITEM WHERE CI_SUPPLIERNAME='" + LSupplier_Name + "' AND CI_STATUS=1", conn);
                    conn.Open();
                    using (SqlDataReader reader = UCategoryCommand.ExecuteReader())//reading row of record using executereader
                    {

                        UCategory.Clear();
                        while (reader.Read())
                        {
                            UCategory.Add(new InventoryModel { Itemcategory = reader["CI_ITEMCATEGORY"].ToString() }); //adding Unique category based on supplier name from DB
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

                    SqlCommand USubCategoryCommand = new SqlCommand("SELECT DISTINCT CI_ITEMSUBCAT  FROM R_CREATEITEM WHERE CI_ITEMCATEGORY='" + Itemcategory + "' AND " +
                        "CI_SUPPLIERNAME='" + LSupplier_Name + "'  AND CI_STATUS=1", conn);
                    conn.Open();
                    using (SqlDataReader reader = USubCategoryCommand.ExecuteReader()) //reading row of record using executereader
                    {

                        USubCategory.Clear();
                        while (reader.Read())
                        {
                            USubCategory.Add(new InventoryModel { SubCategory = reader["CI_ITEMSUBCAT"].ToString() }); //Adding Unique Sub Category based on Itemcategory and supplier Name From DB
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

                    SqlCommand UItemNameCommand = new SqlCommand("SELECT DISTINCT CI_ITEMNAME  FROM R_CREATEITEM WHERE CI_ITEMCATEGORY='" + Itemcategory + "' AND CI_ITEMSUBCAT='" + SubCategory + "' AND CI_STATUS=1", conn);
                    conn.Open();
                    using (SqlDataReader reader = UItemNameCommand.ExecuteReader()) //reading row of record using executereader
                    {

                        UItemName.Clear();
                        while (reader.Read())
                        {
                            UItemName.Add(new InventoryModel { Itemname = reader["CI_ITEMNAME"].ToString() }); //adding item name based on Item Category and Sub Category from DB whose Status is Active
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

        public void UItemNameListBasecOnSupplierName()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {

                    SqlCommand UItemNameCommand = new SqlCommand("SELECT DISTINCT CI_ITEMNAME  FROM R_CREATEITEM WHERE CI_SUPPLIERNAME='" + LSupplier_Name + "'  AND CI_STATUS = 1", conn);
                    conn.Open();
                    using (SqlDataReader reader = UItemNameCommand.ExecuteReader()) //reading row of record using executereader
                    {

                        UItemName.Clear();
                        while (reader.Read())
                        {
                            UItemName.Add(new InventoryModel { Itemname = reader["CI_ITEMNAME"].ToString() }); //Adding Item name based On Supplier Name and Whose Status is Active
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


        public void UPurchasePricePerUnitAndCodeForItemName() //PurchasePricePerUnit, Item Code and Stock In Hand For Specific Item
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {

                    SqlCommand PurchasePriceUnitCommand = new SqlCommand("select CI_PURCHASEPRICEUNIT from R_CREATEITEM where CI_ITEMNAME='" + Itemname + "'", conn);
                    conn.Open();
                    PurchasePricePerUnit = Convert.ToInt32(PurchasePriceUnitCommand.ExecuteScalar());
                    SqlCommand ItemCodeCommand = new SqlCommand("select CI_ITEMCODE from R_CREATEITEM where CI_ITEMNAME='" + Itemname + "'", conn);
                    ItemCode = Convert.ToInt32(ItemCodeCommand.ExecuteScalar());
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

        private void ClearInputControls()
        {
            Itemcategory = string.Empty;
            SubCategory = string.Empty;
            SupplierName = string.Empty;
            Itemname = string.Empty;
            PurchasePricePerUnit = 0;
            PurchaseQuantity = 0;
            StockInHand = 0;
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

        private string _Itemname;
        public string Itemname
        {
            get
            { return _Itemname; }
            set
            {
                _Itemname = value;
                OnPropertyChanged(nameof(Itemname));
                UPurchasePricePerUnitAndCodeForItemName();

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

        /// <summary>
        /// ICommand For Binded Commanded In Xaml
        /// </summary>

        private ICommand _AddPurchaseItemCommand;
        public ICommand AddPurchaseItemCommand
        {
            get { return _AddPurchaseItemCommand; }
            set { _AddPurchaseItemCommand = value; }
        }

        private ICommand _AddPurchaseItem;
        public ICommand AddPurchaseItem
        {
            get { return _AddPurchaseItem; }
            set { _AddPurchaseItem = value; }
        }

        private ICommand _PurchaseInvoiceCommand;
        public ICommand PurchaseInvoiceCommand
        {
            get { return _PurchaseInvoiceCommand; }
            set { _PurchaseInvoiceCommand = value; }
        }

        /// <summary>
        /// store data in list to show on datagrid
        /// </summary>

        private List<PurchaseModel> _PurchaseItemList;
        public List<PurchaseModel> PurchaseItemList
        {
            get { return _PurchaseItemList; }
            set
            {
                _PurchaseItemList = value;
                OnPropertyChanged(nameof(PurchaseItemList));
            }
        }

        private List<PurchaseModel> _PurchaseRecordsList;
        public List<PurchaseModel> PurchaseRecordsList
        {
            get { return _PurchaseRecordsList; }
            set
            {
                _PurchaseRecordsList = value;
                OnPropertyChanged(nameof(PurchaseRecordsList));
            }
        }

        /// <summary>
        /// Accessing All Records from  DB and storing
        /// into single list for different purposes
        /// </summary>

        private List<PurchaseModel> GetPurchaseRecords()
        {

            List<PurchaseModel> listOfPurchaseInvoice = new();

            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
                try
                {
                    if (conn == null)
                    {
                        throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                    }

                    SqlCommand query = new SqlCommand("VIEWPURCHASERECORDS", conn);

                    query.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)  //adding data into list one by one using datarow functionalities
                    {
                        PurchaseModel m = new PurchaseModel();
                        m.InvoiceNumber = (int)row["PU_INVOICENUMBER"];
                        DateTime OnlyDate = (DateTime)row["PU_PURCHASEDATE"];
                        m.TotalPurchaseValueOfInvoice = (int)row["PU_TOTALPURCHASEVALUE"];
                        m.SupplierName = row["PU_SUPPLIERNAME"].ToString();
                        m.PurchaseDate = OnlyDate.ToString("dd-MMM-yyyy").Split()[0];


                        listOfPurchaseInvoice.Add(new PurchaseModel
                        {
                            InvoiceNumber = m.InvoiceNumber,
                            PurchaseDate = m.PurchaseDate,
                            TotalPurchaseValueOfInvoice = m.TotalPurchaseValueOfInvoice,
                            SupplierName = m.SupplierName

                        });

                    }
                    return listOfPurchaseInvoice;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                    OnPropertyChanged(nameof(SelectedPurchaseInvoice));
                    OnPropertyChanged(nameof(SelectedPurchaseItem));
                    AllInOneTotalValueHandlingFunc();

                }

            return listOfPurchaseInvoice;

        }

        private DateTime _DisplayDate;

        public DateTime DisplayDate
        {
            get { return _DisplayDate = DateTime.Today; }
            set { _DisplayDate = value; }
        }


        public Visibility DateTimeVisibility { get; set; } = Visibility.Hidden; //disable/enable datepicker on UI

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
                return ((item as PurchaseModel).InvoiceNumber.ToString().IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Supplier")
                return ((item as PurchaseModel).SupplierName.ToString().IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
            else if (FilteredList == "Purchase Date")
                return ((item as PurchaseModel).PurchaseDate.ToString().IndexOf(TextSearch, StringComparison.OrdinalIgnoreCase) >= 0);
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
                if (_FilteredList == "Purchase Date")
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
        /// 
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


        /// <summary>
        /// Store last values of data
        /// L stands for Last Here
        /// storing Last invoice/supplier/total/purchase date
        /// </summary>
        /// 

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

        private string Supplier_Name;

        public string LSupplier_Name
        {
            get { return Supplier_Name; }
            set
            {
                Supplier_Name = value;
                OnPropertyChanged(nameof(Supplier_Name));
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

        private string Purchase_dt;

        public string LPurchase_dt
        {
            get { return Purchase_dt; }
            set
            {
                Purchase_dt = value;
                OnPropertyChanged(nameof(Purchase_dt));
            }
        }


    }


}

