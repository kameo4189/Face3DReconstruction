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
    /// Interaction logic for Viewport2DView.xaml
    /// </summary>
    public partial class Viewport2DView : UserControl
    {
        public Viewport2DView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MeshTypeProperty =
            DependencyProperty.Register("MeshType", typeof(MESH_TYPE), typeof(Viewport2DView));

        public MESH_TYPE MeshType
        {
            get => (MESH_TYPE)GetValue(MeshTypeProperty);
            set => SetValue(MeshTypeProperty, value);
        }
    }
}
