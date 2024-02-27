using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;

namespace PTTEP_Risk.Controllers
{
    public class GenTransectionController : Controller
    {
        private GenTransection_Repo _s = new GenTransection_Repo();
        [HttpPost]
        [Route("Risk_Insert_Update_Get")]
        public IActionResult API_Risk_Insert_Update_Get([FromForm] RequestMessage<Risk> data)
        {

            if (data != null)
            {
                return Json(_s.API_Risk_Insert_Update_Get(data));
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
            //var result = data;
            //return result;
        }

        [HttpPost]
        [Route("Risk_RootCause_Insert_Update_Get")]
        public IActionResult API_Risk_RootCause_Insert_Update_Get([FromForm] RequestMessage<RootCause> data)
        {
            if (data != null)
            {
                return Json(_s.API_Risk_RootCause_Insert_Update_Get(data));
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
            //var result = data;
            //return result;
        }

        [HttpPost]
        [Route("Risk_Impact_Insert_Update_Get")]
        public IActionResult API_Risk_Impact_Insert_Update_Get([FromForm] RequestMessage<Impact> data)
        {
            if(data != null)
            { 
                return Json(_s.API_Risk_Impact_Insert_Update_Get(data));
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

            //var result = data;

        }


    }
}
