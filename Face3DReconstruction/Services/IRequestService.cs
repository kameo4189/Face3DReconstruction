using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Face3DReconstruction.Models;

namespace Face3DReconstruction.Services
{
    public enum REQUEST_TYPE
    {
        EXTRACT,
        PREDICT,
        EVAL
    }

    interface IRequestService
    {
        Task<List<List<MeshInfo>>> RequestMeshInfo(string serverURL, REQUEST_TYPE requestType, 
            List<string> imagePaths, List<List<string>> matPathsList);
        Task<List<List<MeshInfoEvalValue>>> RequestMeshInfoEval(string serverURL, List<string> imagePaths,
            List<List<string>> matPathsList, List<List<MeshInfo>> meshInfosList);
        string GetDefaultServerURL();
    }
}
