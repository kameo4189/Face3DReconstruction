using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

using Face3DReconstruction.Common;

namespace Face3DReconstruction.Models
{
    class Viewport2DMaskConfigModel : ObservableObject
    {
        public const int MASK_TYPE_NUM = 2;

        public static Dictionary<POS_TYPE, Viewport2DMaskConfigModel> DefaultConfigs = new Dictionary<POS_TYPE, Viewport2DMaskConfigModel>
        {
            { POS_TYPE.FACE, new Viewport2DMaskConfigModel() { MaskType = POS_TYPE.FACE,
                Color = Colors.Blue,
                Name = "Face Mask",
                Opacity = 0.5f,
                PointSize = 3,
                ManualChangeVisibility = true,
                Visibility = true
            }},
            { POS_TYPE.KPT_POINT, new Viewport2DMaskConfigModel() { MaskType = POS_TYPE.KPT_POINT,
                Color = Colors.Red,
                Name = "Keypoint Mask",
                Opacity = 0.5f,
                PointSize = 3,
                ManualChangeVisibility = true,
                Visibility = true
            }},
            { POS_TYPE.SELECTING, new Viewport2DMaskConfigModel() { MaskType = POS_TYPE.SELECTING,
                Color = Colors.Green,
                Name = "Selecting Mask",
                Opacity = 0.5f,
                PointSize = 3,
                ManualChangeVisibility = false,
                Visibility = false
            }},
        };

        private POS_TYPE _maskType;
        public POS_TYPE MaskType
        {
            get => _maskType;
            set => SetProperty(ref _maskType, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private Color _color;
        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        private float _pointSize;
        public float PointSize
        {
            get => _pointSize;
            set => _pointSize = value;
        }

        private bool _visibility;
        public bool Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        private bool _manualChangeVisibility;
        public bool ManualChangeVisibility
        {
            get => _manualChangeVisibility;
            set => SetProperty(ref _manualChangeVisibility, value);
        }

        private float _opacity;
        public float Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }
    }
}
