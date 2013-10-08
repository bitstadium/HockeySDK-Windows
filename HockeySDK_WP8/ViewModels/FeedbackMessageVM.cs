using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HockeyApp.ViewModels
{
    public class FeedbackMessageVM: VMBase
    {
        FeedbackMessage msg;


        public FeedbackMessageVM(FeedbackMessage msg)
        {
            this.msg = msg;
        }

        public bool IsIncoming { get { return !IsOutgoing; } }
        public bool IsOutgoing { get { return msg.via.Equals((int)FeedbackMessage.Via.API); } }

        /*
        static SolidColorBrush Incoming = new SolidColorBrush(Color.FromArgb(255, 120, 120, 0));
        static SolidColorBrush Outgoing = new SolidColorBrush(Color.FromArgb(255, 120, 0, 120));
        public Brush BgColor { get { return IsIncoming ? Incoming : Outgoing; } }
         */

        public Thickness Margin
        {
            get {
                return IsIncoming ? new Thickness(2, 10, 40, 10)
                    : new Thickness(40, 10, 2, 10);
            }
        }

        public string Created
        {
            get { return msg.Created.ToString("dd/MM/yyyy HH:mm"); }
        }
        
        public string Text
        {
            get { return msg.clean_text; }
        }

        
    }
}
