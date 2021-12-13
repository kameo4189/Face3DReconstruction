using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Face3DReconstruction.Services;
using Face3DReconstruction.Models;
using Face3DReconstruction.Common;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace Face3DReconstruction.ViewModels
{
    class MeshEvaluationListViewModel : ObservableRecipient
    {
        private ILoggingService LoggingService => Ioc.Default.GetRequiredService<ILoggingService>();
        public MeshEvaluationListViewModel()
        {
            IsActive = true;
            MouseSelectCommand = new RelayCommand(MouseSelect);
        }

        public ICommand MouseSelectCommand { get; }

        public ObservableCollection<MeshEvalModel> MeshList { get; } = new ObservableCollection<MeshEvalModel>();

        private MeshInfoModel _meshInfoModel;
        public MeshInfoModel MeshInfoModel
        {
            get => _meshInfoModel;
            set
            {
                SetProperty(ref _meshInfoModel, value);
                MeshList.Clear();
                if (value != null && MeshInfoModel.MeshInfos != null)
                {
                    LoggingService.SendLoggingMessage("Setting evaluation result...", MESSAGE_TYPE.INFO);

                    try
                    {                        
                        for (int i = 0; i < MeshInfoModel.MeshInfos.Count; i++)
                        {
                            var evalValues = MeshInfoModel.EvalValues;

                            MeshList.Add(new MeshEvalModel()
                            {
                                No = i,
                                BoundingBox = MeshInfoModel.MeshInfos[i].BoundingBox,
                                EvalResult = evalValues?[i].EvalValues
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.SendLoggingMessage(string.Format("Error occured when setting evaluation result: {0}", ex.Message),
                            MESSAGE_TYPE.ERROR);
                    }

                    LoggingService.SendLoggingMessage("Finish setting evaluation result.", MESSAGE_TYPE.INFO);
                }
            }
        }

        public MeshEvalModel _selectedItem;
        public MeshEvalModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value)
                {
                    return;
                }
                SetProperty(ref _selectedItem, value);
                SendSelectedMeshInfo();
            }
        }

        private void MouseSelect()
        {
            SelectedItem = null;
        }

        protected override void OnActivated()
        {
            Messenger.Register<MeshEvaluationListViewModel, ImageObjectListViewModel.ImageInfoChangedMessage>(this, (r, m) =>
            {
                r.MeshInfoModel = m.Value.MeshInfoDict[MESH_TYPE.AI];
            });
            Messenger.Register<MeshEvaluationListViewModel, SelectedMeshInfoRequestMessage>(this, (r, m) =>
            {
                int selectedIndex = -1;
                if (SelectedItem != null)
                {
                    selectedIndex = SelectedItem.No;
                }
                m.Reply(new SelectedMeshInfoChangingObject()
                {
                    SelectedIndex = selectedIndex
                });
            });
        }

        private void SendSelectedMeshInfo()
        {
            int selectedIndex = -1;
            if (SelectedItem != null)
            {
                selectedIndex = SelectedItem.No;
            }
            Messenger.Send(new SelectedMeshInfoChangedMessage(new SelectedMeshInfoChangingObject()
            {
                SelectedIndex = selectedIndex
            }));
        }


        public class SelectedMeshInfoChangingObject
        {
            public int SelectedIndex;
        }

        public sealed class SelectedMeshInfoRequestMessage : RequestMessage<SelectedMeshInfoChangingObject>
        {
        }

        public sealed class SelectedMeshInfoChangedMessage : ValueChangedMessage<SelectedMeshInfoChangingObject>
        {
            public SelectedMeshInfoChangedMessage(SelectedMeshInfoChangingObject value) : base(value)
            {
            }
        }
    }
}
