using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using PTTEP_Risk.Model;
using System.Xml.Linq;
using RestSharp;
using PTTEP_Risk.Help;

namespace PTTEP_Risk.Help
{
    public class GetData
    {
        Helper _helper = new Helper();
        ConfigurationService _c = new ConfigurationService();
        public CollectionRisk GetCollenctionRiskData(DynamicParameters p,string Role)
        {
            CollectionRisk riskItems = new CollectionRisk();
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                
                var tempRisk = conn.Query<Risk>("sp_Get_Risk_Item", p, commandType: CommandType.StoredProcedure).ToList();
                var statusId = p.Get<int>("@StatusChange");
                if (statusId == 0)
                {
                    if (tempRisk.Count > 0)
                    {
                        //CollectionRisk riskItems = new CollectionRisk();
                        riskItems.Risk = tempRisk;
                        //response.body = riskItems;
                        string tempId = "";
                        int countId = 0;
                        foreach (var risk in tempRisk)
                        {
                            tempId += risk.Risk_Id;
                            if (countId != (tempRisk.Count - 1))//not last
                            {
                                tempId += ',';
                            }
                            countId++;
                        }

                        var p_RootCause = new DynamicParameters();
                        p_RootCause.Add("@Bowtie", "ROOTCAUSE");
                        p_RootCause.Add("@TempId", tempId);
                        p_RootCause.Add("@Role", Role);
                        p_RootCause.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        p_RootCause.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var RootCause_items = conn.Query<RootCause>("sp_Get_Risk_Item", p_RootCause, commandType: CommandType.StoredProcedure).ToList();
                        if (RootCause_items.Count > 0)
                        {
                            foreach (var risk in riskItems.Risk)
                            {
                                var _tempRootCause = RootCause_items.Where(o => o.Risk_Id == risk.Risk_Id).ToList();
                                if (_tempRootCause.Count > 0)
                                {
                                    risk.RootCause = _tempRootCause;
                                }
                            }
                        }
                        var p_Impact = new DynamicParameters();
                        p_Impact.Add("@Bowtie", "IMPACT");
                        p_Impact.Add("@TempId", tempId);
                        p_Impact.Add("@Role", Role);
                        p_Impact.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        p_Impact.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var Impact_items = conn.Query<Impact>("sp_Get_Risk_Item", p_Impact, commandType: CommandType.StoredProcedure).ToList();
                        if (Impact_items.Count > 0)
                        {
                            foreach (var risk in riskItems.Risk)
                            {
                                var _tempImpact = Impact_items.Where(o => o.Risk_Id == risk.Risk_Id).ToList();
                                if (_tempImpact.Count > 0)
                                    risk.Impact = _tempImpact;
                            }
                        }
                        var p_RootCause_Mitigation = new DynamicParameters();
                        p_RootCause_Mitigation.Add("@Bowtie", "ROOTCAUSE_MITIGATION");
                        p_RootCause_Mitigation.Add("@TempId", tempId);
                        p_RootCause_Mitigation.Add("@Role", Role);
                        p_RootCause_Mitigation.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        p_RootCause_Mitigation.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var RootCause_Mitication_items = conn.Query<RootCause_Mitigation>("sp_Get_Risk_Item", p_RootCause_Mitigation, commandType: CommandType.StoredProcedure).ToList();
                        if (RootCause_Mitication_items.Count > 0)
                        {
                            foreach (var risk in riskItems.Risk)
                            {
                                if (!_helper.IsNullOrEmpty(risk.RootCause))
                                {
                                    foreach (var rootcause in risk.RootCause)
                                    {
                                        var _tempRootcauseMitication = RootCause_Mitication_items.Where(o => o.RootCause_Id == rootcause.RootCause_Id).ToList();
                                        if (_tempRootcauseMitication.Count > 0)
                                            rootcause.RootCause_Mitigation = _tempRootcauseMitication;
                                    }
                                }
                            }
                        }

                        var p_Impact_Mitigation = new DynamicParameters();
                        p_Impact_Mitigation.Add("@Bowtie", "IMPACT_MITIGATION");
                        p_Impact_Mitigation.Add("@TempId", tempId);
                        p_Impact_Mitigation.Add("@Role", Role);
                        p_Impact_Mitigation.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        p_Impact_Mitigation.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var Impact_Mitication_items = conn.Query<Impact_Mitigation>("sp_Get_Risk_Item", p_Impact_Mitigation, commandType: CommandType.StoredProcedure).ToList();
                        if (Impact_Mitication_items.Count > 0)
                        {
                            foreach (var risk in riskItems.Risk)
                            {
                                if (!_helper.IsNullOrEmpty(risk.Impact))
                                {
                                    foreach (var impact in risk.Impact)
                                    {
                                        var _tempImpactMitication = Impact_Mitication_items.Where(o => o.Impact_Id == impact.Impact_Id).ToList();
                                        if (_tempImpactMitication.Count > 0)
                                            impact.Impact_Mitigation = _tempImpactMitication;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        //response.ErrorMessage = "[API_Report_Risk_Items] No Data Risk";
                        //response.Status = false;
                    }
                }
                else
                {
                    //response.ErrorMessage = p.Get<string>("@StatusText");
                    //response.Status = false;
                }
            }
            return riskItems;
        }
    }
}
