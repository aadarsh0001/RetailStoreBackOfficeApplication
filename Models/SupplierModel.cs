using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRetailStore.Models
{
   public class SupplierModel
    {

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
        private string _Address;
        public string Address
        {
            get { return _Address; }
            set
            {
                _Address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
        private int _SNo;
        public int SNo
        {
            get { return _SNo; }
            set
            {
                _SNo = value;
                OnPropertyChanged(nameof(SNo));
            }
        }

        private string _SupplierName;
        public string SupplierName
        {
            get { return _SupplierName; }
            set
            {
                _SupplierName = value;
                OnPropertyChanged(nameof(SupplierName));
            }
        }

        

        private string _Mobile;
        public string Mobile
        {
            get { return _Mobile; }
            set
            {
                _Mobile = value;
                OnPropertyChanged(nameof(Mobile));
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
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
}
