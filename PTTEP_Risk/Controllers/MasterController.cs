using Microsoft.AspNetCore.Mvc;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace PTTEP_Risk.Controllers
{
    public class MasterController : Controller
    {
        private MasterRepo _master = new MasterRepo();

        [HttpPost]
        [Route("Master/GetQuarter")]
        public IActionResult GetQuarter([FromForm] RequestMessage<ReqQuarter> request)
        {
            if (request != null)
            {
                return Json(_master.GetQuarter(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetBUCoordinator")]
        public IActionResult GetBUCoordinator([FromForm] RequestMessage<List<ReqBUCoordinator>> request)
        {
            if (request != null)
            {
                return Json(_master.GetBUCoordinator(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetFinancialImpact")]
        public IActionResult GetFinancialImpact([FromForm] RequestMessage<ReqFinancialImpact> request)
        {
            if (request != null)
            {
                return Json(_master.GetFinancialImpact(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetRiskCategory")]
        public IActionResult GetRiskCategory([FromForm] RequestMessage<ReqRiskCategory> request)
        {
            if (request != null)
            {
                return Json(_master.GetRiskCategory(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetRiskRating")]
        public IActionResult GetRiskRating([FromForm] RequestMessage<ReqRiskRating> request)
        {
            if (request != null)
            {
                return Json(_master.GetRiskRating(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetLikelihood")]
        public IActionResult GetLikelihood([FromForm] RequestMessage<ReqLikelihood> request)
        {
            if (request != null)
            {
                return Json(_master.GetLikelihood(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetImpactCategory")]
        public IActionResult GetImpactCategory([FromForm] RequestMessage<ReqImpactCate> request)
        {
            if (request != null)
            {
                return Json(_master.GetImpactCategory(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetPerManagement")]
        public IActionResult GetPerManagement([FromForm] RequestMessage<ReqPerManagement> request)
        {
            if (request != null)
            {
                return Json(_master.GetPerManagement(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetTopMenu")]
        public IActionResult GetTopMenu([FromForm] RequestMessage<ReqTopMenu> request)
        {
            if (request != null)
            {
                return Json(_master.GetTopMenu(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetInstruction")]
        public IActionResult GetInstruction([FromForm] RequestMessage<ReqInstruction> request)
        {
            if (request != null)
            {
                return Json(_master.GetInstruction(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetBanner")]
        public IActionResult GetBanner([FromForm] RequestMessage<ReqBanner> request)
        {
            if (request != null)
            {
                return Json(_master.GetBanner(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetContactUs")]
        public IActionResult GetContactUs([FromForm] RequestMessage<ReqContactUs> request)
        {
            if (request != null)
            {
                return Json(_master.GetContactUs(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetCorpTarget")]
        public IActionResult GetCorpTarget([FromForm] RequestMessage<ReqCorpTarget> request)
        {
            if (request != null)
            {
                return Json(_master.GetCorpTarget(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetRiskCatalog")]
        public IActionResult GetRiskCatalog([FromForm] RequestMessage<ReqRiskCatalog> request)
        {
            if (request != null)
            {
                return Json(_master.GetRiskCatalog(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetAsset")]
        public IActionResult GetAsset([FromForm] RequestMessage<List<ReqMaster_Assset>> request)
        {
            if (request != null)
            {
                return Json(_master.GetAsset(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetWPB")]
        public IActionResult GetWPB([FromForm] RequestMessage<ReqMaster_WPB> request)
        {
            if (request != null)
            {
                return Json(_master.GetWPB(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1" + Json(request), //todo:define message
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetDDL")]
        public IActionResult GetDDL([FromForm] DropDownList_Master request)
        {
            if (request != null)
            {
                return Json(this._master.GetMaster_DDL(request));
            }
            else
            {
                return Json(new ResponseGrid<int>()
                {
                    Status = false,
                    ErrorMessage = "Access denied" + Json(request),
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetTemplateEmail")]
        public IActionResult GetTemplateEmail([FromForm] RequestMessage<EmailModelInsert> request)
        {
            if (request != null)
            {
                return Json(_master.GetTemplateEmail(request));
            }
            else
            {
                return Json(new ResponseGrid<object>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1", //todo:define message
                    body = request,
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/ReplaceCordinator")]
        public IActionResult ReplaceCordinator([FromForm] RequestMessage<Master_ReplaceCordinator> request)
        {
            if (request != null)
            {
                return Json(_master.ReplaceCordinator(request));
            }
            else
            {
                return Json(new ResponseGrid<object>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1", //todo:define message
                    body = request,
                    Total = 0
                });
            }
        }

        [HttpPost]
        [Route("Master/GetOwner")]
        public IActionResult GetOwner([FromForm] RequestMessage<Master_Owner> request)
        {
            if (request != null)
            {
                return Json(_master.GetOwner(request));
            }
            else
            {
                return Json(new ResponseGrid<object>()
                {
                    Status = false,
                    ErrorMessage = "Access denied1", //todo:define message
                    body = request,
                    Total = 0
                });
            }
        }

    }
}
