using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

using Face3DReconstruction.Common;
using Face3DReconstruction.Models;

using HelixToolkit.Wpf.SharpDX;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;

using Face3DReconstruction.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace Face3DReconstruction.ViewModels
{
    class Viewport2DViewModel : ObservableRecipient
    {
        public Viewport2DViewModel()
        {
            IsActive = true;
            ConfigVisibility = false;
            ShowMaskConfigCommand = new RelayCommand(ShowMaskConfig);
            SaveImageCommand = new RelayCommand(SaveImage);
        }

        private IDialogService DialogServices => Ioc.Default.GetRequiredService<IDialogService>();
        private ILoggingService LoggingService => Ioc.Default.GetRequiredService<ILoggingService>();

        public List<Viewport2DMaskModel> _maskLayers;
        public List<Viewport2DMaskModel> MaskLayers
        {
            get => _maskLayers;
            set => SetProperty(ref _maskLayers, value);
        }

        public ICommand ShowMaskConfigCommand { get; }
        public ICommand SaveImageCommand { get; }

        private MESH_TYPE _meshType;
        public MESH_TYPE MeshType
        {
            get => _meshType;
            set => _meshType = value;
        }

        private BitmapImage _bitmapImage;
        public BitmapImage BitmapImage
        {
            get => _bitmapImage;
            set
            {
                value.Freeze();
                SetProperty(ref _bitmapImage, value);
            }
        }

        private bool _configVisibility;
        public bool ConfigVisibility
        {
            get => _configVisibility;
            set => SetProperty(ref _configVisibility, value);
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage, float opacity)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return SetImageOpacity(bitmap, opacity);
            }
        }

        private Bitmap SetImageOpacity(Bitmap image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity  
                    matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, System.Drawing.Imaging.ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Bitmap CreateImage()
        {
            Bitmap result = new Bitmap(BitmapImage.PixelWidth, BitmapImage.PixelHeight);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(BitmapImage2Bitmap(BitmapImage, 1), System.Drawing.Point.Empty);
                foreach (var layer in MaskLayers)
                {
                    if (layer.Config.Visibility)
                    {
                        g.DrawImage(BitmapImage2Bitmap(layer.MaskImage, layer.Config.Opacity), System.Drawing.Point.Empty);
                    }
                }
            }
            return result;
        }

        private void SaveImage()
        {
            LoggingService.SendLoggingMessage("Saving image...", MESSAGE_TYPE.INFO);

            try
            {
                DialogServices.SaveImageDialog(CreateImage());
            }
            catch (Exception ex)
            {
                LoggingService.SendLoggingMessage(string.Format("Error occured when saving image: {0}", ex.Message),
                    MESSAGE_TYPE.ERROR);
            }

            LoggingService.SendLoggingMessage("Finish saving image.", MESSAGE_TYPE.INFO);
            
        }

        protected override void OnActivated()
        {
            Messenger.Register<Viewport2DViewModel, ImageObjectListViewModel.ImageInfoChangedMessage>(this, (r, m) =>
            {
                r.BitmapImage = m.Value.BitmapImage;
                r.MaskLayers = m.Value.MeshInfoDict[MeshType].MaskLayers;
            });
        }

        private void ShowMaskConfig()
        {
            ConfigVisibility = !ConfigVisibility;
        }
    }
}
