using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using System;
using System.Diagnostics;

namespace PTTEP_Risk.Controllers
{
    public class UserAuthController : Controller
    {
        private UserAuthRepo _s = new UserAuthRepo();

        #region Get File VersionInfo
        [HttpGet]
        [Route("api/GetFileVersionInfo")]
        public ActionResult<string> GetFileVersionInfo()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string fversion = fvi.FileVersion;

            Version versionInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string vversion = versionInfo.ToString();
            string stext = "Build Version:" + vversion + "FileVersion:" + fversion;

            return Json(new
            {
                BuildVersion = vversion,
                FileVersion = fversion
            });
        }


        #endregion
        #region Get User
        [HttpPost]
        [Route("GetUserlogin")]
        public IActionResult GetUserlogin([FromForm] UserAuthLogin data)
        {
            if (data != null)
            {
                return Json(_s.API_GetUserlogin(data));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(data), //todo:define message
                    Total = 0,
                });
            }
        }

        [HttpPost]
        [Route("CheckUserlogin")]
        public IActionResult CheckUserlogin([FromForm] UserAuthLogin data)
        {
            if (data != null)
            {
                return Json(_s.CheckUserlogin(data));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(data), //todo:define message
                    Total = 0,
                });
            }
        }
        #endregion

        #region Get 
        [HttpGet]
        [Route("Test")]
        public IActionResult Test()
        {
            return Json("Hello!!!");
            /*if (data != null)
            {
                return Json(_s.API_GetUserlogin(data));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(data), //todo:define message
                    Total = 0,
                });
            }*/
        }
        #endregion
    }
}
