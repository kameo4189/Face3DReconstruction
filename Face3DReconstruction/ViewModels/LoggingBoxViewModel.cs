using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using Face3DReconstruction.Models;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Face3DReconstruction.ViewModels
{
    class LoggingBoxViewModel : ObservableRecipient
    {
        public LoggingBoxViewModel()
        {
            IsActive = true;
        }

        public ObservableCollection<LoggingObjectModel> Logs { get; } = new ObservableCollection<LoggingObjectModel>();

        private void AddLoggingObject(string content, MESSAGE_TYPE type)
        {
            //Logs.Add(new LoggingObjectModel { Content = content, Type = type });

            Action<LoggingObjectModel> addMethod = Logs.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, new LoggingObjectModel { Content = content, Type = type });
        }

        protected override void OnActivated()
        {
            Messenger.Register<LoggingBoxViewModel, LoggingInfoChangedMessage>(this, (r, m) =>
            {
                AddLoggingObject(m.Value.Content, m.Value.Type);
            });
        }

        public class LoggingInfoChangingObject
        {
            public string Content;
            public MESSAGE_TYPE Type;
        }

        public sealed class LoggingInfoRequestMessage : RequestMessage<LoggingInfoChangingObject>
        {
        }

        public sealed class LoggingInfoChangedMessage : ValueChangedMessage<LoggingInfoChangingObject>
        {
            public LoggingInfoChangedMessage(LoggingInfoChangingObject value) : base(value)
            {
            }
        }
    }
}
