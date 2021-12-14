using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using Color = System.Windows.Media.Color;

namespace Face3DReconstruction.Models
{
    class Viewport3DXModel : MeshGeometryModel3D
    {
        public Viewport3DXModel(MeshInfo meshInfo=null) : base()
        {
            Geometry = new MeshGeometry3D();
            Material = new VertColorMaterial();
            if (meshInfo != null)
            {
                Geometry.Positions = meshInfo.PositionCollection;
                Geometry.Colors = meshInfo.ColorCollection;
                Geometry.Indices = meshInfo.TriangleIndicesCollection;
                KptIdxs = meshInfo.KptIdxs;
                KptColors = new Color[KptIdxs.Length];
                for (int idx = 0; idx < KptIdxs.Length; idx++)
                {
                    KptColors[idx] = Geometry.Colors[KptIdxs[idx]].ToColor();
                }
            }
        }

        public int[] KptIdxs
        {
            get;
            private set;
        }

        public Color[] KptColors
        {
            get;
            private set;
        }
    }
}
