using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Face3DReconstruction.Services
{
    interface IDialogService
    {
        string OpenFolderSelectionDialog();
        void SaveImageDialog(Bitmap image);
    }
}
