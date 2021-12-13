using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Face3DReconstruction.Models
{
    public enum MESSAGE_TYPE
    {
        INFO,
        WARNING,
        ERROR
    }

    class LoggingConfigModel : ObservableObject
    {
        public static Dictionary<MESSAGE_TYPE, LoggingConfigModel> DefaultConfigs = new Dictionary<MESSAGE_TYPE, LoggingConfigModel>
        {
            { MESSAGE_TYPE.INFO, new LoggingConfigModel() {
                TextColor = Brushes.Black,
                Prefix = "[INFO]"
            }},
            { MESSAGE_TYPE.WARNING, new LoggingConfigModel() {
                TextColor = Brushes.Orange,
                Prefix = "[WARNING]"
            }},
            { MESSAGE_TYPE.ERROR, new LoggingConfigModel() {
                TextColor = Brushes.Red,
                Prefix = "[ERROR]"
            }},
        };

        private Brush _textColor;
        public Brush TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private string _prefix;
        public string Prefix
        {
            get => _prefix;
            set => SetProperty(ref _prefix, value);
        }
    }

    class LoggingObjectModel : ObservableObject
    {
        public static Dictionary<MESSAGE_TYPE, LoggingConfigModel> LoggingConfigList { get; } 
            = new Dictionary<MESSAGE_TYPE, LoggingConfigModel>(LoggingConfigModel.DefaultConfigs);

        private string _content;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string Text
        {
            get => Config.Prefix + " " + Content;
        }

        private MESSAGE_TYPE _type;
        public MESSAGE_TYPE Type
        {
            get => _type;
            set => _type = value;
        }

        public LoggingConfigModel Config
        {
            get => LoggingConfigList[Type];
        }
    }
}
