using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp.ViewModels
{
    public abstract class VMBase : INotifyPropertyChanged
    {
        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyOfPropertyChange("IsBusy");
                NotifyOfPropertyChange("IsNotBusy");
            }
        }

        public bool IsNotBusy
        {
            get { return !IsBusy; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region INotify
        protected void NotifyOfPropertyChange(string name)
        {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
