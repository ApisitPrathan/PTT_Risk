using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PTTEP_Risk.Help;
using PTTEP_Risk.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace PTTEP_Risk.Repo
{
    public class SeacrhRepo
    {
        GetUser _s = new GetUser();
        Helper _h = new Helper();
        GetOrganizations _o = new GetOrganizations();
        RiskMap_Organization _m = new RiskMap_Organization();
        ConfigurationService _c = new ConfigurationService();
        public ResponseMessage<object> API_RiskSeacrh(RequestMessage<SeacrhModel> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            var data = request.body;
            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    var p_masterAsset = new DynamicParameters();
                    p_masterAsset.Add("@Table_Name", "Master_Asset");
                    p_masterAsset.Add("@TextSearch1", data.QuaterMaster);
                    var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterAsset, commandType: CommandType.StoredProcedure).ToList();
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
                        List<RiskMap_Menu> org = _m.Get_Childe_Organization(userInfo, data.QuaterMaster);
                        if (org.Count > 0)
                        {
                            //builde string org
                            string colBU = _o.GetRecursiveOrganizationFromRiskMap(org[0]);
                            var p = new DynamicParameters();
                            p.Add("@Risk_Category", data.Risk_Category);
                            p.Add("@Risk_Status", data.Risk_Status);
                            //p.Add("@Risk_Business_Unit", data.Risk_Business_Unit);
                            p.Add("@Risk_Name", data.Risk_Name);
                            p.Add("@Risk_Register_By", data.Risk_Register_By);
                            p.Add("@Risk_Register_Date_From", data.Risk_Register_Date_From);
                            p.Add("@Risk_Register_Date_To", data.Risk_Register_Date_To);
                            p.Add("@Risk_Running", data.Risk_Running);
                            p.Add("@Risk_Rating", data.Risk_Rating);
                            p.Add("@Risk_Escalation", data.Risk_Escalation);
                            p.Add("@Filter_Table", data.Filter_Table);
                            p.Add("@QuarterID", data.QuarterID);
                            p.Add("@WPBID", data.WPBID);
                            p.Add("@BUCollection", colBU);
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output, size: 255);
                            List<SeacrhResult> seacrhResults = conn.Query<SeacrhResult>("sp_Risk_Seacrh", p, commandType: CommandType.StoredProcedure).ToList();
                            if (seacrhResults.Count > 0)
                            {
                                if (!_h.CheckNull(data.Risk_Business_Unit))//if filter Unit
                                {
                                    if (data.Child_Node == "0") //Not inculde childe
                                    {
                                        List<SeacrhResult> _result = new List<SeacrhResult>();
                                        if (data.Consolidate == "0") // not include Console of BU
                                        {
                                            _result = seacrhResults.Where(o => o.Risk_Business_Unit == data.Risk_Business_Unit).ToList();
                                        }
                                        else // include Console of BU
                                        {
                                            _result = seacrhResults.Where(o => (o.Risk_Business_Unit == data.Risk_Business_Unit)
                                                                                || (o.FG_Unit == data.Risk_Business_Unit && o.FG_Flag == "Y")
                                                                                || (o.DI_Unit == data.Risk_Business_Unit && o.DI_Flag == "Y")
                                                                                || (o.BU_Unit == data.Risk_Business_Unit && o.BU_Flag == "Y")
                                                                                || (o.Asset_Unit == data.Risk_Business_Unit && o.Asset_Flag == "Y")
                                                                          ).ToList();
                                                                                   

                                        }
                                        
                                        if (_result.Count > 0)//have org unit in seacrhResults
                                        {
                                            response.body = _result;
                                            response.StatusId = p.Get<int>("@StatusChange");
                                            if (response.StatusId == 0)
                                            {
                                                response.ErrorMessage = p.Get<string>("@StatusText");
                                                response.Status = true;
                                            }
                                            else
                                            {
                                                response.ErrorMessage = p.Get<string>("@StatusText");
                                                response.Status = false;
                                            } 
                                        }
                                        else//no data seacrh of org 
                                        {

                                            response.ErrorMessage = "No data report !";
                                            response.Status = false;
                                        }
                                    }
                                    else//Inculde childe
                                    {
                                        //Check Org Level
                                        var p_Org = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", data.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService)
                                        };
                                        var orgInfo = _o.GetOrganization(p_Org);
                                        //Get Childe of BU
                                        var orgChilde = _o.GetOrgALLChildByOrgID_DB(orgInfo[0], data.QuaterMaster);
                                        if (orgChilde.Count > 0)
                                        {
                                            List<SeacrhResult> seacrhResultsChilde = new List<SeacrhResult>();
                                            //loop childe
                                            foreach (var _org in orgChilde)
                                            {
                                                if (_org.ORGANIZATION_LEVEL != "Section")
                                                {
                                                    List<Asset_Menu> asset_Model = new List<Asset_Menu>();
                                                    //find asset of org childe
                                                    var _resultAsset = mAsset.Where(o => o.Asset_Org == _org.ORGANIZATION_ID).ToList();
                                                    if (_resultAsset.Count > 0)
                                                    {
                                                        //find transection asset of org childe in seacrhResults
                                                        var _resiltOfAsset = seacrhResults.Where(x => _resultAsset.Any(r => r.Asset_Code == x.Risk_Business_Unit)).ToList();                                                      
                                                        seacrhResultsChilde.AddRange(_resiltOfAsset);
                                                    }
                                                    //find transecton org of org childe in seacrhResults
                                                    List<SeacrhResult> _resiltOfOrg = new List<SeacrhResult>();
                                                    if (data.Consolidate == "0") // not include Console of BU
                                                    {
                                                        _resiltOfOrg = seacrhResults.Where(x => x.Risk_Business_Unit == _org.ORGANIZATION_ID).ToList();
                                                    }
                                                    else
                                                    {
                                                        _resiltOfOrg = seacrhResults.Where(x => (x.Risk_Business_Unit == _org.ORGANIZATION_ID)
                                                                                                || (x.FG_Unit == _org.ORGANIZATION_ID && x.FG_Flag == "Y")
                                                                                                || (x.DI_Unit == _org.ORGANIZATION_ID && x.DI_Flag == "Y")
                                                                                                || (x.BU_Unit == _org.ORGANIZATION_ID && x.BU_Flag == "Y")
                                                                                                || (x.Asset_Unit == _org.ORGANIZATION_ID && x.Asset_Flag == "Y")
                                                                                           ).ToList();
                                                    }
                                                        
                                                    seacrhResultsChilde.AddRange(_resiltOfOrg);
                                                }
                                            }
                                            if (seacrhResultsChilde.Count > 0)//have transection of org childe
                                            {
                                                response.body = seacrhResultsChilde;
                                                response.StatusId = p.Get<int>("@StatusChange");
                                                if (response.StatusId == 0)
                                                {
                                                    response.ErrorMessage = p.Get<string>("@StatusText");
                                                    response.Status = true;
                                                }
                                                else
                                                {
                                                    response.ErrorMessage = p.Get<string>("@StatusText");
                                                    response.Status = false;
                                                }
                                            }
                                            else//have transection of org childe but can't view 
                                            {
                                                if (seacrhResults.Count > 0)
                                                {
                                                    response.ErrorMessage = "Can't view risk of business unit";
                                                    response.Status = false;
                                                }
                                                else
                                                {
                                                    response.ErrorMessage = "No data report !";
                                                    response.Status = false;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            response.ErrorMessage = "Don't have organization childe";
                                            response.Status = false;
                                        }
                                    }
                                }
                                else// return all transection
                                {
                                    response.body = seacrhResults;
                                    response.StatusId = p.Get<int>("@StatusChange");
                                    if (response.StatusId == 0)
                                    {
                                        //response.ErrorMessage = p.Get<string>("@StatusText");
                                        response.ErrorMessage = seacrhResults.Count.ToString();
                                        response.Status = true;
                                    }
                                    else
                                    {
                                        response.ErrorMessage = p.Get<string>("@StatusText");
                                        response.Status = false;
                                    }
                                }
                            }
                            else
                            {
                                response.ErrorMessage = "No data report !";
                                response.Status = false;
                            }
                        }
                    }
                    else
                    {
                        response.ErrorMessage = "[API_RiskSeacrh]" + "Can't find employee in webservice";
                        response.Status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[Check_Role_Employee]" + ex.Message;
            }
            return response;
        }


    }
}
