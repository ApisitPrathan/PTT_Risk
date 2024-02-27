using System;
using System.Collections.Generic;
using System.Text;
using PTTEP_Risk.Help;
using PTTEP_Risk.Model;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PTTEP_Risk.Repo
{
    public class RiskMapRepo
    {
        Helper _helper = new Helper();
        GetUser _s = new GetUser();
        GetOrganizations _o = new GetOrganizations();
        RiskMap_Organization _menu = new RiskMap_Organization();
        ConfigurationService _c = new ConfigurationService();
        public ResponseMessage<object> API_Get_Impact_Riskmap_BU(RiskMap_Impact request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();

            var p = new DynamicParameters();

            try
            {
                p.Add("@QuarterID", request.QuarterID);
                p.Add("@BusinessCode", request.BusinessCode);
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.Status = true;
                    response.body = conn.Query<object>("sp_Get_Financial_Impact", p, commandType: CommandType.StoredProcedure).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Get_Financial_Impact]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Cal_RiskMap_BU(RiskMap_Impact request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();

            var p = new DynamicParameters();

            try
            {
                p.Add("@QuarterID", request.QuarterID);
                p.Add("@WPBID", request.WPBID);
                p.Add("@QuaterMaster", request.QuaterMaster);
                p.Add("@BusinessCode", request.BusinessCode);
                p.Add("@Year", request.Year);
                p.Add("@TypeMitigate", request.TypeMitigate);
                if(!_helper.CheckNull(request.Risk_Level))
                    p.Add("@Risk_Level", request.Risk_Level);
                if (!_helper.CheckNull(request.Risk_Escalation))
                    p.Add("@Risk_Escalation", request.Risk_Escalation);
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.Status = true;
                    response.body = conn.Query<object>("sp_Cal_RiskMap_BU", p, commandType: CommandType.StoredProcedure).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Cal_RiskMap_BU]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Get_Impact_Riskmap_Unit(RiskMap_Impact request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();

            var p = new DynamicParameters();

            try
            {
                p.Add("@BusinessCode", request.BusinessCode);
                p.Add("@QuaterID", request.QuarterID);
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.Status = true;
                    response.body = conn.Query<object>("sp_Get_Financial_Impact", p, commandType: CommandType.StoredProcedure).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Get_Financial_Impact]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Cal_RiskMap_Unit(RiskMap_Unit request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();

            var p = new DynamicParameters();

            try
            {
                p.Add("@FilterUser", request.FilterUser);
                p.Add("@BUCode", request.BUCode);
                p.Add("@Role", request.Role.ToUpper());
                p.Add("@Module", request.Module.ToUpper());
                p.Add("@QuarterID", request.QuarterID);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.Status = true;
                    var _result = conn.Query<RiskMap_Unit_Result>("sp_Cal_RiskMap_Unit", p, commandType: CommandType.StoredProcedure).ToList();
                    if (_result.Count > 0)
                    {
                        if (request.Filter_Type == "1") //Quester
                        {
                            var filterQuester = _result.Where(o => o.QuarterID != null && o.WPBID == null).ToList();
                            response.body = filterQuester;
                            response.Status = true;
                        }
                        else if (request.Filter_Type == "2")//WPB
                        {
                            var filterWPB = _result.Where(o => o.QuarterID != null && o.WPBID != null).ToList();
                            response.body = filterWPB;
                            response.Status = true;
                        }
                        else
                        {
                            response.body = _result;
                            response.Status = true;
                        }
                    }
                    else
                    {
                        response.body = null;
                        response.Status = true;
                        response.ErrorMessage = "[sp_Cal_RiskMap_Unit] result is null";
                    }
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Cal_RiskMap_Unit]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Get_Risk_Unit(RiskMap_Unit request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();

            var p = new DynamicParameters();

            try
            {
                p.Add("@FilterUser", request.FilterUser);
                p.Add("@BUCode", request.BUCode);
                p.Add("@Role", request.Role.ToUpper());
                p.Add("@Module", request.Module.ToUpper());
                p.Add("@QuarterID", request.QuarterID);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.Status = true;
                    var _result = conn.Query<Risk_Unit_Result>("sp_Get_Risk_Unit", p, commandType: CommandType.StoredProcedure).ToList();
                    if (_result.Count > 0)
                    {
                        if (request.Filter_Type == "1") //Quester
                        {
                            var filterQuester = _result.Where(o => o.QuarterID != null && o.WPBID == null).ToList();
                            if (filterQuester.Count > 0)
                            {
                                foreach (var h in filterQuester)
                                {
                                    var p_History = new DynamicParameters();
                                    p_History.Add("@Table", h.Table_Type);
                                    p_History.Add("@Risk_Id", h.Risk_Id);
                                    var history = conn.Query<History>("sp_Risk_History", p_History, commandType: CommandType.StoredProcedure).ToList();
                                    if (history.Count > 0)
                                        h.History = history;
                                }
                                response.body = filterQuester;
                                response.Status = true;
                            }
                        }
                        else if (request.Filter_Type == "2")//WPB
                        {
                            var filterWPB = _result.Where(o => o.QuarterID != null && o.WPBID != null).ToList();
                            if (filterWPB.Count > 0)
                            {
                                foreach (var h in filterWPB)
                                {
                                    var p_History = new DynamicParameters();
                                    p_History.Add("@Table", h.Table_Type);
                                    p_History.Add("@Risk_Id", h.Risk_Id);
                                    var history = conn.Query<History>("sp_Risk_History", p_History, commandType: CommandType.StoredProcedure).ToList();
                                    if (history.Count > 0)
                                        h.History = history;
                                }
                                response.body = filterWPB;
                                response.Status = true;
                            }
                        }
                        else
                        {

                            foreach (var h in _result)
                            {
                                var p_History = new DynamicParameters();
                                p_History.Add("@Table", h.Table_Type);
                                p_History.Add("@Risk_Id", h.Risk_Id);
                                var history = conn.Query<History>("sp_Risk_History", p_History, commandType: CommandType.StoredProcedure).ToList();
                                if (history.Count > 0)
                                    h.History = history;
                            }
                            response.body = _result;
                            response.Status = true;
                        }
                    }
                    else
                    {
                        response.body = null;
                        response.Status = true;
                        response.ErrorMessage = "[sp_Cal_RiskMap_Unit] result is null";
                    }
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[API_Get_Risk_Unit]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Get_Menu_RiskMap(ResponseMessage<RiskMap_Menu> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var root = builder.Build();
            var data = request.body;
            //Get User From Service
            var paramsEmp = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("EmployeeID", ""),
                new KeyValuePair<string, string>("FirstName", ""),
                new KeyValuePair<string, string>("LastName", ""),
                new KeyValuePair<string, string>("EmailAddress", request.Email),
                new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
            };
            var userInfo = _s.GetEmployee(paramsEmp);
            if (userInfo.Count > 0)
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    var p_Check = new DynamicParameters();
                    p_Check.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                    p_Check.Add("@Type", "Check");
                    p_Check.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    var dataERM = conn.Query<ERM>("sp_Check_Role_ERM", p_Check, commandType: CommandType.StoredProcedure).ToList();
                    if (dataERM.Count > 0)
                    {
                        var p_Get = new DynamicParameters();
                        p_Get.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                        p_Get.Add("@Type", "GETORG");
                        p_Get.Add("@QuarterID", data.QuarterID);
                        p_Get.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        var dataOrg = conn.Query<Organizations>("sp_Check_Role_ERM", p_Get, commandType: CommandType.StoredProcedure).ToList();
                        if (dataOrg.Count > 0)
                        {
                            //get information employee xml from web service GetEmployee
                            try
                            {
                                response.Status = true;
                                response.body = _menu.Get_RiskMap_Organization(dataOrg,data.QuarterID);
                            }
                            catch (Exception ex)
                            {
                                response.Status = false;
                                response.ErrorMessage = "[API_Get_Menu_RiskMap]" + ex.Message;
                            }
                        }
                    }
                    else // not ERM
                    {
                        //Check Owner Emp Id
                        var p_CheckActingEmpId = new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string, string>("EmployeeID", userInfo[0].EMPLOYEE_ID),
                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                        };
                        var empOwnerActing = _o.GetActingOrganization(p_CheckActingEmpId);
                        if (empOwnerActing.Count > 0)
                        {
                            //check is owner section
                            if (empOwnerActing[0].ORGANIZATION_LEVEL == "Section")
                            {
                                var p_Org = new List<KeyValuePair<string, string>>() {
                                    new KeyValuePair<string, string>("OrganizetionID", userInfo[0].ORGUNIT),
                                    new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService)
                                };
                                var orgInfo = _o.GetOrganization(p_Org);
                                if (orgInfo.Count > 0)
                                {
                                    response.Status = true;
                                    response.body = _menu.Get_RiskMap_Organization(orgInfo, data.QuarterID);
                                }
                                else
                                {
                                    response.Status = false;
                                    response.ErrorMessage = "[API_Get_Menu_RiskMap]" + " Can't get Organization from web service";
                                }
                            }
                            else // is not owner section
                            {
                                var p_Get = new DynamicParameters();
                                p_Get.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                                p_Get.Add("@Type", "GETORG");
                                p_Get.Add("@QuarterID", data.QuarterID);
                                p_Get.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                var dataOrg = conn.Query<Organizations>("sp_Check_Role_ERM", p_Get, commandType: CommandType.StoredProcedure).ToList();
                                if (dataOrg.Count > 0)
                                {
                                    //get information employee xml from web service GetEmployee
                                    response.Status = true;
                                    response.body = _menu.Get_RiskMap_Organization(dataOrg, data.QuarterID);
                                }
                                else
                                {
                                    response.Status = false;
                                    response.ErrorMessage = "[API_Get_Menu_RiskMap]" + " Can't get Organization from sp_Check_Role_ERM";
                                }
                            }
                        }
                        else // is not Owner
                        {
                            //is't Erm
                            //Check Org Level
                            var p_Org = new List<KeyValuePair<string, string>>() {
                                new KeyValuePair<string, string>("OrganizetionID", userInfo[0].ORGUNIT),
                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService)
                            };
                            var orgInfo = _o.GetOrganization(p_Org);
                            if (orgInfo.Count > 0)
                            {
                                response.Status = true;
                                response.body = _menu.Get_RiskMap_Organization(orgInfo, data.QuarterID);
                            }
                            else
                            {
                                response.Status = false;
                                response.ErrorMessage = "[API_Get_Menu_RiskMap]" + " Can't get Organization from web service";
                            }
                        }
                    }
                }
            }
            else
            {
                response.Status = false;
                response.ErrorMessage = "[API_Get_Menu_RiskMap]" + " Can't get information from web service";
            }

            return response;
        }
    }
}
