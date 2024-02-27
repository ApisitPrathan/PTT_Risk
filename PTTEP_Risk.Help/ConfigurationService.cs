using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IO;
using PTTEP_Risk.Model;

namespace PTTEP_Risk.Help
{
    public class ConfigurationService
    {
        public List<ServiceModel> ConnectionService()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            var root = builder.Build();
            List<ServiceModel> ServiceModel = new List<ServiceModel>();
            ServiceModel Services = new ServiceModel();
            //Service URL
            var employeeService = root.GetSection("AppConfiguration").GetSection("PTTServiceURL")["EmployeeService"].ToString();
            var organizationService = root.GetSection("AppConfiguration").GetSection("PTTServiceURL")["OrganizationService"].ToString();
            //Service Employee Method
            var searchEmployee = root.GetSection("AppConfiguration").GetSection("PTTServiceEmployeeServiceMethod")["SearchEmployee"].ToString();
            var getEmployee = root.GetSection("AppConfiguration").GetSection("PTTServiceEmployeeServiceMethod")["GetEmployee"].ToString();
            var getActingEmpId = root.GetSection("AppConfiguration").GetSection("PTTServiceEmployeeServiceMethod")["GetActingEmpID"].ToString();
            //Service Org Method
            var searchOrganizations = root.GetSection("AppConfiguration").GetSection("PTTServiceOrganizationServiceMethod")["SearchOrganizations"].ToString();
            var getOrganizations = root.GetSection("AppConfiguration").GetSection("PTTServiceOrganizationServiceMethod")["GetOrganizations"].ToString();
            var getParentOrganizations = root.GetSection("AppConfiguration").GetSection("PTTServiceOrganizationServiceMethod")["GetParentOrganizations"].ToString();
            var getChildOrganizations = root.GetSection("AppConfiguration").GetSection("PTTServiceOrganizationServiceMethod")["GetChildOrganizations"].ToString();

            //partUpload
            var PartUploadFile = root.GetSection("AppConfiguration").GetSection("PartUploadFile")["part"].ToString();
            //Email
            var Email_HostName = root.GetSection("AppConfiguration").GetSection("Email")["HostMailName"].ToString();
            var Email_FromUser = root.GetSection("AppConfiguration").GetSection("Email")["HostEMailUserName"].ToString();
            var Email_FromAddress = root.GetSection("AppConfiguration").GetSection("Email")["HostEMailFromAddress"].ToString();
            var Email_ToAddress = root.GetSection("AppConfiguration").GetSection("Email")["HostEMailFromTo"].ToString();
            var Email_CC = root.GetSection("AppConfiguration").GetSection("Email")["HostEMailCC"].ToString();
            var Email_FromPassword = root.GetSection("AppConfiguration").GetSection("Email")["HostEMailPassword"].ToString();
            var Email_Port = root.GetSection("AppConfiguration").GetSection("Email")["Port"].ToString();
            var Email_SendFrom = root.GetSection("AppConfiguration").GetSection("Email")["SendFrom"].ToString();
            var Email_Link = root.GetSection("AppConfiguration").GetSection("Email")["Link"].ToString();
            var Email_ToUser = bool.Parse(root.GetSection("AppConfiguration").GetSection("Email")["SentToUser"].ToString());

            //PasswordAPI
            var passwordAPI = root.GetSection("AppConfiguration")["PasswordAPI"].ToString();
            //TokenService
            var tokenService = root.GetSection("AppConfiguration")["TokenService"].ToString();

            Services.SearchEmployee = employeeService.Replace("_method", searchEmployee);
            Services.GetEmployee = employeeService.Replace("_method", getEmployee);
            Services.GetActingEmpId = employeeService.Replace("_method", getActingEmpId);

            Services.SearchOrganizations = organizationService.Replace("_method", searchOrganizations);
            Services.GetOrganizations = organizationService.Replace("_method", getOrganizations);
            Services.GetParentOrganizations = organizationService.Replace("_method", getParentOrganizations);
            Services.GetChildOrganizations = organizationService.Replace("_method", getChildOrganizations);

            Services.PartUploadFile = PartUploadFile;

            Services.Email_HostName = Email_HostName;
            Services.Email_FromUser = Email_FromUser;
            Services.Email_FromAddress = Email_FromAddress;
            Services.Email_ToAddress = Email_ToAddress;
            Services.Email_FromPassword = Email_FromPassword;
            Services.Port = Email_Port;
            Services.SendFrom = Email_SendFrom;
            Services.Link = Email_Link;
            Services.Email_CC = Email_CC;
            Services.TokenService = tokenService;
            Services.SentToUsers = Email_ToUser;
            Services.PasswordAPI = passwordAPI;

            ServiceModel.Add(Services);

            return ServiceModel;
        }
    }
}
