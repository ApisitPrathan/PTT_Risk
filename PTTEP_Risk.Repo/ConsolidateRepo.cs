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
    public class ConsolidateRepo
    {
        Helper _helper = new Helper();
        GetOrganizations _o = new GetOrganizations();
        EmailNotification _e = new EmailNotification();
        public ResponseMessage<object> API_Risk_Consolidate(RequestMessage<ConsolidateModel> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var root = builder.Build();
            var SendEmail = bool.Parse(root.GetSection("AppConfiguration")["SendEmail"].ToString());
            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    var p = new DynamicParameters();
                    List<Consolidate_Staff> insertConsoleStaffModel = new List<Consolidate_Staff>();
                    List<Consolidate_Transection> insertConsoleTransectionModel = new List<Consolidate_Transection>();
                    var data = request.body;
                    p.Add("@Module", request.Module.ToUpper());
                    p.Add("@Consolidate_By", data.Consolidate_By);
                    p.Add("@Risk_Catagory", data.Risk_Catagory);
                    p.Add("@Risk_Status", data.Risk_Status);
                    p.Add("@Risk_Business_Unit", data.Risk_Business_Unit);
                    p.Add("@Risk_Name", data.Risk_Name);
                    p.Add("@Risk_Register_By", data.Risk_Register_By);
                    p.Add("@Risk_Register_Date_From", data.Risk_Register_Date_From);
                    p.Add("@Risk_Register_Date_To", data.Risk_Register_Date_To);
                    p.Add("@Risk_Running", data.Risk_Running);
                    p.Add("@Escalation", data.Risk_Escalation);
                    p.Add("@High_Risk", data.Risk_Rating);
                    p.Add("@QuarterID", data.QuarterID);
                    p.Add("@WPBID", data.WPBID);
                    //p.Add("@IdCollection", data.IdCollection);
                    if (!_helper.IsNullOrEmpty(data.Consolidate_Staff)) // consolidate table staff
                    {
                        List<Consolidate_Staff> _listStaff = data.Consolidate_Staff;
                        
                        foreach (var items in _listStaff)
                        {
                            //check Status Workflow
                            if (items.Risk_Status_Workflow == "1")//chang status is 2 CO-Asset Draft
                            {

                                Consolidate_Staff temp = new Consolidate_Staff();
                                temp.Risk_Id = items.Risk_Id;
                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                temp.Risk_Status_Workflow = "2";// CO-Asset Draft
                                temp.Risk_Type = items.Risk_Type;
                                temp.Risk_AssignTo = "";
                                insertConsoleStaffModel.Add(temp);
                            }
                            else if (items.Risk_Status_Workflow == "4")//chang status is 5 CO-BU Draft
                            {
                                Consolidate_Staff temp = new Consolidate_Staff();
                                temp.Risk_Id = items.Risk_Id;
                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                temp.Risk_Status_Workflow = "5";// CO-BU Draft
                                temp.Risk_Type = items.Risk_Type;
                                temp.Risk_AssignTo = "";
                                insertConsoleStaffModel.Add(temp);
                            }
                            else if (items.Risk_Status_Workflow == "7")//find status is 8 CO-BU Draft
                            {
                                Consolidate_Staff temp = new Consolidate_Staff();
                                temp.Risk_Id = items.Risk_Id;
                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                temp.Risk_Status_Workflow = "8";// CO-DI Draft
                                temp.Risk_Type = items.Risk_Type;
                                temp.Risk_AssignTo = "";
                                insertConsoleStaffModel.Add(temp);
                            }
                            else if (items.Risk_Status_Workflow == "10")//find status is 11 CO-BU Draft
                            {
                                Consolidate_Staff temp = new Consolidate_Staff();
                                temp.Risk_Id = items.Risk_Id;
                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                temp.Risk_Status_Workflow = "11";// CO-FG Draft
                                temp.Risk_Type = items.Risk_Type;
                                temp.Risk_AssignTo = "";
                                insertConsoleStaffModel.Add(temp);
                            }
                            else if (items.Risk_Status_Workflow == "13")//Waiting ERM team Consolidate
                            {
                                Consolidate_Staff temp = new Consolidate_Staff();
                                temp.Risk_Id = items.Risk_Id;
                                temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                temp.Risk_Status_Workflow = "14";//ERM Consolidate
                                temp.Risk_Type = items.Risk_Type;
                                temp.Risk_AssignTo = "";
                                insertConsoleStaffModel.Add(temp);
                            }
                        }
                        p.Add("@ConsolidateStaff", _helper.ToDataTable(insertConsoleStaffModel).AsTableValuedParameter());
                    }
                    if (!_helper.IsNullOrEmpty(data.Consolidate_Transection))// consolidate table Transection
                    {
                        List<Consolidate_Transection> _listTransection = data.Consolidate_Transection;
                        bool isError = false;
                        foreach (var items in _listTransection)
                        {
                            //check data is current workflow status 
                            if (!_helper.CheckNull(items.Risk_Status_Workflow) && _helper.CheckCurrentWorklfow(items.Risk_Id, items.Risk_Status_Workflow, "Transection","Console"))
                            {
                                //check Status Workflow
                                if (items.Risk_Status_Workflow == "1")//chang status is 2 CO-Asset Draft
                                {

                                    Consolidate_Transection temp = new Consolidate_Transection();
                                    temp.Risk_Id = items.Risk_Id;
                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                    temp.Risk_Status_Workflow = "2";// CO-Asset Draft
                                    temp.Risk_Type = items.Risk_Type;
                                    temp.Risk_AssignTo = "";
                                    insertConsoleTransectionModel.Add(temp);
                                }
                                else if (items.Risk_Status_Workflow == "4")//chang status is 5 CO-BU Draft
                                {
                                    Consolidate_Transection temp = new Consolidate_Transection();
                                    temp.Risk_Id = items.Risk_Id;
                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                    temp.Risk_Status_Workflow = "5";// CO-BU Draft
                                    temp.Risk_Type = items.Risk_Type;
                                    temp.Risk_AssignTo = "";
                                    insertConsoleTransectionModel.Add(temp);
                                }
                                else if (items.Risk_Status_Workflow == "7")//find status is 8 CO-BU Draft
                                {
                                    Consolidate_Transection temp = new Consolidate_Transection();
                                    temp.Risk_Id = items.Risk_Id;
                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                    temp.Risk_Status_Workflow = "8";// CO-DI Draft
                                    temp.Risk_Type = items.Risk_Type;
                                    temp.Risk_AssignTo = "";
                                    insertConsoleTransectionModel.Add(temp);
                                }
                                else if (items.Risk_Status_Workflow == "10")//find status is 11 CO-BU Draft
                                {
                                    Consolidate_Transection temp = new Consolidate_Transection();
                                    temp.Risk_Id = items.Risk_Id;
                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                    temp.Risk_Status_Workflow = "11";// CO-FG Draft
                                    temp.Risk_Type = items.Risk_Type;
                                    temp.Risk_AssignTo = "";
                                    insertConsoleTransectionModel.Add(temp);
                                }
                                else if (items.Risk_Status_Workflow == "13")//Waiting ERM team Consolidate
                                {
                                    Consolidate_Transection temp = new Consolidate_Transection();
                                    temp.Risk_Id = items.Risk_Id;
                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                    temp.Risk_Status_Workflow = "14";//ERM Consolidate
                                    temp.Risk_Type = items.Risk_Type;
                                    temp.Risk_AssignTo = "";
                                    insertConsoleTransectionModel.Add(temp);
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
                        if(!isError)
                            p.Add("@ConsolidateTransection", _helper.ToDataTable(insertConsoleTransectionModel).AsTableValuedParameter());
                        else
                            insertConsoleTransectionModel = new List<Consolidate_Transection>();
                    }
                    if (insertConsoleTransectionModel.Count > 0 || insertConsoleStaffModel.Count > 0 || request.Module.ToUpper() == "SEARCH")
                    {
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        //loop call web service get org GetOrganizations by business unit
                        //xml return result BU OR FG
                        //update status and assignto in transection 
                        response.body = conn.Query<object>("sp_Risk_Consolidate", p, commandType: CommandType.StoredProcedure).ToList();
                        response.StatusId = p.Get<int>("@StatusChange");
                        if (response.StatusId == 0)
                        {
                            /*if (SendEmail)
                            {
                                _e.SendEmail(request.Email, null, null, null, null, null, null, insertConsoleTransectionModel);
                            }*/
                            /*if (SendEmail)
                            {
                                _e.ConsoleProcessMail(insertConsoleTransectionModel);
                            }*/
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
                        if (_helper.CheckNull(response.ErrorMessage))
                        {
                            response.ErrorMessage = "Please contact to HEM Focal Point.  Risk Cordinator of BU level is not Consolidate.";
                            response.Status = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Risk_Consolidate]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Risk_ReConsolidate(RequestMessage<ReConsolidate> request)
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
                    List<ReConsoleTransection> insertReConsoleTreansectionModel = new List<ReConsoleTransection>();
                    p.Add("@Module", request.Module.ToUpper());
                    p.Add("@ReConsolidate_By", data.ReConsolidateBy);
                    bool isError = false;
                    if (!_helper.IsNullOrEmpty(data.ReConsoleTransection))
                    {
                        List<ReConsoleTransection> tempTransection = data.ReConsoleTransection;

                        //get Master_Cordinator 
                        var p_masterco = new DynamicParameters();
                        p_masterco.Add("@Table_Name", "Master_Cordinator");
                        var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterco, commandType: CommandType.StoredProcedure).ToList();
                        foreach (var items in tempTransection)
                        {
                            //check data is current workflow status 
                            if (!_helper.CheckNull(items.Risk_Status_Workflow) && _helper.CheckCurrentWorklfow(items.Risk_Id, items.Risk_Status_Workflow, "Transection", "ReConsole"))
                            {
                                if (items.Risk_Status_Workflow == "5")// if BU Re Consolidate
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
                                            _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Department")).ToList();
                                            if (_tempCo.Count > 0)//is match CO Department
                                                break;
                                        }
                                        if (_tempCo.Count > 0)
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                            ReConsoleTransection temp = new ReConsoleTransection();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                            temp.Risk_Status_Workflow = "4";
                                            temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                            /*if (_tempCo[0].Coordinator_Level == "Division")
                                                temp.Risk_Status_Workflow = "7";// Waiting CO for parent org Division
                                            else if (_tempCo[0].Coordinator_Level == "Group")
                                                temp.Risk_Status_Workflow = "10";// Waiting CO for parent org Group*/
                                            //temp.Risk_Status_Approve = "20";//status BU Approve
                                            //temp.Risk_Type = "Organization";
                                            temp.Risk_AssignTo = assignTo;
                                            temp.Comment = items.Comment;
                                            #region find Co FG  for assignto
                                            //AssignTo FG Owner
                                            //get recursive org

                                            /*orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
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
                                            }*/
                                            #endregion
                                            insertReConsoleTreansectionModel.Add(temp);
                                        }
                                        else // not have co config or not parent org 
                                        {
                                            bool chkCeo = false;
                                            bool chkOtherOrg = false;
                                            foreach (var orgParent in orgparentInfo)
                                            {
                                                if (orgParent.ORGANIZATION_LEVEL == "CEO")
                                                    chkCeo = true;
                                                if (orgParent.ORGANIZATION_LEVEL == "Division" || orgParent.ORGANIZATION_LEVEL == "Group" || orgParent.ORGANIZATION_LEVEL == "Department")
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
                                                    ReConsoleTransection temp = new ReConsoleTransection();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                    temp.Risk_Status_Workflow = "13";
                                                    //temp.Risk_Status_Approve = "20";//status BU Approve
                                                    //temp.Risk_Type = "Organization";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    insertReConsoleTreansectionModel.Add(temp);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (items.Risk_Status_Workflow == "8")// if DI Re Consolidate
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
                                            _tempCo = mCo.Where(o => o.Coordinator_Department_Id == orgParent.ORGANIZATION_ID && (o.Coordinator_Level == "Group" || o.Coordinator_Level == "Division")).ToList();
                                            if (_tempCo.Count > 0)//is match CO FG OR DI
                                                break;
                                        }
                                        if (_tempCo.Count > 0)
                                        {
                                            var assignTo = _helper.GetAssignToFromConfig(_tempCo);
                                            ReConsoleTransection temp = new ReConsoleTransection();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                            temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;

                                            if (_tempCo[0].Coordinator_Level == "Division")
                                                temp.Risk_Status_Workflow = "7";// Waiting CO DI Console
                                            else if (_tempCo[0].Coordinator_Level == "Group")
                                                temp.Risk_Status_Workflow = "10";// Waiting CO FG Console
                                            //temp.Risk_Status_Approve = "21";//status DI Approve
                                            //temp.Risk_Type = "Organization";
                                            temp.Risk_AssignTo = assignTo;
                                            temp.Comment = items.Comment;
                                            //AssignTo FG Owner
                                            //get recursive org

                                            insertReConsoleTreansectionModel.Add(temp);
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
                                                    ReConsoleTransection temp = new ReConsoleTransection();
                                                    temp.Risk_Id = items.Risk_Id;
                                                    temp.Risk_Business_Unit = items.Risk_Business_Unit;

                                                    temp.Risk_Status_Workflow = "13";
                                                    //temp.Risk_Status_Approve = "21";//status DI Approve
                                                    //temp.Risk_Type = "Organization";
                                                    temp.Risk_AssignTo = assignTo;
                                                    temp.Comment = items.Comment;
                                                    insertReConsoleTreansectionModel.Add(temp);
                                                }
                                            }
                                        }
                                    }

                                }
                                else if (items.Risk_Status_Workflow == "11")// if FG Re Consolidate
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
                                            ReConsoleTransection temp = new ReConsoleTransection();
                                            temp.Risk_Id = items.Risk_Id;
                                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                                            temp.Risk_Business_Unit_WF = _tempCo[0].Coordinator_Department_Id;
                                            temp.Risk_Status_Workflow = "10";// Waiting CO FG Console
                                            //temp.Risk_Status_Approve = "21";//status DI Approve
                                            //temp.Risk_Type = "Organization";
                                            temp.Risk_AssignTo = assignTo;
                                            temp.Comment = items.Comment;
                                            //AssignTo FG Owner
                                            //get recursive org

                                            insertReConsoleTreansectionModel.Add(temp);
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
                            if(!isError)
                                p.Add("@ReConsole", _helper.ToDataTable(insertReConsoleTreansectionModel).AsTableValuedParameter());
                            else
                                insertReConsoleTreansectionModel = new List<ReConsoleTransection>();
                        }
                        /*if (SendEmail)
                        {
                            _e.ApproveProcessMail(insertApproveAssetModel, insertApproveOrgModel);
                        }*/
                        if (insertReConsoleTreansectionModel.Count > 0)
                        {
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            //loop call web service get org GetOrganizations by business unit
                            //xml return result BU OR FG
                            //update status and assignto in transection 

                            response.body = conn.Query<object>("sp_ReConsole", p, commandType: CommandType.StoredProcedure).ToList();
                            response.StatusId = p.Get<int>("@StatusChange");
                            if (response.StatusId == 0)
                            {

                                /*if (SendEmail)
                                {
                                    _e.ApproveProcessMail(insertApproveAssetModel, insertApproveOrgModel);
                                }*/
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
                            if (_helper.CheckNull(response.ErrorMessage))
                            {
                                response.ErrorMessage = "Please contact to HEM Focal Point.  Risk Cordinator of BU level is not Re Worker.";
                                response.Status = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_ReConsole]" + ex.Message;
            }

            return response;
        }

        public ResponseMessage<object> API_Risk_Transfer(RequestMessage<Consolidate_Transfer> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@Risk_Modified_By", request.SessionEmpID);

                if (data != null)
                {
                    p.Add("@Risk_Id", data.Risk_Id);
                    p.Add("@Risk_Co_Id", data.Risk_Co_Id);
                    p.Add("@Risk_Co_Id_New", data.Risk_Co_Id_New);
                    p.Add("@Risk_Register_By", data.Risk_Register_By);
                    p.Add("@Risk_Category", data.Risk_Category);
                    p.Add("@Risk_Status", data.Risk_Status);
                    p.Add("@Risk_Business_Unit", data.Risk_Business_Unit);
                    p.Add("@Risk_Name", data.Risk_Name);
                    p.Add("@Risk_Running", data.Risk_Running);
                    p.Add("@Risk_Rating", data.Risk_Rating);
                    p.Add("@Risk_Escalation", data.Risk_Escalation);
                    p.Add("@QuarterID", data.QuarterID);
                }

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<object>("sp_Risk_Transfer", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    if (response.StatusId == 0)
                    {
                        response.Status = true;
                    }
                    else
                    {
                        response.ErrorMessage = p.Get<string>("@StatusText");
                        response.Status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[GetFinancialImpact]" + ex.Message;
            }
            return response;
        }
    }
}
