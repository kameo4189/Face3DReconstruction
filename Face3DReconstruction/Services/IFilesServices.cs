using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face3DReconstruction.Services
{
    interface IFilesService
    {
        string[] GetImageExts();
        List<string> GetFilePaths(string searchFolder, string[] filters, bool isRecursive = false);
        string GetFileName(string filePath);
        List<string> GetImagePaths(string searchFolder, bool isRecursive = false);
        string GetFilePathWithOtherExt(string filePath, string ext, string parentPath = null);
        string GetMatPath(string filePath);
        string ConvertFileToString(string filePath);
        bool ExistPath(string path);
    }
}
