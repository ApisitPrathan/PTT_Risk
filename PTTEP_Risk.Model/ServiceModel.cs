using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class ServiceModel
    {
        public string SearchEmployee { get; set; }
        public string GetEmployee { get; set; }
        public string GetActingEmpId { get; set; }
        public string GetOrganizations { get; set; }
        public string SearchOrganizations { get; set; }
        public string GetParentOrganizations { get; set; }
        public string GetChildOrganizations { get; set; }
        public string PartUploadFile { get; set; }
        public string Email_HostName { get; set; }
        public string Email_FromUser { get; set; }
        public string Email_FromAddress { get; set; }
        public string Email_ToAddress { get; set; }
        public string Email_FromPassword { get; set; }
        public string TokenService { get; set; }
        public string Port { get; set; }
        public string SendFrom { get; set; }
        public string Link { get; set; }
        public string Email_CC { get; set; }
        public bool SentToUsers { get; set; }
        public string PasswordAPI { get; set; }

    }
}
