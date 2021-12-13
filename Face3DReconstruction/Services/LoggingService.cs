using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Face3DReconstruction.Models;
using Face3DReconstruction.ViewModels;

using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Face3DReconstruction.Services
{
    class LoggingService : ILoggingService
    {
        public void SendLoggingMessage(string content, MESSAGE_TYPE type)
        {
            WeakReferenceMessenger.Default.Send(new LoggingBoxViewModel.LoggingInfoChangedMessage(
                new LoggingBoxViewModel.LoggingInfoChangingObject()
                {
                    Content = content,
                    Type = type
                }));
        }

        public void SendLoggingException(string formatContent, Exception ex)
        {
            string exMessage = ex.Message;
            if (ex.InnerException != null)
            {
                exMessage = ex.InnerException.Message;
            }
            string content = string.Format(formatContent, exMessage);

            WeakReferenceMessenger.Default.Send(new LoggingBoxViewModel.LoggingInfoChangedMessage(
                new LoggingBoxViewModel.LoggingInfoChangingObject()
                {
                    Content = content,
                    Type = MESSAGE_TYPE.ERROR
                }));
        }
    }
}
