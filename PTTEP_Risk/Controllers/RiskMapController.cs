using Microsoft.AspNetCore.Mvc;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PTTEP_Risk.Controllers
{
    public class RiskMapController : Controller
    {
        private RiskMapRepo _s = new RiskMapRepo();
        [HttpPost]
        [Route("Get_Impact_Riskmap_BU")]
        public IActionResult API_Get_Impact_Riskmap_BU([FromForm] RiskMap_Impact data)
        {
            if (data != null)
            {
                return Json(_s.API_Get_Impact_Riskmap_BU(data));
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

        [HttpPost]
        [Route("Cal_RiskMap_BU")]
        public IActionResult API_Cal_RiskMap_BU([FromForm] RiskMap_Impact data)
        {
            if (data != null)
            {
                return Json(_s.API_Cal_RiskMap_BU(data));
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

        [HttpPost]
        [Route("Get_Impact_Riskmap_Unit")]
        public IActionResult API_Get_Impact_Riskmap_Unit([FromForm] RiskMap_Impact data)
        {
            if (data != null)
            {
                return Json(_s.API_Get_Impact_Riskmap_Unit(data));
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

        [HttpPost]
        [Route("Cal_RiskMap_Unit")]
        public IActionResult API_Cal_RiskMap_Unit([FromForm] RiskMap_Unit data)
        {
            if (data != null)
            {
                return Json(_s.API_Cal_RiskMap_Unit(data));
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

        [HttpPost]
        [Route("Get_Risk_Unit")]
        public IActionResult API_Get_Risk_Unit([FromForm] RiskMap_Unit data)
        {
            if (data != null)
            {
                return Json(_s.API_Get_Risk_Unit(data));
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
        [Route("Get_Menu_RiskMap")]
        public IActionResult API_Get_Menu_RiskMap([FromForm] ResponseMessage<RiskMap_Menu> data)
        {
            if (data != null)
            {
                return Json(_s.API_Get_Menu_RiskMap(data));
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
