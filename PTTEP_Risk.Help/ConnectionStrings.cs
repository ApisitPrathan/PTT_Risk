using Microsoft.Extensions.Configuration;
using System.IO;

namespace PTTEP_Risk.Help
{
    public class ConnectionStrings
    {
        public static string ConnectionStringss()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

            var root = builder.Build();
            var datasource = root.GetSection("AppConfiguration")["DataSource"].ToString();
            var dbname = root.GetSection("AppConfiguration")["DatabaseName"].ToString();
            var user = root.GetSection("AppConfiguration")["UserName"].ToString();
            var pass = root.GetSection("AppConfiguration")["Password"].ToString();
            var connect = root.GetSection("AppConfiguration")["DataConnection"].ToString();
            
            connect = connect.Replace("_datasource_", datasource);
            connect = connect.Replace("_dbname_", dbname);
            connect = connect.Replace("_user_", user);
            connect = connect.Replace("_password_", pass);

            return connect;
        }
    }
}
