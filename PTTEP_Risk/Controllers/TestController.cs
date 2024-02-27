using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PTTEP_Risk.Help;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace PTTEP_Risk.Controllers
{
    public class TestController : Controller
    {
        private MasterRepo _master = new MasterRepo();
        private ConfigurationService _c = new ConfigurationService();

        [HttpPost, DisableRequestSizeLimit]
        [Route("api/test/upload")]
        public IActionResult Upload([FromForm(Name = "files")] List<IFormFile> files)
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    try
                    {
                        if (!Directory.Exists(pathToSave))
                        {
                            Directory.CreateDirectory(pathToSave);
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, ex.Message.ToString());
                    }

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("test/UploadMultiple")]
        public IActionResult UploadMultiple([FromForm(Name = "files")] List<IFormFile> files)
        {
            try
            {
                List<ServiceModel> servicesModel = _c.ConnectionService();

                var model = new UploadFileModel();
                var fileData = Request.Form.Files;
                var Data = Request.Form["FormData"];
                model = JsonConvert.DeserializeObject<UploadFileModel>(Data);
                var folderName = Path.Combine("Resources", "FileUpload", model.Form, model.ReqId);
                //var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                var pathToSave = Path.Combine(servicesModel[0].PartUploadFile, folderName);

                if (!files.Any(f => f.Length == 0))
                {
                    try
                    {
                        if (!Directory.Exists(pathToSave))
                        {
                            Directory.CreateDirectory(pathToSave);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new ResponseGrid<int>()
                        {
                            Status = false,
                            ErrorMessage = ex.Message, //todo:define message
                            Total = 0
                        });
                    }
                    foreach (var file in fileData)
                    {

                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var dbPath = Path.Combine(folderName, fileName); //you can add this path to a list and then return all dbPaths to the client if require

                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        model.FileName = fileName;
                        model.RootPath = fullPath;
                        model.PathFile = dbPath;

                        _master.AttachFileInsert(model);
                    }

                    //return Ok(new { pathToSave });
                    return Json(new ResponseMessage<int>()
                    {
                        Status = true,
                        ErrorMessage = "SUCCESS", //todo:define message
                    });
                }
                else
                {
                    return Json(new ResponseGrid<int>()
                    {
                        Status = false,
                        ErrorMessage = "ไม่มีไฟล์", //todo:define message
                        Total = 0
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = ex.Message, //todo:define message
                    Total = 0
                });
                //return StatusCode(500, "Internal server error" + ex.Message);
            }
        }

        [HttpPost]
        [Route("test/DeleteFile")]
        public IActionResult DeleteFile([FromForm] UploadFileModel request)
        {
            if (request != null)
            {
                try
                {
                    var del = request;

                    if (del.AttachFileID != null || del.AttachFileID != "")
                    {
                        _master.AttachFileDelete(del);

                        if (System.IO.File.Exists(del.RootPath))
                        {
                            System.IO.File.Delete(del.RootPath);
                        }
                    }

                    return Json(new ResponseMessage<int>()
                    {
                        Status = true,
                        ErrorMessage = "SUCCESS", //todo:define message
                    });
                }
                catch (Exception ex)
                {
                    return Json(new ResponseMessage<int>()
                    {
                        Status = false,
                        ErrorMessage = ex.Message.ToString(), //todo:define message
                    });
                }

            }
            else
            {
                return Json(new ResponseMessage<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1", //todo:define message
                });
            }
        }

        [HttpGet]
        [Route("api/test/Get")]
        public IEnumerable<object> Get()
        {
            string[] s = { "2", "22" };
            return s;
        }

        [HttpPost]
        [Route("api/test/post")]
        public IActionResult post([FromForm]UploadFileModel request)
        {
            return Json(new ResponseGrid<UploadFileModel>()
            {
                Status = false,
                ErrorMessage = "Access denied1" , //todo:define message
                body = request,
                Total = 0,
            });
        }

    }
}
