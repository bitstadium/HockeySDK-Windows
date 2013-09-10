using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HockeyApp.Tools;
using System.ComponentModel;
using System.Collections.ObjectModel;
using HockeyApp.Model;
using System.Threading.Tasks;

namespace HockeyApp.Views
{
    public partial class FeedbackControl : UserControl
    {
        public FeedbackControl(FeedbackVM viewmodel)
        {
            DataContext = viewmodel;
            InitializeComponent();

            //TODO 
            /*
            SendButton.Click += async (sender, e) =>
            {
                await viewmodel.SubmitForm();
            };
             * */
            AnswerButton.Click += async (sender, e) =>
            {
                await viewmodel.ShowList();
            };
        }


    }
}
