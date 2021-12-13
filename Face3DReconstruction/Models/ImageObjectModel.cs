using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

using Face3DReconstruction.Common;

namespace Face3DReconstruction.Models
{
    public enum PROCESS_STATUS
    {
        NONE,
        PROCESSING,
        FINISH
    }

    class MeshInfoModel : ObservableObject
    {
        public MeshInfoModel()
        {
            MeshInfos = null;
            ProcessMeshStatus = PROCESS_STATUS.NONE;
            var posTypes = Enum.GetValues(typeof(POS_TYPE)).Cast<POS_TYPE>();
            foreach (var type in posTypes)
            {
                MaskLayerDict.Add(type, new List<Viewport2DMaskModel>());
            }
        }

        private List<MeshInfo> _meshInfos;
        public List<MeshInfo> MeshInfos
        {
            get => _meshInfos;
            set
            {
                SetProperty(ref _meshInfos, value);
            }
        }

        private PROCESS_STATUS _processMeshStatus;
        public PROCESS_STATUS ProcessMeshStatus
        {
            get => _processMeshStatus;
            set
            {
                SetProperty(ref _processMeshStatus, value);
            }
        }

        private SortedDictionary<POS_TYPE, List<Viewport2DMaskModel>> _maskLayerDict = new SortedDictionary<POS_TYPE, List<Viewport2DMaskModel>>();
        public SortedDictionary<POS_TYPE, List<Viewport2DMaskModel>> MaskLayerDict
        {
            get => _maskLayerDict;
        }

        public List<Viewport2DMaskModel> MaskLayers
        {
            get
            {
                return MaskLayerDict.Values.SelectMany(x => x).ToList();
            }
        }

        public List<MeshInfoEvalValue> _evalValues;
        public List<MeshInfoEvalValue> EvalValues
        {
            get => _evalValues;
            set => SetProperty(ref _evalValues, value);
        }
    }

    class ImageObjectModel : ObservableObject
    {
        private int _no;
        public int No
        {
            get => _no;
            set => SetProperty(ref _no, value);
        }

        public string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string _filePath;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public Dictionary<MESH_TYPE, MeshInfoModel> _meshInfoDict;
        public Dictionary<MESH_TYPE, MeshInfoModel> MeshInfoDict
        {
            get => _meshInfoDict;
            set => SetProperty(ref _meshInfoDict, value);
        }

        public BitmapImage _bitmap;
        public BitmapImage Bitmap
        {
            get => _bitmap;
            set => SetProperty(ref _bitmap, value);
        }

        public ImageObjectModel ThisItem
        {
            get => this;
        }

        public PROCESS_STATUS _processStatusEval;
        public PROCESS_STATUS ProcessStatusEval
        {
            get => _processStatusEval;
            set => SetProperty(ref _processStatusEval, value);
        }

        public List<POS_TYPE> UpdateMaskTypes { get; } = new List<POS_TYPE>();
        public List<string> MatPaths { get; set; }
    }
}
