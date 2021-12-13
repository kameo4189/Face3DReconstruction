using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Face3DReconstruction.Services
{
    class FilesService : IFilesService
    {
        public string[] GetImageExts()
        {
            return new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
        }

        public List<string> GetFilePaths(String searchFolder, String[] filters, bool isRecursive=false)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound;
        }

        public List<string> GetImagePaths(String searchFolder, bool isRecursive=false)
        {
            List<String> filesFound = new List<String>();
            try
            {
                var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var imageFilters = GetImageExts();
                foreach (var filter in imageFilters)
                {
                    filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
                }
            }
            catch (Exception)
            {
                ;
            }
            return filesFound;
        }

        public string GetFilePathWithOtherExt(string filePath, string ext, string parentPath = null)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string newName = fileNameWithoutExt + ext;
            if (parentPath != null)
            {
                return Path.Combine(parentPath, newName);
            }
            return Path.Combine(Path.GetDirectoryName(filePath), newName);
        }

        public string GetFileName(String filePath)
        {
            return Path.GetFileName(filePath);
        }

        public string GetMatPath(string filePath)
        {
            return GetFilePathWithOtherExt(filePath, ".mat");
        }

        public string ConvertFileToString(string filePath)
        {
            string decodedString = "";
            if (File.Exists(filePath))
            {
                byte[] fileBytes = File.ReadAllBytes(filePath); ;
                decodedString = Convert.ToBase64String(fileBytes);
            }
            return decodedString;
        }

        public bool ExistPath(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }
    }
}
