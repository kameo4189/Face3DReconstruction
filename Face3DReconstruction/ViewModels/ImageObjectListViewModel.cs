using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Face3DReconstruction.Services;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

using Face3DReconstruction.Models;
using Face3DReconstruction.Common;

using HelixToolkit.Wpf.SharpDX;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;
using System.Drawing;
using System.Drawing.Imaging;


namespace Face3DReconstruction.ViewModels
{
    class ImageObjectListViewModel : ObservableRecipient
    {
        private IFilesService FilesService => Ioc.Default.GetRequiredService<IFilesService>();
        private IRequestService RequestService => Ioc.Default.GetRequiredService<IRequestService>();
        private ILoggingService LoggingService => Ioc.Default.GetRequiredService<ILoggingService>();

        public ImageObjectListViewModel()
        {
            IsActive = true;
            SelectItemsCommand = new RelayCommand<object>(SelectItems);
            RequestOriginalMeshCommand = new AsyncRelayCommand<ImageObjectModel>(RequestOriginalMesh, CanRequestOriginalMesh);
            RequestAIMeshCommand = new AsyncRelayCommand<ImageObjectModel>(RequestAIMesh, CanRequestAIMesh);
            RequestEvalResultCommand = new AsyncRelayCommand<ImageObjectModel>(RequestEvalResult, CanRequestEvalResult);
        }

        private TaskNotifier<ObservableCollection<ImageObjectModel>> _imageObjectListTask;
        public Task<ObservableCollection<ImageObjectModel>> ImageObjectListTask
        {
            get => _imageObjectListTask;
            set
            {
                SetPropertyAndNotifyOnCompletion(ref _imageObjectListTask, value);
            }
        }

        public List<ImageObjectModel> SelectedItems;
        private void SelectItems(object selectedItems)
        {
            System.Collections.IList items = (System.Collections.IList)selectedItems;
            SelectedItems = items.Cast<ImageObjectModel>().ToList();
        }

        public IRelayCommand SelectItemsCommand { get; set; }

        public IAsyncRelayCommand AssignImageObjectListCommand { get; set; }
        public IAsyncRelayCommand RequestOriginalMeshCommand { get; set; }
        public IAsyncRelayCommand RequestAIMeshCommand { get; }
        public IAsyncRelayCommand RequestEvalResultCommand { get; }        

        public ImageObjectModel _selectedItem;
        public ImageObjectModel SelectedItem
        {
            get => _selectedItem;
            set 
            {
                if (_selectedItem == value)
                {
                    return;
                }
                SetProperty(ref _selectedItem, value);
                if (SelectedItem == null)
                {
                    return;
                }
                foreach (var type in SelectedItem.UpdateMaskTypes)
                {
                    UpdateMaskLayers(SelectedItem, type);
                }             
                SelectedItem.UpdateMaskTypes.Clear();
                SendImageInfoMessage();
            }
        }

