using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using MatFileHandler;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using System.Net.Http;


using Face3DReconstruction.Models;

namespace Face3DReconstruction.Services
{
    class RequestService : IRequestService
    {
        private IFilesService FilesService => Ioc.Default.GetRequiredService<IFilesService>();

        public string GetDefaultServerURL()
        {
            return "http://localhost:10000";
        }

        public string GetPath(REQUEST_TYPE requestType)
        {
            switch (requestType)
            {
                case REQUEST_TYPE.EXTRACT:
                    return "/extract";
                case REQUEST_TYPE.PREDICT:
                    return "/predict";
                case REQUEST_TYPE.EVAL:
                    return "/evaluate";
            }
            return "/";
        }

        public async Task<List<List<MeshInfo>>> RequestMeshInfo(string serverURL, REQUEST_TYPE requestType,
            List<string> imagePaths, List<List<string>> matPathsList)
        {
            List<string> decodedImageStrings = imagePaths.Select(i => FilesService.ConvertFileToString(i)).ToList();
            List<List<string>> decodedMatStringsList = matPathsList.Select(ms =>
                ms.Select(m => FilesService.ConvertFileToString(m)).ToList()).ToList();

            List<ImageMatData> imageMatList = decodedImageStrings.Zip(decodedMatStringsList,
                (i, ms) => new ImageMatData { rawImage = i, rawMats = ms }).ToList();

            string url = serverURL + GetPath(requestType);
            InputData inputData = new InputData { imageMatList = imageMatList };
            string json = JsonConvert.SerializeObject(inputData);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.Timeout = Timeout.InfiniteTimeSpan;

            var httpResponse = await httpClient.PostAsync(url, data);
            httpResponse.EnsureSuccessStatusCode();

            string responseData = httpResponse.Content.ReadAsStringAsync().Result;
            OutputData outputData = JsonConvert.DeserializeObject<OutputData>(responseData);

            List<List<MeshInfo>> meshMatsList = outputData.imageMeshList.Select(im =>
            {
                if (im.faceMeshList.Count > 0)
                {
                    return im.faceMeshList.Select(fm =>
                        new MeshInfo(fm.rawMesh)).ToList();
                }
                else
                {
                    return null;
                }
            }).ToList();

            return meshMatsList;
        }

        public async Task<List<List<MeshInfoEvalValue>>> RequestMeshInfoEval(string serverURL, List<string> imagePaths,
            List<List<string>> matPathsList, List<List<MeshInfo>> meshInfosList)
        {
            List<string> decodedImageStrings = imagePaths.Select(i => FilesService.ConvertFileToString(i)).ToList();
            List<List<string>> decodedMatStringsList = matPathsList.Select(ms =>
                ms.Select(m => FilesService.ConvertFileToString(m)).ToList()).ToList();
            List<List<string>> aiMatStringsList = meshInfosList.Select(ms =>
                ms.Select(m => m.RawMatFile).ToList()).ToList();

            string url = serverURL + GetPath(REQUEST_TYPE.EVAL);
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";

            List<ImageMatEvalData> imageMatList = decodedImageStrings.Zip(decodedMatStringsList.Zip(aiMatStringsList, (ms, ams) => (ms, ams)),
                (i, ms_ams) => new ImageMatEvalData { rawImage = i, rawOrgMats = ms_ams.ms, rawPredictMats = ms_ams.ams }).ToList();

            InputEvalData inputData = new InputEvalData { imageMatList = imageMatList };
            string json = JsonConvert.SerializeObject(inputData);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.Timeout = Timeout.InfiniteTimeSpan;

            var httpResponse = await httpClient.PostAsync(url, data);
            httpResponse.EnsureSuccessStatusCode();

            string responseData = httpResponse.Content.ReadAsStringAsync().Result;
            OutputEvalData outputData = JsonConvert.DeserializeObject<OutputEvalData>(responseData);

            List<List<MeshInfoEvalValue>> meshEvalValuesList = new List<List<MeshInfoEvalValue>>();
            List<ImageEvalResult> evalResultList = outputData.evaluationDataList;
            List<string> evaluationTypes = outputData.evaluationTypes;

            foreach (var evalResult in evalResultList)
            {
                List<MeshInfoEvalValue> meshEvalValues = new List<MeshInfoEvalValue>();
                foreach (var meshEvalResult in evalResult.imageEvalValues)
                {
                    meshEvalValues.Add(new MeshInfoEvalValue { EvalValues = new List<EvalValue>() });
                    for (int idx = 0; idx < evaluationTypes.Count; idx++)
                    {
                        meshEvalValues.Last().EvalValues.Add(new EvalValue { Type = evaluationTypes[idx], Value = meshEvalResult.evalValues[idx] });
                    }
                }
                meshEvalValuesList.Add(meshEvalValues);
            }
            return meshEvalValuesList;
        }
    }
}
