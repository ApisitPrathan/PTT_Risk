using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;

namespace PTTEP_Risk.Controllers
{
    public class ReportController : Controller
    {
        private ReportRepo _r = new ReportRepo();
        [HttpPost]
        [Route("ReportItems")]
        public IActionResult ReportItems([FromForm] RequestMessage<Report_Risk_Items> data)
        {
            if (data != null)
            {
                return Json(_r.API_Report_Risk_Items(data));
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
        [Route("ReportDashboardCategory")]
        public IActionResult API_Report_Dashboard_Category([FromForm] RequestMessage<Report_Dashboard_Category> data)
        {
            if (data != null)
            {
                return Json(_r.API_Report_Dashboard_Category(data));
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
        [Route("ReportDashboardStatus")]
        public IActionResult API_Report_Dashboard_Status([FromForm] RequestMessage<Report_Dashboard_Status> data)
        {
            if (data != null)
            {
                return Json(_r.API_Report_Dashboard_Status(data));
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
