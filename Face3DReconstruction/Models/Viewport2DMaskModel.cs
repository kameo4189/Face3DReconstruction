using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;
using System.Drawing;
using Point = System.Windows.Point;

namespace Face3DReconstruction.Models
{
    class Viewport2DMaskModel : ObservableObject
    {
        private BitmapImage _bitmapImage;
        public BitmapImage BitmapImage
        {
            get => _bitmapImage;
            set => SetProperty(ref _bitmapImage, value);
        }

        private BitmapImage _maskImage;
        public BitmapImage MaskImage
        {
            get => _maskImage;
            set => SetProperty(ref _maskImage, value);
        }

        public Viewport2DMaskConfigModel Config
        {
            get;
            set;
        }

        public List<Point> Points
        {
            get;
            set;
        }

        public bool IsSavedMask
        {
            get;
            set;
        } = true;

        private bool _visibility = false;
        public bool Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }
    }
}
