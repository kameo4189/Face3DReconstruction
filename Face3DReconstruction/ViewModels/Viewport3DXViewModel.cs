using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Media.Imaging;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX.Model;
using Color = System.Windows.Media.Color;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Colors = System.Windows.Media.Colors;
using Transform3D = System.Windows.Media.Media3D.Transform3D;
using Media3D = System.Windows.Media.Media3D;
using SharpDX;

using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

using Face3DReconstruction.Common;
using Face3DReconstruction.Models;

namespace Face3DReconstruction.ViewModels
{
    class Viewport3DXViewModel : Viewport3DXBaseViewModel
    {
        public Viewport3DXViewModel()
        {
            IsActive = true;
            EffectsManager = new DefaultEffectsManager();

            // camera setup
            defaultPerspectiveCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 100),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 5000000,
                FieldOfView = 60
            };

            defaultOrthographicCamera = new OrthographicCamera
            {
                Position = new Point3D(0, 0, 100),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 5000000,
            };

            Camera = new PerspectiveCamera();
            defaultPerspectiveCamera.CopyTo(Camera);

            // setup lighting            
            AmbientLightColor = Colors.DimGray;
            DirectionalLightColor = Colors.White;

            // scene model3d
            GroupModels = new ObservableElement3DCollection();
            UpDirection = new Vector3D(0, 1, 0);

            // command
            ResetCameraCommand = new RelayCommand(ResetCamera);
            ShowKeypointsCommand = new RelayCommand(ShowKeypoints);
        }

        public ICommand ResetCameraCommand { get; }
        public ICommand ShowKeypointsCommand { get; }

        private MESH_TYPE _meshType;
        public MESH_TYPE MeshType
        {
            get => _meshType;
            set => _meshType = value;
        }

        private Color _keypointColor;
        public Color KeypointColor
        {
            get => _keypointColor;
            set => SetProperty(ref _keypointColor, value);
        }

        private bool _isShowKeypoint;
        public bool IsShowKeypoint
        {
            get => _isShowKeypoint;
            set => SetProperty(ref _isShowKeypoint, value);
        }

        private ObservableElement3DCollection _groupModels;
        public ObservableElement3DCollection GroupModels 
        { 
            get => _groupModels; 
            private set => SetProperty(ref _groupModels, value); 
        }

        private Transform3D _transform;
        public Transform3D Transform
        {
            get => _transform;
            set => SetProperty(ref _transform, value);
        }

        private Geometry3D _selectedGeometry;
        public Geometry3D SelectedGeometry
        {
            set => SetProperty(ref _selectedGeometry, value);
            get { return _selectedGeometry; }
        }

        public Material MainMaterial { get; }

        public Material SelectedMaterial { get; } = new PhongMaterial { 
            DiffuseColor = Color.FromArgb(128, 0, 0, 0).ToColor4(),
            EmissiveColor = Colors.Yellow.ToColor4()};

        private Color _directionalLightColor;
        public Color DirectionalLightColor
        {
            get => _directionalLightColor;
            set => SetProperty(ref _directionalLightColor, value);
        }

        private Color _ambientLightColor;
        public Color AmbientLightColor
        {
            get => _ambientLightColor;
            set => SetProperty(ref _ambientLightColor, value);
        }

        private Vector3D _upDirection;
        public Vector3D UpDirection
        {
            get => _upDirection;
            set => SetProperty(ref _upDirection, value);
        }

        private BitmapImage _bitmapImage;

        private List<MeshInfo> _meshInfos;
        public List<MeshInfo> MeshInfos
        {
            get => _meshInfos;
            set
            {
                SetProperty(ref _meshInfos, value);
                if (value != null)
                {
                    ApplyGeometry();
                }
                else
                {
                    ClearGeometry();
                }
            }
        }

        private float MaxZValue;

        protected override void OnActivated()
        {
            Messenger.Register<Viewport3DXViewModel, ImageObjectListViewModel.ImageInfoChangedMessage>(this, (r, m) =>
            {
                r._bitmapImage = m.Value.BitmapImage;
                r.MeshInfos = m.Value.MeshInfoDict[MeshType].MeshInfos;
            });

            Messenger.Register<Viewport3DXViewModel, MeshEvaluationListViewModel.SelectedMeshInfoChangedMessage>(this, (r, m) =>
            {
                r.SelectedGeometry = null;
                if (m.Value.SelectedIndex >= 0 && m.Value.SelectedIndex < GroupModels.Count)
                {
                    r.SelectedGeometry = (GroupModels[m.Value.SelectedIndex] as Viewport3DXModel)?.Geometry;
                } 
            });
        }

        private void ApplyGeometry()
        {
            ClearGeometry();
            if (MeshInfos == null)
            {
                return;
            }

            foreach (MeshInfo meshInfo in MeshInfos)
            {
                GroupModels.Add(new Viewport3DXModel(meshInfo));
                if (MaxZValue < meshInfo.BoundingBox.Maximum[2])
                {
                    MaxZValue = meshInfo.BoundingBox.Maximum[2];
                }
            }
            if (IsShowKeypoint)
            {
                ShowKeypoints();
            }            
            ResetCamera();
        }

        private void ClearGeometry()
        {
            if (GroupModels != null)
            {
                GroupModels.Clear();
            }
        }

        private void ResetCamera()
        {
            if (_bitmapImage == null)
            {
                if (Camera is PerspectiveCamera)
                {
                    defaultPerspectiveCamera.CopyTo(Camera);
                }
                else
                {
                    defaultOrthographicCamera.CopyTo(Camera);
                }
            }
            else
            {
                double zDistance = CalculateZDistance();
                Camera.Position = new Point3D(_bitmapImage.PixelWidth / 2, _bitmapImage.PixelHeight / 2, zDistance);
                Camera.LookDirection = new Vector3D(0, 0, -zDistance);
                Camera.UpDirection = new Vector3D(0, _bitmapImage.PixelHeight, 0);
                if (Camera is OrthographicCamera)
                {
                    (Camera as OrthographicCamera).Width = _bitmapImage.PixelWidth;
                }
            }
        }

        private double CalculateZDistance()
        {
            double zDistance = MaxZValue;
            if (Camera is PerspectiveCamera)
            {
                PerspectiveCamera perspectiveCamera = Camera as PerspectiveCamera;
                double fieldOfView = perspectiveCamera.FieldOfView;
                double fieldOfViewInRadians = fieldOfView * (Math.PI / 180.0);
                zDistance = 0.5 * _bitmapImage.PixelWidth / Math.Tan(0.5 * fieldOfViewInRadians);
            }
            return zDistance;
        }

        private void ShowKeypoints()
        {
            IsShowKeypoint = !IsShowKeypoint;
            if (IsShowKeypoint)
            {
                foreach (Viewport3DXModel model in GroupModels)
                {
                    foreach (int kptIdx in model.KptIdxs)
                    {
                        model.Geometry.Colors[kptIdx] = KeypointColor.ToColor4();
                    }
                    model.Geometry.UpdateColors();
                }
            }
            else
            {
                foreach (Viewport3DXModel model in GroupModels)
                {
                    for (int kptIdx = 0; kptIdx < model.KptIdxs.Length; kptIdx++)
                    {
                        model.Geometry.Colors[model.KptIdxs[kptIdx]] = model.KptColors[kptIdx].ToColor4();
                    }
                    model.Geometry.UpdateColors();
                }
            }
        }
    }
}
