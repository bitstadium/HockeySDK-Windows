using HockeyApp.Common;
using HockeyApp.Tools;
using HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HockeyApp.Views
{
    public sealed partial class FeedbackImagePage : Page
    {
        private NavigationHelper navigationHelper;
        private FeedbackAttachmentVM defaultViewModel = new FeedbackAttachmentVM();

        private Point _previousContactPt;
        private Point _currentContactPt;
        private double _x1;
        private double _y1;
        private double _x2;
        private double _y2;
        Color _strokeColor;

        public FeedbackImagePage()
        {
            this._strokeColor = (LocalizedStrings.LocalizedResources.FeedbackDrawingColor as string).ConvertStringToColor(Colors.Blue);

            this.InitializeComponent();

            MyCanvas.PointerPressed += new PointerEventHandler(MyCanvas_PointerPressed);
            MyCanvas.PointerMoved += new PointerEventHandler(MyCanvas_PointerMoved);
            MyCanvas.PointerReleased += new PointerEventHandler(MyCanvas_PointerReleased);
            MyCanvas.PointerExited += new PointerEventHandler(MyCanvas_PointerReleased);


            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
         
            DisplayInformation displayInfo = DisplayInformation.GetForCurrentView();
            displayInfo.OrientationChanged += displayInfo_OrientationChanged;
                
        }

        void displayInfo_OrientationChanged(DisplayInformation sender, object args)
        {
            (this.DataContext as FeedbackAttachmentVM).OrientationChanged(this.MyCanvas, this.ShowArea);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            await StatusBar.GetForCurrentView().HideAsync();

            MyCommandBar.Opened += (s, o) => { MyCommandBar.Opacity = 0.7; };
            MyCommandBar.Closed += (s, o) => { MyCommandBar.Opacity = 0; };

            this.DataContext = await FeedbackManager.Current.GetDataContextNavigatedToImagePage(e,MyCanvas,ShowArea);
        }

        #region PointerEvents
        private void MyCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint(MyCanvas);
            e.Handled = true;
        }

        private void MyCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

            PointerPoint pt = e.GetCurrentPoint(MyCanvas);

            _currentContactPt = pt.Position;
            _x1 = _previousContactPt.X;
            _y1 = _previousContactPt.Y;
            _x2 = _currentContactPt.X;
            _y2 = _currentContactPt.Y;

            if (Distance(_x1, _y1, _x2, _y2) > 2.0)
            {
                Line line = new Line()
                {
                    X1 = _x1,
                    Y1 = _y1,
                    X2 = _x2,
                    Y2 = _y2,
                    StrokeThickness = 4.0,
                    Stroke = new SolidColorBrush(_strokeColor)
                };

                _previousContactPt = _currentContactPt;

                // Draw the line on the canvas by adding the Line object as
                // a child of the Canvas object.
                MyCanvas.Children.Add(line);
            }
        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            double d = 0;
            d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }

        private void MyCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint(MyCanvas);
            _previousContactPt = pt.Position;
            PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;

            e.Handled = true;
        }

        #endregion

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        internal FeedbackAttachmentVM DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            await StatusBar.GetForCurrentView().ShowAsync();
            
            // Remove current page from history
            var pageStackEntry = Frame.BackStack.LastOrDefault(entry => entry.SourcePageType == this.GetType());
            if (pageStackEntry != null)
            {
                Frame.BackStack.Remove(pageStackEntry);
            }
        }

    }
}
