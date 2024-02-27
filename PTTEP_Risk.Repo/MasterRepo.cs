using Dapper;
using Newtonsoft.Json;
using PTTEP_Risk.Help;
using PTTEP_Risk.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PTTEP_Risk.Repo
{
    public class MasterRepo
    {
        private Helper _help = new Helper();
        public ResponseMessage<List<ResQuarter>> GetQuarter(RequestMessage<ReqQuarter> request)
        {
            ResponseMessage<List<ResQuarter>> response = new ResponseMessage<List<ResQuarter>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@QuarterID", data.QuarterID);
                p.Add("@QuarterCode", data.QuarterCode);
                p.Add("@Year", data.Year);
                p.Add("@QuarterName", data.QuarterName);
                p.Add("@BuCoodinator", data.BuCoodinator);
                p.Add("@BusinessUniteValue", data.BusinessUniteValue);
                p.Add("@ImpactRating", data.ImpactRating);
                p.Add("@LikelihoodRating", data.LikelihoodRating);
                p.Add("@RiskCategory", data.RiskCategory);
                p.Add("@RiskRating", data.RiskRating);
                p.Add("@StartQuarter", data.StartQuarter);
                p.Add("@EndQuarter", data.EndQuarter);
                p.Add("@LockDate", data.LockDate);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResQuarter>("sp_Mas_Quarter_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = "[GetQuarter]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResBUCoordinator>> GetBUCoordinator(RequestMessage<List<ReqBUCoordinator>> request)
        {
            ResponseMessage<List<ResBUCoordinator>> response = new ResponseMessage<List<ResBUCoordinator>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;

                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    if (request.Module != "exportexcel")
                    {
                        foreach (var item in data)
                        {
                            p.Add("@CoordinatorId", item.CoordinatorId);
                            p.Add("@QuarterID", item.QuarterID);
                            p.Add("@EmpID", item.EmpID);
                            p.Add("@DeptID", item.DeptID);
                            p.Add("@Level", item.Level);
                            p.Add("@CoorBU", item.CoorBU);
                            p.Add("@BULevel", item.BULevel);

                            if (item.sBUCoorEx != null)
                            {
                                DataTable dt = _help.ToDataTable(item.sBUCoorEx);
                                p.Add("@sBUCoorEx", dt.AsTableValuedParameter());
                            }

                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                            response.body = conn.Query<ResBUCoordinator>("sp_Mas_BuCordinator_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        }
                    }
                    else
                    {
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                        response.body = conn.Query<ResBUCoordinator>("sp_Mas_BuCordinator_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    }
                    
                    response.StatusId = p.Get<int>("@StatusChange");
                    if (response.StatusId == 0)
                    {
                        response.Status = true;
                    }
                    else
                    {
                        if (request.Module == "importExcel")
                        {
                            response.body[0].sBUCoorEx = conn.Query<sBUCoorEx>("sp_Mas_BuCordinator_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        }

                        response.ErrorMessage = p.Get<string>("@StatusText");
                        response.Status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[GetRiskCategory]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResFinancialImpact>> GetFinancialImpact(RequestMessage<ReqFinancialImpact> request)
        {
            ResponseMessage<List<ResFinancialImpact>> response = new ResponseMessage<List<ResFinancialImpact>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);
                if (request.Module != "exportexcel")
                {
                    p.Add("@BusinessID", data.BusinessID);
                    p.Add("@QuarterID", data.QuarterID);
                    p.Add("@BusinessCode", data.BusinessCode);
                    p.Add("@BusinessUnit", data.BusinessUnit);
                    p.Add("@NI", data.NI);
                    p.Add("@NPV_EMV", data.NPV_EMV);
                    p.Add("@DelFlag", data.DelFlag);
                    if (data.sFinancialEx != null)
                    {
                        DataTable dt = _help.ToDataTable(data.sFinancialEx);
                        p.Add("@sFinancialEx", dt.AsTableValuedParameter());
                    }
                }

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResFinancialImpact>("sp_Mas_FinancialImpact_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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

        public ResponseMessage<List<ResRiskCategory>> GetRiskCategory(RequestMessage<ReqRiskCategory> request)
        {
            ResponseMessage<List<ResRiskCategory>> response = new ResponseMessage<List<ResRiskCategory>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@RiskCategoryID", data.RiskCategoryID);
                p.Add("@QuarterID", data.QuarterID);
                p.Add("@RiskCategoryCode", data.RiskCategoryCode);
                p.Add("@RiskCategoryName", data.RiskCategoryName);
                p.Add("@ErmFlag", data.ErmFlag);
                p.Add("@OrderNum", data.OrderNum);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResRiskCategory>("sp_Mas_RiskCategory_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = "[GetRiskCategory]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResRiskRating>> GetRiskRating(RequestMessage<ReqRiskRating> request)
        {
            ResponseMessage<List<ResRiskRating>> response = new ResponseMessage<List<ResRiskRating>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@RiskRatingID", data.RiskRatingID);
                p.Add("@QuarterID", data.QuarterID);
                p.Add("@RiskRatingCode", data.RiskRatingCode);
                p.Add("@Likelihood", data.Likelihood);
                p.Add("@Impact", data.Impact);
                p.Add("@LikelihoodAndImpact", data.LikelihoodAndImpact);
                p.Add("@RiskRating", data.RiskRating);
                p.Add("@EscalationLevel", data.EscalationLevel);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResRiskRating>("sp_Mas_RiskRating_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = "[GetRiskRating]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResLikelihood>> GetLikelihood(RequestMessage<ReqLikelihood> request)
        {
            ResponseMessage<List<ResLikelihood>> response = new ResponseMessage<List<ResLikelihood>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                var sitem = data.sReqLikelihoodItem;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@LikelihoodID", data.LikelihoodID);
                p.Add("@QuarterID", data.QuarterID);
                p.Add("@LikelihoodCode", data.LikelihoodCode);
                p.Add("@LikelihoodName", data.LikelihoodName);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResLikelihood>("sp_Mas_Likelihood_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    var LikelihoodID = p.Get<int>("@OutputId");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist" || request.Module == "getlistinfo")
                        {
                            foreach (var g in response.body)
                            {
                                var pg = new DynamicParameters();
                                pg.Add("@Module", "getlistitem");
                                pg.Add("@LikelihoodID", g.LikelihoodID);
                                pg.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                g.sResLikelihoodItem = conn.Query<ResLikelihoodItem>("sp_Mas_Likelihood_Insert_Update_Get", pg, commandType: CommandType.StoredProcedure).ToList();
                                response.StatusId = p.Get<int>("@StatusChange");
                            }

                            if (response.StatusId == 0)
                            {
                                response.Status = true;
                            }
                        }
                        else if (request.Module == "insert")
                        {
                            foreach (var i in sitem)
                            {
                                var pi = new DynamicParameters();
                                pi.Add("@Module", "insertitem");
                                pi.Add("@LikelihoodID", LikelihoodID);
                                pi.Add("@LikelihoodItemCode", i.LikelihoodItemCode);
                                pi.Add("@LikelihoodItemName", i.LikelihoodItemName);
                                pi.Add("@Description", i.Description);
                                pi.Add("@DelFlag", i.DelFlag);
                                pi.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_Likelihood_Insert_Update_Get", pi, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
                        else if (request.Module == "update")
                        {
                            foreach (var u in sitem)
                            {
                                var pu = new DynamicParameters();
                                pu.Add("@Module", "updateitem");
                                pu.Add("@LikelihoodItemID", u.LikelihoodItemID);
                                pu.Add("@LikelihoodItemCode", u.LikelihoodItemCode);
                                pu.Add("@LikelihoodItemName", u.LikelihoodItemName);
                                pu.Add("@Description", u.Description);
                                pu.Add("@DelFlag", u.DelFlag);
                                pu.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_Likelihood_Insert_Update_Get", pu, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
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
                response.ErrorMessage = "[GetLikelihood]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResImpactCate>> GetImpactCategory(RequestMessage<ReqImpactCate> request)
        {
            ResponseMessage<List<ResImpactCate>> response = new ResponseMessage<List<ResImpactCate>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                var sitem = data.sReqImpactCateItem;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@ImpactCateID", data.ImpactCateID);
                p.Add("@QuarterID", data.QuarterID);
                p.Add("@ImpactCateCode", data.ImpactCateCode);
                p.Add("@ImpactCateName", data.ImpactCateName);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResImpactCate>("sp_Mas_ImpactCategory_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    var ImpactCateID = p.Get<int>("@OutputId");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist" || request.Module == "getlistinfo")
                        {
                            foreach (var g in response.body)
                            {
                                var pg = new DynamicParameters();
                                pg.Add("@Module", "getlistitem");
                                pg.Add("@ImpactCateID", g.ImpactCateID);
                                pg.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                g.sResImpactCateItem = conn.Query<ResImpactCateItem>("sp_Mas_ImpactCategory_Insert_Update_Get", pg, commandType: CommandType.StoredProcedure).ToList();
                                response.StatusId = p.Get<int>("@StatusChange");
                            }

                            if (response.StatusId == 0)
                            {
                                response.Status = true;
                            }
                        }
                        else if (request.Module == "insert")
                        {
                            foreach (var i in sitem)
                            {
                                var pi = new DynamicParameters();
                                pi.Add("@Module", "insertitem");
                                pi.Add("@ImpactCateID", ImpactCateID);
                                pi.Add("@ImpactCateItemCode", i.ImpactCateItemCode);
                                pi.Add("@ImpactCateItemName", i.ImpactCateItemName);
                                pi.Add("@Description", i.Description);
                                pi.Add("@DelFlag", i.DelFlag);
                                pi.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_ImpactCategory_Insert_Update_Get", pi, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
                        else if (request.Module == "update")
                        {
                            foreach (var u in sitem)
                            {
                                var pu = new DynamicParameters();
                                pu.Add("@Module", "updateitem");
                                pu.Add("@ImpactCateItemID", u.ImpactCateItemID);
                                pu.Add("@ImpactCateItemCode", u.ImpactCateItemCode);
                                pu.Add("@ImpactCateItemName", u.ImpactCateItemName);
                                pu.Add("@Description", u.Description);
                                pu.Add("@DelFlag", u.DelFlag);
                                pu.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_ImpactCategory_Insert_Update_Get", pu, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
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
                response.ErrorMessage = "[GetImpactCategory]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResPerManagement>> GetPerManagement(RequestMessage<ReqPerManagement> request)
        {
            ResponseMessage<List<ResPerManagement>> response = new ResponseMessage<List<ResPerManagement>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                var sitem = data.sReqPerManagementItem;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                if (data != null)
                {
                    p.Add("@GroupID", data.GroupID);
                    p.Add("@GroupName", data.GroupName);
                    p.Add("@PermissionLevel", data.PermissionLevel);
                    p.Add("@SearchBox", data.SearchBox);
                    p.Add("@DelFlag", data.DelFlag);
                }

                p.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResPerManagement>("sp_Mas_PerManagement_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    var PerManagement = p.Get<int>("@OutputId");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist" || request.Module == "getlistinfo")
                        {
                            foreach (var g in response.body)
                            {
                                var pg = new DynamicParameters();
                                pg.Add("@Module", "getlistitem");
                                pg.Add("@GroupID", g.GroupID);
                                pg.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                g.sResPerManagementItem = conn.Query<ResPerManagementItem>("sp_Mas_PerManagement_Insert_Update_Get", pg, commandType: CommandType.StoredProcedure).ToList();
                                response.StatusId = p.Get<int>("@StatusChange");
                            }

                            if (response.StatusId == 0)
                            {
                                response.Status = true;
                            }
                        }
                        else if (request.Module == "insert")
                        {
                            foreach (var i in sitem)
                            {
                                var pi = new DynamicParameters();
                                pi.Add("@Module", "insertitem");
                                pi.Add("@GroupID", data.GroupID);
                                pi.Add("@EmpCode", i.EmpCode);
                                pi.Add("@EmpName", i.EmpName);
                                pi.Add("@Email", i.Email);
                                pi.Add("@DelFlag", i.DelFlag);
                                pi.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_PerManagement_Insert_Update_Get", pi, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
                        else if (request.Module == "update")
                        {
                            foreach (var u in sitem)
                            {
                                var pu = new DynamicParameters();
                                pu.Add("@Module", "updateitem");
                                pu.Add("@GroupItemID", u.GroupItemID);
                                pu.Add("@EmpCode", u.EmpCode);
                                pu.Add("@EmpName", u.EmpName);
                                pu.Add("@Email", u.Email);
                                pu.Add("@DelFlag", u.DelFlag);
                                pu.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_PerManagement_Insert_Update_Get", pu, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
                        else if (request.Module == "delete")
                        {
                            foreach (var u in sitem)
                            {
                                var pu = new DynamicParameters();
                                pu.Add("@Module", "deleteitem");
                                pu.Add("@GroupItemID", u.GroupItemID);
                                //pu.Add("@EmpCode", u.EmpCode);
                                //pu.Add("@EmpName", u.EmpName);
                                //pu.Add("@Email", u.Email);
                                //pu.Add("@DelFlag", u.DelFlag);
                                pu.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_PerManagement_Insert_Update_Get", pu, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
                        else if (request.Module == "SearchBox")
                        {
                            response.Status = true;
                        }

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
                response.ErrorMessage = "[GetPerManagement]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResTopMenu>> GetTopMenu(RequestMessage<ReqTopMenu> request)
        {
            ResponseMessage<List<ResTopMenu>> response = new ResponseMessage<List<ResTopMenu>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);
                if (request.body != null)
                {
                    p.Add("@MenuID", data.MenuID);
                    p.Add("@MenuCode", data.MenuCode);
                    p.Add("@MenuName", data.MenuName);
                    p.Add("@Parents", data.Parents);
                    p.Add("@Link", data.Link);
                    p.Add("@FlagTag", data.FlagTag);
                    p.Add("@OrderBy", data.OrderBy);

                    if (data.PermissionGroup != null)
                    {
                        var PermissionGroup = string.Empty;
                        for (int i = 0; i < data.PermissionGroup.Length; i++)
                        {
                            var str = data.PermissionGroup[i];
                            if ((i + 1) == data.PermissionGroup.Length)
                            {
                                PermissionGroup += str;
                            }
                            else
                            {
                                PermissionGroup += str + ',';
                            }

                        }

                        p.Add("@PermissionGroup", PermissionGroup);
                    }

                    p.Add("@MenuIcon", data.MenuIcon);
                    p.Add("@EmpID", data.EmpID);
                    p.Add("@DelFlag", data.DelFlag);
                }

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResTopMenu>("sp_Mas_TopMenu_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist")
                        {
                            foreach (var item in response.body)
                            {
                                if (item.PermissionGroup != null && item.PermissionGroup != "")
                                {
                                    var a = item.PermissionGroup.Split(',');
                                    item.arrPermissionGroup = a;
                                }
                            }
                        }
                        else if (request.Module == "getmenu")
                        {
                            foreach (var a in response.body)
                            {
                                var ps = new DynamicParameters();

                                ps.Add("@Module", "getparents");
                                ps.Add("@Parents", a.MenuID);
                                ps.Add("@EmpID", data.EmpID);

                                ps.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                ps.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                                a.sResTopMenuItem = conn.Query<ResTopMenuItem>("sp_Mas_TopMenu_Insert_Update_Get", ps, commandType: CommandType.StoredProcedure).ToList();
                            }
                        }
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
                response.ErrorMessage = "[GetTopMenu]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResInstruction>> GetInstruction(RequestMessage<ReqInstruction> request)
        {
            ResponseMessage<List<ResInstruction>> response = new ResponseMessage<List<ResInstruction>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@InstructionID", data.InstructionID);
                p.Add("@Area", data.Area);
                p.Add("@Page", data.Page);
                p.Add("@Field", data.Field);
                p.Add("@InstructionName", data.InstructionName);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResInstruction>("sp_Mas_Instruction_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = "[GetInstruction]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResContactUs>> GetContactUs(RequestMessage<ReqContactUs> request)
        {
            ResponseMessage<List<ResContactUs>> response = new ResponseMessage<List<ResContactUs>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                if (request.body != null)
                {
                    p.Add("@ContactID", data.ContactID);
                    p.Add("@ContactName", data.ContactName);
                    p.Add("@Position", data.Position);
                    p.Add("@Email", data.Email);
                    p.Add("@Phone", data.Phone);
                    p.Add("@PicPath", data.PicPath);
                    p.Add("@FirstRow", data.FirstRow);

                    if (data.GroupMap != null)
                    {
                        var GroupMap = string.Empty;
                        for (int i = 0; i < data.GroupMap.Length; i++)
                        {
                            var str = data.GroupMap[i];
                            if ((i + 1) == data.GroupMap.Length)
                            {
                                GroupMap += str;
                            }
                            else
                            {
                                GroupMap += str + ',';
                            }

                        }

                        p.Add("@GroupMap", GroupMap);
                    }

                    p.Add("@DelFlag", data.DelFlag);
                }

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@ReqId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = response.body = conn.Query<ResContactUs>("sp_Mas_ContactUs_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist")
                        {
                            foreach (var item in response.body)
                            {
                                if (item.GroupMap != null && item.GroupMap != "")
                                {
                                    var a = item.GroupMap.Split(',');
                                    item.arrGroupMap = a;
                                }
                            }

                            foreach (var i in response.body)
                            {
                                var a = new DynamicParameters();
                                a.Add("@Module", "FileGetList");
                                a.Add("@Form", "2");
                                a.Add("@ReqId", i.ContactID);
                                a.Add("@SeqNo", "1");
                                a.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                a.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                                var iAtt = conn.Query<UploadFileModel>("USP_Attach_Files_Update", a, commandType: CommandType.StoredProcedure).ToList();
                                i.itemAttSeq1 = iAtt;
                            }

                            foreach (var i in response.body)
                            {
                                var a = new DynamicParameters();
                                a.Add("@Module", "FileGetList");
                                a.Add("@Form", "2");
                                a.Add("@ReqId", i.ContactID);
                                a.Add("@SeqNo", "2");
                                a.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                a.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                                var iAtt = conn.Query<UploadFileModel>("USP_Attach_Files_Update", a, commandType: CommandType.StoredProcedure).ToList();
                                i.itemAttSeq2 = iAtt;
                            }
                            response.Status = true;
                        }
                        else
                        {
                            response.ReqId = p.Get<int>("@ReqId");
                            response.Status = true;
                        }
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
                response.ErrorMessage = "[GetInstruction]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResBanner>> GetBanner(RequestMessage<ReqBanner> request)
        {
            ResponseMessage<List<ResBanner>> response = new ResponseMessage<List<ResBanner>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                if (request.body != null)
                {
                    p.Add("@BannerId", data.BannerId);
                    p.Add("@BannerName", data.BannerName);
                    p.Add("@BusinessId", data.BusinessId);
                    p.Add("@DelFlag", data.DelFlag);
                }

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@ReqId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = response.body = conn.Query<ResBanner>("sp_Mas_Banner_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist")
                        {
                            foreach (var i in response.body)
                            {
                                var a = new DynamicParameters();
                                a.Add("@Module", "FileGetList");
                                a.Add("@Form", "3");
                                a.Add("@ReqId", i.BannerId);
                                a.Add("@SeqNo", "1");
                                a.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                a.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                                var iAtt = conn.Query<UploadFileModel>("USP_Attach_Files_Update", a, commandType: CommandType.StoredProcedure).ToList();
                                i.itemAttSeq1 = iAtt;
                            }

                            foreach (var i in response.body)
                            {
                                var a = new DynamicParameters();
                                a.Add("@Module", "FileGetList");
                                a.Add("@Form", "3");
                                a.Add("@ReqId", i.BannerId);
                                a.Add("@SeqNo", "2");
                                a.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                a.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                                var iAtt = conn.Query<UploadFileModel>("USP_Attach_Files_Update", a, commandType: CommandType.StoredProcedure).ToList();
                                i.itemAttSeq2 = iAtt;
                            }
                            response.Status = true;
                        }
                        else
                        {
                            response.ReqId = p.Get<int>("@ReqId");
                            response.Status = true;
                        }
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
                response.ErrorMessage = "[GetInstruction]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResCorpTarget>> GetCorpTarget(RequestMessage<ReqCorpTarget> request)
        {
            ResponseMessage<List<ResCorpTarget>> response = new ResponseMessage<List<ResCorpTarget>>();
            try
            {
                var sitem = new List<ReqCorpTargetItem>();
                var p = new DynamicParameters();
                var data = request.body;

                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                if (data != null)
                {
                    p.Add("@CorpTargetID", data.CorpTargetID);
                    p.Add("@CorpTargetCode", data.CorpTargetCode);
                    p.Add("@CorpTargetName", data.CorpTargetName);
                    p.Add("@CorpTargetYear", data.CorpTargetYear);
                    p.Add("@DelFlag", data.DelFlag);

                    sitem = data.sReqCorpTargetItem;
                }

                p.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResCorpTarget>("sp_Mas_CorpTarget_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    var CorpTargetID = p.Get<int>("@OutputId");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist" || request.Module == "getlistinfo")
                        {
                            foreach (var g in response.body)
                            {
                                var pg = new DynamicParameters();
                                pg.Add("@Module", "getlistitem");
                                pg.Add("@CorpTargetID", g.CorpTargetID);
                                pg.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pg.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                g.sResCorpTargetItem = conn.Query<ResCorpTargetItem>("sp_Mas_CorpTarget_Insert_Update_Get", pg, commandType: CommandType.StoredProcedure).ToList();
                                response.StatusId = p.Get<int>("@StatusChange");
                            }

                            if (response.StatusId == 0)
                            {
                                response.Status = true;
                            }
                        }
                        else if (request.Module == "insert")
                        {
                            foreach (var i in sitem)
                            {
                                var pi = new DynamicParameters();
                                pi.Add("@Module", "insertitem");
                                pi.Add("@CorpTargetID", CorpTargetID);
                                pi.Add("@CorpTargetItemCode", i.CorpTargetItemCode);
                                pi.Add("@CorpTargetItemName", i.CorpTargetItemName);
                                pi.Add("@DelFlag", i.DelFlag);
                                pi.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pi.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_CorpTarget_Insert_Update_Get", pi, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
                        else if (request.Module == "update")
                        {
                            foreach (var u in sitem)
                            {
                                var pu = new DynamicParameters();
                                pu.Add("@Module", "updateitem");
                                pu.Add("@CorpTargetID", data.CorpTargetID);
                                pu.Add("@CorpTargetItemID", u.CorpTargetItemID);
                                pu.Add("@CorpTargetItemCode", u.CorpTargetItemCode);
                                pu.Add("@CorpTargetItemName", u.CorpTargetItemName);
                                pu.Add("@DelFlag", u.DelFlag);
                                pu.Add("@OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                pu.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                conn.Execute("sp_Mas_CorpTarget_Insert_Update_Get", pu, commandType: CommandType.StoredProcedure);
                            }
                            response.Status = true;
                        }
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
                response.ErrorMessage = "[GetRiskCatalog]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<ResRiskCatalog>> GetRiskCatalog(RequestMessage<ReqRiskCatalog> request)
        {
            ResponseMessage<List<ResRiskCatalog>> response = new ResponseMessage<List<ResRiskCatalog>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@RiskCatalogID", data.RiskCatalogID);
                p.Add("@RiskCatalogTitle", data.RiskCatalogTitle);
                p.Add("@RiskCatalogDesc", data.RiskCatalogDesc);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@ReqId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<ResRiskCatalog>("sp_Mas_RiskCatalog_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    response.StatusId = p.Get<int>("@StatusChange");
                    if (response.StatusId == 0)
                    {
                        if (request.Module == "getlist")
                        {
                            foreach (var i in response.body)
                            {
                                var a = new DynamicParameters();
                                a.Add("@Module", "FileGetList");
                                a.Add("@Form", "1");
                                a.Add("@ReqId", i.RiskCatalogID);
                                a.Add("@SeqNo", "1");
                                a.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                a.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                                var iAtt = conn.Query<UploadFileModel>("USP_Attach_Files_Update", a, commandType: CommandType.StoredProcedure).ToList();
                                i.itemAttSeq1 = iAtt;
                            }

                            foreach (var i in response.body)
                            {
                                var a = new DynamicParameters();
                                a.Add("@Module", "FileGetList");
                                a.Add("@Form", "1");
                                a.Add("@ReqId", i.RiskCatalogID);
                                a.Add("@SeqNo", "2");
                                a.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                                a.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                                var iAtt = conn.Query<UploadFileModel>("USP_Attach_Files_Update", a, commandType: CommandType.StoredProcedure).ToList();
                                i.itemAttSeq2 = iAtt;
                            }
                            response.Status = true;
                        }
                        else
                        {
                            response.ReqId = p.Get<int>("@ReqId");
                            response.Status = true;
                        }

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
                response.ErrorMessage = "[GetRiskCategory]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<Master_Assset>> GetAsset(RequestMessage<List<ReqMaster_Assset>> request)
        {
            ResponseMessage<List<Master_Assset>> response = new ResponseMessage<List<Master_Assset>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    foreach (var item in data)
                    {
                        p.Add("@Asset_Id", item.Asset_Id);
                        p.Add("@QuarterYear", item.QuarterYear);
                        p.Add("@QuarterID", item.QuarterID);
                        p.Add("@Asset_Code", item.Asset_Code);
                        p.Add("@Asset_Name", item.Asset_Name);
                        p.Add("@Asset_Short", item.Asset_Short);
                        p.Add("@Asset_Coordinators", item.Asset_Coordinators);
                        p.Add("@Asset_Level", item.Asset_Level);
                        p.Add("@Asset_Org", item.Asset_Org);
                        p.Add("@ActveDate", item.ActveDate);
                        p.Add("@DelFlag", item.DelFlag);

                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                        response.body = conn.Query<Master_Assset>("sp_Mas_Asset_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                    }
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
                response.ErrorMessage = "[GetRiskCategory]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<Master_Owner>> GetOwner(RequestMessage<Master_Owner> request)
        {
            ResponseMessage<List<Master_Owner>> response = new ResponseMessage<List<Master_Owner>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    p.Add("@Module", "GetOwner");
                    p.Add("@TextSearch1", data.Dept_Id);
                    response.body = conn.Query<Master_Owner>("sp_Get_DDL", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = "[GetOwner]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<Master_WPB>> GetWPB(RequestMessage<ReqMaster_WPB> request)
        {
            ResponseMessage<List<Master_WPB>> response = new ResponseMessage<List<Master_WPB>>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", request.Module);
                p.Add("@SessionEmpID", request.SessionEmpID);


                p.Add("@WpbId", data.WpbId);
                p.Add("@Year", data.Year);
                p.Add("@StartDate", data.StartDate);
                p.Add("@EndDate", data.EndDate);
                p.Add("@DelFlag", data.DelFlag);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<Master_WPB>("sp_Mas_WPB_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = "[GetRiskCategory]" + ex.Message;
            }
            return response;
        }

        public ResponseMessage<List<DropDownList_Master>> GetMaster_DDL(DropDownList_Master request)
        {
            ResponseMessage<List<DropDownList_Master>> response = new ResponseMessage<List<DropDownList_Master>>();
            try
            {
                var para = new DynamicParameters();
                para.Add("@Module", request.Module);
                para.Add("@TextSearch1", request.TextSearch1);
                para.Add("@TextSearch2", request.TextSearch2);
                para.Add("@TextSearch3", request.TextSearch3);
                para.Add("@TextSearch4", request.TextSearch4);
                para.Add("@TextSearch5", request.TextSearch5);
                para.Add("@TextSearch6", request.TextSearch6);
                para.Add("@TextSearch7", request.TextSearch7);
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    response.body = conn.Query<DropDownList_Master>("sp_Get_DDL", para, commandType: CommandType.StoredProcedure).ToList();
                    response.Status = true;
                }

            }
            catch (Exception ex)
            {
                response.Status = true;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public ResponseMessage<object> GetTemplateEmail(RequestMessage<EmailModelInsert> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Module", data.Module);
                p.Add("@Status", data.Status);
                p.Add("@Subject", data.Subject);
                p.Add("@Body1", data.Body1);
                p.Add("@Body2", data.Body2);
                p.Add("@Body3", data.Body3);
                p.Add("@Description", data.Description);
                p.Add("@SessionEmpID", request.SessionEmpID);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<object>("sp_Mas_Mail_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public ResponseMessage<object> ReplaceCordinator(RequestMessage<Master_ReplaceCordinator> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            try
            {
                var p = new DynamicParameters();
                var data = request.body;
                p.Add("@Risk_Business_Unit", data.Risk_Business_Unit);
                p.Add("@Co_Old", data.Co_Old);
                p.Add("@Co_New", data.Co_New);
                p.Add("@Type_Co", data.Type_Co);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    response.body = conn.Query<object>("sp_AddCo_ToTransection", p, commandType: CommandType.StoredProcedure).ToList();
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
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public ResponseMessage<object> AttachFileInsert(UploadFileModel request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            try
            {
                var p = new DynamicParameters();
                var data = request;
                p.Add("@SessionEmpID", data.SessionEmpID);
                p.Add("@Module", "FileInsert");
                p.Add("@Form", data.Form);
                p.Add("@ReqId", data.ReqId);
                p.Add("@SeqNo", data.SeqNo);
                p.Add("@FileName", data.FileName);
                p.Add("@RootPath", data.RootPath);
                p.Add("@PathFile", data.PathFile);

                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    conn.Execute("USP_Attach_Files_Update", p, commandType: CommandType.StoredProcedure);
                    response.Status = true;
                    response.ErrorMessage = "บันทึกสำเร็จ";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[AttachFileInsert]: " + ex.Message;
            }
            return response;
        }

        public ResponseMessage<object> AttachFileDelete(UploadFileModel request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            try
            {
                var p = new DynamicParameters();
                var data = request;
                p.Add("@Module", "FileDelete");
                p.Add("@AttachFileID", data.AttachFileID);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    conn.Execute("USP_Attach_Files_Update", p, commandType: CommandType.StoredProcedure);
                    response.Status = true;
                    response.ErrorMessage = "ลบสำเร็จ";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[AttachFileDelete]: " + ex.Message;
            }
            return response;
        }


    }
}
