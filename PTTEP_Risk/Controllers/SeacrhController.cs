using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using System;
using System.Diagnostics;

namespace PTTEP_Risk.Controllers
{
    public class SeacrhController : Controller
    {
        private SeacrhRepo _s = new SeacrhRepo();
        [HttpPost]
        [Route("RiskSeacrh")]
        public IActionResult RiskSeacrh([FromForm] RequestMessage<SeacrhModel> data)
        {
            if (data != null)
            {
                return Json(_s.API_RiskSeacrh(data));
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
