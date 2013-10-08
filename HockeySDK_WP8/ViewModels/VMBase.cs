using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.ViewModels
{
    public abstract class VMBase : INotifyPropertyChanged
    {

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
