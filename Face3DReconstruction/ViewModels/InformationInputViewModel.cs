using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Face3DReconstruction.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

using Face3DReconstruction.Models;

namespace Face3DReconstruction.ViewModels
{
    class InformationInputViewModel : ObservableRecipient
    {
        private IDialogService DialogServices => Ioc.Default.GetRequiredService<IDialogService>();
        private ISettingsService SettingService => Ioc.Default.GetRequiredService<ISettingsService>();
        private IRequestService RequestService => Ioc.Default.GetRequiredService<IRequestService>();
        private ILoggingService LoggingService => Ioc.Default.GetRequiredService<ILoggingService>();
        public InformationInputViewModel()
        {
            IsActive = true;
            SelectFolderCommand = new RelayCommand(SelectFolder);
            ControlLoadedCommand = new RelayCommand(ControlLoaded);
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand ControlLoadedCommand { get; }

        public string _folderPath;
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath == value)
                {
                    return;
                }
                SettingService.SetValue("FolderPath", value);
                SetProperty(ref _folderPath, value);

                LoggingService.SendLoggingMessage(string.Format("Input folder path is set to {0}.", FolderPath),
                    MESSAGE_TYPE.INFO);

                SendInputFolderPathMessage();
            }
        }

        public string _serverURL;
        public string ServerURL
        {
            get => _serverURL;
            set
            {
                if (_serverURL == value)
                {
                    return;
                }
                SettingService.SetValue("ServerURL", value);
                SetProperty(ref _serverURL, value);

                LoggingService.SendLoggingMessage(string.Format("Server URL is set to {0}.", ServerURL),
                    MESSAGE_TYPE.INFO);
            }
        }

        public bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private void SelectFolder()
        {
            string selectedFolder = DialogServices.OpenFolderSelectionDialog();
            if (selectedFolder != null)
            {
                FolderPath = selectedFolder;
            }
        }

        private void ControlLoaded()
        {
            FolderPath = SettingService.GetValue("FolderPath");
            ServerURL = SettingService.GetValue("ServerURL");
        }

        protected override void OnActivated()
        {
            Messenger.Register<InformationInputViewModel, InputInfoRequestMessage>(this, (r, m) =>
            {
                if (ServerURL == null || ServerURL.Trim().Length == 0)
                {
                    ServerURL = RequestService.GetDefaultServerURL();
                }
                m.Reply(
                    new InputInfoChangingObject()
                    {
                        FolderPath = r.FolderPath,
                        ServerURL = r.ServerURL
                    });
            });

            Messenger.Register<InformationInputViewModel, ImageObjectListViewModel.ProcessInfoChangedMessage>(this, (r, m) =>
            {
                r.IsEnabled = m.Value.IsProcessing == false;
            });
        }

        public void SendInputFolderPathMessage()
        {
            Messenger.Send(new InputInfoChangedMessage(new InputInfoChangingObject()
            {
                FolderPath = FolderPath,
            }));
        }

        public class InputInfoChangingObject
        {
            public string FolderPath;
            public string ServerURL;
        }

        public sealed class InputInfoRequestMessage : RequestMessage<InputInfoChangingObject>
        {
        }

        public sealed class InputInfoChangedMessage : ValueChangedMessage<InputInfoChangingObject>
        {
            public InputInfoChangedMessage(InputInfoChangingObject value) : base(value)
            {
            }
        }
    }
}
