using Microsoft.AspNetCore.Mvc;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PTTEP_Risk.Controllers
{
    public class MenuConfigController : Controller
    {
        private MenuConfigBuRepo _s = new MenuConfigBuRepo();
        [HttpPost]
        [Route("Get_Menu_BU_Config")]
        public IActionResult API_Get_Menu_BU_Config([FromForm] ResponseMessage<RiskMap_Menu> data)
        {
            if (data != null)
            {
                return Json(_s.API_Get_BU_Config(data));
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
    }
}
