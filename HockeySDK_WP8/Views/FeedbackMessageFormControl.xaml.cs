using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HockeyApp.ViewModels;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;
using HockeyApp.Model;

namespace HockeyApp.Views
{
    public partial class FeedbackMessageFormControl : UserControl
    {

        protected FeedbackPage ParentControl { get; set; }

        public FeedbackMessageFormVM VM
        {
            get { return (this.DataContext as FeedbackMessageFormVM); }
            protected set { this.DataContext = value; }
        }

        public FeedbackMessageFormControl(FeedbackPage parent)
        {
            InitializeComponent();
            this.ParentControl = parent;
            this.DataContext = new FeedbackMessageFormVM(parent.VM);
        }
        
        internal async Task<IFeedbackMessage> SendButtonClicked()
        {
            object focusObj = FocusManager.GetFocusedElement();
            if (focusObj != null && focusObj is TextBox)
            {
                var binding = (focusObj as TextBox).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }

            return await VM.SubmitForm();
        }

        internal void AttachButtonClicked()
        {
            PhotoChooserTask task = new PhotoChooserTask();

            task.ShowCamera = false;
            task.Completed += async (s, result) =>
            {
                if (result != null && result.ChosenPhoto != null)
                {
                    await this.VM.ShowPhotoResult(result);
                }
            };
            task.Show();
        }
    }
}
