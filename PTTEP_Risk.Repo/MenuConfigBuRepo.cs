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
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PTTEP_Risk.Repo
{
    public class MenuConfigBuRepo
    {
        Helper _helper = new Helper();
        GetUser _s = new GetUser();
        GetOrganizations _o = new GetOrganizations();
        RiskMap_Organization _menu = new RiskMap_Organization();
        ConfigurationService _c = new ConfigurationService();
        public ResponseMessage<object> API_Get_BU_Config(ResponseMessage<RiskMap_Menu> request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var root = builder.Build();
            var data = request.body;
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {

                var p_Get = new DynamicParameters();
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
                        response.body = _menu.Get_RiskMap_Organization(dataOrg, data.QuarterID);
                    }
                    catch (Exception ex)
                    {
                        response.Status = false;
                        response.ErrorMessage = "[API_Get_Menu_RiskMap]" + ex.Message;
                    }
                }

            }

            return response;
        }
    }
}
