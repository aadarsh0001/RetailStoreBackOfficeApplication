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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MyRetailStore.Commands;
using MyRetailStore.Models;

namespace MyRetailStore.ViewModels
{
  public  class UserManagementViewModel : BaseViewModel
    {

        //--------------------------------------Constructor

        public UserManagementViewModel()
        {
            SaveCommand = new RelayCommand(new Action<object>(AddUser));

            AddUserCommand = new RelayCommand(new Action<object>(JumpToAddUser));

            SaveUpdateCommand = new RelayCommand(new Action<object>(UpdateUser));
            JumpToUpdateUser = new DelegateCommand<object>((obj) => { jumpUserUpdate(obj); });
            Users = new ObservableCollection<UserModel>();
            IEnumerable MyActiveUsers = null;
            IEnumerable MyInactiveUsers = null;
            GetUserInfo(out MyActiveUsers, out MyInactiveUsers);
            Uuser = new ObservableCollection<UserModel>();
            Active.Clear();
            foreach (UserModel item in MyActiveUsers)
            {
                Active.Add(item);
            }
            Users = Active;
        }


        //--------------------------------------Property for User Record

        private ObservableCollection<UserModel> _Users;

        public ObservableCollection<UserModel> Users
        {
            get { return _Users; }
            set
            {
                _Users = value;
                OnPropertyChanged(nameof(Users));
            }
        }


        //--------------------------------------Property for user selection

        private UserModel _SelectedUser;

