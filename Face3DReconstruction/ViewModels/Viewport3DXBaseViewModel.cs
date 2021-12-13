using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

using Color = System.Windows.Media.Color;

namespace Face3DReconstruction.ViewModels
{
    class Viewport3DXBaseViewModel : ObservableRecipient, IDisposable
    {
        public const string Orthographic = "Orthographic Camera";

        public const string Perspective = "Perspective Camera";

        private string _cameraModel;

        private Camera _camera;

        private string _subTitle;

        private string _title;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string SubTitle
        {
            get => _subTitle;
            set => SetProperty(ref _subTitle, value);
        }

        public List<string> CameraModelCollection { get; private set; }

        public string CameraModel
        {
            get => _cameraModel;
            set
            {
                if (SetProperty(ref _cameraModel, value))
                {
                    OnCameraModelChanged();
                }
            }
        }

        public Camera Camera
        {
            get => _camera;
            protected set
            {
                SetProperty(ref _camera, value);
                CameraModel = value is PerspectiveCamera
                                       ? Perspective
                                       : value is OrthographicCamera ? Orthographic : null;
            }
        }

        private IEffectsManager _effectsManager;
        public IEffectsManager EffectsManager
        {
            get => _effectsManager;
            protected set => SetProperty(ref _effectsManager, value);
        }

        protected OrthographicCamera defaultOrthographicCamera = new OrthographicCamera { Position = new System.Windows.Media.Media3D.Point3D(0, 0, 5), LookDirection = new System.Windows.Media.Media3D.Vector3D(-0, -0, -5), UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0), NearPlaneDistance = 1, FarPlaneDistance = 100 };

        protected PerspectiveCamera defaultPerspectiveCamera = new PerspectiveCamera { Position = new System.Windows.Media.Media3D.Point3D(0, 0, 5), LookDirection = new System.Windows.Media.Media3D.Vector3D(-0, -0, -5), UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0), NearPlaneDistance = 0.5, FarPlaneDistance = 150 };

        public event EventHandler CameraModelChanged;

        protected Viewport3DXBaseViewModel()
        {
            // camera models
            CameraModelCollection = new List<string>()
            {
                Orthographic,
                Perspective,
            };

            // on camera changed callback
            CameraModelChanged += (s, e) =>
            {
                if (_cameraModel == Orthographic)
                {
                    if (!(Camera is OrthographicCamera))
                        Camera = defaultOrthographicCamera;
                }
                else if (_cameraModel == Perspective)
                {
                    if (!(Camera is PerspectiveCamera))
                        Camera = defaultPerspectiveCamera;
                }
                else
                {
                    throw new HelixToolkitException("Camera Model Error.");
                }
            };

            // default camera model
            CameraModel = Perspective;

            Title = "Demo (HelixToolkitDX)";
            SubTitle = "Default Base View Model";
        }

        protected virtual void OnCameraModelChanged()
        {
            var eh = CameraModelChanged;
            if (eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (EffectsManager != null)
                {
                    var effectManager = EffectsManager as IDisposable;
                    Disposer.RemoveAndDispose(ref effectManager);
                }
                disposedValue = true;
                GC.SuppressFinalize(this);
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Viewport3DXBaseViewModel()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
