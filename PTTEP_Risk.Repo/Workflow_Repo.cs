using System;
using System.Collections.Generic;
using System.Text;
using PTTEP_Risk.Help;
using PTTEP_Risk.Model;
using PTTEP_Risk.Repo;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace PTTEP_Risk.Repo
{
    public class Workflow_Repo
    {
        Helper _helper = new Helper();
        EmailNotification _e = new EmailNotification();
        GetOrganizations _o = new GetOrganizations();
        public ResponseMessage<object> API_Risk_Workflow_Submit(RequestMessage<SubmitModel> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var root = builder.Build();
            var pOrg = new DynamicParameters();
            var data = request.body;
            try
            {
                var ModuleData = root.GetSection("AppConfiguration")["ModuleData"].ToString();
                var SendEmail = bool.Parse(root.GetSection("AppConfiguration")["SendEmail"].ToString());
                //string assignTo = "", orgSubmit = "";
                if (request.body.Risk_Submit_Workflow.ToUpper() == "STAFF")// staff submit
                {
                    using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                    {
                        conn.Open();
                        var p = new DynamicParameters();
                        List<TypeAssetModel> insertAssetModel = new List<TypeAssetModel>();
                        List<TypeOrganizationModel> insertOrgModel = new List<TypeOrganizationModel>();
                        p.Add("@Risk_Modified_By", request.body.Risk_Modified_By);
                        p.Add("@Risk_Submit_Workflow", request.body.Risk_Submit_Workflow);
                        if (!_helper.IsNullOrEmpty(data.SubmitTypeAsset))
                        {
                            var p_masterasset = new DynamicParameters();
                            p_masterasset.Add("@Table_Name", "Master_Asset");
                            var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterasset, commandType: CommandType.StoredProcedure).ToList();
                            List<TypeAssetModel> tempAsset = data.SubmitTypeAsset;
                            
                            foreach (var items in tempAsset)
                            {
                                if (!_helper.CheckNull(items.Risk_Status_Workflow))
                                {
                                    if (items.Risk_Status_Workflow == "0" || items.Risk_Status_Workflow == "23")//Draff or Waiting for More Assessment
                                    {
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(_result);
                                            TypeAssetModel temp = new TypeAssetModel();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                            temp.Risk_Business_Unit_WF = _result[0].Asset_Org;
                                            temp.Risk_Business_Unit_WF_Level = _result[0].Asset_Level;
                                            temp.Risk_Status_Workflow = "1";// Waiting CO-Asset
                                            temp.Risk_Type = "Asset";
                                            temp.Risk_AssignTo = assignTo;
                                            insertAssetModel.Add(temp);
                                        }
                                    }
                                }
                            }
                            p.Add("@SubmitTypeAsset", _helper.ToDataTable(insertAssetModel).AsTableValuedParameter());
                        }
                        if (!_helper.IsNullOrEmpty(data.SubmitTypeOrganization))
                        {
                            //get Master_Cordinator 
                            var p_masterco = new DynamicParameters();
                            p_masterco.Add("@Table_Name", "Master_Cordinator");
                            var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterco, commandType: CommandType.StoredProcedure).ToList();
                            List<TypeOrganizationModel> tempOrg = data.SubmitTypeOrganization;
                            
                            foreach (var items in tempOrg)//loop SubmitTypeOrganization
                            {
                                if (!_helper.CheckNull(items.Risk_Status_Workflow))
                                {
                                    if (items.Risk_Status_Workflow == "0" || items.Risk_Status_Workflow == "23")//Draff or Waiting for More Assessment
                                    {
                                        var _tempCo = mCo.Where(o => o.Coordinator_Department_Id == items.Risk_Business_Unit).ToList();
                                        if (_tempCo.Count() > 0)//have co in Master_Cordinator 
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                            TypeOrganizationModel temp = new TypeOrganizationModel();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                            temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                            temp.Risk_Business_Unit_WF_Level = _tempCo[0].Coordinator_Level;
                                            if (_tempCo[0].Coordinator_Level == "Department")
                                                temp.Risk_Status_Workflow = "4";// Waiting CO for parent org Department
                                            else if (_tempCo[0].Coordinator_Level == "Division")
                                                temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                            else if (_tempCo[0].Coordinator_Level == "Group")
                                                temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                            temp.Risk_Type = "Organization";
                                            temp.Risk_AssignTo = assignTo;
                                            insertOrgModel.Add(temp);
                                        }
                                        else//no co in Master_Cordinator
                                        {
                                            //get parent org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                    new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                                    new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)//have parent org
                                            {
                                                //List<Master_CO> _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                    if (_tempCo.Count > 0)//is match CO
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)//have Co match is parent org
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    TypeOrganizationModel temp = new TypeOrganizationModel();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Business_Unit_WF_Level = _tempCo[0].Coordinator_Level;
                                                    if (_tempCo[0].Coordinator_Level == "Department")
                                                        temp.Risk_Status_Workflow = "4";// Waiting CO for parent org Department
                                                    else if (_tempCo[0].Coordinator_Level == "Division")
                                                        temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                                    else if (_tempCo[0].Coordinator_Level == "Group")
                                                        temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                                    temp.Risk_Type = "Organization";
                                                    temp.Risk_AssignTo = assignTo;
                                                    insertOrgModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            p.Add("@SubmitTypeOrganization", _helper.ToDataTable(insertOrgModel).AsTableValuedParameter());
                        }
                        /*if (SendEmail)
                        {
                            _e.SubmitProcessMail(insertAssetModel, insertOrgModel, "Staff");
                        }*/
                        if (insertAssetModel.Count > 0 || insertOrgModel.Count > 0)
                        {
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                            response.body = conn.Query<object>("sp_Risk_Workflow_Submit_WS", p, commandType: CommandType.StoredProcedure).ToList();
                            response.StatusId = p.Get<int>("@StatusChange");
                            if (response.StatusId == 0)
                            {
                                /*if (SendEmail)
                                {
                                    _e.SendEmail(insertAssetModel, insertOrgModel, null, null);
                                }*/
                                if (SendEmail)
                                {
                                    _e.SubmitProcessMail(insertAssetModel, insertOrgModel, "Staff");
                                }
                                response.ErrorMessage = p.Get<string>("@StatusText");
                                response.Status = true;
                            }
                            else
                            {
                                response.ErrorMessage = p.Get<string>("@StatusText");
                                response.Status = false;
                            }
                        }
                        else // Not have next Bu Assign
                        {
                            response.ErrorMessage = "Please contact to HEM Focal Point.  Risk Coordinator of next BU level has not assigned.";
                            response.Status = false;
                        }
                    }
                }
                else if (request.body.Risk_Submit_Workflow.ToUpper() == "CO") // co submit
                {
                    using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                    {
                        conn.Open();
                        
                        var p = new DynamicParameters();
                        List<TypeAssetModel> insertAssetModel = new List<TypeAssetModel>();
                        List<TypeOrganizationModel> insertOrgModel = new List<TypeOrganizationModel>();
                        p.Add("@Risk_Modified_By", request.body.Risk_Modified_By);
                        p.Add("@Risk_Submit_Workflow", request.body.Risk_Submit_Workflow);
                        if (!_helper.IsNullOrEmpty(data.SubmitTypeAsset))
                        {
                            var p_masterAsset = new DynamicParameters();
                            p_masterAsset.Add("@Table_Name", "Master_Asset");
                            var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterAsset, commandType: CommandType.StoredProcedure).ToList();
                            List<TypeAssetModel> tempAsset = data.SubmitTypeAsset;
                            bool isError = false;
                            //List<TypeAssetModel> insertModel = new List<TypeAssetModel>();
                            foreach (var items in tempAsset)
                            {
                                //check data is current workflow status 
                                if (!_helper.CheckNull(items.Risk_Status_Workflow) && _helper.CheckCurrentWorklfow(items.Risk_Id, items.Risk_Status_Workflow, "Transection",""))
                                {
                                    if (items.Risk_Status_Workflow == "0" || items.Risk_Status_Workflow == "23")//Draff
                                    {
                                        var _tempAsset = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_tempAsset.Count() > 0)
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(_tempAsset);
                                            if (!assignTo.Contains(data.Risk_Modified_By)) // check Risk_Modified_By is Not Co Config
                                            {

                                                TypeAssetModel temp = new TypeAssetModel();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempAsset[0].Asset_Org;
                                                temp.Risk_Business_Unit_WF_Level = _tempAsset[0].Asset_Level;
                                                temp.Risk_Status_Workflow = "1";// Waiting CO-Asset
                                                temp.Risk_Type = "Asset";
                                                temp.Risk_AssignTo = assignTo;
                                                insertAssetModel.Add(temp);
                                            }
                                            else // is Co config asset Create
                                            {
                                                //get parent org Config
                                                var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                    new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                    new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                };
                                                var orgAsset = _o.GetOrganization(paramsOrg);
                                                if (orgAsset.Count > 0)
                                                {

                                                    TypeAssetModel temp = new TypeAssetModel();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = orgAsset[0].ORGANIZATION_ID;
                                                    temp.Risk_Business_Unit_WF_Level = orgAsset[0].ORGANIZATION_LEVEL;
                                                    temp.Risk_Status_Workflow = "3";// Waiting owner config Approve 
                                                    temp.Risk_Type = "Asset";
                                                    if (!orgAsset[0].HeadActing[0].ACTING_STATUS)
                                                        temp.Risk_AssignTo = orgAsset[0].HeadActing[0].HEAD_ID;
                                                    else
                                                        temp.Risk_AssignTo = orgAsset[0].HeadActing[0].ACTING_HEAD_ID;
                                                    insertAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                    else//other submit find owner parent org
                                    {
                                        //get org from asset master config
                                        var _tempAsset = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_tempAsset.Count() > 0)
                                        {
                                            if (items.Risk_Status_Workflow == "2" || items.Risk_Status_Workflow == "15")//CO Asset Submit Or Reject find owner config 
                                            {
                                                //get parent org
                                                var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                    new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                    new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                };
                                                var orgAsset = _o.GetOrganization(paramsOrg);
                                                if (orgAsset.Count > 0)
                                                {
                                                    TypeAssetModel temp = new TypeAssetModel();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = orgAsset[0].ORGANIZATION_ID;
                                                    temp.Risk_Business_Unit_WF_Level = orgAsset[0].ORGANIZATION_LEVEL;
                                                    temp.Risk_Status_Workflow = "3"; //Owner-Org Config Draft
                                                    temp.Risk_Type = "Asset";
                                                    if (!orgAsset[0].HeadActing[0].ACTING_STATUS)
                                                        temp.Risk_AssignTo = orgAsset[0].HeadActing[0].HEAD_ID;
                                                    else
                                                        temp.Risk_AssignTo = orgAsset[0].HeadActing[0].ACTING_HEAD_ID;
                                                    insertAssetModel.Add(temp);
                                                }
                                            }
                                            else if (items.Risk_Status_Workflow == "5" || items.Risk_Status_Workflow == "16" || items.Risk_Status_Workflow == "34")//CO BU Submit Or Reject find owner BU org
                                            {
                                                if ((!_helper.CheckNull(items.Risk_Submit_Action) ? items.Risk_Submit_Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                                {
                                                    //get parent org asset
                                                    var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                        new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                        new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                    };
                                                    var orgAsset = _o.GetOrganization(paramsOrg);
                                                    if (orgAsset.Count > 0)
                                                    {
                                                        TypeAssetModel temp = new TypeAssetModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = orgAsset[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = orgAsset[0].ORGANIZATION_LEVEL;
                                                        temp.Risk_Status_Workflow = "28";// Waiting Owner Department Approvve (ReConsole)
                                                        temp.Risk_Type = "Asset";
                                                        if (!orgAsset[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = orgAsset[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = orgAsset[0].HeadActing[0].ACTING_HEAD_ID;
                                                        temp.Risk_Submit_Action = items.Risk_Submit_Action.ToUpper();
                                                        temp.Risk_Submit_Reason = items.Risk_Submit_Reason;
                                                        insertAssetModel.Add(temp);
                                                    }
                                                }
                                                else //normal case
                                                {
                                                    //get parent org asset
                                                    var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                        new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                        new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                    };
                                                    var orgAsset = _o.GetOrganization(paramsOrg);
                                                    if (orgAsset.Count > 0)
                                                    {
                                                        TypeAssetModel temp = new TypeAssetModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = orgAsset[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = orgAsset[0].ORGANIZATION_LEVEL;
                                                        if (orgAsset[0].ORGANIZATION_LEVEL == "Department")
                                                            temp.Risk_Status_Workflow = "6";// Waiting owner for parent org Department
                                                        /*else if (orgAsset[0].ORGANIZATION_LEVEL == "Division")
                                                            temp.Risk_Status_Workflow = "9";// Waiting owner for parent org Division
                                                        else if (orgAsset[0].ORGANIZATION_LEVEL == "Group")
                                                            temp.Risk_Status_Workflow = "12";// Waiting owner for parent org Group*/
                                                        temp.Risk_Type = "Asset";
                                                        if (!orgAsset[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = orgAsset[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = orgAsset[0].HeadActing[0].ACTING_HEAD_ID;
                                                        insertAssetModel.Add(temp);
                                                    }
                                                }
                                            }
                                            else if (items.Risk_Status_Workflow == "8" || items.Risk_Status_Workflow == "17" || items.Risk_Status_Workflow == "35")//CO DI Submit Or Reject find owner Dvision org
                                            {
                                                if ((!_helper.CheckNull(items.Risk_Submit_Action) ? items.Risk_Submit_Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                                {
                                                    //get parent org asset
                                                    var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                        new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                        new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                    };
                                                    List<Organizations> org = new List<Organizations>();
                                                    List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                                    if (orgparentInfo.Count > 0)
                                                    {
                                                        //get Owner division from asset config
                                                        var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Division").ToList();
                                                        if (_tempOrg.Count > 0)
                                                        {
                                                            TypeAssetModel temp = new TypeAssetModel();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                            temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                            temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                            temp.Risk_Status_Workflow = "29";// Waiting Owner Division Approvve (ReConsole)
                                                            temp.Risk_Type = "Asset";
                                                            if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                            else
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                            temp.Risk_Submit_Action = items.Risk_Submit_Action.ToUpper();
                                                            temp.Risk_Submit_Reason = items.Risk_Submit_Reason;
                                                            insertAssetModel.Add(temp);
                                                        }
                                                    }
                                                }
                                                else //normal case
                                                {
                                                    //get parent org asset
                                                    var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                        new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                        new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                    };
                                                    List<Organizations> org = new List<Organizations>();
                                                    List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                                    if (orgparentInfo.Count > 0)
                                                    {
                                                        //get Owner division from asset config
                                                        var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Division").ToList();
                                                        if (_tempOrg.Count > 0)
                                                        {
                                                            TypeAssetModel temp = new TypeAssetModel();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                            temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                            temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                            temp.Risk_Status_Workflow = "9";// Waiting Owner Division Approvve
                                                            temp.Risk_Type = "Asset";
                                                            if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                            else
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                            insertAssetModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (items.Risk_Status_Workflow == "11" || items.Risk_Status_Workflow == "18" || items.Risk_Status_Workflow == "36")//CO FG Submit Or Reject find owner FG org
                                            {
                                                if ((!_helper.CheckNull(items.Risk_Submit_Action) ? items.Risk_Submit_Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                                {
                                                    //get parent org asset
                                                    var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                        new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                        new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                    };
                                                    List<Organizations> org = new List<Organizations>();
                                                    List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                                    if (orgparentInfo.Count > 0)
                                                    {
                                                        //get Owner division from asset config
                                                        var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Group").ToList();
                                                        if (_tempOrg.Count > 0)
                                                        {
                                                            TypeAssetModel temp = new TypeAssetModel();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                            temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                            temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                            temp.Risk_Status_Workflow = "30";// Waiting Owner Group Approvve
                                                            temp.Risk_Type = "Asset";
                                                            if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                            else
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                            temp.Risk_Submit_Action = items.Risk_Submit_Action.ToUpper();
                                                            temp.Risk_Submit_Reason = items.Risk_Submit_Reason;
                                                            insertAssetModel.Add(temp);
                                                        }
                                                    }
                                                }
                                                else //normal case
                                                {
                                                    //get parent org asset
                                                    var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                        new KeyValuePair<string, string>("OrganizetionID", _tempAsset[0].Asset_Org),
                                                        new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                    };
                                                    List<Organizations> org = new List<Organizations>();
                                                    List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                                    if (orgparentInfo.Count > 0)
                                                    {
                                                        //get Owner division from asset config
                                                        var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Group").ToList();
                                                        if (_tempOrg.Count > 0)
                                                        {
                                                            TypeAssetModel temp = new TypeAssetModel();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                            temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                            temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                            temp.Risk_Status_Workflow = "12";// Waiting Owner Group Approvve
                                                            temp.Risk_Type = "Asset";
                                                            if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                            else
                                                                temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                            insertAssetModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    response.ErrorMessage = "Risk is not current workflow status!";
                                    response.Status = false;
                                    isError = true;
                                    break;
                                }

                            }
                            //check error
                            if(!isError)
                                p.Add("@SubmitTypeAsset", _helper.ToDataTable(insertAssetModel).AsTableValuedParameter());
                            else
                                insertAssetModel = new List<TypeAssetModel>();
                        }
                        if (!_helper.IsNullOrEmpty(data.SubmitTypeOrganization))
                        {
                            //get Master_Cordinator 
                            var p_masterco = new DynamicParameters();
                            p_masterco.Add("@Table_Name", "Master_Cordinator");
                            var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterco, commandType: CommandType.StoredProcedure).ToList();
                            List<TypeOrganizationModel> tempOrg = data.SubmitTypeOrganization;
                            bool isError = false;
                            //List<TypeOrganizationModel> insertModel = new List<TypeOrganizationModel>();
                            foreach (var items in tempOrg)//loop SubmitTypeOrganization
                            {
                                //check data is current workflow status 
                                if (!_helper.CheckNull(items.Risk_Status_Workflow) && _helper.CheckCurrentWorklfow(items.Risk_Id, items.Risk_Status_Workflow, "Transection",""))
                                {
                                    if (items.Risk_Status_Workflow == "0" || items.Risk_Status_Workflow == "23")//Draff
                                    {
                                        var _tempCo = mCo.Where(o => o.Coordinator_Department_Id == items.Risk_Business_Unit).ToList();
                                        if (_tempCo.Count() > 0)//have co in Master_Cordinator 
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                            if (!assignTo.Contains(data.Risk_Modified_By)) // co register is not co config
                                            {
                                                TypeOrganizationModel temp = new TypeOrganizationModel();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Business_Unit_WF_Level = _tempCo[0].Coordinator_Level;
                                                if (_tempCo[0].Coordinator_Level == "Department")
                                                    temp.Risk_Status_Workflow = "4";// Waiting CO for parent org Department
                                                else if (_tempCo[0].Coordinator_Level == "Division")
                                                    temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                                else if (_tempCo[0].Coordinator_Level == "Group")
                                                    temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                insertOrgModel.Add(temp);
                                            }
                                            else // is Risk_Modified_By is Co Config
                                            {
                                                //get parent org Co
                                                var paramsOrgCo = new List<KeyValuePair<string, string>>() {
                                                        new KeyValuePair<string, string>("OrganizetionID", _tempCo[0].Coordinator_Department_Id),
                                                        new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                    };
                                                var orgCo = _o.GetOrganization(paramsOrgCo);
                                                if (orgCo.Count > 0)
                                                {
                                                    TypeOrganizationModel temp = new TypeOrganizationModel();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = orgCo[0].ORGANIZATION_ID;
                                                    temp.Risk_Business_Unit_WF_Level = orgCo[0].ORGANIZATION_LEVEL;
                                                    if (orgCo[0].ORGANIZATION_LEVEL == "Department")
                                                        temp.Risk_Status_Workflow = "6";// Waiting Ownner Department Approve
                                                    else if (orgCo[0].ORGANIZATION_LEVEL == "Division")
                                                        temp.Risk_Status_Workflow = "9";// Waiting Ownner Division Approve
                                                    else if (orgCo[0].ORGANIZATION_LEVEL == "Group")
                                                        temp.Risk_Status_Workflow = "12";// Waiting Ownner Group Approve
                                                    temp.Risk_Type = "Organization";
                                                    if (!orgCo[0].HeadActing[0].ACTING_STATUS)
                                                        temp.Risk_AssignTo = orgCo[0].HeadActing[0].HEAD_ID;
                                                    else
                                                        temp.Risk_AssignTo = orgCo[0].HeadActing[0].ACTING_HEAD_ID;
                                                    insertOrgModel.Add(temp);
                                                }
                                            }
                                        }
                                        else//no co in Master_Cordinator
                                        {
                                            //get parent org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                    new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                                    new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)//have parent org
                                            {
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                    if (_tempCo.Count > 0)//is match CO
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)//have Co match is parent org
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    if (!assignTo.Contains(data.Risk_Modified_By)) // check Risk_Modified_By is Co Org parent 
                                                    {
                                                        
                                                        TypeOrganizationModel temp = new TypeOrganizationModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                        temp.Risk_Business_Unit_WF_Level = _tempCo[0].Coordinator_Level;
                                                        if (_tempCo[0].Coordinator_Level == "Department")
                                                            temp.Risk_Status_Workflow = "4";// Waiting CO for parent org Department
                                                        else if (_tempCo[0].Coordinator_Level == "Division")
                                                            temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                                        else if (_tempCo[0].Coordinator_Level == "Group")
                                                            temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                                        temp.Risk_Type = "Organization";
                                                        temp.Risk_AssignTo = assignTo;
                                                        insertOrgModel.Add(temp);
                                                    }
                                                    else // is Risk_Modified_By is Co Org parent 
                                                    {
                                                        //get parent org Co
                                                        var paramsOrgCo = new List<KeyValuePair<string, string>>() {
                                                                new KeyValuePair<string, string>("OrganizetionID", _tempCo[0].Coordinator_Department_Id),
                                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                                            };
                                                        var orgCo = _o.GetOrganization(paramsOrgCo);
                                                        if (orgCo.Count > 0)
                                                        {
                                                            TypeOrganizationModel temp = new TypeOrganizationModel();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                            temp.Risk_Business_Unit_WF = orgCo[0].ORGANIZATION_ID;
                                                            temp.Risk_Business_Unit_WF_Level = orgCo[0].ORGANIZATION_LEVEL;
                                                            if (orgCo[0].ORGANIZATION_LEVEL == "Department")
                                                                temp.Risk_Status_Workflow = "6";// Waiting Ownner Department Approve
                                                            else if (orgCo[0].ORGANIZATION_LEVEL == "Division")
                                                                temp.Risk_Status_Workflow = "9";// Waiting Ownner Division Approve
                                                            else if (orgCo[0].ORGANIZATION_LEVEL == "Group")
                                                                temp.Risk_Status_Workflow = "12";// Waiting Ownner Group Approve
                                                            temp.Risk_Type = "Organization";
                                                            if (!orgCo[0].HeadActing[0].ACTING_STATUS)
                                                                temp.Risk_AssignTo = orgCo[0].HeadActing[0].HEAD_ID;
                                                            else
                                                                temp.Risk_AssignTo = orgCo[0].HeadActing[0].ACTING_HEAD_ID;
                                                            insertOrgModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else//Submit other find owner parent org
                                    {
                                        //get parent org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            if (items.Risk_Status_Workflow == "5" || items.Risk_Status_Workflow == "16" || items.Risk_Status_Workflow == "34")//Co - BU submit Or Reject find owner BU approve
                                            {
                                                if ((!_helper.CheckNull(items.Risk_Submit_Action) ? items.Risk_Submit_Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                                {
                                                    //get Owner department from org
                                                    var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Department").ToList();
                                                    if (_tempOrg.Count > 0)
                                                    {
                                                        TypeOrganizationModel temp = new TypeOrganizationModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                        temp.Risk_Status_Workflow = "28";// Waiting Owner Department Approvve (ReConsole)
                                                        temp.Risk_Type = "Organization";
                                                        if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                        temp.Risk_Submit_Action = items.Risk_Submit_Action.ToUpper();
                                                        temp.Risk_Submit_Reason = items.Risk_Submit_Reason;
                                                        insertOrgModel.Add(temp);
                                                    }
                                                }
                                                else // normal case
                                                {
                                                    //get Owner department from org
                                                    var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Department").ToList();
                                                    if (_tempOrg.Count > 0)
                                                    {
                                                        TypeOrganizationModel temp = new TypeOrganizationModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                        temp.Risk_Status_Workflow = "6";// Waiting Owner Department Approvve
                                                        temp.Risk_Type = "Organization";
                                                        if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                        insertOrgModel.Add(temp);
                                                    }
                                                }
                                            }
                                            else if (items.Risk_Status_Workflow == "8" || items.Risk_Status_Workflow == "17" || items.Risk_Status_Workflow == "35")//Co - DI submit Or Reject find owner DI approve
                                            {
                                                if ((!_helper.CheckNull(items.Risk_Submit_Action) ? items.Risk_Submit_Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                                {
                                                    //get Owner division from org
                                                    var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Division").ToList();
                                                    if (_tempOrg.Count > 0)
                                                    {
                                                        TypeOrganizationModel temp = new TypeOrganizationModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                        temp.Risk_Status_Workflow = "29";// Waiting Owner Division Approvve (ReConsole)
                                                        temp.Risk_Type = "Organization";
                                                        if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                        temp.Risk_Submit_Action = items.Risk_Submit_Action.ToUpper();
                                                        temp.Risk_Submit_Reason = items.Risk_Submit_Reason;
                                                        insertOrgModel.Add(temp);
                                                    }
                                                }
                                                else // normal case
                                                {
                                                    //get Owner division from org
                                                    var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Division").ToList();
                                                    if (_tempOrg.Count > 0)
                                                    {
                                                        TypeOrganizationModel temp = new TypeOrganizationModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                        temp.Risk_Status_Workflow = "9";// Waiting Owner Division Approvve
                                                        temp.Risk_Type = "Organization";
                                                        if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                        insertOrgModel.Add(temp);
                                                    }
                                                }  
                                            }
                                            else if (items.Risk_Status_Workflow == "11" || items.Risk_Status_Workflow == "18" || items.Risk_Status_Workflow == "36")//Co - FG submit Or Reject find Owner FG approvve
                                            {
                                                if ((!_helper.CheckNull(items.Risk_Submit_Action) ? items.Risk_Submit_Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                                {
                                                    //get Owner group from org
                                                    var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Group").ToList();
                                                    if (_tempOrg.Count > 0)
                                                    {
                                                        TypeOrganizationModel temp = new TypeOrganizationModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                        temp.Risk_Status_Workflow = "30";// Waiting Owner Group Approvve (ReConsole)
                                                        temp.Risk_Type = "Organization";
                                                        if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                        temp.Risk_Submit_Action = items.Risk_Submit_Action.ToUpper();
                                                        temp.Risk_Submit_Reason = items.Risk_Submit_Reason;
                                                        insertOrgModel.Add(temp);
                                                    }
                                                }
                                                else // normal case
                                                {
                                                    //get Owner group from org
                                                    var _tempOrg = orgparentInfo.Where(o => o.ORGANIZATION_LEVEL == "Group").ToList();
                                                    if (_tempOrg.Count > 0)
                                                    {
                                                        TypeOrganizationModel temp = new TypeOrganizationModel();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Business_Unit_WF = _tempOrg[0].ORGANIZATION_ID;
                                                        temp.Risk_Business_Unit_WF_Level = _tempOrg[0].ORGANIZATION_LEVEL;
                                                        temp.Risk_Status_Workflow = "12";// Waiting Owner Group Approvve
                                                        temp.Risk_Type = "Organization";
                                                        if (!_tempOrg[0].HeadActing[0].ACTING_STATUS)
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].HEAD_ID;
                                                        else
                                                            temp.Risk_AssignTo = _tempOrg[0].HeadActing[0].ACTING_HEAD_ID;
                                                        insertOrgModel.Add(temp);
                                                    }
                                                } 
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    response.ErrorMessage = "Risk is not current workflow status!";
                                    response.Status = false;
                                    isError = true;
                                    break;
                                }
                            }
                            //check error
                            if (!isError)
                                p.Add("@SubmitTypeOrganization", _helper.ToDataTable(insertOrgModel).AsTableValuedParameter());
                            else
                                insertOrgModel = new List<TypeOrganizationModel>();
                        }
                        /*if (SendEmail)
                        {
                            _e.SubmitProcessMail(insertAssetModel, insertOrgModel, "Transection");
                        }*/
                        if (insertAssetModel.Count > 0 || insertOrgModel.Count > 0)
                        {
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                            response.body = conn.Query<object>("sp_Risk_Workflow_Submit_WS", p, commandType: CommandType.StoredProcedure).ToList();
                            response.StatusId = p.Get<int>("@StatusChange");
                            if (response.StatusId == 0)
                            {
                                if (SendEmail)
                                {
                                    _e.SubmitProcessMail(insertAssetModel, insertOrgModel, "Transection");
                                }
                                response.ErrorMessage = p.Get<string>("@StatusText");
                                response.Status = true;
                            }
                            else
                            {
                                response.ErrorMessage = p.Get<string>("@StatusText");
                                response.Status = false;
                            }
                        }
                        else // Not have next Bu Assign
                        {
                            if (_helper.CheckNull(response.ErrorMessage))
                            {
                                response.ErrorMessage = "Please contact to HEM Focal Point.  Risk Owner of BU level has not assigned.";
                                response.Status = false;
                            }
                        }
                    }
                }
                else if (request.body.Risk_Submit_Workflow.ToUpper() == "ERM") // ERM submit
                {
                    using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                    {
                        conn.Open();
                        var p = new DynamicParameters();
                        p.Add("@Risk_Modified_By", request.body.Risk_Modified_By);
                        p.Add("@Risk_Submit_Workflow", request.body.Risk_Submit_Workflow);
                        if (!_helper.IsNullOrEmpty(data.SubmitTypeCorporate))
                        {
                            List<TypeCorporateModel> tempCorporate = data.SubmitTypeCorporate;
                            List<TypeCorporateModel> insertModel = new List<TypeCorporateModel>();
                            var p_role = new DynamicParameters();
                            p_role.Add("@Table_Name", "Master_PerManagement_Item");

                            var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                            if (dataERM.Count > 0)
                            {
                                var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                foreach (var items in tempCorporate)//loop SubmitTypeCorporate
                                {
                                    TypeCorporateModel temp = new TypeCorporateModel();
                                    temp.Risk_Id = items.Risk_Id;
                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                    temp.Risk_Business_Unit_WF = items.Risk_Business_Unit;
                                    temp.Risk_Business_Unit_WF_Level = items.Risk_Type;
                                    temp.Risk_Status_Workflow = "13";// Waiting ERM Consolidate
                                    temp.Risk_Type = items.Risk_Type;
                                    temp.Risk_AssignTo = assignTo;
                                    insertModel.Add(temp);
                                }
                                p.Add("@SubmitTypeCorporate", _helper.ToDataTable(insertModel).AsTableValuedParameter());
                            }
                        }
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                        response.body = conn.Query<object>("sp_Risk_Workflow_Submit_WS", p, commandType: CommandType.StoredProcedure).ToList();
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
                    
                }
                else
                {
                    response.ErrorMessage = "[sp_Risk_Workflow_Submit]" + "Don't have risk_submit_workflow";
                    response.Status = false;
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Risk_Workflow_Submit]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Risk_Workflow_Approve(RequestMessage<ApproveModel> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var root = builder.Build();
            var SendEmail = bool.Parse(root.GetSection("AppConfiguration")["SendEmail"].ToString());
            var data = request.body;
            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    var p = new DynamicParameters();
                    List<ApproveTypeAsset> insertApproveAssetModel = new List<ApproveTypeAsset>();
                    List<ApproveTypeOrganization> insertApproveOrgModel = new List<ApproveTypeOrganization>();
                    p.Add("@Module", request.Module.ToUpper());
                    p.Add("@Approve_By", data.Approve_By);
                    //p.Add("@IdCollection", data.IdCollection);
                    if (!_helper.IsNullOrEmpty(data.ApproveTypeAsset))
                    {
                        List<ApproveTypeAsset> tempAsset = data.ApproveTypeAsset;
                        
                        //get temp Asset
                        var p_masterasset = new DynamicParameters();
                        p_masterasset.Add("@Table_Name", "Master_Asset");
                        var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterasset, commandType: CommandType.StoredProcedure).ToList();
                        //get Master_Cordinator 
                        var p_masterco = new DynamicParameters();
                        p_masterco.Add("@Table_Name", "Master_Cordinator");
                        var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterco, commandType: CommandType.StoredProcedure).ToList();
                        foreach (var items in tempAsset)
                        {
                            if (!_helper.CheckNull(items.Risk_Status_Workflow))
                            {
                                if (items.Risk_Status_Workflow == "3")//if Owner Asset config approve
                                {
                                    //find org config asset
                                    var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                    if (_result.Count() > 0)
                                    {
                                        var _tempCo = mCo.Where(o => o.Coordinator_Department_Id == _result[0].Asset_Org).ToList();
                                        if (_tempCo.Count > 0)//have Co org config asset config
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                            

                                            ApproveTypeAsset temp = new ApproveTypeAsset();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                            temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;

                                            if (_tempCo[0].Coordinator_Level == "Department")
                                            {
                                                temp.Risk_Status_Workflow = "4";// Waiting CO for parent org Department 
                                            }
                                            else if (_tempCo[0].Coordinator_Level == "Division")
                                            {
                                                temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                            }
                                            else if (_tempCo[0].Coordinator_Level == "Group")
                                            {
                                                temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                            }
                                            temp.Risk_Status_Approve = "19";//status Asset Approve
                                            temp.Risk_Type = "Asset";
                                            temp.Risk_AssignTo = assignTo;
                                            temp.Comment = items.Comment;
                                            #region find Co FG  for assignto
                                            //AssignTo FG Owner
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                            #endregion
                                            insertApproveAssetModel.Add(temp);
                                        }
                                        else // not have co next org config
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                    if (_tempCo.Count > 0)//is match CO
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)//have Co match is parent org
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    ApproveTypeAsset temp = new ApproveTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    
                                                    if (_tempCo[0].Coordinator_Level == "Department")
                                                        temp.Risk_Status_Workflow = "4";// Waiting CO for parent org Department
                                                    else if (_tempCo[0].Coordinator_Level == "Division")
                                                        temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                                    else if (_tempCo[0].Coordinator_Level == "Group")
                                                        temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                                    temp.Risk_Status_Approve = "19";//status Asset Approve
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    #region find Co FG  for assignto
                                                    //AssignTo FG Owner
                                                    //get recursive org

                                                    orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    insertApproveAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "6" || items.Risk_Status_Workflow == "28")// if Owner BU Approve OR Owner BU Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co department
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Department")).ToList();
                                                    if (_tempCo.Count > 0)//is match CO Department
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "4";// Waiting CO for Department
                                                    temp.Risk_Status_Approve = "31";// Status BU Approve  
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    temp.Action = items.Action.ToUpper();
                                                    #region find Co  for CC
                                                    // Get CC Codinator
                                                    orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    insertApproveOrgModel.Add(temp);
                                                }
                                                else // not have co config or not parent org 
                                                {
                                                    bool chkCeo = false;
                                                    bool chkOtherOrg = false;
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                            chkCeo = true;
                                                        if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                            chkOtherOrg = true;

                                                    }
                                                    if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                    {
                                                        var p_role = new DynamicParameters();
                                                        p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                        var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                        if (dataERM.Count > 0)
                                                        {
                                                            //assign ERM Consolidate
                                                            var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                            ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                            temp.Risk_Status_Workflow = "13";
                                                            temp.Risk_Status_Approve = "20";// Status BU Approve 
                                                            temp.Risk_Type = "Asset";
                                                            temp.Risk_AssignTo = assignTo;
                                                            temp.Comment = items.Comment;
                                                            temp.Action = items.Action.ToUpper();
                                                            insertApproveOrgModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else // normal case
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Division OR Group
                                                //List<Master_CO> _tempCo = new List<Master_CO>();
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Division" || o.Coordinator_Level == "Group")).ToList();
                                                    if (_tempCo.Count > 0)//is match CO Divison OR CO FG
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    ApproveTypeAsset temp = new ApproveTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;

                                                    if (_tempCo[0].Coordinator_Level == "Division")
                                                        temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                                    else if (_tempCo[0].Coordinator_Level == "Group")
                                                        temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                                    temp.Risk_Status_Approve = "20";//status BU Approve
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    #region find Co FG  for assignto
                                                    //AssignTo FG Owner
                                                    //get recursive org

                                                    orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    insertApproveAssetModel.Add(temp);
                                                }
                                                else // not have co config or not parent org 
                                                {
                                                    bool chkCeo = false;
                                                    bool chkOtherOrg = false;
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                            chkCeo = true;
                                                        if (orgParent.ORGANIZATION_LEVEL == "Division" || orgParent.ORGANIZATION_LEVEL == "Group")
                                                            chkOtherOrg = true;

                                                    }
                                                    if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                    {
                                                        //List<TypeCorporateModel> tempCorporate = data.SubmitTypeCorporate;
                                                        //List<TypeCorporateModel> insertModel = new List<TypeCorporateModel>();
                                                        var p_role = new DynamicParameters();
                                                        p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                        var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                        if (dataERM.Count > 0)
                                                        {
                                                            //assign ERM Consolidate
                                                            var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                            ApproveTypeAsset temp = new ApproveTypeAsset();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                            temp.Risk_Status_Workflow = "13";
                                                            temp.Risk_Status_Approve = "20";//status BU Approve
                                                            temp.Risk_Type = "Asset";
                                                            temp.Risk_AssignTo = assignTo;
                                                            temp.Comment = items.Comment;
                                                            insertApproveAssetModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "9" || items.Risk_Status_Workflow == "29")// if Owner DI Approve OR Owner DI Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Division
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Division")).ToList();
                                                    if (_tempCo.Count > 0)//is match CO Divison
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "7";// Waiting CO for Division
                                                    temp.Risk_Status_Approve = "32";//status DI Approve
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    temp.Action = items.Action.ToUpper();
                                                    #region find Co  for CC
                                                    // Get CC Codinator
                                                    orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    insertApproveOrgModel.Add(temp);
                                                }
                                                else // not have co config or not parent org 
                                                {
                                                    bool chkCeo = false;
                                                    bool chkOtherOrg = false;
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                            chkCeo = true;

                                                    }
                                                    if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                    {
                                                        var p_role = new DynamicParameters();
                                                        p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                        var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                        if (dataERM.Count > 0)
                                                        {
                                                            //assign ERM Consolidate
                                                            var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                            ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                            temp.Risk_Status_Approve = "21";//status DI Approve
                                                            temp.Risk_Status_Workflow = "13";
                                                            temp.Risk_Type = "Asset";
                                                            temp.Risk_AssignTo = assignTo;
                                                            temp.Comment = items.Comment;
                                                            temp.Action = items.Action.ToUpper();
                                                            insertApproveOrgModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Group
                                                //List<Master_CO> _tempCo = new List<Master_CO>();
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Group").ToList();
                                                    if (_tempCo.Count > 0)//is match CO FG
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    ApproveTypeAsset temp = new ApproveTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;

                                                    temp.Risk_Status_Workflow = "10";// Waiting CO FG
                                                    temp.Risk_Status_Approve = "21";//status DI Approve
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    //AssignTo FG Owner
                                                    //get recursive org

                                                    insertApproveAssetModel.Add(temp);
                                                }
                                                else // not have co config or not parent org 
                                                {
                                                    bool chkCeo = false;
                                                    bool chkOtherOrg = false;
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                            chkCeo = true;

                                                    }
                                                    if (chkCeo) // find have CEO Only
                                                    {
                                                        var p_role = new DynamicParameters();
                                                        p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                        var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                        if (dataERM.Count > 0)
                                                        {
                                                            //assign ERM Consolidate
                                                            var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                            ApproveTypeAsset temp = new ApproveTypeAsset();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                            temp.Risk_Status_Workflow = "13";
                                                            temp.Risk_Status_Approve = "21";//status DI Approve
                                                            temp.Risk_Type = "Asset";
                                                            temp.Risk_AssignTo = assignTo;
                                                            temp.Comment = items.Comment;
                                                            insertApproveAssetModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "12" || items.Risk_Status_Workflow == "30")// if Owner FG Approve OR Owner FG Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Group
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Group")).ToList();
                                                    if (_tempCo.Count > 0)//is match CO Group
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "10";// Waiting CO for Group
                                                    temp.Risk_Status_Approve = "33";//status FG Group
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    temp.Action = items.Action.ToUpper();
                                                    #region find Co  for CC
                                                    // Get CC Codinator
                                                    orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                        else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                        {
                                                            var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                            var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                            if (assignToCoParent.Length > 0)
                                                            {
                                                                temp.Risk_CC = assignToCoParent;
                                                            }
                                                            break;
                                                        }
                                                    }
                                                    #endregion
                                                    insertApproveOrgModel.Add(temp);
                                                }
                                                else // not have co config or not parent org 
                                                {
                                                    bool chkCeo = false;
                                                    bool chkOtherOrg = false;
                                                    foreach (var orgParent in orgparentInfo)
                                                    {
                                                        if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                            chkCeo = true;

                                                    }
                                                    if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                    {
                                                        var p_role = new DynamicParameters();
                                                        p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                        var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                        if (dataERM.Count > 0)
                                                        {
                                                            //assign ERM Consolidate
                                                            var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                            ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                            temp.Risk_Id = items.Risk_Id;
                                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                            temp.Risk_Status_Workflow = "13";// Waiting ERM team Consolidate
                                                            temp.Risk_Status_Approve = "22";//status FG Approve
                                                            temp.Risk_Type = "Asset";
                                                            temp.Risk_AssignTo = assignTo;
                                                            temp.Comment = items.Comment;
                                                            temp.Action = items.Action.ToUpper();
                                                            insertApproveOrgModel.Add(temp);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var p_role = new DynamicParameters();
                                        p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                        var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                        if (dataERM.Count > 0)
                                        {
                                            //Waiting ERM team Consolidate
                                            var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                            ApproveTypeAsset temp = new ApproveTypeAsset();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                            temp.Risk_Status_Workflow = "13";// Waiting ERM team Consolidate
                                            temp.Risk_Status_Approve = "22";//status FG Approve
                                            temp.Risk_Type = "Asset";
                                            temp.Risk_AssignTo = assignTo;
                                            temp.Comment = items.Comment;
                                            insertApproveAssetModel.Add(temp);
                                        }
                                    }
                                }
                            }
                        }
                        p.Add("@ApproveTypeAsset", _helper.ToDataTable(insertApproveAssetModel).AsTableValuedParameter());
                    }
                    if (!_helper.IsNullOrEmpty(data.ApproveTypeOrganization))
                    {
                        List<ApproveTypeOrganization> tempOrganization = data.ApproveTypeOrganization;
                        
                        //get Master_Cordinator 
                        var p_masterco = new DynamicParameters();
                        p_masterco.Add("@Table_Name", "Master_Cordinator");
                        var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterco, commandType: CommandType.StoredProcedure).ToList();
                        foreach (var items in tempOrganization)
                        {
                            if (!_helper.CheckNull(items.Risk_Status_Workflow))
                            {
                                if (items.Risk_Status_Workflow == "6" || items.Risk_Status_Workflow == "28")// if Owner BU Approve OR Owner BU Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "" ) == "RECONSOLE") // Approve Reconsole
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co department
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Department")).ToList();
                                                if (_tempCo.Count > 0)//is match CO Department
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "4";// Waiting CO for Department
                                                temp.Risk_Status_Approve = "31";// Status BU Approve  
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                temp.Action = items.Action.ToUpper();
                                                #region find Co  for CC
                                                // Get CC Codinator
                                                orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                }
                                                #endregion
                                                insertApproveOrgModel.Add(temp);
                                            }
                                            else // not have co config or not parent org 
                                            {
                                                bool chkCeo = false;
                                                bool chkOtherOrg = false;
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                        chkCeo = true;
                                                    if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                        chkOtherOrg = true;

                                                }
                                                if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                {
                                                    var p_role = new DynamicParameters();
                                                    p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                    var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                    if (dataERM.Count > 0)
                                                    {
                                                        //assign ERM Consolidate
                                                        var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                        ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                        temp.Risk_Status_Workflow = "13";
                                                        temp.Risk_Status_Approve = "20";// Status BU Approve 
                                                        temp.Risk_Type = "Organization";
                                                        temp.Risk_AssignTo = assignTo;
                                                        temp.Comment = items.Comment;
                                                        temp.Action = !_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "";
                                                        insertApproveOrgModel.Add(temp);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else // Normal Case
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Division" || o.Coordinator_Level == "Group")).ToList();
                                                if (_tempCo.Count > 0)//is match CO Divison OR CO FG
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;

                                                if (_tempCo[0].Coordinator_Level == "Division")
                                                    temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                                else if (_tempCo[0].Coordinator_Level == "Group")
                                                    temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group
                                                temp.Risk_Status_Approve = "20";//status BU Approve
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                #region find Co FG  for assignto
                                                //AssignTo FG Owner
                                                //get recursive org

                                                orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                }
                                                #endregion
                                                insertApproveOrgModel.Add(temp);
                                            }
                                            else // not have co config or not parent org 
                                            {
                                                bool chkCeo = false;
                                                bool chkOtherOrg = false;
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                        chkCeo = true;
                                                    if (orgParent.ORGANIZATION_LEVEL == "Division" || orgParent.ORGANIZATION_LEVEL == "Group")
                                                        chkOtherOrg = true;

                                                }
                                                if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                {
                                                    var p_role = new DynamicParameters();
                                                    p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                    var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                    if (dataERM.Count > 0)
                                                    {
                                                        //assign ERM Consolidate
                                                        var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                        ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                        temp.Risk_Status_Workflow = "13";
                                                        temp.Risk_Status_Approve = "20";//status BU Approve
                                                        temp.Risk_Type = "Organization";
                                                        temp.Risk_AssignTo = assignTo;
                                                        temp.Comment = items.Comment;
                                                        insertApproveOrgModel.Add(temp);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "9" || items.Risk_Status_Workflow == "29")// if Owner DI Approve OR Owner DI Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Division")).ToList();
                                                if (_tempCo.Count > 0)//is match CO Divison
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "7";// Waiting CO for Division
                                                temp.Risk_Status_Approve = "32";//status DI Approve
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                temp.Action = items.Action.ToUpper();
                                                #region find Co  for CC
                                                // Get CC Codinator
                                                orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                }
                                                #endregion
                                                insertApproveOrgModel.Add(temp);
                                            }
                                            else // not have co config or not parent org 
                                            {
                                                bool chkCeo = false;
                                                bool chkOtherOrg = false;
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                        chkCeo = true;

                                                }
                                                if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                {
                                                    var p_role = new DynamicParameters();
                                                    p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                    var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                    if (dataERM.Count > 0)
                                                    {
                                                        //assign ERM Consolidate
                                                        var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                        ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                        temp.Risk_Status_Approve = "21";//status DI Approve
                                                        temp.Risk_Status_Workflow = "13";
                                                        temp.Risk_Type = "Organization";
                                                        temp.Risk_AssignTo = assignTo;
                                                        temp.Comment = items.Comment;
                                                        temp.Action = !_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "";
                                                        insertApproveOrgModel.Add(temp);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else //Normal Case
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Group").ToList();
                                                if (_tempCo.Count > 0)//is match CO FG
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;

                                                temp.Risk_Status_Workflow = "10";// Waiting CO FG
                                                temp.Risk_Status_Approve = "21";//status DI Approve
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                //AssignTo FG Owner
                                                //get recursive org

                                                insertApproveOrgModel.Add(temp);
                                            }
                                            else // not have co config or not parent org 
                                            {
                                                bool chkCeo = false;
                                                bool chkOtherOrg = false;
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                        chkCeo = true;

                                                }
                                                if (chkCeo) // find have CEO Only
                                                {
                                                    var p_role = new DynamicParameters();
                                                    p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                    var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                    if (dataERM.Count > 0)
                                                    {
                                                        //assign ERM Consolidate
                                                        var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                        ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                        temp.Risk_Status_Workflow = "13";
                                                        temp.Risk_Status_Approve = "21";//status DI Approve
                                                        temp.Risk_Type = "Organization";
                                                        temp.Risk_AssignTo = assignTo;
                                                        temp.Comment = items.Comment;
                                                        insertApproveOrgModel.Add(temp);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "12" || items.Risk_Status_Workflow == "30")// if Owner FG Approve OR Owner FG Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE") // Approve Reconsole
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Group
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Group")).ToList();
                                                if (_tempCo.Count > 0)//is match CO Group
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "10";// Waiting CO for Group
                                                temp.Risk_Status_Approve = "33";//status DI Group
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                temp.Action = items.Action.ToUpper();
                                                #region find Co  for CC
                                                // Get CC Codinator
                                                orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "Group")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                    else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                                    {
                                                        var _tempCoParent = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID).ToList();
                                                        var assignToCoParent = _helper.GetAssignToFromConfig(_tempCoParent);
                                                        if (assignToCoParent.Length > 0)
                                                        {
                                                            temp.Risk_CC = assignToCoParent;
                                                        }
                                                        break;
                                                    }
                                                }
                                                #endregion
                                                insertApproveOrgModel.Add(temp);
                                            }
                                            else // not have co config or not parent org 
                                            {
                                                bool chkCeo = false;
                                                bool chkOtherOrg = false;
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                        chkCeo = true;

                                                }
                                                if (chkCeo && !chkOtherOrg) // find have CEO Only
                                                {
                                                    var p_role = new DynamicParameters();
                                                    p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                                    var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                                    if (dataERM.Count > 0)
                                                    {
                                                        //assign ERM Consolidate
                                                        var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                                        ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                                        temp.Risk_Id = items.Risk_Id;
                                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                        temp.Risk_Status_Workflow = "13";// Waiting ERM team Consolidate
                                                        temp.Risk_Status_Approve = "22";//status FG Approve
                                                        temp.Risk_Type = "Organization";
                                                        temp.Risk_AssignTo = assignTo;
                                                        temp.Comment = items.Comment;
                                                        insertApproveOrgModel.Add(temp);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else // Normal Case
                                    {
                                        //Waiting ERM team Consolidate
                                        var p_role = new DynamicParameters();
                                        p_role.Add("@Table_Name", "Master_PerManagement_Item");

                                        var dataERM = conn.Query<Master_ERM>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                                        if (dataERM.Count > 0)
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(dataERM);
                                            ApproveTypeOrganization temp = new ApproveTypeOrganization();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                            temp.Risk_Status_Approve = "22";//status FG Approve
                                            temp.Risk_Status_Workflow = "13";// Waiting ERM team Consolidate
                                            temp.Risk_Type = "Organization";
                                            temp.Risk_AssignTo = assignTo;
                                            temp.Comment = items.Comment;
                                            temp.Action = !_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "";
                                            insertApproveOrgModel.Add(temp);
                                        }
                                    }
                                }
                            }
                        }
                        p.Add("@ApproveTypeOrganization", _helper.ToDataTable(insertApproveOrgModel).AsTableValuedParameter());
                    }
                    /*if (SendEmail)
                    {
                        _e.ApproveProcessMail(insertApproveAssetModel, insertApproveOrgModel);
                    }*/
                    p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    //loop call web service get org GetOrganizations by business unit
                    //xml return result BU OR FG
                    //update status and assignto in transection 
                    if (insertApproveAssetModel.Count > 0 || insertApproveOrgModel.Count > 0)
                    {
                        response.body = conn.Query<object>("sp_Risk_Workflow_Approve", p, commandType: CommandType.StoredProcedure).ToList();
                        response.StatusId = p.Get<int>("@StatusChange");
                        if (response.StatusId == 0)
                        {

                            if (SendEmail)
                            {
                                _e.ApproveProcessMail(insertApproveAssetModel, insertApproveOrgModel);
                            }
                            response.ErrorMessage = p.Get<string>("@StatusText");
                            response.Status = true;
                        }
                        else
                        {
                            response.ErrorMessage = p.Get<string>("@StatusText");
                            response.Status = false;
                        }
                    }
                    else // Not have next Bu Assign
                    {
                        response.ErrorMessage = "Please contact to HEM Focal Point.  Risk Coordinator of next BU level has not assigned.";
                        response.Status = false;
                    }
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Risk_Workflow_Approve]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Risk_Workflow_Reject(RequestMessage<RejectModel> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var root = builder.Build();
            var data = request.body;
            try
            {
                var SendEmail = bool.Parse(root.GetSection("AppConfiguration")["SendEmail"].ToString());
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    var p = new DynamicParameters();
                    List<RejectTypeAsset> insertRejectAssetModel = new List<RejectTypeAsset>();
                    List<RejectTypeOrganization> insertRejectOrgModel = new List<RejectTypeOrganization>();
                    p.Add("@Module", request.Module.ToUpper());
                    p.Add("@Reject_By", data.Reject_By);
                    if (!_helper.IsNullOrEmpty(data.RejectTypeAsset))
                    {
                        List<RejectTypeAsset> tempAsset = data.RejectTypeAsset;
                        //List<RejectTypeAsset> insertModel = new List<RejectTypeAsset>();
                        //get temp Asset
                        var p_masterasset = new DynamicParameters();
                        p_masterasset.Add("@Table_Name", "Master_Asset");
                        var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterasset, commandType: CommandType.StoredProcedure).ToList();
                        //get Master_Cordinator 
                        var p_masterco = new DynamicParameters();
                        p_masterco.Add("@Table_Name", "Master_Cordinator");
                        var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterco, commandType: CommandType.StoredProcedure).ToList();
                        foreach (var items in tempAsset)
                        {
                            if (!_helper.CheckNull(items.Risk_Status_Workflow))
                            {
                                if (items.Risk_Status_Workflow == "3")//Owner-Asset Draft
                                {
                                    //find org config asset
                                    var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                    if (_result.Count() > 0)
                                    {
                                        var assignTo = _helper.GetAssignToFromConfig(_result);
                                        RejectTypeAsset temp = new RejectTypeAsset();
                                        temp.Risk_Id = items.Risk_Id;
                                        temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                        temp.Risk_Business_Unit_WF = _result[0].Asset_Org;
                                        temp.Risk_Status_Workflow = "15";// Waiting CO Asset Draft
                                        temp.Risk_Type = "Asset";
                                        temp.Risk_AssignTo = assignTo;
                                        temp.Comment = items.Comment;
                                        insertRejectAssetModel.Add(temp);
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "6" || items.Risk_Status_Workflow == "28") //Owner - BU Approve OR Owner BU Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE")
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Department
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Department").ToList();
                                                    if (_tempCo.Count > 0)//is match CO Department
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    RejectTypeAsset temp = new RejectTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "34";// CO-BU Draft
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    temp.Action = items.Action.ToUpper();
                                                    insertRejectAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Department
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Department").ToList();
                                                    if (_tempCo.Count > 0)//is match CO Department
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    RejectTypeAsset temp = new RejectTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "16";// CO-BU Draft
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    insertRejectAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "9" || items.Risk_Status_Workflow == "29") //Owner-DI Approve OR Owner DI Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE")
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Division OR Group
                                                //List<Master_CO> _tempCo = new List<Master_CO>();
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Division").ToList();
                                                    if (_tempCo.Count > 0)//is match CO DI
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    RejectTypeAsset temp = new RejectTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "35";// CO-DI Draft
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    temp.Action = items.Action.ToUpper();
                                                    insertRejectAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Division OR Group
                                                //List<Master_CO> _tempCo = new List<Master_CO>();
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Division").ToList();
                                                    if (_tempCo.Count > 0)//is match CO DI
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    RejectTypeAsset temp = new RejectTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "17";// CO-DI Draft
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    insertRejectAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }   
                                }
                                else if (items.Risk_Status_Workflow == "12" || items.Risk_Status_Workflow == "30") //Owner-FG Approve OR Owner FG Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE")
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Division OR Group
                                                //List<Master_CO> _tempCo = new List<Master_CO>();
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Group").ToList();
                                                    if (_tempCo.Count > 0)//is match CO Group
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    RejectTypeAsset temp = new RejectTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "36";// CO-FG Draft
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    temp.Action = items.Action.ToUpper();
                                                    insertRejectAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //find org config asset
                                        var _result = mAsset.Where(o => o.Asset_Code == items.Risk_Business_Unit).ToList();
                                        if (_result.Count() > 0)
                                        {
                                            //get recursive org
                                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", _result[0].Asset_Org),
                                                new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                            };
                                            List<Organizations> org = new List<Organizations>();
                                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                            if (orgparentInfo.Count > 0)
                                            {
                                                //find co Division OR Group
                                                //List<Master_CO> _tempCo = new List<Master_CO>();
                                                var _tempCo = new List<Master_CO>();
                                                foreach (var orgParent in orgparentInfo)
                                                {
                                                    _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Group").ToList();
                                                    if (_tempCo.Count > 0)//is match CO Group
                                                        break;
                                                }
                                                if (_tempCo.Count > 0)
                                                {
                                                    var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                    RejectTypeAsset temp = new RejectTypeAsset();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                    temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                    temp.Risk_Status_Workflow = "18";// CO-FG Draft
                                                    temp.Risk_Type = "Asset";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    insertRejectAssetModel.Add(temp);
                                                }
                                            }
                                        }
                                    }   
                                }
                            }
                        }
                        p.Add("@RejectTypeAsset", _helper.ToDataTable(insertRejectAssetModel).AsTableValuedParameter());
                    }
                    if (!_helper.IsNullOrEmpty(data.RejectTypeOrganization))
                    {
                        List<RejectTypeOrganization> tempOrganization = data.RejectTypeOrganization;
                        //List<RejectTypeOrganization> insertModel = new List<RejectTypeOrganization>();
                        //get Master_Cordinator 
                        var p_masterco = new DynamicParameters();
                        p_masterco.Add("@Table_Name", "Master_Cordinator");
                        var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterco, commandType: CommandType.StoredProcedure).ToList();
                        foreach (var items in tempOrganization)
                        {
                            if (!_helper.CheckNull(items.Risk_Status_Workflow))
                            {
                                if (items.Risk_Status_Workflow == "6" || items.Risk_Status_Workflow == "28")// if Owner BU Approve OR Owner BU Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE")
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Department").ToList();
                                                if (_tempCo.Count > 0)//is match DEp
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                RejectTypeOrganization temp = new RejectTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "34";// CO-BU Draft
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                temp.Action = items.Action.ToUpper();
                                                insertRejectOrgModel.Add(temp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Department").ToList();
                                                if (_tempCo.Count > 0)//is match DEp
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                RejectTypeOrganization temp = new RejectTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "16";// CO-BU Draft
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                insertRejectOrgModel.Add(temp);
                                            }
                                        }
                                    }   
                                }
                                else if (items.Risk_Status_Workflow == "9" || items.Risk_Status_Workflow == "29")// if Owner DI Approve OR Owner DI Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE")
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Division").ToList();
                                                if (_tempCo.Count > 0)//is match CO DI
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                RejectTypeOrganization temp = new RejectTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "35";// Owner-DI Draft
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                temp.Action = items.Action.ToUpper();
                                                insertRejectOrgModel.Add(temp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Division").ToList();
                                                if (_tempCo.Count > 0)//is match CO DI
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                RejectTypeOrganization temp = new RejectTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "17";// Owner-DI Draft
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                insertRejectOrgModel.Add(temp);
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "12" || items.Risk_Status_Workflow == "30")// if Owner FG Approve OR Owner FG Approve (Re Console) 
                                {
                                    if ((!_helper.CheckNull(items.Action) ? items.Action.ToUpper() : "") == "RECONSOLE")
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Group").ToList();
                                                if (_tempCo.Count > 0)//is match CO Group
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                RejectTypeOrganization temp = new RejectTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "36";// Owner-FG Draft
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                temp.Action = items.Action.ToUpper();
                                                insertRejectOrgModel.Add(temp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //get recursive org
                                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                                            new KeyValuePair<string, string>("OrganizetionID", items.Risk_Business_Unit),
                                            new KeyValuePair<string, string>("SecurityCode", request.SecurityCode),
                                        };
                                        List<Organizations> org = new List<Organizations>();
                                        List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                        if (orgparentInfo.Count > 0)
                                        {
                                            //find co Division OR Group
                                            //List<Master_CO> _tempCo = new List<Master_CO>();
                                            var _tempCo = new List<Master_CO>();
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && o.Coordinator_Level == "Group").ToList();
                                                if (_tempCo.Count > 0)//is match CO Group
                                                    break;
                                            }
                                            if (_tempCo.Count > 0)
                                            {
                                                var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                                RejectTypeOrganization temp = new RejectTypeOrganization();
                                                temp.Risk_Id = items.Risk_Id;
                                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                                temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                                temp.Risk_Status_Workflow = "18";// Owner-FG Draft
                                                temp.Risk_Type = "Organization";
                                                temp.Risk_AssignTo = assignTo;
                                                temp.Comment = items.Comment;
                                                insertRejectOrgModel.Add(temp);
                                            }
                                        }
                                    }    
                                }
                            }
                        }
                        p.Add("@RejectTypeOrganization", _helper.ToDataTable(insertRejectOrgModel).AsTableValuedParameter());
                    }
                    /*if (SendEmail)
                    {
                        _e.RejectProcessMail(insertRejectAssetModel, insertRejectOrgModel);
                    }*/
                    p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    //loop call web service get org GetOrganizations by business unit
                    //xml return result BU OR FG
                    //update status and assignto in transection 
                    if (insertRejectAssetModel.Count > 0 || insertRejectOrgModel.Count > 0)
                    {
                        response.body = conn.Query<object>("sp_Risk_Workflow_Reject", p, commandType: CommandType.StoredProcedure).ToList();
                        response.StatusId = p.Get<int>("@StatusChange");
                        if (response.StatusId == 0)
                        {
                            /*if (SendEmail)
                            {
                                _e.SendEmail(request.Email, null, null, insertRejectAssetModel, insertRejectOrgModel, null, null, null);
                            }*/
                            if (SendEmail)
                            {
                                _e.RejectProcessMail(insertRejectAssetModel, insertRejectOrgModel);
                            }
                            response.ErrorMessage = p.Get<string>("@StatusText");
                            response.Status = true;
                        }
                        else
                        {
                            response.ErrorMessage = p.Get<string>("@StatusText");
                            response.Status = false;
                        }
                    }
                    else
                    {
                        response.ErrorMessage = "Please contact to HEM Focal Point.  Risk Coordinator of level has not assigned.";
                        response.Status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Risk_Workflow_Reject]" + ex.Message;
            }

            return response;
        }
    }
}
