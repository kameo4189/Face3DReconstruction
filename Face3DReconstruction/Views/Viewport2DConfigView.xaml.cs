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

namespace Face3DReconstruction.Views
{
    /// <summary>
    /// Interaction logic for Viewport2DConfigView.xaml
    /// </summary>
    public partial class Viewport2DConfigView : UserControl
    {
        public Viewport2DConfigView()
        {
            InitializeComponent();
        }

        //public static readonly DependencyProperty MaskConfigVisibilityProperty =
        //    DependencyProperty.Register("MaskConfigVisibility", typeof(Visibility), typeof(Viewport2DConfigView));

        //public Visibility MaskConfigVisibility
        //{
        //    get
        //    {
        //        return (Visibility)GetValue(MaskConfigVisibilityProperty);
        //    }
        //    set
        //    {
        //        SetValue(MaskConfigVisibilityProperty, value);
        //    }
        //}
    }
}
