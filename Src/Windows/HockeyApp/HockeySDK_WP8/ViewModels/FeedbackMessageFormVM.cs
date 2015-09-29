using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HockeyApp.Tools;
using Microsoft.Phone.Tasks;
using HockeyApp.Model;
using System.IO;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using HockeyApp.Views;

namespace HockeyApp.ViewModels
{
    public class FeedbackMessageFormVM: VMBase
    {

        #region fields

        FeedbackMessageFormControl view;
        bool isLightTheme;
        Style darkTextBoxStyle;
        Style lightTextBoxStyle;

        #endregion

        #region ctor

        public FeedbackMessageFormVM(FeedbackPageVM parentVM, FeedbackMessageFormControl view)
        {
            this.ParentVM = parentVM;
            this.view = view;
            isLightTheme = Visibility.Visible.Equals(Application.Current.Resources["PhoneLightThemeVisibility"]);
            SetFormFieldDefaults();
            lightTextBoxStyle = view.Resources["inputDark"] as Style;
            darkTextBoxStyle = view.Resources["inputLight"] as Style;
        }
        internal void SetFormFieldDefaults()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var persistedValues = FeedbackManager.Instance.ThreadMetaInfos;
                this.Email = persistedValues.Email;
                this.Username = persistedValues.Username;
                this.Subject = persistedValues.Subject;
            });
        }

        #endregion

        #region Properties

        public Style TextBoxStyle
        {
            get
            {
                return isLightTheme ? lightTextBoxStyle : darkTextBoxStyle;
            }
        }
        protected FeedbackPageVM ParentVM { get; set; }

        public bool IsThreadActive {get { return this.ParentVM.IsThreadActive; }}

        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                NotifyOfPropertyChange("Email");
            }
        }

        private string subject;
        public string Subject
        {
            get { return subject; }
            set
            {
                subject = value;
                NotifyOfPropertyChange("Subject");
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                NotifyOfPropertyChange("Message");
            }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyOfPropertyChange("Username");
            }
        }

        private ObservableCollection<FeedbackImageVM> attachments = new ObservableCollection<FeedbackImageVM>();
        public ObservableCollection<FeedbackImageVM> Attachments
        {
            get { return attachments; }
        }
        public void AddAttachment(FeedbackImageVM attachment)
        {
            this.Attachments.Add(attachment);
        }

        public void RemoveAttachment(FeedbackImageVM attachment)
        {
            this.Attachments.Remove(attachment);
        }

        #endregion

        #region Commandmethods

        protected bool ValidateInput()
        {
            var errors = new List<string>();
            if (!this.Email.IsValidEmail())
            {
                errors.Add(LocalizedStrings.LocalizedResources.EmailEmptyWarning);
            }

            if (this.Subject.IsEmpty())
            {
                errors.Add(LocalizedStrings.LocalizedResources.SubjectEmptyWarning);
            }

            if (this.Username.IsEmpty())
            {
                errors.Add(LocalizedStrings.LocalizedResources.UserEmptyWarning);
            }

            if (this.Message.IsEmpty() && !this.Attachments.Any())
            {
                errors.Add(LocalizedStrings.LocalizedResources.MessageEmptyWarning);
            }

            if (errors.Any())
            {
                MessageBox.Show(errors.Aggregate((a, b) => a + "\n" + b));
                return false;
            }
            return true;
        }

        public async Task<IFeedbackMessage> SubmitForm()
        {
            if (ValidateInput())
            {
                ParentVM.ShowOverlay();

                var returnedMsg = await FeedbackManager.Instance.SendFeedback(this.Message, this.Email, this.Subject, this.Username, this.Attachments.Select(a => a.FeedbackImage));

                if (returnedMsg != null)
                {
                    this.ClearFormAfterSubmit();
                    var imgReadOnlyVM = new FeedbackMessageReadOnlyVM(returnedMsg, this.ParentVM);
                    ParentVM.Messages.Add(imgReadOnlyVM);
                    ParentVM.SwitchToMessageList();
                    return returnedMsg;
                }
                else
                {
                    ParentVM.HideOverlay();
                    MessageBox.Show(LocalizedStrings.LocalizedResources.FeedbackSendError);
                }
            }
            return null;
        }

        internal void ClearForm()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                SetFormFieldDefaults();
                this.Message = "";
                this.Attachments.Clear();
            });
        }

        private void ClearFormAfterSubmit()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ParentVM.IsThreadActive = true;
                this.Message = "";
                this.Attachments.Clear();
            });
        }

        internal async Task ShowPhotoResult(PhotoResult result)
        {
            var imageVM = new FeedbackImageVM(this.ParentVM);
            imageVM.IsEditable = true;
            byte[] imageBytes = null;
            if (Path.GetExtension(result.OriginalFileName).ToLower().EndsWith("jpg"))
            {
                BitmapImage img = new BitmapImage();
                img.SetSource(result.ChosenPhoto);
                var wbBitmap = new WriteableBitmap(img);
                using (var stream = new MemoryStream())
                {
                    wbBitmap.WritePNG(stream);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    imageBytes = new byte[stream.Length];
                    stream.Read(imageBytes, 0, (int)stream.Length);
                }
            }
            else //png
            {
                imageBytes = new byte[result.ChosenPhoto.Length];
                await result.ChosenPhoto.ReadAsync(imageBytes, 0, (int)result.ChosenPhoto.Length);
            }
            imageVM.FeedbackImage = new FeedbackAttachment(Path.GetFileName(result.OriginalFileName), imageBytes, null);
            Attachments.Add(imageVM);
            this.ParentVM.SwitchToImageEditor(imageVM);
        }
        #endregion

    }
}
