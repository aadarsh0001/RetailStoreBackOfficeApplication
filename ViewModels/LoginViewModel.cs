using System;
using System.Collections.Generic;
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
   public class LoginViewModel: BaseViewModel

    {
        //--------------------------------------Constructor

        public LoginViewModel()
        {
            LogInCommand = new RelayCommand(new Action<object>(ValidateLogin));
            CloseCommand = new RelayCommand(new Action<object>(ShutDown));
           
        }

        //-----------------------------------Properties

        public bool IsLoginSuccessfull { get; set; } = false;
        public string Username { get; set; }
        
        public string Password { get; set; }

        

        public Visibility LoginViewVisiblity { get; set; } = Visibility.Visible;

        public ICommand LogInCommand { get; set; }



        //-----------------------------------Method for shutdown
        private void ShutDown(object commandParameter)
        {
            string parameter = commandParameter.ToString();
            switch (parameter)
            {
                case "CloseApp":
                    Application.Current.Shutdown();
                    break;
            }
                
        }

        //-----------------------------------Property for Close the UI

        private ICommand mCloseCommand;

        public ICommand CloseCommand
        {
            get { return mCloseCommand; }
            set { mCloseCommand = value; }
        }

        //-----------------------------------Method for clear our Input Record
        private void ClearInputControls()
        {
            Username = string.Empty;
            Password = string.Empty;
          
            OnPropertyChanged("");
        }

        //------------------------ Variable to count Users

        int count = 0;

        //------------------------Method for validate the login Details

        public void ValidateLogin(object obj)
        {
            List<UserModel> listOfUsers = new List<UserModel>();
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
               
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                SqlCommand query = new SqlCommand("GETCREDENTIAL", conn);
                
                query.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);

                foreach (DataRow row in dataTable.Rows)
                {
                    UserModel m = new UserModel();
                   
                    m.Usrname = row["USR_USERNAME"].ToString();
                   
                    m.Password = row["USR_PASSWORD"].ToString();

                    m.Status = (int)row["USR_ISDISABLED"];

                    m.UserID = (int)row["USR_ID"];

                    listOfUsers.Add(m);
                }
                try
                {
                    conn.Open();
                    query.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
                finally
                {
                    
                    conn.Close();
                }


                foreach (var Users in listOfUsers)
                {
                    if (Users.Usrname == this.Username && Users.Password == this.Password)
                    {
                        if (Users.Status != 1)
                        {
                            count = 1;
                            MessageBox.Show("User is inactive", "Login failed", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                        IsLoginSuccessfull = true;
                        Window mainwindow = new Views.MainView();
                        Window CreateUserWindow = new Views.Windows.CreateNewUsr();
                        if (Username == "admin" && Password == "admin")
                        {
                            CreateUserWindow.Show();
                            ClearInputControls();
                        }
                        else
                        {
                            mainwindow.Show();
                            LoginViewVisiblity = Visibility.Collapsed;
                            OnPropertyChanged(nameof(LoginViewVisiblity));
                        }
                        
                    }

                }
                if (IsLoginSuccessfull == false)
                {
                    if (count != 1)
                    {
                        MessageBox.Show("Please provide valid credential", "Login failed", MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearInputControls();
                    }
                }
               
            }
        }
    }
}
