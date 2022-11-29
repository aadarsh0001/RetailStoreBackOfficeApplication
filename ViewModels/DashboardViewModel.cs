using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRetailStore.ViewModels
{
    class DashboardViewModel : BaseViewModel
    {
        // Constructor
        public DashboardViewModel()
        {
            SelectedDateForData = DateTime.Today;
            _MyCurrentDate = _SelectedDateForData.ToString("yyyy-MM-dd");
            LastDate = DateTime.Today;
            TotalByDate();
            _TotalPurchase = totalPurchasebyDate;
           
            _TotalSales = totalSalesbyDate;
           
        }

       //Propertiy for current date

        private string _MyCurrentDate;
        public string MyCurrentDate
        {
            get
            {

                _MyCurrentDate = _SelectedDateForData.ToString("yyyy-MM-dd");
                TotalByDate();
                _TotalPurchase = totalPurchasebyDate;
                
                _TotalSales = totalSalesbyDate;
               
              
                OnPropertyChanged(nameof(TotalPurchase));
                OnPropertyChanged(nameof(TotalSales));
                OnPropertyChanged(nameof(TotalValuation));

                return _MyCurrentDate;
            }
            set
            {
                _MyCurrentDate = value;

                OnPropertyChanged(nameof(MyCurrentDate));
            }

        }


        //Property  for last date
        private DateTime _LastDate;
        public DateTime LastDate
        {
            get
            {
                return _LastDate;
            }
            set
            {
                _LastDate = value;
            }
        }


        //Property for select date
        private DateTime _SelectedDateForData;
        public DateTime SelectedDateForData
        {
            get
            {

                _MyCurrentDate = _SelectedDateForData.ToString("yyyy-MM-dd");
                TotalByDate();
                _TotalPurchase = totalPurchasebyDate;
                _TotalSales = totalSalesbyDate;
               
                OnPropertyChanged(nameof(TotalPurchase));
                OnPropertyChanged(nameof(TotalSales));
                OnPropertyChanged(nameof(TotalValuation));

                return _SelectedDateForData;
            }
            set
            {
                _SelectedDateForData = value;
                OnPropertyChanged(nameof(SelectedDateForData));
            }
        }

       
        //Property for Total Purchase
        private int _TotalPurchase;

        public int TotalPurchase
        {
            get
            {
                TotalByDate();
                return _TotalPurchase = totalPurchasebyDate;
            }
            set
            {
                _TotalPurchase = value;
                OnPropertyChanged(nameof(TotalPurchase));
            }
        }
        //Property for Total sales
        private int _TotalSales;
        public int TotalSales
        {
            get
            {
                return _TotalSales = totalSalesbyDate;
            }
            set
            {
                _TotalSales = value;
                OnPropertyChanged(nameof(TotalSales));
            }
        }

      //Property for total valuation
        private int _TotalValuation;
        public int TotalValuation
        {
            get
            {
                return _TotalValuation = totalValuationOfToday;
            }
            set
            {
                _TotalValuation = value;
                OnPropertyChanged(nameof(TotalValuation));
            }
        }

        //Property for total purchase by date
        int totalPurchasebyDate;

        int totalSalesbyDate;

        int totalValuationOfToday;
        public void TotalByDate()
        {
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                try
                {
                    // total purchase by date

                    SqlCommand TotalPurchaseCommand = new SqlCommand("Select SUM(PU_TOTALPURCHASEVALUE) FROM R_PURCHASERECORD WHERE PU_PURCHASEDATE='" + _MyCurrentDate + "' ", conn);
                    conn.Open();
                    var queryResult = TotalPurchaseCommand.ExecuteScalar();

                    if (queryResult != DBNull.Value)
                    {
                        totalPurchasebyDate = Convert.ToInt32(TotalPurchaseCommand.ExecuteScalar());
                    }
                    else
                    {
                        totalPurchasebyDate = 0;
                    }

                    //total sales by date
                    SqlCommand TotalSalesCommand = new SqlCommand("Select SUM(SU_TOTALSALESVALUE) FROM R_SALESRECORD WHERE SU_SALESDATE='" + _MyCurrentDate + "' ", conn);                 
                    var TotalSalesResult = TotalSalesCommand.ExecuteScalar();

                    if (TotalSalesResult != DBNull.Value)
                    {
                        totalSalesbyDate = Convert.ToInt32(TotalSalesCommand.ExecuteScalar());
                    }
                    else
                    {
                        totalSalesbyDate = 0;
                    }

                    // total stock valueation till today
                    SqlCommand TotalValuationCommand = new SqlCommand("select sum(CI_STOCKINHAND * CI_PURCHASEPRICEUNIT) from R_CREATEITEM", conn);
                   
                    var totalValueationResult = TotalValuationCommand.ExecuteScalar();

                    if (totalValueationResult != DBNull.Value)
                    {
                        totalValuationOfToday = Convert.ToInt32(TotalValuationCommand.ExecuteScalar());
                    }
                    else
                    {
                        totalValuationOfToday = 0;
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

    }
}
