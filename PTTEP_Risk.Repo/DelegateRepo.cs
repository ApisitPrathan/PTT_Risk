using System;
using System.Collections.Generic;
using System.Text;
using PTTEP_Risk.Model;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using PTTEP_Risk.Help;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PTTEP_Risk.Repo
{
    public class DelegateRepo
    {
        GetUser _s = new GetUser();
        Helper _h = new Helper();
        GetOrganizations _o = new GetOrganizations();
        RiskMap_Organization _m = new RiskMap_Organization();
        ConfigurationService _c = new ConfigurationService();
        public ResponseMessage<object> API_Delegate_Seacrh(RequestMessage<DelegateModel> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            var data = request.body;
            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
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
                            p.Add("@Module", "Search");
                            if (data.Delegate_Type == "1") //case chang co register
                            {
                                p.Add("@Risk_Register_By", data.Delegate_To);
                                p.Add("@Risk_Status_Workflow", "");
                            }
                            else if (data.Delegate_Type == "2") // case chang co submit
                            {
                                if(data.Delegate_Status == "1") // Wait CO Asset Submit
                                    p.Add("@Risk_Status_Workflow", "2");
                                else if (data.Delegate_Status == "2") // Wait CO Department Submit
                                    p.Add("@Risk_Status_Workflow", "5");
                                else if (data.Delegate_Status == "3") // Wait CO Division Submit
                                    p.Add("@Risk_Status_Workflow", "8");
                                else if (data.Delegate_Status == "4") // Wait CO FG Submit
                                    p.Add("@Risk_Status_Workflow", "11");
                                p.Add("@Risk_AssignTo", data.Delegate_To);
                            }
                            else if (data.Delegate_Type == "3")// case chang owner approve
                            {
                                if (data.Delegate_Status == "1") // Wait Owner Asset Submit
                                    p.Add("@Risk_Status_Workflow", "3");
                                else if (data.Delegate_Status == "2") // Wait Owner Department Submit
                                    p.Add("@Risk_Status_Workflow", "6");
                                else if (data.Delegate_Status == "3") // Wait Owner Division Submit
                                    p.Add("@Risk_Status_Workflow", "9");
                                else if (data.Delegate_Status == "4") // Wait Owner FG Submit
                                    p.Add("@Risk_Status_Workflow", "12");
                                p.Add("@Risk_AssignTo", data.Delegate_To);
                            }
                            p.Add("@Risk_Category", data.Risk_Category);
                            p.Add("@Risk_Status", data.Risk_Status);
                            p.Add("@Risk_Business_Unit", data.Risk_Business_Unit);
                            p.Add("@Risk_Name", data.Risk_Name);
                            p.Add("@Risk_Running", data.Risk_Running);
                            p.Add("@QuarterID", data.QuarterID);
                            p.Add("@WPBID", data.WPBID);
                            p.Add("@BUCollection", colBU);
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output, size: 255);
                            List<DelegateResult> seacrhResults = conn.Query<DelegateResult>("sp_Risk_Delegate", p, commandType: CommandType.StoredProcedure).ToList();
                            if (seacrhResults.Count > 0)
                            {
                                response.body = seacrhResults;
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
                            else
                            {
                                response.ErrorMessage = "No data seacrh !";
                                response.Status = false;
                            }
                        }
                    }
                    else
                    {
                        response.ErrorMessage = "[API_Delegate_Seacrh]" + "Can't find employee in webservice";
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

        public ResponseMessage<object> API_Delegate(RequestMessage<DelegateModel> request)
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
                    List<Delegate_Transection> insertDelegateModel = new List<Delegate_Transection>();
                    var data = request.body;
                    p.Add("@Module", "Delegate");
                    p.Add("@SessionEmpID", request.SessionEmpID);
                    //p.Add("@IdCollection", data.IdCollection);
                    if (!_h.IsNullOrEmpty(data.Delegate_Transection)) // consolidate table staff
                    {
                        List<Delegate_Transection> _listDelegate = data.Delegate_Transection;

                        foreach (var items in _listDelegate)
                        {
                            Delegate_Transection temp = new Delegate_Transection();
                            temp.Risk_Id = items.Risk_Id;
                            temp.Delegate_Type = items.Delegate_Type;
                            temp.Risk_Business_Unit = items.Risk_Business_Unit;
                            temp.Risk_Status_Workflow = items.Risk_Status_Workflow;
                            temp.Risk_DelegateTo = items.Risk_DelegateTo;
                            temp.Remark = items.Remark;
                            insertDelegateModel.Add(temp);
                        }
                        p.Add("@DelegateTransection", _h.ToDataTable(insertDelegateModel).AsTableValuedParameter());
                    }
                    
                    p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    //loop call web service get org GetOrganizations by business unit
                    //xml return result BU OR FG
                    //update status and assignto in transection 
                    response.body = conn.Query<object>("sp_Risk_Delegate", p, commandType: CommandType.StoredProcedure).ToList();
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
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[sp_Risk_Consolidate]" + ex.Message;
            }

            return response;
        }
    }
}
