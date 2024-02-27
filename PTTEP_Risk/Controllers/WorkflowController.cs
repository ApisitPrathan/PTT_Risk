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
    public class WorkflowController : Controller
    {
        private Workflow_Repo _s = new Workflow_Repo();
        [HttpPost]
        [Route("Risk_Workflow_Submit")]
        public IActionResult API_Risk_Workflow_Submit([FromForm] RequestMessage<SubmitModel> data)
        {
            if (data != null)
            {
                return Json(_s.API_Risk_Workflow_Submit(data));
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
        [Route("Risk_Workflow_Approve")]
        public IActionResult API_Risk_Workflow_Approve([FromForm] RequestMessage<ApproveModel> data)
        {

            if (data != null)
            {
                return Json(_s.API_Risk_Workflow_Approve(data));
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
        [Route("Risk_Workflow_Reject")]
        public IActionResult API_Risk_Workflow_Reject([FromForm] RequestMessage<RejectModel> data)
        {

            if (data != null)
            {
                return Json(_s.API_Risk_Workflow_Reject(data));
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
