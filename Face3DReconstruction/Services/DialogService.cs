using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Drawing;

namespace Face3DReconstruction.Services
{
    class DialogService : IDialogService
    {
        public string OpenFolderSelectionDialog()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            return null;
        }

        public void SaveImageDialog(Bitmap image)
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog();
            dialog.AlwaysAppendDefaultExtension = true;
            dialog.DefaultExtension = ".png";
            dialog.Filters.Add(new CommonFileDialogFilter("PNG Files", "*.png"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                image.Save(dialog.FileName);
            }
        }
    }
}
