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

namespace PTTEP_Risk.Repo
{
    public class GenTransection_Repo
    {
        Helper _helper = new Helper();
        public ResponseMessage<List<RiskView>> API_Risk_Insert_Update_Get(RequestMessage<Risk> request)
        {
            ResponseMessage<List<RiskView>> response = new ResponseMessage<List<RiskView>>();

            var p = new DynamicParameters();
            var p_History = new DynamicParameters();
            var data = request.body;

            try
            {
                p.Add("@Module", request.Module.ToUpper());
                p.Add("@SessionEmpID", request.SessionEmpID);
                p.Add("@Role", request.body.Role.ToUpper());
                p.Add("@Risk_Id", request.body.Risk_Id);
                p.Add("@Risk_Co_Id", request.body.Risk_Co_Id);
                p.Add("@Risk_Register_Date", request.body.Risk_Register_Date);
                p.Add("@Risk_Name", request.body.Risk_Name);
                p.Add("@Risk_Business_Unit", request.body.Risk_Business_Unit);
                p.Add("@Risk_Business_Unit_Abbreviation", request.body.Risk_Business_Unit_Abbreviation);
                p.Add("@Risk_Status", request.body.Risk_Status);
                p.Add("@Risk_Likelihood", request.body.Risk_Likelihood);
                p.Add("@Risk_Impact", request.body.Risk_Impact);
                p.Add("@Risk_Category", request.body.Risk_Category);
                p.Add("@Risk_Objective", request.body.Risk_Objective);
                p.Add("@Risk_Objective_Parent", request.body.Risk_Objective_Parent);
                p.Add("@Risk_Unit_KPI", request.body.Risk_Unit_KPI);
                p.Add("@Risk_Context", request.body.Risk_Context);
                p.Add("@Risk_Status_Workflow", request.body.Risk_Status_Workflow);
                p.Add("@Risk_Register_By", request.body.Risk_Register_By);
                p.Add("@Risk_Modified_By", request.body.Risk_Modified_By);
                p.Add("@Risk_Type", request.body.Risk_Type);
                p.Add("@QuarterID", request.body.QuarterID);
                p.Add("@WPBID", request.body.WPBID);
                p.Add("@Delete_Flag", request.body.Delete_Flag);
                p.Add("@ReCall", request.body.ReCall);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p_History.Add("@Table", request.body.Role.ToUpper());
                p_History.Add("@Risk_Id", request.body.Risk_Id);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    if (request.Module.ToUpper() == "GET")
                    {
                        p.Add("@Bowtie", "RISK");
                        response.body = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        p.Add("@Bowtie", "ROOTCAUSE");
                        response.body[0].RootCause = conn.Query<RootCause>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        p.Add("@Bowtie", "ROOTCAUSE_MITIGATION");
                        response.body[0].RootCause_Mitigation = conn.Query<RootCause_Mitigation>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        p.Add("@Bowtie", "IMPACT");
                        response.body[0].Impact = conn.Query<Impact>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        p.Add("@Bowtie", "IMPACT_MITIGATION");
                        response.body[0].Impact_Mitigation = conn.Query<Impact_Mitigation>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        response.body[0].Risk_History = conn.Query<History>("sp_Risk_History", p_History, commandType: CommandType.StoredProcedure).ToList();
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
                    else//Module Insert Or Update
                    {
                        response.body = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        response.StatusId = p.Get<int>("@StatusChange");
                        response.Return_Id = p.Get<int>("@Return_Id");
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
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[API_Risk_Insert_Update_Get]" + ex.Message;
            }

            return response;
        }
        public ResponseMessage<List<RootCause>> API_Risk_RootCause_Insert_Update_Get(RequestMessage<RootCause> request)
        {
            ResponseMessage<List<RootCause>> response = new ResponseMessage<List<RootCause>>();

            var p = new DynamicParameters();
            var data = request.body;

            try
            {
                p.Add("@Module", request.Module.ToUpper());
                p.Add("@SessionEmpID", request.SessionEmpID);
                p.Add("@Role", request.body.Role.ToUpper());
                p.Add("@Risk_Id", request.body.Risk_Id);
                p.Add("@RootCause_Id", request.body.RootCause_Id);
                p.Add("@RootCause_Category", request.body.RootCause_Category);
                p.Add("@RootCause_Likelihood", request.body.RootCause_Likelihood);
                p.Add("@RootCause_Mitigation_Type", request.body.RootCause_Mitigation_Type);
                p.Add("@RootCause_After_Mitigated", request.body.RootCause_After_Mitigated);
                p.Add("@RootCause_KRI_Name", request.body.RootCause_KRI_Name);
                p.Add("@RootCause_KRI_Threshold_Green", request.body.RootCause_KRI_Threshold_Green);
                p.Add("@RootCause_KRI_Threshold_Red", request.body.RootCause_KRI_Threshold_Red);
                p.Add("@RootCause_KRI_Status", request.body.RootCause_KRI_Status);
                p.Add("@RootCause_KRI_Justification", request.body.RootCause_KRI_Justification);
                if (!_helper.CheckNull(request.body.DeleteFag))
                {
                    p.Add("@DeleteFag", request.body.DeleteFag.ToUpper());
                    p.Add("@Delete_RootCause_Id", request.body.Delete_RootCause_Id);
                    p.Add("@Delete_RootCause_Mitigation_Id", request.body.Delete_RootCause_Mitigation_Id);
                }
                if (!_helper.IsNullOrEmpty(data.RootCause_Mitigation))
                {
                    p.Add("@RootCause_Mitigation", _helper.ToDataTable(data.RootCause_Mitigation).AsTableValuedParameter());
                }

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    if (request.Module.ToUpper() == "GET")
                    {
                        p.Add("@Bowtie", "ROOTCAUSE");
                        response.body = conn.Query<RootCause>("sp_Risk_RootCause_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        p.Add("@Bowtie", "ROOTCAUSE_MITIGATION");
                        response.body[0].RootCause_Mitigation = conn.Query<RootCause_Mitigation>("sp_Risk_RootCause_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();

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
                    else {//Module Insert , Update , Delete
                        response.body = conn.Query<RootCause>("sp_Risk_RootCause_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[API_Risk_RootCause_Insert_Update_Get]" + ex.Message;
            }

            return response;
        }
        public ResponseMessage<List<Impact>> API_Risk_Impact_Insert_Update_Get(RequestMessage<Impact> request)
        {
            ResponseMessage<List<Impact>> response = new ResponseMessage<List<Impact>>();

            var p = new DynamicParameters();
            var data = request.body;

            try
            {
                p.Add("@Module", request.Module.ToUpper());
                p.Add("@SessionEmpID", request.SessionEmpID);
                p.Add("@Role", request.body.Role.ToUpper());
                p.Add("@Risk_Id", request.body.Risk_Id);
                p.Add("@Impact_Id", request.body.Impact_Id);
                p.Add("@Impact_Category", request.body.Impact_Category);
                p.Add("@Impact_NPT_EMV", request.body.Impact_NPT_EMV);
                p.Add("@Impact_Total_Amont", request.body.Impact_Total_Amont);
                p.Add("@Impact_Description", request.body.Impact_Description);
                p.Add("@Impact_Level", request.body.Impact_Level);
                p.Add("@Impact_Rating", request.body.Impact_Rating);
                p.Add("@Impact_Mitigation_Type", request.body.Impact_Mitigation_Type);
                p.Add("@Impact_After_Mitigated", request.body.Impact_After_Mitigated);
                if (!_helper.CheckNull(request.body.DeleteFag))
                {
                    p.Add("@DeleteFag", request.body.DeleteFag.ToUpper());
                    p.Add("@Delete_Impact_Id", request.body.Delete_Impact_Id);
                    p.Add("@Delete_Impact_Mitigation_Id", request.body.Delete_Impact_Mitigation_Id);
                }
                if (!_helper.IsNullOrEmpty(data.Impact_Mitigation))
                {
                    p.Add("@Impact_Mitigation", _helper.ToDataTable(data.Impact_Mitigation).AsTableValuedParameter());
                }

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    if (request.Module.ToUpper() == "GET") //Module GET
                    {
                        p.Add("@Bowtie", "IMPACT");
                        response.body = conn.Query<Impact>("sp_Risk_Impact_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        p.Add("@Bowtie", "IMPACT_MITIGATION");
                        response.body[0].Impact_Mitigation = conn.Query<Impact_Mitigation>("sp_Risk_Impact_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();

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
                    else //Module Insert , Update , Delete
                    {
                        response.body = conn.Query<Impact>("sp_Risk_Impact_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[API_Risk_Impact_Insert_Update_Get]" + ex.Message;
            }

            return response;
        }

    }
}
