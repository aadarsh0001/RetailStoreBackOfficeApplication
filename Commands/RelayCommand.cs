using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyRetailStore.Commands
{
    class RelayCommand:ICommand
    {
        public RelayCommand(Action<object> action)
        {
            mAction = action;
        }
        private Action<object> mAction;
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                mAction(parameter);
            }
            else
            {
                mAction(System.Windows.MessageBox.Show("The Parameter does not have any value", "Error-Parameter value", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information));
            }
        }
    }
}
