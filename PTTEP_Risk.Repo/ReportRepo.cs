using System;
using System.Collections.Generic;
using System.Text;
using PTTEP_Risk.Help;
using PTTEP_Risk.Model;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PTTEP_Risk.Repo
{
    public class ReportRepo
    {
        GetData _g = new GetData();
        GetUser _s = new GetUser();
        Helper _h = new Helper();
        GetOrganizations _o = new GetOrganizations();
        RiskMap_Organization _menu = new RiskMap_Organization();
        ConfigurationService _c = new ConfigurationService();
        public ResponseMessage<List<Risk>> API_Report_Risk_Items(RequestMessage<Report_Risk_Items> request)
        {
            ResponseMessage<List<Risk>> response = new ResponseMessage<List<Risk>>();

            var data = request.body;
            List<Risk> colsRisk = new List<Risk>();
            try
            {
                //Get User From Service
                var paramsEmp = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("EmployeeID", ""),
                    new KeyValuePair<string, string>("FirstName", ""),
                    new KeyValuePair<string, string>("LastName", ""),
                    new KeyValuePair<string, string>("EmailAddress", request.Email),
                    new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                    new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                };
                var userInfo = _s.GetEmployee(paramsEmp);
                if (userInfo.Count > 0)
                {
                    //get riskmap org of Org
                    List<RiskMap_Menu> org = _menu.Get_Childe_Organization(userInfo, data.QuaterMaster);
                    if (org.Count > 0)
                    {
                        //builde string org
                        string colBU = _o.GetRecursiveOrganizationFromRiskMap(org[0]);
                        var p = new DynamicParameters();
                        p.Add("@Risk_Business_Unit", data.Risk_Business_Unit);
                        p.Add("@Risk_Status", data.Risk_Status);
                        p.Add("@Risk_KRI_Status", data.Risk_KRI_Status);
                        p.Add("@Risk_Catagory", data.Risk_Catagory);
                        p.Add("@Risk_Register_Date_From", data.Risk_Register_Date_From);
                        p.Add("@Risk_Register_Date_To", data.Risk_Register_Date_To);
                        p.Add("@Risk_Rating", data.Risk_Rating);
                        p.Add("@Risk_Name", data.Risk_Name);
                        p.Add("@QuarterID", data.QuarterID);
                        p.Add("@WPBID", data.WPBID);
                        p.Add("@TempBUId", colBU); // temp bu id
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@Bowtie", "RISKBYBU"); // query by bu
                        if (data.Filter_Table == "1") //Transection Staff
                        {
                            p.Add("@Role", "Staff");
                            var colRiskStaff = _g.GetCollenctionRiskData(p, "Staff");
                            if (!_h.IsNullOrEmpty(colRiskStaff.Risk))
                            {
                                if (colRiskStaff.Risk.Count > 0)
                                {
                                    if (!_h.CheckNull(data.QuarterID))
                                    {
                                        //transection is quarter
                                        var _resultRisk = colRiskStaff.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                        response.body = _resultRisk;
                                        response.Status = true;
                                    }
                                    else if (!_h.CheckNull(data.WPBID))
                                    {
                                        //transection is wpb
                                        var _resultRisk = colRiskStaff.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                        response.body = _resultRisk;
                                        response.Status = true;
                                    }
                                    else
                                    {
                                        //transection is all
                                        response.body = colRiskStaff.Risk;
                                        response.Status = true;
                                    }

                                }
                                else
                                {
                                    response.ErrorMessage = "No data report!";
                                    response.Status = false;
                                }
                            }
                            else
                            {
                                response.ErrorMessage = "No data report!";
                                response.Status = false;
                            }
                        }
                        else if (data.Filter_Table == "2") //Transection 
                        {
                            p.Add("@Role", "Transection");
                            var colRiskTransection = _g.GetCollenctionRiskData(p, "Transection");
                            if (!_h.IsNullOrEmpty(colRiskTransection.Risk))
                            {
                                if (colRiskTransection.Risk.Count > 0)
                                {
                                    if (!_h.CheckNull(data.QuarterID))
                                    {
                                        //transection is quarter
                                        var _resultRisk = colRiskTransection.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                        response.body = _resultRisk;
                                        response.Status = true;
                                    }
                                    else if (!_h.CheckNull(data.WPBID))
                                    {
                                        //transection is wpb
                                        var _resultRisk = colRiskTransection.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                        response.body = _resultRisk;
                                        response.Status = true;
                                    }
                                    else
                                    {
                                        //transection is all
                                        response.body = colRiskTransection.Risk;
                                        response.Status = true;
                                    }
                                }
                                else
                                {
                                    response.ErrorMessage = "No data report!";
                                    response.Status = false;
                                }
                            }
                            else
                            {
                                response.ErrorMessage = "No data report!";
                                response.Status = false;
                            }
                        }
                        else // All Transection
                        {
                            p.Add("@Role", "Staff");
                            var colRiskStaff = _g.GetCollenctionRiskData(p, "Staff");
                            if (!_h.IsNullOrEmpty(colRiskStaff.Risk))
                            {
                                if (colRiskStaff.Risk.Count > 0)
                                {

                                    if (!_h.CheckNull(data.QuarterID))
                                    {
                                        //transection is quarter
                                        var _resultRisk = colRiskStaff.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                        colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                        //response.body = _resultRisk;
                                        //response.Status = true;
                                    }
                                    else if (!_h.CheckNull(data.WPBID))
                                    {
                                        //transection is wpb
                                        var _resultRisk = colRiskStaff.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                        colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                        //response.body = _resultRisk;
                                        //response.Status = true;
                                    }
                                    else
                                    {
                                        //transection is all
                                        colsRisk = colsRisk.Concat(colRiskStaff.Risk).ToList();
                                    }
                                }
                            }
                            p.Add("@Role", "Transection");
                            var colRiskTransection = _g.GetCollenctionRiskData(p, "Transection");
                            if (!_h.IsNullOrEmpty(colRiskTransection.Risk))
                            {
                                if (colRiskTransection.Risk.Count > 0)
                                {
                                    if (!_h.CheckNull(data.QuarterID))
                                    {
                                        //transection is quarter
                                        var _resultRisk = colRiskTransection.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                        colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                        //response.body = _resultRisk;
                                        //response.Status = true;
                                    }
                                    else if (!_h.CheckNull(data.WPBID))
                                    {
                                        //transection is wpb
                                        var _resultRisk = colRiskTransection.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                        colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                        //response.body = _resultRisk;
                                        //response.Status = true;
                                    }
                                    else
                                    {
                                        //transection is all
                                        colsRisk = colsRisk.Concat(colRiskTransection.Risk).ToList();
                                    }
                                }

                            }
                            if (colsRisk.Count > 0)
                            {
                                response.body = colsRisk;
                                response.Status = true;
                            }
                            else
                            {
                                response.ErrorMessage = "No data report!";
                                response.Status = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[API_Report_Risk_Items]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<List<Risk>> API_Report_Dashboard_Category(RequestMessage<Report_Dashboard_Category> request)
        {
            ResponseMessage<List<Risk>> response = new ResponseMessage<List<Risk>>();
            List<ServiceModel> servicesModel = _c.ConnectionService();

            var data = request.body;
            List<Risk> colsRisk = new List<Risk>();
            try
            {
                if (data.Child_Node == "0")//Not inculde childe
                {
                    var p = new DynamicParameters();
                    p.Add("@TempBUId", data.Risk_Business_Unit);
                    p.Add("@QuarterID", data.QuarterID);
                    p.Add("@WPBID", data.WPBID);
                    p.Add("@Bowtie", "RISKBYPARENT");
                    p.Add("@Role", "Transection");
                    p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    var colRisk = _g.GetCollenctionRiskData(p, "Transection");
                    if (!_h.IsNullOrEmpty(colRisk.Risk))
                    {
                        if (!_h.CheckNull(data.QuarterID))
                        {
                            //transection is quarter
                            colsRisk = colRisk.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                            //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                            //response.body = _resultRisk;
                            //response.Status = true;
                        }
                        else if (!_h.CheckNull(data.WPBID))
                        {
                            //transection is wpb
                            colsRisk = colRisk.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                            //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                            //response.body = _resultRisk;
                            //response.Status = true;
                        }
                        else
                        {
                            //transection is all
                            colsRisk = colsRisk.Concat(colRisk.Risk).ToList();
                            //response.body = colsRisk;
                            //response.Status = true;
                        }
                        /*var filterOrg = colsRisk.Where(r => r.Risk_Business_Unit == data.Risk_Business_Unit ||
                                                                (r.FG_Unit == data.Risk_Business_Unit && r.FG_Flag == "Y") ||
                                                                (r.DI_Unit == data.Risk_Business_Unit && r.DI_Flag == "Y") ||
                                                                (r.BU_Unit == data.Risk_Business_Unit && r.BU_Flag == "Y") ||
                                                                (r.Asset_Unit == data.Risk_Business_Unit && r.Asset_Flag == "Y")
                                                           ).ToList();*/
                        response.body = colsRisk;
                        response.Status = true;

                    }
                    else
                    {
                        response.ErrorMessage = "No data report!";
                        response.Status = false;
                    }
                }
                else//Inculde childe
                {
                    using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                    {
                        if (data.Risk_Business_Unit != "PTTEP")
                        {
                            //Check Org Level
                            var p_Org = new List<KeyValuePair<string, string>>() {
                                new KeyValuePair<string, string>("OrganizetionID", data.Risk_Business_Unit),
                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService)
                            };
                            var orgInfo = _o.GetOrganization(p_Org);
                            if (orgInfo.Count > 0)//Get org to service
                            {
                                #region build str childe org
                                //get master asset 
                                var p_masterAsset = new DynamicParameters();
                                p_masterAsset.Add("@Table_Name", "Master_Asset");
                                p_masterAsset.Add("@TextSearch1", data.QuaterMaster);
                                var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterAsset, commandType: CommandType.StoredProcedure).ToList();
                                //Get Childe of BU
                                var orgChilde = _o.GetOrgALLChildByOrgID_DB(orgInfo[0], data.QuaterMaster);
                                var colStr = "";
                                if (orgChilde.Count > 0)
                                {
                                    //loop childe
                                    foreach (var _org in orgChilde)
                                    {
                                        if (_org.ORGANIZATION_LEVEL != "Section")
                                        {
                                            List<Asset_Menu> asset_Model = new List<Asset_Menu>();
                                            var _resultAsset = mAsset.Where(o => o.Asset_Org == _org.ORGANIZATION_ID).ToList();
                                            if (_resultAsset.Count > 0)
                                            {
                                                foreach (var items in _resultAsset)
                                                {
                                                    if (items.Asset_Level != "Section")
                                                    {
                                                        //add asset of bu to colStr
                                                        if (!_h.CheckNull(colStr))
                                                            colStr += ",";
                                                        colStr += items.Asset_Code;
                                                    }
                                                }
                                            }
                                            //add bu to colStr
                                            if (!_h.CheckNull(colStr))
                                                colStr += ",";
                                            colStr += _org.ORGANIZATION_ID;
                                        }
                                    }
                                }
                                else // not childe
                                {
                                    List<Asset_Menu> asset_Model = new List<Asset_Menu>();
                                    var _resultAsset = mAsset.Where(o => o.Asset_Org == orgInfo[0].ORGANIZATION_ID).ToList();
                                    if (_resultAsset.Count > 0)
                                    {
                                        foreach (var items in _resultAsset)
                                        {
                                            if (items.Asset_Level != "Section")
                                            {
                                                //add asset of bu to colStr
                                                if (!_h.CheckNull(colStr))
                                                    colStr += ",";
                                                colStr += items.Asset_Code;
                                            }
                                        }
                                    }
                                    //add bu to colStr
                                    if (!_h.CheckNull(colStr))
                                        colStr += ",";
                                    colStr += orgInfo[0].ORGANIZATION_ID;
                                }
                                #endregion
                                var p = new DynamicParameters();
                                p.Add("@QuarterID", data.QuarterID);
                                p.Add("@WPBID", data.WPBID);
                                p.Add("@Bowtie", "RISKBYPARENT");
                                p.Add("@TempBUId", colStr);
                                p.Add("@Role", "Transection");
                                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                var colRisk = _g.GetCollenctionRiskData(p, "Transection");
                                if (!_h.IsNullOrEmpty(colRisk.Risk))
                                {
                                    if (!_h.CheckNull(data.QuarterID))
                                    {
                                        //transection is quarter
                                        colsRisk = colRisk.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                        //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                        //response.body = _resultRisk;
                                        //response.Status = true;
                                    }
                                    else if (!_h.CheckNull(data.WPBID))
                                    {
                                        //transection is wpb
                                        colsRisk = colRisk.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                        //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                        //response.body = _resultRisk;
                                        //response.Status = true;
                                    }
                                    else
                                    {
                                        //transection is all
                                        colsRisk = colsRisk.Concat(colRisk.Risk).ToList();
                                        //response.body = colRisk.Risk;
                                        //response.Status = true;
                                    }
                                    
                                    //var arrOrgChide = String.Join(",", colStr);
                                    /*var filterOrg = colsRisk.Where(r => colStr.Split(',').Contains(r.Risk_Business_Unit) ||
                                                                (colStr.Split(',').Contains(r.FG_Unit) && r.FG_Flag == "Y") ||
                                                                (colStr.Split(',').Contains(r.DI_Unit) && r.DI_Flag == "Y") ||
                                                                (colStr.Split(',').Contains(r.BU_Unit) && r.BU_Flag == "Y") ||
                                                                (colStr.Split(',').Contains(r.Asset_Unit) && r.Asset_Flag == "Y")
                                                           ).ToList();*/
                                    response.body = colsRisk;
                                    response.Status = true;
                                }
                                else
                                {
                                    response.ErrorMessage = "No data report!";
                                    response.Status = false;
                                }
                            }
                            else//can't not get org
                            {
                                response.ErrorMessage = "Can't not get organizetion to service";
                                response.Status = false;
                            }
                        }
                        else//PTTEP
                        {
                            var p = new DynamicParameters();
                            p.Add("@QuarterID", data.QuarterID);
                            p.Add("@WPBID", data.WPBID);
                            p.Add("@Bowtie", "RISKBYBU");
                            //p.Add("@TempBUId", colStr);
                            p.Add("@Role", "Transection");
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            var colRisk = _g.GetCollenctionRiskData(p, "Transection");
                            if (!_h.IsNullOrEmpty(colRisk.Risk))
                            {
                                if (!_h.CheckNull(data.QuarterID))
                                {
                                    //transection is quarter
                                    var _resultRisk = colRisk.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                    //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                    response.body = _resultRisk;
                                    response.Status = true;
                                }
                                else if (!_h.CheckNull(data.WPBID))
                                {
                                    //transection is wpb
                                    var _resultRisk = colRisk.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                    //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                    response.body = _resultRisk;
                                    response.Status = true;
                                }
                                else
                                {
                                    //transection is all
                                    //colsRisk = colsRisk.Concat(colRisk.Risk).ToList();
                                    response.body = colRisk.Risk;
                                    response.Status = true;
                                }
                            }
                            else
                            {
                                response.ErrorMessage = "No data report!";
                                response.Status = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[API_Report_Dashboard_Category]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<List<Organizations>> API_Report_Dashboard_Status(RequestMessage<Report_Dashboard_Status> request)
        {
            ResponseMessage<List<Organizations>> response = new ResponseMessage<List<Organizations>>();
            List<ServiceModel> servicesModel = _c.ConnectionService();

            var data = request.body;

            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    var p_Get = new DynamicParameters();
                    p_Get.Add("@Type", "GETORG");
                    p_Get.Add("@QuarterID", data.QuarterMasterID);
                    p_Get.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    var dataOrg = conn.Query<Organizations>("sp_Check_Role_ERM", p_Get, commandType: CommandType.StoredProcedure).ToList();
                    if (dataOrg.Count > 0)
                    {
                        #region build str childe org oe ceo
                        //get master asset 
                        var p_masterAsset = new DynamicParameters();
                        p_masterAsset.Add("@Table_Name", "Master_Asset");
                        p_masterAsset.Add("@TextSearch1", data.QuarterMasterID);
                        var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterAsset, commandType: CommandType.StoredProcedure).ToList();

                        foreach (var org in dataOrg)
                        {
                            List<Risk> colsRiskofCeo = new List<Risk>();
                            //Get Childe of BU
                            var orgChilde = _o.GetOrgALLChildByOrgID_DB(org, data.QuarterMasterID);
                            var colStr = "";
                            if (orgChilde.Count > 0)
                            {
                                //loop childe
                                foreach (var _org in orgChilde)
                                {
                                    if (_org.ORGANIZATION_LEVEL != "Section")
                                    {
                                        List<Asset_Menu> asset_Model = new List<Asset_Menu>();
                                        var _resultAsset = mAsset.Where(o => o.Asset_Org == _org.ORGANIZATION_ID).ToList();
                                        if (_resultAsset.Count > 0)
                                        {
                                            foreach (var items in _resultAsset)
                                            {
                                                if (items.Asset_Level != "Section")
                                                {
                                                    //add asset of bu to colStr
                                                    if (!_h.CheckNull(colStr))
                                                        colStr += ",";
                                                    colStr += items.Asset_Code;
                                                }
                                            }
                                        }
                                        //add bu to colStr
                                        if (!_h.CheckNull(colStr))
                                            colStr += ",";
                                        colStr += _org.ORGANIZATION_ID;
                                    }
                                }
                            }
                            var p = new DynamicParameters();
                            p.Add("@QuarterID", data.QuarterID);
                            p.Add("@WPBID", data.WPBID);
                            p.Add("@Bowtie", "RISKBYBU");
                            p.Add("@TempBUId", colStr);
                            p.Add("@Role", "Transection");
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            var colRiskofCeo = _g.GetCollenctionRiskData(p, "Transection");
                            if (!_h.IsNullOrEmpty(colRiskofCeo.Risk))
                            {
                                if (!_h.CheckNull(data.QuarterID))
                                {
                                    //transection is quarter
                                    var _resultRisk = colRiskofCeo.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                    //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                    org.RiskItems = _resultRisk;
                                    //response.Status = true;
                                }
                                else if (!_h.CheckNull(data.WPBID))
                                {
                                    //transection is wpb
                                    var _resultRisk = colRiskofCeo.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                    //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                    org.RiskItems = _resultRisk;
                                    //response.Status = true;
                                }
                                else
                                {
                                    //transection is all
                                    //colsRisk = colsRisk.Concat(colRisk.Risk).ToList();
                                    org.RiskItems = colRiskofCeo.Risk;
                                    //response.Status = true;
                                }
                            }
                            //org.RiskItems = colsRiskofCeo.Concat(colRiskofCeo.Risk).ToList();
                        }
                        #endregion
                        #region build str childe org pttep
                        var p_pttep = new DynamicParameters();
                        Organizations orgPttep = new Organizations();
                        List<Risk> colsRiskofPttep = new List<Risk>();
                        orgPttep.ORGANIZATION_ID = "PTTEP";
                        orgPttep.NAME = "PTTEP";
                        p_pttep.Add("@QuarterID", data.QuarterID);
                        p_pttep.Add("@WPBID", data.WPBID);
                        p_pttep.Add("@Bowtie", "RISKBYBU");
                        p_pttep.Add("@TempBUId", "PTTEP");
                        p_pttep.Add("@Role", "Transection");
                        p_pttep.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        p_pttep.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var colRiskofPttep = _g.GetCollenctionRiskData(p_pttep, "Transection");
                        if (!_h.IsNullOrEmpty(colRiskofPttep.Risk))
                        {
                            if (!_h.CheckNull(data.QuarterID))
                            {
                                //transection is quarter
                                var _resultRisk = colRiskofPttep.Risk.Where(o => !_h.CheckNull(o.QuarterID) && _h.CheckNull(o.WPBID)).ToList();
                                //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                orgPttep.RiskItems = _resultRisk;
                                //response.Status = true;
                            }
                            else if (!_h.CheckNull(data.WPBID))
                            {
                                //transection is wpb
                                var _resultRisk = colRiskofPttep.Risk.Where(o => !_h.CheckNull(o.QuarterID) && !_h.CheckNull(o.WPBID)).ToList();
                                //colsRisk = colsRisk.Concat(_resultRisk).ToList();
                                orgPttep.RiskItems = _resultRisk;
                                //response.Status = true;
                            }
                            else
                            {
                                //transection is all
                                //colsRisk = colsRisk.Concat(colRisk.Risk).ToList();
                                orgPttep.RiskItems = colRiskofPttep.Risk;
                                //response.Status = true;
                            }
                        }
                        //orgPttep.RiskItems = colsRiskofPttep.Concat(colRiskofPttep.Risk).ToList();
                        dataOrg.Add(orgPttep);
                        #endregion
                        response.body = dataOrg;
                        response.Status = true;
                    }
                    else
                    {
                        response.Status = false;
                        response.ErrorMessage = "[API_Report_Dashboard_Status]" + "Can't not get bu of ceo";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[API_Report_Dashboard_Status]" + ex.Message;
            }

            return response;
        }
    }
}
