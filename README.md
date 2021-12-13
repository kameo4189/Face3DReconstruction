# Face3DReconstruction
A WPF application for displaying 3D face mesh results which are restored from 2D images.  
This application is built following Model–view–viewmodel (MVVM) architectural pattern and use HTTP request for getting 3D mesh from server.  
Server for HTTP request is needed to started when using this application.  
Implementation for HTTP server is a python application in file *load_model_server.py* of repo [PRNet-keras](https://github.com/kameo4189/PRNet-keras).  

****

## Contents

* [Requirements](#Requirements)
* [Usage](#Usage)
* [References](#References)

## Requirements
You need to install .Net Framework 4.6.1 or above to run this application.

## Usage

### Start server
Do **Installation** part in git repo [PRNet-keras](https://github.com/kameo4189/PRNet-keras).  
Run file *load_model_server.py* for getting server url from terminal.  
For example, local url and internet url are outputed at below:  
  *Public URL: NgrokTunnel: "https://1f5e-14-161-7-170.ngrok.io" -> "http://localhost:20000"*    
<img src="https://github.com/kameo4189/PRNet-keras/blob/main/evaluation_result/run_server.PNG">

## Start application
You can build application by using Visual Studio 2019 or get from Release of this repo.  
Run file Face3DReconstruction.exe for stating application.  

### Overview
Overview GUI of application  
* Info Input: area for 2 texboxes on the top is used for inputting folder path and server url
* Image List: area for list view on the center right is used for displaying list of images
* Logging List: area for list view on the bottom right is used for displaying errors and status log
* Tab View: area for tab view on the center is used for displaying images and result meshs
* Mesh List: area for list view on the bottom center is used for displaying list of result meshs
* Evaluation List: area for list view on the bottom right is used for displaying list of evaluation results
<img src="Usage/GUI.PNG">

