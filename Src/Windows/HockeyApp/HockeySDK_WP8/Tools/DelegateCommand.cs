using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace HockeyApp.Tools
{
    public class DelegateCommand : ICommand
    {
        //Delegate to the action that the command executes
        private Action<object> _executeAction;
        //Delegate to the function that check if the command can be executed or not
        private Func<object, bool> _canExecute;

        public bool canExecuteCache;

        public DelegateCommand(Action<object> executeAction)
            : this(executeAction, null)
        {

        }

        public DelegateCommand(Action<object> action, Func<object, bool> canExecute)
        {
            this._executeAction = action;
            this._canExecute = canExecute;
        }

        //interface method, called when CanExecuteChanged event handler is fired
        public bool CanExecute(object parameter)
        {
            //true by default (in case _canExecute is null)
            bool result = true;
            Func<object, bool> canExecuteHandler = this._canExecute;
            if (canExecuteHandler != null)
            {
                result = canExecuteHandler(parameter);
            }

            return result;
        }

        //Event handler that the controld subscribe to 
        public event EventHandler CanExecuteChanged;


        //interface method
        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }


        //rause the CanExecuteChanged event handler manually
        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

        }
    }
}
