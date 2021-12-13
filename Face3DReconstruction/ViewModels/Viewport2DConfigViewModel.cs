using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

using Face3DReconstruction.Models;
using Face3DReconstruction.Common;

namespace Face3DReconstruction.ViewModels
{
    class Viewport2DConfigViewModel : ObservableRecipient
    {
        public Viewport2DConfigViewModel()
        {
            IsActive = true;
            SelectedItem = MaskConfigList[POS_TYPE.FACE];
            SendMaskConfigCommand = new RelayCommand(SendMaskConfigInfoMessage);
        }

        public ICommand SendMaskConfigCommand { get; }

        public static Dictionary<POS_TYPE, Viewport2DMaskConfigModel> MaskConfigList { get; }
            = new Dictionary<POS_TYPE, Viewport2DMaskConfigModel>(Viewport2DMaskConfigModel.DefaultConfigs);

        private Viewport2DMaskConfigModel _selectedItem;
        public Viewport2DMaskConfigModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private Visibility _maskConfigVisibility;
        public Visibility MaskConfigVisibility
        {
            get => _maskConfigVisibility;
            set => SetProperty(ref _maskConfigVisibility, value);
        }

        protected override void OnActivated()
        {
            Messenger.Register<Viewport2DConfigViewModel, MaskConfigInfoRequestMessage>(this, (r, m) => m.Reply(
                new MaskConfigInfoChangingObject()
                {
                    MaskType = r.SelectedItem.MaskType,
                }));
        }

        public void SendMaskConfigInfoMessage()
        {
            Messenger.Send(new MaskConfigInfoChangedMessage(new MaskConfigInfoChangingObject()
            {
                MaskType = SelectedItem.MaskType,
            }));
        }

        public class MaskConfigInfoChangingObject
        {
            public POS_TYPE MaskType;
        }

        public sealed class MaskConfigInfoRequestMessage : RequestMessage<MaskConfigInfoChangingObject>
        {
        }

        public sealed class MaskConfigInfoChangedMessage : ValueChangedMessage<MaskConfigInfoChangingObject>
        {
            public MaskConfigInfoChangedMessage(MaskConfigInfoChangingObject value) : base(value)
            {
            }
        }
    }
}
