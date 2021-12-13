using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

using SharpDX;

namespace Face3DReconstruction.Models
{
    class MeshEvalModel : ObservableObject
    {
        private int _no;
        public int No
        {
            get => _no;
            set => SetProperty(ref _no, value);
        }

        private BoundingBox _boundingBox;
        public BoundingBox BoundingBox
        {
            get => _boundingBox;
            set
            {
                SetProperty(ref _boundingBox, value);
                OnPropertyChanged("MeshPosition");
            }
        }

        private List<EvalValue> _evalResult;
        public List<EvalValue> EvalResult
        {
            get => _evalResult;
            set => SetProperty(ref _evalResult, value);
        }

        public string MeshPosition
        {
            get
            {
                return (int)BoundingBox.Minimum.X + "_" +
                    (int)BoundingBox.Minimum.Y + "_" +
                    (int)BoundingBox.Minimum.Z + "_" +
                    (int)BoundingBox.Maximum.X + "_" +
                    (int)BoundingBox.Maximum.Y + "_" +
                    (int)BoundingBox.Maximum.Z;
            }
        }
    }
}
