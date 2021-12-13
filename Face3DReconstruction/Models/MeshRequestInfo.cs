using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatFileHandler;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

using Face3DReconstruction.Common;

namespace Face3DReconstruction.Models
{
    class ImageMatData
    {
        public string rawImage { get; set; }
        public List<string> rawMats { get; set; }
    }

    class InputData
    {
        public string matType { get; set; } = "dotnet";
        public List<ImageMatData> imageMatList { get; set; }
    }

    class FaceMeshData
    {
        public string rawMesh { get; set; }

        public List<int> faceBB { get; set; }
    }

    class ImageMeshData
    {
        public List<FaceMeshData> faceMeshList { get; set; }
    }

    class OutputData
    {
        public List<ImageMeshData> imageMeshList { get; set; }
    }

    class ImageMatEvalData
    {
        public string rawImage { get; set; }
        public List<string> rawOrgMats { get; set; }
        public List<string> rawPredictMats { get; set; }
    }

    class InputEvalData
    {
        public List<ImageMatEvalData> imageMatList { get; set; }
    }

    class MeshEvalResult
    {
        public List<float> evalValues { get; set; }
    }

    class ImageEvalResult
    {
        public List<MeshEvalResult> imageEvalValues { get; set; }
    }

    class OutputEvalData
    {
        public List<string> evaluationTypes { get; set; }
        public List<ImageEvalResult> evaluationDataList { get; set; }
    }

    public class MeshInfo
    {
        public string RawMatFile { get; private set; }
        public float[,] Vertices { get; private set; }
        public float[,] Colors { get; private set; }
        public int[,] Triangles { get; private set; }
        public int[] KptIdxs { get; private set; }        
        public float[,] VertexNormals { get; private set; }
        public float[] BoundingPosition { get; private set; }
        public float[] Center { get; private set; }

        public Vector3Collection PositionCollection { get; private set; } = new Vector3Collection();
        public Vector3Collection KptPositionCollection { get; private set; } = new Vector3Collection();
        public Color4Collection ColorCollection { get; private set; } = new Color4Collection();
        public IntCollection TriangleIndicesCollection { get; private set; } = new IntCollection();
        public Vector3Collection NormalCollection { get; private set; } = new Vector3Collection();
        public BoundingBox BoundingBox { get; private set; }
        public Vector3Collection GetPositionCollection(POS_TYPE posType)
        {
            Vector3Collection posCollection = null;
            switch (posType)
            {
                case POS_TYPE.FACE:
                case POS_TYPE.SELECTING:
                    posCollection = PositionCollection;
                    break;
                case POS_TYPE.KPT_POINT:
                    posCollection = KptPositionCollection;
                    break;
            }
            return posCollection;
        }

        public MeshInfo(string rawMatFile)
        {
            byte[] meshMat = Convert.FromBase64String(rawMatFile);
            var reader = new MatFileReader(new MemoryStream(meshMat));
            IMatFile matFile = reader.Read();
            RawMatFile = rawMatFile;
            Initialize(matFile);
        }

        public MeshInfo(IMatFile matFile)
        {
            Initialize(matFile);
        }

        private void Initialize(IMatFile matFile)
        {
            IArrayOf<float> matBoundingBox = matFile["bounding_box"].Value as IArrayOf<float>;
            BoundingPosition = new float[matBoundingBox.Count];
            Buffer.BlockCopy(matBoundingBox.Data, 0, BoundingPosition, 0, matBoundingBox.Count * sizeof(float));
            BoundingBox = new BoundingBox(new Vector3(BoundingPosition[0], BoundingPosition[1], BoundingPosition[2]),
                new Vector3(BoundingPosition[3], BoundingPosition[4], BoundingPosition[5]));

            IArrayOf<float> matVertices = matFile["vertices"].Value as IArrayOf<float>;
            Vertices = new float[matVertices.Dimensions[1], matVertices.Dimensions[0]];
            Buffer.BlockCopy(matVertices.Data, 0, Vertices, 0, matVertices.Count * sizeof(float));

            float maxValueZ = BoundingBox.Maximum[2];
            float minValueZ = BoundingBox.Minimum[2];
            float centerValueZ = (minValueZ + maxValueZ) / 2;
            for (int i = 0; i < Vertices.GetLength(0); i++)
            {
                Vertices[i, 2] = Vertices[i, 2] - centerValueZ;
            }
            for (int posIdx = 0; posIdx < Vertices.GetLength(0); posIdx++)
            {
                PositionCollection.Add(new Vector3(Vertices[posIdx, 0],
                    Vertices[posIdx, 1], Vertices[posIdx, 2]));
            }

            IArrayOf<float> matVertexNormals = matFile["vertex_normals"].Value as IArrayOf<float>;
            VertexNormals = new float[matVertexNormals.Dimensions[1], matVertexNormals.Dimensions[0]];
            Buffer.BlockCopy(matVertexNormals.Data, 0, VertexNormals, 0, matVertexNormals.Count * sizeof(float));
            for (int posIdx = 0; posIdx < VertexNormals.GetLength(0); posIdx++)
            {
                NormalCollection.Add(new Vector3(VertexNormals[posIdx, 0],
                    VertexNormals[posIdx, 1], VertexNormals[posIdx, 2]));
            }

            IArrayOf<float> matColors = matFile["colors"].Value as IArrayOf<float>;
            Colors = new float[matColors.Dimensions[1], matColors.Dimensions[0]];
            Buffer.BlockCopy(matColors.Data, 0, Colors, 0, matColors.Count * sizeof(float));
            for (int posIdx = 0; posIdx < Colors.GetLength(0); posIdx++)
            {
                ColorCollection.Add(new Color4(Colors[posIdx, 0],
                    Colors[posIdx, 1], Colors[posIdx, 2], 1));
            }

            IArrayOf<int> matTriangles = matFile["full_triangles"].Value as IArrayOf<int>;
            Triangles = new int[matTriangles.Dimensions[1], matTriangles.Dimensions[0]];
            Buffer.BlockCopy(matTriangles.Data, 0, Triangles, 0, matTriangles.Count * sizeof(int));
            for (int posIdx = 0; posIdx < Triangles.GetLength(0); posIdx++)
            {
                for (int triIdx = 0; triIdx < 3; triIdx++)
                {
                    TriangleIndicesCollection.Add(Triangles[posIdx, triIdx]);
                }
            }

            IArrayOf<int> matKptIdxs = matFile["kptIdxs"].Value as IArrayOf<int>;
            KptIdxs = new int[matKptIdxs.Count];
            Buffer.BlockCopy(matKptIdxs.Data, 0, KptIdxs, 0, matKptIdxs.Count * sizeof(int));

            foreach (var posIdx in KptIdxs)
            {
                KptPositionCollection.Add(PositionCollection[posIdx]);
            }
        }
    }

    public class MeshInfoEvalValue
    {
        public List<EvalValue> EvalValues { get; set; }
    }

    public class EvalValue
    {
        public string Type { get; set; }
        public float Value { get; set; }
    }
}