        private string _folderPath = "";
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath == value)
                {
                    return;
                }
                SetProperty(ref _folderPath, value);
                Task.Run(SetImageObjectList);
            }
        }

        private string _serverURL = "";
        public string ServerURL
        {
            get => _serverURL;
        }

        private int _selectedMeshIndex;
        public int SelectedMeshIndex
        {
            get => _selectedMeshIndex;
            set
            {
                if (SelectedMeshIndex == value)
                {
                    return;
                }
                SetProperty(ref _selectedMeshIndex, value);
                SelectedItem.MeshInfoDict[MESH_TYPE.AI].MaskLayerDict[POS_TYPE.SELECTING].ForEach(x => x.Visibility = false);
                if (SelectedMeshIndex >= 0 && 
                    SelectedMeshIndex < SelectedItem.MeshInfoDict[MESH_TYPE.AI].MaskLayerDict[POS_TYPE.SELECTING].Count)
                {
                    SelectedItem.MeshInfoDict[MESH_TYPE.AI].MaskLayerDict[POS_TYPE.SELECTING][SelectedMeshIndex].Visibility = true;
                }
            }
        }

        private bool _isEnabledDataGrid;
        public bool IsEnabledDataGrid
        {
            get => _isEnabledDataGrid;
            set => SetProperty(ref _isEnabledDataGrid, value);
        }

        private bool _progressBarVisibility;
        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty(ref _progressBarVisibility, value);
        }

        private int _loadingFilesPercentage;
        public int LoadingFilesPercentage
        {
            get => _loadingFilesPercentage;
            set => SetProperty(ref _loadingFilesPercentage, value);
        }

        protected override void OnActivated()
        {
            Messenger.Register<ImageObjectListViewModel, InformationInputViewModel.InputInfoChangedMessage>(this, (r, m) =>
                {
                    r.FolderPath = m.Value.FolderPath;
                }
            );

            Messenger.Register<ImageObjectListViewModel, ImageInfoRequestMessage>(this, (r, m) => m.Reply(
                new ImageInfoChangingObject()
                {
                    BitmapImage = r.SelectedItem.Bitmap,
                    MeshInfoDict = r.SelectedItem.MeshInfoDict
                }
            ));

            Messenger.Register<ImageObjectListViewModel, ProcessInfoRequestMessage>(this, (r, m) => m.Reply(
                new ProcessInfoChangingObject()
                {
                    IsProcessing = r.ImageObjectListTask != null && r.ImageObjectListTask.Status == TaskStatus.Running
                }
            ));

            Messenger.Register<ImageObjectListViewModel, Viewport2DConfigViewModel.MaskConfigInfoChangedMessage>(this, (r, m) =>
            {
                UpdateMaskLayers(SelectedItem, m.Value.MaskType);
                SendImageInfoMessage();
                ImageObjectListTask.Result.ToList().ForEach(item => item.UpdateMaskTypes.Add(m.Value.MaskType));
                SelectedItem.UpdateMaskTypes.Clear();
            });

            Messenger.Register<ImageObjectListViewModel, MeshEvaluationListViewModel.SelectedMeshInfoChangedMessage>(this, (r, m) =>
            {
                r.SelectedMeshIndex = m.Value.SelectedIndex;
            });
        }

        private string RequestInputInfoServerURL()
        {
            return Messenger.Send<InformationInputViewModel.InputInfoRequestMessage>().Response.ServerURL;
        }

        private void SendImageInfoMessage()
        {
            if (SelectedItem != null)
            {
                Messenger.Send(new ImageInfoChangedMessage(new ImageInfoChangingObject()
                {
                    BitmapImage = SelectedItem.Bitmap,
                    MeshInfoDict = SelectedItem.MeshInfoDict
                }));
            }
        }

        private void SendProcessInfoMessage(bool isProcessing)
        {
            Messenger.Send(new ProcessInfoChangedMessage(new ProcessInfoChangingObject()
            {
                IsProcessing = isProcessing
            }));
        }

        private async Task SetImageObjectList()
        {
            LoggingService.SendLoggingMessage("Loading images from input folder...", MESSAGE_TYPE.INFO);

            try 
            {
                SendProcessInfoMessage(true);
                IsEnabledDataGrid = false;
                ProgressBarVisibility = true;
                List<ImageObjectModel> imageObjectList = await CreateImageObjectList();
                ImageObjectListTask = Task.Run(() => new ObservableCollection<ImageObjectModel>(imageObjectList));
                ProgressBarVisibility = false;
                IsEnabledDataGrid = true;
                SendProcessInfoMessage(false);
            }
            catch (Exception ex)
            {
                LoggingService.SendLoggingException("Error occured when loading images: '{0}'.", ex);
            }

            LoggingService.SendLoggingMessage("Finish loading images from input folder.", MESSAGE_TYPE.INFO);
        }

        private async Task<List<ImageObjectModel>> CreateImageObjectList()
        {
            LoadingFilesPercentage = 0;
            List<string> filePaths = FilesService.GetImagePaths(FolderPath);
            ImageObjectModel[] ImageObjectArray = new ImageObjectModel[filePaths.Count];
            for (int i = 0; i < filePaths.Count; i++)
            {
                string fileName = FilesService.GetFileName(filePaths[i]);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePaths[i]);
                ImageObjectArray[i] = new ImageObjectModel
                {
                    No = i + 1,
                    FileName = fileName,
                    FilePath = filePaths[i],
                    Bitmap = bitmap,
                    MeshInfoDict = new Dictionary<MESH_TYPE, MeshInfoModel>
                    {
                        { MESH_TYPE.ORIGINAL, new MeshInfoModel() },
                        { MESH_TYPE.AI, new MeshInfoModel() },
                    },
                    ProcessStatusEval = PROCESS_STATUS.NONE,
                    MatPaths = new List<string>{ FilesService.GetMatPath(filePaths[i]) },
                };
                bitmap.EndInit();
                bitmap.Freeze();

                LoadingFilesPercentage = (int)((i + 1) * 1.0 / filePaths.Count * 100);
            }
            return await Task.Run(() => ImageObjectArray.ToList());
        }

        private async Task RequestMesh(List<ImageObjectModel> items, MESH_TYPE meshType, REQUEST_TYPE requestType)
        {
            try
            {
                _serverURL = RequestInputInfoServerURL();

                items.ForEach(i => i.MeshInfoDict[meshType].ProcessMeshStatus = PROCESS_STATUS.PROCESSING);
                List<string> imagePaths = items.Select(i => i.FilePath).ToList();
                List<List<string>> matPathsList = items.Select(i => i.MatPaths).ToList();

                List<List<MeshInfo>> meshInfosList = await RequestService.RequestMeshInfo(_serverURL, requestType,
                    imagePaths, matPathsList);

                items.Zip(meshInfosList, (i, mis) => (i, mis)).ToList().ForEach(i_mis =>
                {
                    i_mis.i.MeshInfoDict[meshType].MeshInfos = i_mis.mis;
                    if (i_mis.i.MeshInfoDict[meshType].MeshInfos == null)
                    {
                        i_mis.i.MeshInfoDict[meshType].ProcessMeshStatus = PROCESS_STATUS.NONE;
                    }
                    else
                    {
                        i_mis.i.MeshInfoDict[meshType].ProcessMeshStatus = PROCESS_STATUS.FINISH;
                        CreateMaskLayers(i_mis.i, meshType);
                    }

                });
                SendImageInfoMessage();
            }
            finally
            {
                items.FindAll(i => i.MeshInfoDict[meshType].MeshInfos == null).ToList().ForEach(i =>
                    i.MeshInfoDict[meshType].ProcessMeshStatus = PROCESS_STATUS.NONE);
                items.FindAll(i => i.MeshInfoDict[meshType].MeshInfos != null).ToList().ForEach(i =>
                    i.MeshInfoDict[meshType].ProcessMeshStatus = PROCESS_STATUS.FINISH);
            }
        }

        private async Task RequestOriginalMesh(ImageObjectModel item)
        {
            LoggingService.SendLoggingMessage("Requesting original mesh...", MESSAGE_TYPE.INFO);

            try
            {
                List<ImageObjectModel> validItems = new List<ImageObjectModel> { item };
                if (SelectedItems != null && SelectedItems.Count > 0)
                {
                    validItems = SelectedItems.FindAll(CanRequestOriginalMesh);
                }
                await RequestMesh(validItems, MESH_TYPE.ORIGINAL, REQUEST_TYPE.EXTRACT);
            }
            catch (Exception ex)
            {
                LoggingService.SendLoggingException("Error occured when requesting original mesh: '{0}'.", ex);
            }

            LoggingService.SendLoggingMessage("Finish requesting original mesh.", MESSAGE_TYPE.INFO);
        }

        private async Task RequestAIMesh(ImageObjectModel item)
        {
            LoggingService.SendLoggingMessage("Requesting AI mesh...", MESSAGE_TYPE.INFO);

            try
            {
                List<ImageObjectModel> validItems = new List<ImageObjectModel> { item };
                if (SelectedItems != null && SelectedItems.Count > 0)
                {
                    validItems = SelectedItems.FindAll(CanRequestAIMesh);
                }
                await RequestMesh(validItems, MESH_TYPE.AI, REQUEST_TYPE.PREDICT);
            }
            catch (Exception ex)
            {
                LoggingService.SendLoggingException("Error occured when requesting AI mesh: '{0}'.", ex);
            }

            LoggingService.SendLoggingMessage("Finish requesting AI mesh.", MESSAGE_TYPE.INFO);
        }

        private async Task RequestEvalResult(ImageObjectModel item)
        {
            LoggingService.SendLoggingMessage("Requesting evaluation result...", MESSAGE_TYPE.INFO);

            List<ImageObjectModel> validItems = new List<ImageObjectModel> { item };

            try
            {
                _serverURL = RequestInputInfoServerURL();               
                if (SelectedItems != null && SelectedItems.Count > 0)
                {
                    validItems = SelectedItems.FindAll(CanRequestEvalResult);
                }

                validItems.ForEach(i => i.ProcessStatusEval = PROCESS_STATUS.PROCESSING);

                List<ImageObjectModel> nullAIMeshItems = validItems.FindAll(i => i.MeshInfoDict[MESH_TYPE.AI].MeshInfos == null).ToList();
                if (nullAIMeshItems.Count > 0)
                {
                    await RequestMesh(nullAIMeshItems, MESH_TYPE.AI, REQUEST_TYPE.PREDICT);
                }

                validItems = validItems.FindAll(i => i.MeshInfoDict[MESH_TYPE.AI].MeshInfos != null);
                if (validItems.Count == 0)
                {
                    return;
                }

                List<string> imagePaths = validItems.Select(i => i.FilePath).ToList();
                List<List<string>> matPathsList = validItems.Select(i => i.MatPaths).ToList();
                List<List<MeshInfo>> meshInfosList = validItems.Select(i => i.MeshInfoDict[MESH_TYPE.AI].MeshInfos).ToList();

                List<List<MeshInfoEvalValue>> evalValuesList = await RequestService.RequestMeshInfoEval(_serverURL, imagePaths,
                    matPathsList, meshInfosList);

                validItems.Zip(evalValuesList, (i, evs) => (i, evs)).ToList().ForEach(i_evs =>
                {
                    i_evs.i.MeshInfoDict[MESH_TYPE.AI].EvalValues = i_evs.evs;
                    if (i_evs.i.MeshInfoDict[MESH_TYPE.AI].EvalValues == null)
                    {
                        i_evs.i.ProcessStatusEval = PROCESS_STATUS.NONE;
                    }
                    else
                    {
                        i_evs.i.ProcessStatusEval = PROCESS_STATUS.FINISH;
                    }
                });
                SendImageInfoMessage();
            }
            catch (Exception ex)
            {
                LoggingService.SendLoggingException("Error occured when requesting evaluation result: '{0}'.", ex);
            }
            finally
            {
                validItems.FindAll(i => i.MeshInfoDict[MESH_TYPE.AI].EvalValues == null).ToList().ForEach(i =>
                    i.ProcessStatusEval = PROCESS_STATUS.NONE);
                validItems.FindAll(i => i.MeshInfoDict[MESH_TYPE.AI].EvalValues != null).ToList().ForEach(i =>
                    i.ProcessStatusEval = PROCESS_STATUS.FINISH);
            }

            LoggingService.SendLoggingMessage("Finish requesting evaluation result.", MESSAGE_TYPE.INFO);

        }

        private bool CanRequestOriginalMesh(ImageObjectModel item)
        {
            if (item == null)
            {
                return false;
            }

            bool matExist = item.MatPaths.Any(m => FilesService.ExistPath(m));
            if (matExist == false)
            {
                return false;
            }
            return item.MeshInfoDict[MESH_TYPE.ORIGINAL].ProcessMeshStatus != PROCESS_STATUS.PROCESSING;
        }

        private bool CanRequestAIMesh(ImageObjectModel item)
        {
            if (item == null)
            {
                return false;
            }
            return item.MeshInfoDict[MESH_TYPE.AI].ProcessMeshStatus != PROCESS_STATUS.PROCESSING;
        }

        private bool CanRequestEvalResult(ImageObjectModel item)
        {
            if (item == null)
            {
                return false;
            }
            bool matExist = item.MatPaths.Any(m => FilesService.ExistPath(m));
            if (matExist == false)
            {
                return false;
            }
            return item.ProcessStatusEval != PROCESS_STATUS.PROCESSING;
        }

        private List<Point> Get2DPointList(Vector3Collection positionCollection, int height)
        {
            List<Point> points = new List<Point>();
            foreach (var pos in positionCollection)
            {
                points.Add(new Point((int)Math.Round(pos.X), (int)Math.Round(height - pos.Y - 1)));
            }
            return points;
        }

        private Viewport2DMaskModel CreateMaskLayer(POS_TYPE maskType, BitmapImage bitmapImage, Vector3Collection positionCollection)
        {
            Viewport2DMaskModel maskLayer = new Viewport2DMaskModel()
            {
                BitmapImage = bitmapImage,
                Config = Viewport2DConfigViewModel.MaskConfigList[maskType],
                Points = Get2DPointList(positionCollection, bitmapImage.PixelHeight)
            };
            UpdateMaskLayerImage(maskLayer);
            return maskLayer;
        }

        private void CreateMaskLayers(ImageObjectModel item, MESH_TYPE meshType)
        {
            MeshInfoModel meshInfoModel = item.MeshInfoDict[meshType];
            foreach (var type in Viewport2DConfigViewModel.MaskConfigList.Keys)
            {
                meshInfoModel.MaskLayerDict[type].Clear();
            }
            if (meshInfoModel.MeshInfos != null)
            {
                for (int meshIdx = 0; meshIdx < meshInfoModel.MeshInfos.Count; meshIdx++)
                {
                    var meshInfo = meshInfoModel.MeshInfos[meshIdx];
                    foreach (var type in Viewport2DConfigViewModel.MaskConfigList.Keys)
                    {
                        Vector3Collection posCollection = meshInfo.GetPositionCollection(type);
                        meshInfoModel.MaskLayerDict[type].Add(CreateMaskLayer(type, item.Bitmap, posCollection));
                    }
                }
            }
        }

        private void UpdateMaskLayers(ImageObjectModel item, POS_TYPE maskType)
        {
            foreach (var mesh in item.MeshInfoDict)
            {
                foreach (var layer in mesh.Value.MaskLayerDict[maskType])
                {
                    UpdateMaskLayerImage(layer);
                }
            }
        }

        private void UpdateMaskLayerImage(Viewport2DMaskModel maskLayer)
        {
            int w = maskLayer.BitmapImage.PixelWidth;
            int h = maskLayer.BitmapImage.PixelHeight;
            float pointSize = maskLayer.Config.PointSize;
            Color color = Color.FromArgb(maskLayer.Config.Color.A, maskLayer.Config.Color.R,
                maskLayer.Config.Color.G, maskLayer.Config.Color.B);

            double[] xVals = new double[maskLayer.Points.Count];
            double[] yVals = new double[maskLayer.Points.Count];

            for (int i = 0; i < maskLayer.Points.Count; i++)
            {
                xVals[i] = maskLayer.Points[i].X;
                yVals[i] = maskLayer.Points[i].Y;
            }

            Bitmap bitmap = new Bitmap(w, h);
            using (var gfx = Graphics.FromImage(bitmap))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                float halfSize = pointSize / 2;
                foreach (var point in maskLayer.Points)
                {
                    gfx.FillEllipse(new SolidBrush(color), (float)point.X - halfSize,
                        (float)point.Y - halfSize, pointSize, pointSize);
                }
            }

            BitmapImage maskImage = new BitmapImage();
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                maskImage.BeginInit();
                maskImage.StreamSource = memory;
                maskImage.CacheOption = BitmapCacheOption.OnLoad;
                maskImage.EndInit();
                maskImage.Freeze();
            }
            maskLayer.MaskImage = maskImage;
        }

        public class ImageInfoChangingObject
        {
            public BitmapImage BitmapImage;
            public Dictionary<MESH_TYPE, MeshInfoModel> MeshInfoDict;
        }

        public sealed class ImageInfoRequestMessage : RequestMessage<ImageInfoChangingObject>
        {
        }

        public sealed class ImageInfoChangedMessage : ValueChangedMessage<ImageInfoChangingObject>
        {
            public ImageInfoChangedMessage(ImageInfoChangingObject value) : base(value)
            {
            }
        }

        public class ProcessInfoChangingObject
        {
            public bool IsProcessing;
        }

        public sealed class ProcessInfoRequestMessage : RequestMessage<ProcessInfoChangingObject>
        {
        }

        public sealed class ProcessInfoChangedMessage : ValueChangedMessage<ProcessInfoChangingObject>
        {
            public ProcessInfoChangedMessage(ProcessInfoChangingObject value) : base(value)
            {
            }
        }
    }
}
