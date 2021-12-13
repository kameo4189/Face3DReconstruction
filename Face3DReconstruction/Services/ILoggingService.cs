using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Face3DReconstruction.Models;

namespace Face3DReconstruction.Services
{
    interface ILoggingService
    {
        void SendLoggingMessage(string content, MESSAGE_TYPE type);
        void SendLoggingException(string formatContent, Exception ex);
    }
}
