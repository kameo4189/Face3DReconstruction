using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Face3DReconstruction.Common;

namespace Face3DReconstruction.Views
{
    /// <summary>
    /// Interaction logic for Viewport3DXView.xaml
    /// </summary>
    public partial class Viewport3DXView : UserControl
    {
        public Viewport3DXView()
        {
            InitializeComponent();
            Loaded += (us, ue) =>
            {
                Window window = Window.GetWindow(this);
                window.Closed += (s, e) =>
                {
                    if (DataContext is IDisposable)
                    {
                        (DataContext as IDisposable).Dispose();
                    }
                };
            };

            KeypointColor = Colors.Red;
        }

        public static readonly DependencyProperty MeshTypeProperty =
            DependencyProperty.Register("MeshType", typeof(MESH_TYPE), typeof(Viewport3DXView));
        public static readonly DependencyProperty KeypointColorProperty =
            DependencyProperty.Register("KeypointColor", typeof(Color), typeof(Viewport3DXView));
        public static readonly DependencyProperty IsShowKeypointProperty =
            DependencyProperty.Register("IsShowKeypoint", typeof(bool), typeof(Viewport3DXView));

        public MESH_TYPE MeshType
        {
            get
            {
                return (MESH_TYPE)GetValue(MeshTypeProperty);
            }
            set
            {
                SetValue(MeshTypeProperty, value);
            }
        }

        public Color KeypointColor
        {
            get
            {
                return (Color)GetValue(KeypointColorProperty);
            }
            set
            {
                SetValue(KeypointColorProperty, value);
            }
        }

        public bool IsShowKeypoint
        {
            get
            {
                return (bool)GetValue(IsShowKeypointProperty);
            }
            set
            {
                SetValue(IsShowKeypointProperty, value);
            }
        }
    }
}