        public UserModel SelectedUser
        {
            get { return _SelectedUser; }
            set
            {
                _SelectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));

            }
        }


        //--------------------------------------Properties

        ObservableCollection<UserModel> Active = new();
        ObservableCollection<UserModel> Inactive = new();



        //-------------------------------------Property for check box
        private bool _IsSelectedBox;
        public bool IsSelectedBox
        {
            get
            { return _IsSelectedBox; }
            set
            {

                if (_IsSelectedBox == value) return;
                IEnumerable MyActiveUsers = null;
                IEnumerable MyInactiveUsers = null;
                GetUserInfo(out MyActiveUsers, out MyInactiveUsers);
                Active.Clear();
                Inactive.Clear();
                foreach (UserModel item in MyActiveUsers)
                {
                    Active.Add(item);

                }

                foreach (UserModel item in MyInactiveUsers)
                {
                    Inactive.Add(item);
                }

                _IsSelectedBox = value;

                Users = _IsSelectedBox ? Inactive : Active;

                OnPropertyChanged(nameof(IsSelectedBox));
                OnPropertyChanged(nameof(Users));

            }

        }


        //-------------------------------------Method for clear Input
        private void ClearInputControls()
        {
            Name = string.Empty;
            Usrname = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            OnPropertyChanged("");
        }



        //------------------------------ Method  for refreshing grid
        public void HotReload(string ShowList)
        {
            IEnumerable MyActiveUsers = null;
            IEnumerable MyInactiveUsers = null;
            GetUserInfo(out MyActiveUsers, out MyInactiveUsers);
            Active.Clear();
            Inactive.Clear();
            foreach (UserModel item in MyActiveUsers)
            {
                Active.Add(item);

            }

            foreach (UserModel item in MyInactiveUsers)
            {
                Inactive.Add(item);
            }
            if (ShowList.ToUpper() == "A")
            {
                Users = Active;

            }
            else if (ShowList.ToUpper() == "IA")
            {
                Users = Inactive;
            }
            else
            {
                //{
                //    Users = Active;
                //}
                //OnPropertyChanged(nameof(Users));
            }
        }

        //------------------------------ Method for getting User information
        public void GetUserInfo(out IEnumerable MyActiveUsers, out IEnumerable MyInactiveUsers)
        {
            IEnumerable ActiveUsers = null;
            IEnumerable InactiveUsers = null;

            List<UserModel> listOfUsers = new();
            using (SqlConnection conn = new SqlConnection(Properties.Settings.Default.connString))
            {
                if (conn == null)
                {
                    throw new Exception("Connection String is Null. Set the value of Connection String in Retail Store->Properties-?Settings.settings");
                }

                SqlCommand query = new SqlCommand("VIEWUSER", conn);

                query.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query);
                DataTable dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
                foreach (DataRow row in dataTable.Rows)
                {
                    UserModel m = new UserModel();
                    m.UserID = (int)row["USR_ID"];
                    m.Name = row["USR_NAME"].ToString();
                    m.Usrname = row["USR_USERNAME"].ToString();
                    m.Email = row["USR_EMAIL"].ToString();
                    m.Status = (int)row["USR_ISDISABLED"];
                    if (m.Status == 0)
                    {
                        m.isStatus = "Inactive";
                    }
                    else
                    {
                        m.isStatus = "Active";
                    }
                    listOfUsers.Add(new UserModel
                    {
                        UserID = m.UserID,
                        Usrname = m.Usrname,
                        Name = m.Name,
                        Password = m.Password,
                        Email = m.Email,
                        isStatus = m.isStatus,
                        Status = m.Status
                    });

                    ActiveUsers = from s in listOfUsers
                                  where s.isStatus == "Active"
                                  select s;

                    InactiveUsers = from s in listOfUsers
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
                    MyActiveUsers = ActiveUsers;
                    MyInactiveUsers = InactiveUsers;
                    conn.Close();
                }
            }
        }


        //------------------------------ Property for save record
        private ICommand uSaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return uSaveCommand;
            }
            set
            {
                uSaveCommand = value;
            }
        }

        //------------------------------ Property for Full Name
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        //------------------------------ Property for User Name
        private string _Usrname;
        public string Usrname
        {
            get { return _Usrname; }
            set
            {
                _Usrname = value;
                OnPropertyChanged(nameof(Usrname));
            }
        }

        //------------------------------ Property for Status(UI)
        private string _isStatus;
        public string isStatus
        {
            get { return _isStatus; }
            set
            {
                _isStatus = value;
                OnPropertyChanged(nameof(isStatus));
            }
        }

        //------------------------------ Property for Password
        private string _Password;

        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        //------------------------------ Property for Email
        private string _Email;

        public string Email
        {
            get { return _Email; }
            set
            {
                _Email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        //------------------------------ Property for Status(DB)
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

        //------------------------------ Property to check duplicate user

        private ObservableCollection<UserModel> _Uuser;
        public ObservableCollection<UserModel> Uuser
        {
            get { return _Uuser; }
            set
            {
                _Uuser = value;
                OnPropertyChanged(nameof(Uuser));
            }
        }



        //------------------------------ Variable for duplicacy

        bool Canexist = false;
        string checkduplicacy;

        bool oneTime = true;

        //------------------------ Method for add New user
        public void AddUser(object obj)
        {


            if (Name == string.Empty|| Usrname == string.Empty || Password == string.Empty || Email == string.Empty)
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
                            SqlCommand query = new SqlCommand("ADDUSER", conn);
                            conn.Open();
                            SqlCommand UuserCommand = new SqlCommand("SELECT DISTINCT  USR_USERNAME, USR_EMAIL FROM R_Users", conn);



                            using (SqlDataReader reader = UuserCommand.ExecuteReader())
                            {



                                Uuser.Clear();
                                while (reader.Read())
                                {
                                    Uuser.Add(new UserModel { Usrname = reader["USR_USERNAME"].ToString(), Email = reader["USR_EMAIL"].ToString() });



                                }
                            }



                            foreach (var user in Uuser)
                            {



                                if (user.Usrname == Usrname || user.Email == Email)
                                {
                                    Canexist = true;
                                    if (user.Usrname == Usrname)
                                    {
                                        checkduplicacy = "User name";
                                    }



                                    if (user.Email == Email)
                                    { checkduplicacy = "Email"; }



                                    if (user.Usrname == Usrname && user.Email == Email)
                                    { checkduplicacy = "User name and eMail"; }
                                }
                            }
                            if (Canexist)
                            {
                                MessageBox.Show(checkduplicacy + " already exists", "Exisiting Details", MessageBoxButton.OK);
                                Canexist = false;
                                return;
                            }
                            query.CommandType = CommandType.StoredProcedure;
                            SqlParameter pName = new SqlParameter("@uUSR_NAME", SqlDbType.Text);
                            SqlParameter pUsrname = new SqlParameter("@uUSR_USRNAME", SqlDbType.Text);
                            SqlParameter pPassword = new SqlParameter("@uUSR_PASSWORD", SqlDbType.VarChar);
                            SqlParameter pEmail = new SqlParameter("@uUSR_EMAIL", SqlDbType.VarChar);


                            pName.Value = Name;
                            pUsrname.Value = Usrname;
                            pPassword.Value = Password;

                            Regex regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                            Match match = regex.Match(Email);
                            if (match.Success)
                            {
                                pEmail.Value = Email;
                            }
                            else
                            {
                                MessageBox.Show("Please enter valid email ID", "Invalid email", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                                JumpToAddUser(obj);

                                Email = string.Empty;
                                OnPropertyChanged("");

                                return;
                            }

                            query.Parameters.Add(pName);
                            query.Parameters.Add(pUsrname);
                            query.Parameters.Add(pPassword);
                            query.Parameters.Add(pEmail);


                            query.ExecuteNonQuery();

                            MessageBox.Show("User created successfully", "User added", MessageBoxButton.OK);
                            if (oneTime)
                            {
                                SqlCommand UpdateAdmin = new SqlCommand("INACTIVEUSER", conn);



                                UpdateAdmin.CommandType = CommandType.StoredProcedure;
                                SqlParameter pId = new SqlParameter("@UId", SqlDbType.Int);
                                pId.Value = 0;



                                UpdateAdmin.Parameters.Add(pId);
                                UpdateAdmin.ExecuteNonQuery();
                                oneTime = false;
                            }
                            ClearInputControls();
                            HotReload("a");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                   //

                }
            }
               
         }



        //------------------------ Property for ID
        public int Id { get; set; }


        //------------------------ Property for saveUpdate record

        private ICommand _SaveUpdateCommand;

        public ICommand SaveUpdateCommand
        {
            get { return _SaveUpdateCommand; }
            set { _SaveUpdateCommand = value; }
        }


        //------------------------------ Method for Update User
        public void UpdateUser(object obj)
        {
            if (SelectedUser.Name == null || SelectedUser.Usrname == null || SelectedUser.Password == null || SelectedUser.Email == null)
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
                            throw new Exception("Connection String is Null. Set the value of Connection String in RetailDBN->Properties-?Settings.settings");
                        }

                        else
                        {
                            SqlCommand query = new SqlCommand("UPDATEUSER", conn);
                            conn.Open();
                            query.CommandType = CommandType.StoredProcedure;
                            SqlParameter pStatus = new SqlParameter("@sUSR_ISDISABLED", SqlDbType.Int);
                            SqlParameter pId = new SqlParameter("@sUSR_ID", SqlDbType.Int);
                            SqlParameter pName = new SqlParameter("@sUSR_NAME", SqlDbType.Text);
                            SqlParameter pUserName = new SqlParameter("@sUSR_USERNAME", SqlDbType.Text);
                            SqlParameter pPassword = new SqlParameter("@sUSR_PASSWORD", SqlDbType.VarChar);
                            SqlParameter pEmail = new SqlParameter("@sUSR_EMAIL", SqlDbType.VarChar);


                            pId.Value = SelectedUser.UserID;
                            pName.Value = SelectedUser.Name;
                            pUserName.Value = SelectedUser.Usrname;
                            Regex regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                            Match match = regex.Match(SelectedUser.Email);

                            if (match.Success)
                            {
                                pEmail.Value = SelectedUser.Email;
                            }
                            else
                            {
                                MessageBox.Show("Please enter valid email ID", "Invalid email", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                                jumpUserUpdate(obj);
                                SelectedUser.Email = string.Empty;
                                OnPropertyChanged("");
                                return;

                            }

                            pPassword.Value = SelectedUser.Password;
                            pStatus.Value = SelectedUser.isStatus == "Inactive" ? 0 : (object)1;


                            query.Parameters.Add(pUserName);
                            query.Parameters.Add(pName);
                            query.Parameters.Add(pPassword);
                            query.Parameters.Add(pEmail);
                            query.Parameters.Add(pStatus);
                            query.Parameters.Add(pId);


                            query.ExecuteNonQuery();
                            MessageBox.Show("User updated successfully", "User updated", System.Windows.MessageBoxButton.OK);
                            IsSelectedBox = false;
                        }
                    }

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


        //------------------------------Property for update User UI
        private ICommand uJumpToUpdateUser;
        public ICommand JumpToUpdateUser
        {
            get { return uJumpToUpdateUser; }
            set { uJumpToUpdateUser = value; }
        }

        //------------------------------Method for open update User UI
        private void jumpUserUpdate(object obj)
        {
            Window window = new Views.Windows.UpdateUserView();
            window.DataContext = this;
            window.Show();

        }


        //------------------------------ Property for add user UI
        private ICommand mAddUserCommand;
        public ICommand AddUserCommand
        {
            get { return mAddUserCommand; }
            set { mAddUserCommand = value; }
        }

        //------------------------------Method for open add User UI
        private void JumpToAddUser(object obj)
        {
            Window addUser = new Views.Windows.AddUserView();
            ClearInputControls();
            addUser.DataContext = this;
            addUser.Show();
        }

    }


}
