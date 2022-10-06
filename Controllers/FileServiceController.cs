using System;
using System.IO;
using System.Linq;
using System.Text;
using TodoApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TodoApi.Util;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileServiceController : BaseController
    {
        private static readonly IConfiguration _configuration = Startup.StaticConfiguration!;
        private readonly string baseDirectory = _configuration.GetSection("appSettings:UploadPath").Value;
        private readonly string[] allowext = _configuration.GetSection("appSettings:AllowExtension").Get<string[]>();
        private readonly string[] allowfunction = _configuration.GetSection("appSettings:AllowFunction").Get<string[]>();
        public FileServiceController()
        {
        }

        
        [HttpGet("ProfilePhoto", Name = "ProfilePhoto")]
        public async Task<string> ProfilePhoto()  //to show current login user profile photo
        {
            int AdminID = int.Parse(GetTokenData("UserID"));
            try
            {
                string uploadDirectory = _configuration.GetSection("appSettings:AdminPhoto").Value;

                string folderPath = "";
                
                if (uploadDirectory != null)
                {
                    folderPath = baseDirectory + uploadDirectory;
                }
                else
                {
                    throw new Exception("Invalid Function Path " + uploadDirectory); 
                }

                if(folderPath.Contains(".."))  //if found .. in the file name or path
                    throw new Exception("Invalid path " + folderPath);

                // find extension by file name
                string? existingFile = Directory.EnumerateFiles(folderPath, AdminID.ToString() + ".*").FirstOrDefault();
                string extension = "";
                if (!string.IsNullOrEmpty(existingFile))
                {
                    extension = Path.GetExtension(existingFile).ToLower().TrimStart('.');
                    if (allowext.Contains(extension))
                    {
                        byte[] m_Bytes = await ReadToEndAsync(new FileStream(existingFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                        Response.ContentType = "application/json";
                        string imageBase64Data = Convert.ToBase64String(m_Bytes);
                        string imageDataURL = string.Format("data:image/" + extension + ";base64,{0}", imageBase64Data);
                        return imageDataURL;
                    }
                    else
                        throw new Exception("Invalid File Extension " + extension);
                }
                else {
                    Log.Warning("Get ProfilePhoto File Path not found: {@Folder}, {@AdminID}", folderPath, AdminID);  //it is not error, but log to see someone trying random file path or not.
                    return "";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Get ProfilePhoto fail AdminID: {@ID}", AdminID);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "ProfilePhoto File Error";
            }
        }

        [HttpGet("Download/{functionname}/{param}", Name = "Download")]
        public async Task<string> DownLoadFile(string functionname, string param)  //restrict user level permission upto function name level
        {
            
            string filename = Encryption.Decode_URLParam(param);

            if (!allowfunction.Contains(functionname)) {
                throw new Exception("Function Name Not Allow : " + functionname);  
            }

            string uploadDirectory = _configuration.GetSection("appSettings:" + functionname).Value;
            string folderPath;
            
            if (uploadDirectory != null)
                folderPath = baseDirectory + uploadDirectory;
            else {
                Log.Error("Invalid Function Path " + functionname); 
                throw new Exception("Invalid Function Path " + functionname); 
            }
                
            try
            {
                if(folderPath.Contains("..") || filename.Contains("..")) {  //if found .. in the file name or path
                    Log.Error("Invalid path " + folderPath + "-" + filename);
                    throw new Exception("Invalid file path " + filename);
                }

                // find extension by file name
                string? existingFile = Directory.EnumerateFiles(folderPath, filename + ".*").FirstOrDefault();
                string extension = "";
                if (!string.IsNullOrEmpty(existingFile))
                {
                    extension = Path.GetExtension(existingFile).ToLower().TrimStart('.');
                    if (allowext.Contains(extension))
                    {
                        byte[] m_Bytes = await ReadToEndAsync(new FileStream(existingFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                        Response.ContentType = "application/json";
                        string imageBase64Data = Convert.ToBase64String(m_Bytes);
                        string imageDataURL = string.Format("data:image/" + extension + ";base64,{0}", imageBase64Data);
                        return imageDataURL;
                    }
                    else
                        throw new Exception("Invalid File Extension " + extension);
                }
                else {
                    Log.Warning("DownLoadFile File Path not found: {@Folder}, {@FileName}", folderPath, filename);  //it is not error, but log to see someone trying random file path or not.
                    return "";
                }
                    

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Download File Error";
            }

        }

        [HttpGet("DownloadDir/{functionname}/{param}", Name = "DownloadDir")]
        public async Task<dynamic> DownLoadFileDir(string functionname, string param) //restrict user level permission upto function name level
        {
            string dirname = Encryption.Decode_URLParam(param);

            if (!allowfunction.Contains(functionname)) {
                throw new Exception("Function Name Not Allow : " + functionname);  
            }

            string uploadDirectory = _configuration.GetSection("appSettings:" + functionname).Value;
            string folderPath;
            
            if (uploadDirectory != null)
            {
                folderPath = baseDirectory + uploadDirectory + dirname;
            }
            else
            {
                Log.Error("Invalid Function Path " + functionname); 
                throw new Exception("Invalid Function Path " + functionname); 
            }

            try
            {
                if(folderPath.Contains("..")) { //if found .. in the file name or path
                    Log.Error("Invalid folder path " + folderPath);
                    throw new Exception("Invalid folder path " + dirname);
                }
                
                Dictionary<string, dynamic> filelist = new();

                if(Directory.Exists(folderPath)) {
                    string[] files = System.IO.Directory.GetFiles(folderPath);
                    string fileName;
                    string extension = "";
                    

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        fileName = System.IO.Path.GetFileName(s);
                        extension = Path.GetExtension(fileName).ToLower().TrimStart('.');
                        if (allowext.Contains(extension))
                        {
                            byte[] m_Bytes = await ReadToEndAsync(new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.Read));
                            
                            string imageBase64Data = Convert.ToBase64String(m_Bytes);
                            string imageDataURL = string.Format("data:image/" + extension + ";base64,{0}", imageBase64Data);
                            //filelist.Add(fileName, imageDataURL);
                            filelist.Add(fileName, new { myUid = Encryption.EncryptFileName(fileName), imgURL = imageDataURL });
                        }
                    }
                }
                else {
                    Log.Warning("DownLoadFileDir Folder Path not found: {@Folder}", folderPath); //it is not error, but log to see someone trying random folder or not.
                }
                Response.ContentType = "application/json";
                return filelist;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Download File List Error";
            }
        }

        [HttpPost("Upload/TempDir")]
        public async Task<dynamic> FileUploadTempDir([FromForm] string tempdir, [FromForm] string enFile)
        {
            Response.ContentType = "application/json";
            try
            {
                var files = Request.Form.Files;
                
                if (files.Count > 0)
                {
                    // Save the file
                    var file = files[0];
                    if (file.Length > 0)
                    {
                        enFile = Encryption.DecryptClientFileName(enFile);  //client side decryption
                        if(enFile != file.FileName)
                            throw new Exception("Invalid file name " + enFile + ", File Name" + file.FileName);

                        string uploadfilename = file.FileName;
                        string ext = FileService.GetFileExtension(uploadfilename);
                        string folderPath = _configuration.GetSection("appSettings:UploadTempPath").Value;

                        if (!allowext.Contains(ext))
                        {
                            throw new Exception("Invalid File Extension " + ext);
                        }

                        if(tempdir == null || tempdir == "-") {  // new temp dir
                            tempdir = Guid.NewGuid().ToString();
                            folderPath = baseDirectory + folderPath + tempdir;
                            Directory.CreateDirectory(folderPath);
                        }
                        else { // existing temp dir
                            tempdir = Encryption.DecryptFileName(tempdir);
                            folderPath = baseDirectory + folderPath + tempdir;
                            if (!Directory.Exists(folderPath))
                            {
                                Log.Error("Invalid Temp folder path " + folderPath);    
                                throw new Exception("Invalid Temp folder path " + tempdir);
                            }
                        }
                        
                        string fullPath = folderPath + System.IO.Path.DirectorySeparatorChar + uploadfilename;  // IMPORTANT do not use hardcoded directory separator e.g. / or \, separator is different in linux and windows.
                        //fullPath = System.IO.Path.Combine(folderPath, uploadfilename);   //IMPORTANT do not use Path.Combine method, it allow absolute path as parameter and it is security issue

                        if(fullPath.Contains("..")) { //if found .. in the file name or path
                            Log.Error("Invalid file path " + fullPath);
                            throw new Exception("Invalid file path");
                        }

                        using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        return new { tempdir = Encryption.EncryptFileName(tempdir), myUid = Encryption.EncryptFileName(uploadfilename)};
                    }
                    
                    throw new Exception("Empty File.");
                }
                else
                    throw new Exception("No File.");

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Fail to Upload";
            }
        }


        [HttpPost("Upload/Temp")]
        public async Task<string> FileUploadTemp([FromForm] string enFile)
        {
            Response.ContentType = "application/json";
            try
            {
                var files = Request.Form.Files;
                if (files.Count > 0)
                {
                    // Save the file
                    var file = files[0];
                    if (file.Length > 0)
                    {
                        enFile = Encryption.DecryptClientFileName(enFile);  //client side decryption
                        if(enFile != file.FileName)
                            throw new Exception("Invalid file name " + file.FileName + ", enFile" + enFile);

                        string uploadfilename = file.FileName;
                        string ext = FileService.GetFileExtension(uploadfilename);
                        
                        string fullPath = "";
                        string folderPath = _configuration.GetSection("appSettings:UploadTempPath").Value;

                        if (!allowext.Contains(ext))
                        {
                            throw new Exception("Invalid File Extension " + ext);
                        }

                        folderPath = baseDirectory + folderPath;
                        if (!Directory.Exists(folderPath))
                        {
                            Log.Error("Temp folder not found " + folderPath);    
                            throw new Exception("Temp folder not found");
                        }


                        string filename = Guid.NewGuid().ToString();
                        fullPath = folderPath + filename + "." + ext.ToLower();

                        if(fullPath.Contains("..")) { //if found .. in the file name or path
                            Log.Error("Invalid path " + fullPath);
                            throw new Exception("Invalid path");
                        }

                        using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        return Encryption.EncryptFileName(filename + "." + ext.ToLower());
                    }
                    
                    throw new Exception("Empty File.");
                }
                else
                    throw new Exception("No File.");

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Fail to Upload";
            }
        }

        [HttpPost("Remove/{functionname}/{param}")]
        public string FileUploadRemove(string functionname, string param) //restrict user level permission upto function name level
        {
            Response.ContentType = "application/json";
            try
            {
                string paramname = Encryption.Decode_URLParam(param);

                if (!allowfunction.Contains(functionname)) {
                    throw new Exception("Function Name Not Allow : " + functionname);  
                }
                string uploadDirectory = _configuration.GetSection("appSettings:" + functionname).Value;
                if (uploadDirectory == null || uploadDirectory == "") {
                    Log.Error("Invalid Function Path " + functionname); 
                    throw new Exception("Invalid Function Path " + functionname); 
                }
                    
                string? fullPath = "";
                string folderPath = "";
                string ext = "";

                folderPath = baseDirectory + uploadDirectory;
                fullPath = Directory.EnumerateFiles(folderPath, paramname + ".*").FirstOrDefault();

                if(fullPath == null || fullPath.Contains("..")) {  //if found .. in the file name or path
                    Log.Error("Invalid path " + fullPath);
                    throw new Exception("Invalid path " + paramname);
                }                    

                if (System.IO.File.Exists(fullPath))
                {
                    ext = Path.GetExtension(fullPath).ToLower().TrimStart('.');
                    if (!allowext.Contains(ext))
                        throw new Exception("Invalid File Extension " + ext);

                    System.IO.File.Delete(fullPath);
                    return "";
                }
                else
                {
                    Log.Error("File not found " + fullPath);
                    throw new Exception("File not found " + paramname);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Fail to Remove File";
            }
        }

        [HttpPost("RemoveDir/{functionname}/{param}")]
        public string FileUploadRemoveDir(string functionname, string param, [FromBody] string bodyParam) //restrict user level permission upto function name level
        {
            Response.ContentType = "application/json";
            try
            {
                string paramname = Encryption.Decode_URLParam(param);

                if (!allowfunction.Contains(functionname)) {
                    throw new Exception("Function Name Not Allow : " + functionname);  
                }
                string uploadDirectory = _configuration.GetSection("appSettings:" + functionname).Value;
                if (uploadDirectory == null || uploadDirectory == "") {
                    Log.Error("Invalid Function Path " + functionname); 
                    throw new Exception("Invalid Function Path " + functionname); 
                }
                    
                string? fullPath = "";
                string folderPath = "";
                string ext = "";

                if(bodyParam != null && bodyParam != "") {  //sub folder multiple file
                    
                    string fileName = Encryption.DecryptFileName(bodyParam);
                    folderPath = baseDirectory + uploadDirectory + paramname;
                    fullPath = folderPath + Path.DirectorySeparatorChar + fileName;
                    if(fullPath.Contains("..")) { //if found .. in the file name or path
                        Log.Error("Invalid path " + fullPath);
                        throw new Exception("Invalid path " + paramname + "-" + fileName);
                    }
                }
                else { 
                    Log.Error("Invalid parameter");
                    throw new Exception("Invalid parameter");
                }
              
                if (System.IO.File.Exists(fullPath))
                {
                    ext = Path.GetExtension(fullPath).ToLower().TrimStart('.');
                    if (!allowext.Contains(ext))
                        throw new Exception("Invalid File Extension " + ext);

                    System.IO.File.Delete(fullPath);
                    return "";
                }
                else
                {
                    Log.Error("File not found " + fullPath);
                    throw new Exception("File not found " + paramname);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Fail to Remove File";
            }
        }

        [HttpPost("Upload/TempRemove")]
        public string FileUploadTempRemove([FromForm] string fileNames)
        {
            Response.ContentType = "application/json";
            try
            {
                if(fileNames == null || fileNames == "")
                    throw new Exception("Invalid temp file");
                else 
                    fileNames = Encryption.DecryptFileName(fileNames);

                string uploadfilename = fileNames;
                string ext = FileService.GetFileExtension(uploadfilename);
               
                string fullPath = "";
                string folderPath = _configuration.GetSection("appSettings:UploadTempPath").Value;

                fullPath = baseDirectory + folderPath + fileNames;
                if(fullPath.Contains("..")) { //if found .. in the file name or path
                    Log.Error("Invalid path " + fullPath);
                    throw new Exception("Invalid path " + fileNames);
                }
                if (!allowext.Contains(ext))
                {
                    throw new Exception("Invalid File Extension " + ext);
                }

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    return "";
                }
                else
                {
                    Log.Error("File not found " + fullPath);
                    throw new Exception("File not found " + fileNames);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Fail to Remove ";
            }
        }

        [HttpPost("Upload/TempRemoveDir")]
        public string FileUploadTempRemoveDir([FromForm] string fileNames, [FromForm] string tempdir, [FromForm] string myUid)
        {
            Response.ContentType = "application/json";
            try
            {
                string encfileNames = "";
                if(myUid == null || myUid == "")
                    throw new Exception("Invalid temp file");
                else 
                    encfileNames = Encryption.DecryptFileName(myUid);

                if(encfileNames != fileNames)
                    throw new Exception("Invalid temp file");

                tempdir = Encryption.DecryptFileName(tempdir);
                string ext = FileService.GetFileExtension(fileNames);
                
                string fullPath = "";
                string folderPath = _configuration.GetSection("appSettings:UploadTempPath").Value;

                fullPath = baseDirectory + folderPath + tempdir + System.IO.Path.DirectorySeparatorChar + fileNames;

                if(fullPath.Contains("..")) { //if found .. in the file name or path
                    Log.Error("Invalid path " + fullPath);
                    throw new Exception("Invalid path " + fileNames);
                }
                if (!allowext.Contains(ext))
                {
                    throw new Exception("Invalid File Extension " + ext);
                }

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    return "";
                }
                else
                {
                    Log.Error("File not found " + fullPath);
                    throw new Exception("File not found " + fileNames);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
                Response.StatusCode = 400;
                return "Fail to Upload";
            }
        }

        public static byte[] ReadToEnd(System.IO.Stream inputStream)
        {
            if (!inputStream.CanRead)
            {
                throw new ArgumentException("Invalid file stream");
            }

            // This is optional
            if (inputStream.CanSeek)
            {
                inputStream.Seek(0, SeekOrigin.Begin);
            }

            byte[] output = new byte[inputStream.Length];
            _ = inputStream.Read(output, 0, output.Length);
            inputStream.Dispose();//.Close();
            return output;
        }
        
        public static async Task<byte[]> ReadToEndAsync(System.IO.Stream inputStream)
        {
            if (!inputStream.CanRead)
            {
                throw new ArgumentException("Invalid file stream");
            }

            // This is optional
            if (inputStream.CanSeek)
            {
                inputStream.Seek(0, SeekOrigin.Begin);
            }

            byte[] output = new byte[inputStream.Length];
            _ = await inputStream.ReadAsync(output);
            inputStream.Dispose();
            return output;
        }
    }
}