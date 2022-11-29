using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyRetailStore.Commands;
using MyRetailStore.Models;

namespace MyRetailStore.ViewModels
{
    public class StoreViewModel : BaseViewModel
    {
        //--------------------------------------Constructor
        public StoreViewModel()
        {
            SaveStoreCommand = new RelayCommand(new Action<object>(AddStore));

            GetStoreInfo();
        }



        //--------------------------------------Properties 
        public int StoreId { get; set; } = 0;
        public string LblStoreName { get; set; }
        public string LblStoreAddress { get; set; }
        public string TxtStoreName { get; set; }
        public string TxtAddressLineOne { get; set; }
        public string TxtAddressLineTwo { get; set; }
        public string TxtAddressCity { get; set; }
        public string TxtAddressState { get; set; }
        public string TxtAddressCountry { get; set; }
        public string TxtAddressZipCode { get; set; }


        //--------------------------------------Property for save Command
        private ICommand mSaveStoreCommand;
        public ICommand SaveStoreCommand
        {
            get { return mSaveStoreCommand; }
            set { mSaveStoreCommand = value; }
        }

        //--------------------------------------Method for getting store information
        public void GetStoreInfo()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                SqlCommand query = new SqlCommand("VIEWSTORE", conn);

                query.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                if (dataTable.Rows.Count == 1)
                {
                    StoreId = Convert.ToInt32(dataTable.Rows[0]["RS_ID"]);
                    LblStoreName = dataTable.Rows[0]["RS_StoreName"].ToString();
                    TxtStoreName = LblStoreName;
                    TxtAddressLineOne = dataTable.Rows[0]["RS_ADDRESSLineOne"].ToString();
                    TxtAddressLineTwo = dataTable.Rows[0]["RS_ADDRESSLineTwo"].ToString();
                    TxtAddressCity = dataTable.Rows[0]["RS_ADDRESSCity"].ToString();
                    TxtAddressState = dataTable.Rows[0]["RS_ADDRESSState"].ToString();
                    TxtAddressCountry = dataTable.Rows[0]["RS_ADDRESSCountry"].ToString();
                    TxtAddressZipCode = dataTable.Rows[0]["RS_ADDRESSZipCode"].ToString();

                    string address = string.Empty;
                    if (dataTable.Rows[0]["RS_ADDRESSLineTwo"].ToString().Trim() == "")
                    {
                        address = $"{TxtAddressLineOne},{Environment.NewLine}{TxtAddressCity}, {TxtAddressState}, {TxtAddressCountry} - {TxtAddressZipCode}";
                    }
                    else
                    {
                        address = $"{TxtAddressLineOne},{Environment.NewLine}{TxtAddressLineTwo},{Environment.NewLine}{TxtAddressCity}, {TxtAddressState}, {TxtAddressCountry} - {TxtAddressZipCode}";
                    }
                    LblStoreAddress = address;

                    OnPropertyChanged(nameof(StoreId));
                    OnPropertyChanged(nameof(LblStoreName));
                    OnPropertyChanged(nameof(TxtAddressLineOne));
                    OnPropertyChanged(nameof(TxtAddressLineTwo));
                    OnPropertyChanged(nameof(TxtAddressCity));
                    OnPropertyChanged(nameof(TxtAddressState));
                    OnPropertyChanged(nameof(TxtAddressCountry));
                    OnPropertyChanged(nameof(TxtAddressZipCode));
                    OnPropertyChanged(nameof(LblStoreAddress));
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
                    conn.Close();
                }


            }
        }

        //--------------------------------------Method for adding Store information
        private void AddStore(object obj)
        {
            if (TxtStoreName == string.Empty || TxtAddressLineOne == string.Empty || TxtAddressCity == string.Empty || TxtAddressZipCode == string.Empty || TxtAddressState == string.Empty || TxtAddressCountry == string.Empty || TxtAddressLineTwo== string.Empty)
            {
                MessageBox.Show("All field are mandatory to fill", "Details Missing", MessageBoxButton.OK);
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

                        else
                        {
                            if (StoreId > 0)
                            {
                                SqlCommand query = new SqlCommand("UPDATESTORE", conn);
                                conn.Open();
                                query.CommandType = CommandType.StoredProcedure;
                                SqlParameter pStoreName = new SqlParameter("@rRS_StoreName", SqlDbType.Text);
                                SqlParameter pAddressLineOne = new SqlParameter("@rRS_ADDRESSLineOne", SqlDbType.Text);
                                SqlParameter pAddressLineTwo = new SqlParameter("@rRS_ADDRESSLineTwo", SqlDbType.Text);
                                SqlParameter pAddressCity = new SqlParameter("@rRS_ADDRESSCity", SqlDbType.Text);
                                SqlParameter pAddressState = new SqlParameter("@rRS_ADDRESSState", SqlDbType.Text);
                                SqlParameter pAddressCountry = new SqlParameter("@rRS_ADDRESSCountry", SqlDbType.Text);
                                SqlParameter pAddressZipCode = new SqlParameter("@rRS_ADDRESSZipCode", SqlDbType.Int);
                                SqlParameter pStoreId = new SqlParameter("@rRs_ID", SqlDbType.Int);



                                pStoreName.Value = TxtStoreName;
                                pAddressLineOne.Value = TxtAddressLineOne;
                                pAddressLineTwo.Value = TxtAddressLineTwo;
                                pAddressCity.Value = TxtAddressCity;
                                pAddressState.Value = TxtAddressState;
                                pAddressCountry.Value = TxtAddressCountry;
                                pAddressZipCode.Value = Convert.ToInt32(TxtAddressZipCode);
                                pStoreId.Value = StoreId;

                                query.Parameters.Add(pStoreName);
                                query.Parameters.Add(pAddressLineOne);
                                query.Parameters.Add(pAddressLineTwo);
                                query.Parameters.Add(pAddressCity);
                                query.Parameters.Add(pAddressState);
                                query.Parameters.Add(pAddressCountry);
                                query.Parameters.Add(pAddressZipCode);
                                query.Parameters.Add(pStoreId);
                                query.ExecuteNonQuery();
                            }
                            else
                            {


                                SqlCommand query = new SqlCommand("ADDSTORE", conn);
                                conn.Open();
                                query.CommandType = CommandType.StoredProcedure;
                                SqlParameter pStoreName = new SqlParameter("@rRS_StoreName", SqlDbType.Text);
                                SqlParameter pAddressLineOne = new SqlParameter("@rRS_ADDRESSLineOne", SqlDbType.Text);
                                SqlParameter pAddressLineTwo = new SqlParameter("@rRS_ADDRESSLineTwo", SqlDbType.Text);
                                SqlParameter pAddressCity = new SqlParameter("@rRS_ADDRESSCity", SqlDbType.Text);
                                SqlParameter pAddressState = new SqlParameter("@rRS_ADDRESSState", SqlDbType.Text);
                                SqlParameter pAddressCountry = new SqlParameter("@rRS_ADDRESSCountry", SqlDbType.Text);
                                SqlParameter pAddressZipCode = new SqlParameter("@rRS_ADDRESSZipCode", SqlDbType.Int);


                                pStoreName.Value = TxtStoreName;
                                pAddressLineOne.Value = TxtAddressLineOne;
                                pAddressLineTwo.Value = TxtAddressLineTwo;
                                pAddressCity.Value = TxtAddressCity;
                                pAddressState.Value = TxtAddressState;
                                pAddressCountry.Value = TxtAddressCountry;
                                pAddressZipCode.Value = Convert.ToInt32(TxtAddressZipCode);

                                query.Parameters.Add(pStoreName);
                                query.Parameters.Add(pAddressLineOne);
                                query.Parameters.Add(pAddressLineTwo);
                                query.Parameters.Add(pAddressCity);
                                query.Parameters.Add(pAddressState);
                                query.Parameters.Add(pAddressCountry);
                                query.Parameters.Add(pAddressZipCode);
                                query.ExecuteNonQuery();
                            }
                           // ClearInputControls();

                            GetStoreInfo();
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
           

        }

        //--------------------------------------Method for clearing text info.
        private void ClearInputControls()
        {
            TxtAddressCity = string.Empty;
            TxtAddressCountry = string.Empty;
            TxtAddressLineOne = string.Empty;
            TxtAddressLineTwo = string.Empty;
            TxtAddressState = string.Empty;
            TxtAddressZipCode = string.Empty;
            TxtStoreName = string.Empty;
            OnPropertyChanged("ClearInputControls");
        }
    }

}
