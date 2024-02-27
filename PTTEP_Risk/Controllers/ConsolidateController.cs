using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using PTTEP_Risk.Help;

namespace PTTEP_Risk.Controllers
{
    public class ConsolidateController : Controller
    {
        private ConsolidateRepo _s = new ConsolidateRepo();
        [HttpPost]
        [Route("Risk_Consolidate")]
        public IActionResult API_Risk_Consolidate([FromForm] RequestMessage<ConsolidateModel> data)
        {

            if (data != null)
            {
                return Json(_s.API_Risk_Consolidate(data));
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
        [Route("Risk_ReConsolidate")]
        public IActionResult API_Risk_ReConsolidate([FromForm] RequestMessage<ReConsolidate> data)
        {

            if (data != null)
            {
                return Json(_s.API_Risk_ReConsolidate(data));
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
        [Route("Risk_Transfer")]
        public IActionResult API_Risk_Transfer([FromForm] RequestMessage<Consolidate_Transfer> data)
        {

            if (data != null)
            {
                return Json(_s.API_Risk_Transfer(data));
            }
            else
            {
                return Json(new ResponseGrid<object>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1",
                    Total = 0,
                    body = data
                });
            }
        }
    }
}
