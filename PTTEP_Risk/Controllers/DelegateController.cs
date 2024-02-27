using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PTTEP_Risk.Repo;
using PTTEP_Risk.Model;

namespace PTTEP_Risk.Controllers
{
    public class DelegateController : Controller
    {
        private DelegateRepo _s = new DelegateRepo();
        [HttpPost]
        [Route("Delegate_RiskSeacrh")]
        public IActionResult API_Delegate_Seacrh([FromForm] RequestMessage<DelegateModel> data)
        {
            if (data != null)
            {
                return Json(_s.API_Delegate_Seacrh(data));
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
        [Route("Delegate")]
        public IActionResult API_Delegate([FromForm] RequestMessage<DelegateModel> data)
        {
            if (data != null)
            {
                return Json(_s.API_Delegate(data));
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
