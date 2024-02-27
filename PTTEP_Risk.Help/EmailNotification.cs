using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using PTTEP_Risk.Model;
using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;

namespace PTTEP_Risk.Help
{
    public class EmailNotification
    {
        Helper _h = new Helper();
        EmailModel _e = new EmailModel();
        ConfigurationService _c = new ConfigurationService();     
        GetUser _u = new GetUser();
        GetData _g = new GetData();
        GetOrganizations _o = new GetOrganizations();

        public void SendEmail(string responeBy,List<TypeAssetModel> dataAsset, List<TypeOrganizationModel> dataOrganization, List<RejectTypeAsset> dataRejectAsset, List<RejectTypeOrganization> dataRejectOrganization,List<ApproveTypeAsset> dataApproveAsset,List<ApproveTypeOrganization> dataApproveOrganization,List<Consolidate_Transection> console_Transections)
        {

            List<ServiceModel> servicesModel = _c.ConnectionService();
            List<EmailModel> emailModels = GetTemplateEmail();
            SmtpClient smtpEmail = GetSmtlEmail(servicesModel);
            if (!_h.IsNullOrEmpty(dataAsset))
            {
                if(dataAsset.Count > 0)
                    SendEmailTypeAsset(dataAsset, emailModels, servicesModel, smtpEmail, responeBy);
            }
            if ( !_h.IsNullOrEmpty(dataOrganization))
            {
                if(dataOrganization.Count > 0)
                    SentEmailTypeOrganization(dataOrganization, emailModels, servicesModel, smtpEmail, responeBy);
            }
            if (!_h.IsNullOrEmpty(dataRejectAsset))
            {
                if(dataRejectAsset.Count > 0)
                    SendEmailTypeRejectAsset(dataRejectAsset, emailModels, servicesModel, smtpEmail, responeBy);
            }
            if (!_h.IsNullOrEmpty(dataRejectOrganization))
            {
                if(dataRejectOrganization.Count > 0)
                    SentEmailTypeRejectOrganization(dataRejectOrganization, emailModels, servicesModel, smtpEmail, responeBy);
            }
            if (!_h.IsNullOrEmpty(dataApproveAsset))
            {
                if (dataApproveAsset.Count > 0)
                    SentEmailTypeApproveAsset(dataApproveAsset, emailModels, servicesModel, smtpEmail, responeBy);
            }
            if (!_h.IsNullOrEmpty(dataApproveOrganization))
            {
                if (dataApproveOrganization.Count > 0)
                    SentEmailTypeApproveOrganization(dataApproveOrganization, emailModels, servicesModel, smtpEmail, responeBy);
            }
            if (!_h.IsNullOrEmpty(console_Transections))
            {
                if (console_Transections.Count > 0)
                    SentEmailTypeConsoleOrganization(console_Transections, emailModels, servicesModel, smtpEmail, responeBy);
            }
        }
        private void SendEmailTypeAsset(List<TypeAssetModel> dataAsset, List<EmailModel> emailTemplate, List<ServiceModel> servicesModel, SmtpClient smtp, string responeBy)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                foreach (var _data in dataAsset)
                {
                    var _result = emailTemplate.Where(e => e.Status == _data.Risk_Status_Workflow).ToList();
                    if (_result.Count > 0)
                    {

                        var p = new DynamicParameters();
                        p.Add("@Module", "GET");
                        p.Add("@Role", "TRANSECTION");
                        p.Add("@Risk_Id", _data.Risk_Id);
                        p.Add("@Bowtie", "RISK");
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                        p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var risk = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        int status = p.Get<int>("@StatusChange");
                        if (status == 0)
                        {
                            MailMessage mail = new MailMessage();
                            mail.From = new MailAddress(servicesModel[0].Email_FromAddress);
                            #region get AssignTo
                            string allUser = _h.GetUserEmailFromId(_data.Risk_AssignTo);
                            string To = "";
                            string CC = "";
                            List<EmailModel> replaceEmail = ReplaceTemplateEmail(_result, risk, servicesModel);
                            #endregion
                            #region get mail to and cc
                            if (dataAsset[0].Risk_Status_Workflow == "1" || dataAsset[0].Risk_Status_Workflow == "4" || dataAsset[0].Risk_Status_Workflow == "7" || dataAsset[0].Risk_Status_Workflow == "10") // STAFF SUBMIT
                            {

                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    To = allUser;
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    To = allUser;
                                    CC = "";
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }
                                
                            }
                            else if (dataAsset[0].Risk_Status_Workflow == "3" || dataAsset[0].Risk_Status_Workflow == "6" || dataAsset[0].Risk_Status_Workflow == "9" || dataAsset[0].Risk_Status_Workflow == "12") // CO SUBMIT
                            {
                                if (dataAsset[0].Risk_Status_Workflow == "12")//CO FG Submit
                                {
                                    //var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                                    //var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? _h.GetUserEmailFromId(risk[0].FG_Console) : "";
                                    var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? _h.GetUserEmailFromId(risk[0].DI_Approve) : "";
                                    var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? _h.GetUserEmailFromId(risk[0].DI_Console) : "";
                                    var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? _h.GetUserEmailFromId(risk[0].BU_Approve) : "";
                                    var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? _h.GetUserEmailFromId(risk[0].BU_Console) : "";
                                    var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? _h.GetUserEmailFromId(risk[0].Asset_Approve) : "";
                                    var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? _h.GetUserEmailFromId(risk[0].Asset_Console) : "";
                                    string[] strJoin = { DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                                    if (strJoin.Length == 0)
                                    {
                                        CC = _h.GetUserEmailFromId(risk[0].Risk_Register_By);
                                    }
                                    else
                                    {
                                        CC = string.Join(";", strJoin);
                                    }
                                }

                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    To = allUser;
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                    if (!_h.CheckNull(CC))
                                    {
                                        if (CC.Contains(';'))
                                        {
                                            string[] userCC = CC.Split(';');
                                            foreach (string MultiCC in userCC)
                                            {
                                                if (!_h.CheckNull(MultiCC))
                                                    mail.CC.Add(new MailAddress(MultiCC));
                                            }
                                        }
                                        else
                                        {
                                            mail.CC.Add(CC);
                                        }
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    To = allUser;
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC); 
                                }

                            }
                            #region not use
                            /*else if (dataAsset[0].Risk_Status_Workflow == "19" || dataAsset[0].Risk_Status_Workflow == "20" || dataAsset[0].Risk_Status_Workflow == "21" || dataAsset[0].Risk_Status_Workflow == "22") // Owner Approve
                            {
                                //string To = "";
                                if (dataAsset[0].Risk_Status_Workflow == "19")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "20")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "21")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "22")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                                if (allUser.Contains(';'))
                                {
                                    string[] userCC = allUser.Split(';');
                                    foreach (string MultiCC in userCC)
                                    {
                                        mail.CC.Add(new MailAddress(MultiCC));
                                    }
                                }
                                else
                                {
                                    mail.CC.Add(allUser);
                                }

                            }
                            else if (dataAsset[0].Risk_Status_Workflow == "15" || dataAsset[0].Risk_Status_Workflow == "16" || dataAsset[0].Risk_Status_Workflow == "17" || dataAsset[0].Risk_Status_Workflow == "18") //Owner Reject
                            {
                                //string To = "";
                                if (dataAsset[0].Risk_Status_Workflow == "15")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "16")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "17")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "18")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);

                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }

                            }*/
                            #endregion
                            #endregion
                            mail.Subject = replaceEmail[0].Subject;
                            if (servicesModel[0].SentToUsers)
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3);
                            else
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />TO : {3}<br />CC :{4}", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3, To, CC);
                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }
                    }
                }
            }
        }
        private void SentEmailTypeOrganization(List<TypeOrganizationModel> dataOrganization, List<EmailModel> emailTemplate, List<ServiceModel> servicesModel, SmtpClient smtp, string responeBy)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                foreach (var _data in dataOrganization)
                {
                    var _result = emailTemplate.Where(e => e.Status == _data.Risk_Status_Workflow).ToList();
                    if (_result.Count > 0)
                    {

                        var p = new DynamicParameters();
                        p.Add("@Module", "GET");
                        p.Add("@Role", "TRANSECTION");
                        p.Add("@Risk_Id", _data.Risk_Id);
                        p.Add("@Bowtie", "RISK");
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                        p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var risk = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        int status = p.Get<int>("@StatusChange");
                        if (status == 0)
                        {
                            MailMessage mail = new MailMessage();
                            //SmtpClient SmtpServer = new SmtpClient(servicesModel[0].Email_HostName);
                            mail.From = new MailAddress(servicesModel[0].Email_FromAddress);
                            #region get AssignTo
                            string allUser = _h.GetUserEmailFromId(_data.Risk_AssignTo);
                            string To = "";
                            string CC = "";
                            List<EmailModel> replaceEmail = ReplaceTemplateEmail(_result, risk, servicesModel);
                            #endregion
                            #region get mail to and cc
                            if (dataOrganization[0].Risk_Status_Workflow == "1" || dataOrganization[0].Risk_Status_Workflow == "4" || dataOrganization[0].Risk_Status_Workflow == "7" || dataOrganization[0].Risk_Status_Workflow == "10") // STAFF SUBMIT
                            {
                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    To = allUser;
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    To = allUser;
                                    CC = "";
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }
                            }
                            else if (dataOrganization[0].Risk_Status_Workflow == "3" || dataOrganization[0].Risk_Status_Workflow == "6" || dataOrganization[0].Risk_Status_Workflow == "9" || dataOrganization[0].Risk_Status_Workflow == "12") // CO SUBMIT
                            {
                                if (dataOrganization[0].Risk_Status_Workflow == "12")//CO FG Submit
                                {
                                    //var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                                    //var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? _h.GetUserEmailFromId(risk[0].FG_Console) : "";
                                    var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? _h.GetUserEmailFromId(risk[0].DI_Approve) : "";
                                    var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? _h.GetUserEmailFromId(risk[0].DI_Console) : "";
                                    var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? _h.GetUserEmailFromId(risk[0].BU_Approve) : "";
                                    var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? _h.GetUserEmailFromId(risk[0].BU_Console) : "";
                                    var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? _h.GetUserEmailFromId(risk[0].Asset_Approve) : "";
                                    var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? _h.GetUserEmailFromId(risk[0].Asset_Console) : "";
                                    string[] strJoin = { DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                                    if (strJoin.Length == 0)
                                    {
                                        CC = _h.GetUserEmailFromId(risk[0].Risk_Register_By);
                                    }
                                    else
                                    {
                                        CC = string.Join(";", strJoin);
                                    }
                                }
                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    To = allUser;
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if(!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                    if (!_h.CheckNull(CC))
                                    {
                                        if (CC.Contains(';'))
                                        {
                                            string[] userCC = CC.Split(';');
                                            foreach (string MultiCC in userCC)
                                            {
                                                if (!_h.CheckNull(MultiCC))
                                                    mail.CC.Add(new MailAddress(MultiCC));
                                            }
                                        }
                                        else
                                        {
                                            mail.CC.Add(CC);
                                        }
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    To = allUser;
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }
                            }
                            #region not use
                            /*else if (dataOrganization[0].Risk_Status_Workflow == "19" || dataOrganization[0].Risk_Status_Workflow == "20" || dataOrganization[0].Risk_Status_Workflow == "21" || dataOrganization[0].Risk_Status_Workflow == "22") // Owner Approve
                            {
                                //string To = "";
                                if (dataOrganization[0].Risk_Status_Workflow == "19")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "20")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "21")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "22")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);
                                *//*UAT*//*
                                //CC = allUser;
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = "test01@fusionsoft.co.th;apisitpr@fusionsoft.co.th";
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                                if (allUser.Contains(';'))
                                {
                                    string[] userCC = allUser.Split(';');
                                    foreach (string MultiCC in userCC)
                                    {
                                        mail.CC.Add(new MailAddress(MultiCC));
                                    }
                                }
                                else
                                {
                                    mail.CC.Add(allUser);
                                }
                            }
                            else if (dataOrganization[0].Risk_Status_Workflow == "15" || dataOrganization[0].Risk_Status_Workflow == "16" || dataOrganization[0].Risk_Status_Workflow == "17" || dataOrganization[0].Risk_Status_Workflow == "18") //Owner Reject
                            {
                                //string To = "";
                                if (dataOrganization[0].Risk_Status_Workflow == "15")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "16")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "17")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "18")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);

                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }*/
                            #endregion
                            #endregion
                            mail.Subject = replaceEmail[0].Subject;
                            if (servicesModel[0].SentToUsers)
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3);
                            else
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />TO : {3}<br />CC :{4}", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3, To, CC);
                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }
                    }
                }
            }
        }
        private void SendEmailTypeRejectAsset(List<RejectTypeAsset> dataAsset, List<EmailModel> emailTemplate, List<ServiceModel> servicesModel, SmtpClient smtp, string responeBy)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                foreach (var _data in dataAsset)
                {
                    var _result = emailTemplate.Where(e => e.Status == _data.Risk_Status_Workflow).ToList();
                    if (_result.Count > 0)
                    {

                        var p = new DynamicParameters();
                        p.Add("@Module", "GET");
                        p.Add("@Role", "TRANSECTION");
                        p.Add("@Risk_Id", _data.Risk_Id);
                        p.Add("@Bowtie", "RISK");
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                        p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var risk = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        int status = p.Get<int>("@StatusChange");
                        if (status == 0)
                        {
                            MailMessage mail = new MailMessage();
                            //SmtpClient SmtpServer = new SmtpClient(servicesModel[0].Email_HostName);
                            mail.From = new MailAddress(servicesModel[0].Email_FromAddress);
                            #region get AssignTo
                            string allUser = _h.GetUserEmailFromId(_data.Risk_AssignTo);
                            string To = "";
                            string CC = "";
                            List<EmailModel> replaceEmail = ReplaceTemplateEmailTypeRejectAsset(_result, risk, servicesModel, dataAsset);
                            #endregion
                            #region get mail to and cc
                            #region not use
                            /*if (dataAsset[0].Risk_Status_Workflow == "1" || dataAsset[0].Risk_Status_Workflow == "4" || dataAsset[0].Risk_Status_Workflow == "7" || dataAsset[0].Risk_Status_Workflow == "10") // STAFF SUBMIT
                            {
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = allUser;
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }
                            else if (dataAsset[0].Risk_Status_Workflow == "3" || dataAsset[0].Risk_Status_Workflow == "6" || dataAsset[0].Risk_Status_Workflow == "9" || dataAsset[0].Risk_Status_Workflow == "12") // CO SUBMIT
                            {
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = allUser;
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }
                            else if (dataAsset[0].Risk_Status_Workflow == "19" || dataAsset[0].Risk_Status_Workflow == "20" || dataAsset[0].Risk_Status_Workflow == "21" || dataAsset[0].Risk_Status_Workflow == "22") // Owner Approve
                            {
                                //string To = "";
                                if (dataAsset[0].Risk_Status_Workflow == "19")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "20")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "21")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "22")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                                if (allUser.Contains(';'))
                                {
                                    string[] userCC = allUser.Split(';');
                                    foreach (string MultiCC in userCC)
                                    {
                                        mail.CC.Add(new MailAddress(MultiCC));
                                    }
                                }
                                else
                                {
                                    mail.CC.Add(allUser);
                                }
                            }*/
                            #endregion
                            if (dataAsset[0].Risk_Status_Workflow == "15" || dataAsset[0].Risk_Status_Workflow == "16" || dataAsset[0].Risk_Status_Workflow == "17" || dataAsset[0].Risk_Status_Workflow == "18") //Owner Reject
                            {
                                //string To = "";
                                if (dataAsset[0].Risk_Status_Workflow == "15")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "16")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "17")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "18")//Owner FG Reject
                                {
                                    //var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                                    var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? _h.GetUserEmailFromId(risk[0].FG_Console) : "";
                                    var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? _h.GetUserEmailFromId(risk[0].DI_Approve) : "";
                                    var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? _h.GetUserEmailFromId(risk[0].DI_Console) : "";
                                    var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? _h.GetUserEmailFromId(risk[0].BU_Approve) : "";
                                    var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? _h.GetUserEmailFromId(risk[0].BU_Console) : "";
                                    var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? _h.GetUserEmailFromId(risk[0].Asset_Approve) : "";
                                    var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? _h.GetUserEmailFromId(risk[0].Asset_Console) : "";
                                    string[] strJoin = { FG_Console, DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                                    if (strJoin.Length == 0)
                                    {
                                        To = _h.GetUserEmailFromId(risk[0].Risk_Register_By);
                                    }
                                    else
                                    {
                                        To = string.Join(";", strJoin);
                                    }
                                }
                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    //To = allUser;
                                    CC = "";
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }

                            }
                            #endregion
                            mail.Subject = replaceEmail[0].Subject;
                            if (servicesModel[0].SentToUsers)
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3);
                            else
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />TO : {3}<br />CC :{4}", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3, To, CC);
                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }
                    }
                }
            }
        }
        private void SentEmailTypeRejectOrganization(List<RejectTypeOrganization> dataOrganization, List<EmailModel> emailTemplate, List<ServiceModel> servicesModel, SmtpClient smtp, string responeBy)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                foreach (var _data in dataOrganization)
                {
                    var _result = emailTemplate.Where(e => e.Status == _data.Risk_Status_Workflow).ToList();
                    if (_result.Count > 0)
                    {

                        var p = new DynamicParameters();
                        p.Add("@Module", "GET");
                        p.Add("@Role", "TRANSECTION");
                        p.Add("@Risk_Id", _data.Risk_Id);
                        p.Add("@Bowtie", "RISK");
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                        p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var risk = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        int status = p.Get<int>("@StatusChange");
                        if (status == 0)
                        {
                            MailMessage mail = new MailMessage();
                            //SmtpClient SmtpServer = new SmtpClient(servicesModel[0].Email_HostName);
                            mail.From = new MailAddress(servicesModel[0].Email_FromAddress);
                            #region get AssignTo
                            string allUser = _h.GetUserEmailFromId(_data.Risk_AssignTo);
                            string To = "";
                            string CC = "";
                            List<EmailModel> replaceEmail = ReplaceTemplateEmailTypeRejectOrganization(_result, risk, servicesModel, dataOrganization);
                            #endregion
                            #region get mail to and cc
                            #region not use
                            /*if (dataOrganization[0].Risk_Status_Workflow == "1" || dataOrganization[0].Risk_Status_Workflow == "4" || dataOrganization[0].Risk_Status_Workflow == "7" || dataOrganization[0].Risk_Status_Workflow == "10") // STAFF SUBMIT
                            {
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = allUser;
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }
                            else if (dataOrganization[0].Risk_Status_Workflow == "3" || dataOrganization[0].Risk_Status_Workflow == "6" || dataOrganization[0].Risk_Status_Workflow == "9" || dataOrganization[0].Risk_Status_Workflow == "12") // CO SUBMIT
                            {
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = allUser;
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }
                            else if (dataOrganization[0].Risk_Status_Workflow == "19" || dataOrganization[0].Risk_Status_Workflow == "20" || dataOrganization[0].Risk_Status_Workflow == "21" || dataOrganization[0].Risk_Status_Workflow == "22") // Owner Approve
                            {
                                //string To = "";
                                if (dataOrganization[0].Risk_Status_Workflow == "19")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "20")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "21")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "22")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                                if (allUser.Contains(';'))
                                {
                                    string[] userCC = allUser.Split(';');
                                    foreach (string MultiCC in userCC)
                                    {
                                        mail.CC.Add(new MailAddress(MultiCC));
                                    }
                                }
                                else
                                {
                                    mail.CC.Add(allUser);
                                }
                            }*/
                            #endregion
                            if (dataOrganization[0].Risk_Status_Workflow == "15" || dataOrganization[0].Risk_Status_Workflow == "16" || dataOrganization[0].Risk_Status_Workflow == "17" || dataOrganization[0].Risk_Status_Workflow == "18") //Owner Reject
                            {
                                //string To = "";
                                if (dataOrganization[0].Risk_Status_Workflow == "15")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "16")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "17")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Workflow == "18")//Owner FG Reject
                                {
                                    //var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                                    var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? _h.GetUserEmailFromId(risk[0].FG_Console) : "";
                                    var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? _h.GetUserEmailFromId(risk[0].DI_Approve) : "";
                                    var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? _h.GetUserEmailFromId(risk[0].DI_Console) : "";
                                    var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? _h.GetUserEmailFromId(risk[0].BU_Approve) : "";
                                    var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? _h.GetUserEmailFromId(risk[0].BU_Console) : "";
                                    var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? _h.GetUserEmailFromId(risk[0].Asset_Approve) : "";
                                    var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? _h.GetUserEmailFromId(risk[0].Asset_Console) : "";
                                    string[] strJoin = { FG_Console, DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                                    if (strJoin.Length == 0)
                                    {
                                        To = _h.GetUserEmailFromId(risk[0].Risk_Register_By);
                                    }
                                    else
                                    {
                                        To = string.Join(";", strJoin);
                                    }
                                }

                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    //To = allUser;
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }
                            }
                            #endregion
                            mail.Subject = replaceEmail[0].Subject;
                            if (servicesModel[0].SentToUsers)
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3);
                            else
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />TO : {3}<br />CC :{4}", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3, To, CC);
                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }
                    }
                }
            }
        }
        private void SentEmailTypeApproveOrganization(List<ApproveTypeOrganization> dataOrganization, List<EmailModel> emailTemplate, List<ServiceModel> servicesModel, SmtpClient smtp, string responeBy)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                foreach (var _data in dataOrganization)
                {
                    var _result = emailTemplate.Where(e => e.Status == _data.Risk_Status_Approve).ToList();
                    if (_result.Count > 0)
                    {

                        var p = new DynamicParameters();
                        p.Add("@Module", "GET");
                        p.Add("@Role", "TRANSECTION");
                        p.Add("@Risk_Id", _data.Risk_Id);
                        p.Add("@Bowtie", "RISK");
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                        p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var risk = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        int status = p.Get<int>("@StatusChange");
                        if (status == 0)
                        {
                            MailMessage mail = new MailMessage();
                            //SmtpClient SmtpServer = new SmtpClient(servicesModel[0].Email_HostName);
                            mail.From = new MailAddress(servicesModel[0].Email_FromAddress);
                            #region get AssignTo
                            string allUser = _h.GetUserEmailFromId(_data.Risk_AssignTo);
                            string To = "";
                            string CC = "";
                            List<EmailModel> replaceEmail = ReplaceTemplateEmailTypeApproveOrganization(_result, risk, servicesModel, dataOrganization);
                            #endregion
                            #region get mail to and cc
                            #region not use
                            /* if (dataOrganization[0].Risk_Status_Workflow == "1" || dataOrganization[0].Risk_Status_Workflow == "4" || dataOrganization[0].Risk_Status_Workflow == "7" || dataOrganization[0].Risk_Status_Workflow == "10") // STAFF SUBMIT
                             {
                                 *//*UAT*//*
                                 //mail.To.Add(servicesModel[0].Email_ToAddress);
                                 //mail.CC.Add(servicesModel[0].Email_CC);
                                 *//*PRD*//*
                                 To = allUser;
                                 if (To.Contains(';'))
                                 {
                                     string[] userTo = To.Split(';');
                                     foreach (string MultiTo in userTo)
                                     {
                                         mail.To.Add(new MailAddress(MultiTo));
                                     }
                                 }
                                 else
                                 {
                                     mail.To.Add(To);
                                 }
                             }*/
                            /*else if (dataOrganization[0].Risk_Status_Workflow == "3" || dataOrganization[0].Risk_Status_Workflow == "6" || dataOrganization[0].Risk_Status_Workflow == "9" || dataOrganization[0].Risk_Status_Workflow == "12") // CO SUBMIT
                            {
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = allUser;
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }*/
                            #endregion
                            if (dataOrganization[0].Risk_Status_Approve == "19" || dataOrganization[0].Risk_Status_Approve == "20" || dataOrganization[0].Risk_Status_Approve == "21" || dataOrganization[0].Risk_Status_Approve == "22") // Owner Approve
                            {
                                //string To = "";
                                if (dataOrganization[0].Risk_Status_Approve == "19")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Approve == "20")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Approve == "21")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataOrganization[0].Risk_Status_Approve == "22")//Owner FG Approve
                                {
                                    //var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                                    var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? _h.GetUserEmailFromId(risk[0].FG_Console) : "";
                                    var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? _h.GetUserEmailFromId(risk[0].DI_Approve) : "";
                                    var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? _h.GetUserEmailFromId(risk[0].DI_Console) : "";
                                    var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? _h.GetUserEmailFromId(risk[0].BU_Approve) : "";
                                    var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? _h.GetUserEmailFromId(risk[0].BU_Console) : "";
                                    var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? _h.GetUserEmailFromId(risk[0].Asset_Approve) : "";
                                    var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? _h.GetUserEmailFromId(risk[0].Asset_Console) : "";
                                    string[] strJoin = { FG_Console, DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                                    if (strJoin.Length == 0)
                                    {
                                        To = _h.GetUserEmailFromId(risk[0].Risk_Register_By);
                                    }
                                    else
                                    {
                                        To = string.Join(";", strJoin);
                                    }
                                }
                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                    if (allUser.Contains(';'))
                                    {
                                        string[] userCC = allUser.Split(';');
                                        foreach (string MultiCC in userCC)
                                        {
                                            if (!_h.CheckNull(MultiCC))
                                                mail.CC.Add(new MailAddress(MultiCC));
                                        }
                                    }
                                    else
                                    {
                                        mail.CC.Add(allUser);
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    CC = allUser;
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }

                            }
                            #region not use
                            /* else if (dataOrganization[0].Risk_Status_Workflow == "15" || dataOrganization[0].Risk_Status_Workflow == "16" || dataOrganization[0].Risk_Status_Workflow == "17" || dataOrganization[0].Risk_Status_Workflow == "18") //Owner Reject
                             {
                                 //string To = "";
                                 if (dataOrganization[0].Risk_Status_Workflow == "15")
                                     To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                 else if (dataOrganization[0].Risk_Status_Workflow == "16")
                                     To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                 else if (dataOrganization[0].Risk_Status_Workflow == "17")
                                     To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                 else if (dataOrganization[0].Risk_Status_Workflow == "18")
                                     To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);

                                 *//*UAT*//*
                                 //mail.To.Add(servicesModel[0].Email_ToAddress);
                                 //mail.CC.Add(servicesModel[0].Email_CC);
                                 *//*PRD*//*
                                 if (To.Contains(';'))
                                 {
                                     string[] userTo = To.Split(';');
                                     foreach (string MultiTo in userTo)
                                     {
                                         mail.To.Add(new MailAddress(MultiTo));
                                     }
                                 }
                                 else
                                 {
                                     mail.To.Add(To);
                                 }
                             }*/
                            #endregion
                            #endregion
                            mail.Subject = replaceEmail[0].Subject;
                            if (servicesModel[0].SentToUsers)
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3);
                            else
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />TO : {3}<br />CC :{4}", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3, To, CC);
                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }
                    }
                }
            }
        }
        private void SentEmailTypeApproveAsset(List<ApproveTypeAsset> dataAsset, List<EmailModel> emailTemplate, List<ServiceModel> servicesModel, SmtpClient smtp, string responeBy)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                foreach (var _data in dataAsset)
                {
                    var _result = emailTemplate.Where(e => e.Status == _data.Risk_Status_Approve).ToList();
                    if (_result.Count > 0)
                    {

                        var p = new DynamicParameters();
                        p.Add("@Module", "GET");
                        p.Add("@Role", "TRANSECTION");
                        p.Add("@Risk_Id", _data.Risk_Id);
                        p.Add("@Bowtie", "RISK");
                        p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                        p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        var risk = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                        int status = p.Get<int>("@StatusChange");
                        if (status == 0)
                        {
                            MailMessage mail = new MailMessage();
                            //SmtpClient SmtpServer = new SmtpClient(servicesModel[0].Email_HostName);
                            mail.From = new MailAddress(servicesModel[0].Email_FromAddress);
                            #region get AssignTo
                            string allUser = _h.GetUserEmailFromId(_data.Risk_AssignTo);
                            string To = "";
                            string CC = "";
                            List<EmailModel> replaceEmail = ReplaceTemplateEmailTypeApproveAsset(_result, risk, servicesModel, dataAsset);
                            #endregion
                            #region get mail to and cc
                            #region not use
                            /*if (dataAsset[0].Risk_Status_Workflow == "1" || dataAsset[0].Risk_Status_Workflow == "4" || dataAsset[0].Risk_Status_Workflow == "7" || dataAsset[0].Risk_Status_Workflow == "10") // STAFF SUBMIT
                            {
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = allUser;
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }*/
                            /*else if (dataAsset[0].Risk_Status_Workflow == "3" || dataAsset[0].Risk_Status_Workflow == "6" || dataAsset[0].Risk_Status_Workflow == "9" || dataAsset[0].Risk_Status_Workflow == "12") // CO SUBMIT
                            {
                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                To = allUser;
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }*/
                            #endregion
                            if (dataAsset[0].Risk_Status_Approve == "19" || dataAsset[0].Risk_Status_Approve == "20" || dataAsset[0].Risk_Status_Approve == "21" || dataAsset[0].Risk_Status_Approve == "22") // Owner Approve
                            {
                                //string To = "";
                                if (dataAsset[0].Risk_Status_Approve == "19")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Approve == "20")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Approve == "21")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Approve == "22")//Owner FG Approve
                                {
                                    //var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                                    var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? _h.GetUserEmailFromId(risk[0].FG_Console) : "";
                                    var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? _h.GetUserEmailFromId(risk[0].DI_Approve) : "";
                                    var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? _h.GetUserEmailFromId(risk[0].DI_Console) : "";
                                    var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? _h.GetUserEmailFromId(risk[0].BU_Approve) : "";
                                    var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? _h.GetUserEmailFromId(risk[0].BU_Console) : "";
                                    var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? _h.GetUserEmailFromId(risk[0].Asset_Approve) : "";
                                    var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? _h.GetUserEmailFromId(risk[0].Asset_Console) : "";
                                    string[] strJoin = { FG_Console, DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                                    if (strJoin.Length == 0)
                                    {
                                        To = _h.GetUserEmailFromId(risk[0].Risk_Register_By);
                                    }
                                    else
                                    {
                                        To = string.Join(";", strJoin);
                                    }
                                }

                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                    if (allUser.Contains(';'))
                                    {
                                        string[] userCC = allUser.Split(';');
                                        foreach (string MultiCC in userCC)
                                        {
                                            if (!_h.CheckNull(MultiCC))
                                                mail.CC.Add(new MailAddress(MultiCC));
                                        }
                                    }
                                    else
                                    {
                                        mail.CC.Add(allUser);
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    CC = allUser;
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }
                            }
                            #region not use
                            /*else if (dataAsset[0].Risk_Status_Workflow == "15" || dataAsset[0].Risk_Status_Workflow == "16" || dataAsset[0].Risk_Status_Workflow == "17" || dataAsset[0].Risk_Status_Workflow == "18") //Owner Reject
                            {
                                //string To = "";
                                if (dataAsset[0].Risk_Status_Workflow == "15")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "16")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "17")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By);
                                else if (dataAsset[0].Risk_Status_Workflow == "18")
                                    To = _h.GetUserEmailFromId(!_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By);

                                *//*UAT*//*
                                //mail.To.Add(servicesModel[0].Email_ToAddress);
                                //mail.CC.Add(servicesModel[0].Email_CC);
                                *//*PRD*//*
                                if (To.Contains(';'))
                                {
                                    string[] userTo = To.Split(';');
                                    foreach (string MultiTo in userTo)
                                    {
                                        mail.To.Add(new MailAddress(MultiTo));
                                    }
                                }
                                else
                                {
                                    mail.To.Add(To);
                                }
                            }*/
                            #endregion
                            #endregion
                            mail.Subject = replaceEmail[0].Subject;
                            if (servicesModel[0].SentToUsers)
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3);
                            else
                                mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />TO : {3}<br />CC :{4}", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3, To, CC);
                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }
                    }
                }
            }
        }
        private void SentEmailTypeConsoleOrganization(List<Consolidate_Transection> dataOrganization, List<EmailModel> emailTemplate, List<ServiceModel> servicesModel, SmtpClient smtp, string responeBy)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                foreach (var _data in dataOrganization)
                {
                    if (_data.Risk_Status_Workflow == "14")//ERM CONSOLE
                    {
                        var _result = emailTemplate.Where(e => e.Status == _data.Risk_Status_Workflow).ToList(); //ERM Console Only
                        if (_result.Count > 0)
                        {
                            MailMessage mail = new MailMessage();
                            //SmtpClient SmtpServer = new SmtpClient(servicesModel[0].Email_HostName);
                            mail.From = new MailAddress(servicesModel[0].Email_FromAddress);


                            var p = new DynamicParameters();
                            p.Add("@Module", "GET");
                            p.Add("@Role", "TRANSECTION");
                            p.Add("@Risk_Id", _data.Risk_Id);
                            p.Add("@Bowtie", "RISK");
                            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);
                            p.Add("@Return_Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                            var risk = conn.Query<RiskView>("sp_Risk_Insert_Update_Get", p, commandType: CommandType.StoredProcedure).ToList();
                            int status = p.Get<int>("@StatusChange");
                            if (status == 0)
                            {
                                #region get AssignTo
                                var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                                var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? _h.GetUserEmailFromId(risk[0].FG_Console) : "";
                                var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? _h.GetUserEmailFromId(risk[0].DI_Approve) : "";
                                var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? _h.GetUserEmailFromId(risk[0].DI_Console) : "";
                                var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? _h.GetUserEmailFromId(risk[0].BU_Approve) : "";
                                var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? _h.GetUserEmailFromId(risk[0].BU_Console) : "";
                                var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? _h.GetUserEmailFromId(risk[0].Asset_Approve) : "";
                                var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? _h.GetUserEmailFromId(risk[0].Asset_Console) : "";
                                List<EmailModel> replaceEmail = ReplaceTemplateEmail(_result, risk, servicesModel);
                                string To = FG_Approve + ";" + FG_Console;
                                #endregion
                                if (servicesModel[0].SentToUsers)
                                {
                                    /*PRD*/
                                    if (To.Contains(';'))
                                    {
                                        string[] userTo = To.Split(';');
                                        foreach (string MultiTo in userTo)
                                        {
                                            if (!_h.CheckNull(MultiTo))
                                                mail.To.Add(new MailAddress(MultiTo));
                                        }
                                    }
                                    else
                                    {
                                        mail.To.Add(To);
                                    }
                                }
                                else
                                {
                                    /*UAT*/
                                    mail.To.Add(servicesModel[0].Email_ToAddress);
                                    mail.CC.Add(servicesModel[0].Email_CC);
                                }
                                string CC = "";
                                mail.Subject = replaceEmail[0].Subject;
                                if (servicesModel[0].SentToUsers)
                                    mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3);
                                else
                                    mail.Body = String.Format("{0}<br /><br />{1}<br /><br />{2}<br />TO : {3}<br />CC :{4}", replaceEmail[0].Body1, replaceEmail[0].Body2, replaceEmail[0].Body3, To, CC);
                                mail.IsBodyHtml = true;
                                smtp.Send(mail);
                            }
                        }
                    }
                }
            }
        }

        private List<EmailModel> ReplaceTemplateEmail(List<EmailModel> EmailModel,List<RiskView> Risk,List<ServiceModel> serviceModels)
        {
            List<EmailModel> EmailTemplate = new List<EmailModel>();
            EmailModel e = new EmailModel();
            e.Status = EmailModel[0].Status;
            e.Subject = EmailModel[0].Subject;
            StringBuilder sb1 = new StringBuilder(EmailModel[0].Body1);
            sb1.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb1.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb1.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb1.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb1.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb1.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb1.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb1.Replace("[Link]", "<a href="+ (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body1 = sb1.ToString();

            StringBuilder sb2 = new StringBuilder(EmailModel[0].Body2);
            sb2.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb2.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb2.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb2.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb2.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb2.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb2.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb2.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body2 = sb2.ToString();

            StringBuilder sb3 = new StringBuilder(EmailModel[0].Body3);
            sb3.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb3.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb3.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb3.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb3.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb3.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb3.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb3.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body3 = sb3.ToString();
            EmailTemplate.Add(e);
            return EmailTemplate;
        }
        private List<EmailModel> ReplaceTemplateEmailTypeRejectAsset(List<EmailModel> EmailModel, List<RiskView> Risk, List<ServiceModel> serviceModels,List<RejectTypeAsset> rejectTypeAssets)
        {
            List<EmailModel> EmailTemplate = new List<EmailModel>();
            EmailModel e = new EmailModel();
            e.Status = EmailModel[0].Status;
            e.Subject = EmailModel[0].Subject;
            StringBuilder sb1 = new StringBuilder(EmailModel[0].Body1);
            sb1.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb1.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb1.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb1.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb1.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb1.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb1.Replace("[Comment]", !_h.CheckNull(rejectTypeAssets[0].Comment) ? rejectTypeAssets[0].Comment : "");
            sb1.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb1.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body1 = sb1.ToString();

            StringBuilder sb2 = new StringBuilder(EmailModel[0].Body2);
            sb2.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb2.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb2.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb2.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb2.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb2.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb2.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb2.Replace("[Comment]", !_h.CheckNull(rejectTypeAssets[0].Comment) ? rejectTypeAssets[0].Comment : "");
            sb2.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body2 = sb2.ToString();

            StringBuilder sb3 = new StringBuilder(EmailModel[0].Body3);
            sb3.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb3.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb3.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb3.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb3.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb3.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb3.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb3.Replace("[Comment]", !_h.CheckNull(rejectTypeAssets[0].Comment) ? rejectTypeAssets[0].Comment : "");
            sb3.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body3 = sb3.ToString();
            EmailTemplate.Add(e);
            return EmailTemplate;
        }
        private List<EmailModel> ReplaceTemplateEmailTypeApproveAsset(List<EmailModel> EmailModel, List<RiskView> Risk, List<ServiceModel> serviceModels, List<ApproveTypeAsset> approveTypeAsset)
        {
            List<EmailModel> EmailTemplate = new List<EmailModel>();
            EmailModel e = new EmailModel();
            e.Status = EmailModel[0].Status;
            e.Subject = EmailModel[0].Subject;
            StringBuilder sb1 = new StringBuilder(EmailModel[0].Body1);
            sb1.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb1.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb1.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb1.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb1.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb1.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb1.Replace("[Comment]", !_h.CheckNull(approveTypeAsset[0].Comment) ? approveTypeAsset[0].Comment : "");
            sb1.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb1.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body1 = sb1.ToString();

            StringBuilder sb2 = new StringBuilder(EmailModel[0].Body2);
            sb2.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb2.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb2.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb2.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb2.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb2.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb2.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb2.Replace("[Comment]", !_h.CheckNull(approveTypeAsset[0].Comment) ? approveTypeAsset[0].Comment : "");
            sb2.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body2 = sb2.ToString();

            StringBuilder sb3 = new StringBuilder(EmailModel[0].Body3);
            sb3.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb3.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb3.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb3.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb3.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb3.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb3.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb3.Replace("[Comment]", !_h.CheckNull(approveTypeAsset[0].Comment) ? approveTypeAsset[0].Comment : "");
            sb3.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body3 = sb3.ToString();
            EmailTemplate.Add(e);
            return EmailTemplate;
        }
        private List<EmailModel> ReplaceTemplateEmailTypeRejectOrganization(List<EmailModel> EmailModel, List<RiskView> Risk, List<ServiceModel> serviceModels, List<RejectTypeOrganization> rejectTypeOrganizations)
        {
            List<EmailModel> EmailTemplate = new List<EmailModel>();
            EmailModel e = new EmailModel();
            e.Status = EmailModel[0].Status;
            e.Subject = EmailModel[0].Subject;
            StringBuilder sb1 = new StringBuilder(EmailModel[0].Body1);
            sb1.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb1.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb1.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb1.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb1.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb1.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb1.Replace("[Comment]", !_h.CheckNull(rejectTypeOrganizations[0].Comment) ? rejectTypeOrganizations[0].Comment : "");
            sb1.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb1.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body1 = sb1.ToString();

            StringBuilder sb2 = new StringBuilder(EmailModel[0].Body2);
            sb2.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb2.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb2.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb2.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb2.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb2.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb2.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb2.Replace("[Comment]", !_h.CheckNull(rejectTypeOrganizations[0].Comment) ? rejectTypeOrganizations[0].Comment : "");
            sb2.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body2 = sb2.ToString();

            StringBuilder sb3 = new StringBuilder(EmailModel[0].Body3);
            sb3.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb3.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb3.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb3.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb3.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb3.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb3.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb3.Replace("[Comment]", !_h.CheckNull(rejectTypeOrganizations[0].Comment) ? rejectTypeOrganizations[0].Comment : "");
            sb3.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body3 = sb3.ToString();
            EmailTemplate.Add(e);
            return EmailTemplate;
        }
        private List<EmailModel> ReplaceTemplateEmailTypeApproveOrganization(List<EmailModel> EmailModel, List<RiskView> Risk, List<ServiceModel> serviceModels, List<ApproveTypeOrganization> approveTypeOrganizations)
        {
            List<EmailModel> EmailTemplate = new List<EmailModel>();
            EmailModel e = new EmailModel();
            e.Status = EmailModel[0].Status;
            e.Subject = EmailModel[0].Subject;
            StringBuilder sb1 = new StringBuilder(EmailModel[0].Body1);
            sb1.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb1.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb1.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb1.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb1.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb1.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb1.Replace("[Comment]", !_h.CheckNull(approveTypeOrganizations[0].Comment) ? approveTypeOrganizations[0].Comment : "");
            sb1.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb1.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body1 = sb1.ToString();

            StringBuilder sb2 = new StringBuilder(EmailModel[0].Body2);
            sb2.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb2.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb2.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb2.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb2.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb2.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb2.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb2.Replace("[Comment]", !_h.CheckNull(approveTypeOrganizations[0].Comment) ? approveTypeOrganizations[0].Comment : "");
            sb2.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body2 = sb2.ToString();

            StringBuilder sb3 = new StringBuilder(EmailModel[0].Body3);
            sb3.Replace("[Risk_Name]", !_h.CheckNull(Risk[0].Risk_Name) ? Risk[0].Risk_Name : "");
            sb3.Replace("[Risk_Co_Id]", !_h.CheckNull(Risk[0].Risk_Co_Id) ? Risk[0].Risk_Co_Id : "");
            sb3.Replace("[Risk_Impact]", !_h.CheckNull(Risk[0].Risk_Impact) ? Risk[0].Risk_Impact : "");
            sb3.Replace("[Risk_Likelihood]", !_h.CheckNull(Risk[0].Risk_Likelihood) ? Risk[0].Risk_Likelihood : "");
            sb3.Replace("[Risk_Rating]", !_h.CheckNull(Risk[0].Risk_Rating) ? Risk[0].Risk_Rating : "");
            sb3.Replace("[Risk_Register_Date]", !_h.CheckNull(Risk[0].Risk_Register_Date) ? Risk[0].Risk_Register_Date : "");
            sb3.Replace("[Risk_Business_Unit]", !_h.CheckNull(Risk[0].ABBREVIATION) ? Risk[0].ABBREVIATION : "");
            sb3.Replace("[Comment]", !_h.CheckNull(approveTypeOrganizations[0].Comment) ? approveTypeOrganizations[0].Comment : "");
            sb3.Replace("[Link]", "<a href=" + (!_h.CheckNull(serviceModels[0].Link) ? serviceModels[0].Link + "?Risk_Id=" + Risk[0].Risk_Id + "&Role=CO" : "") + ">Link</a>");
            e.Body3 = sb3.ToString();
            EmailTemplate.Add(e);
            return EmailTemplate;
        }
        
        public void SubmitProcessMail(List<TypeAssetModel> dataAsset, List<TypeOrganizationModel> dataOrganization,string Table)
        {
            List<EmailModel> _tempData = SetDataSubmit(dataAsset, dataOrganization, Table);
            List<ServiceModel> servicesModel = _c.ConnectionService();
            SmtpClient smtpEmail = GetSmtlEmail(servicesModel);
            if (_tempData.Count > 0)
            {
                foreach (var _data in _tempData)
                {
                    SendMail(_data, servicesModel, smtpEmail, Table);
                }
            }
        }
        public void ApproveProcessMail(List<ApproveTypeAsset> dataAsset,List<ApproveTypeOrganization> dataOrganization)
        {
            List<EmailModel> _tempData = SetDataApprove(dataAsset, dataOrganization);
            List<ServiceModel> servicesModel = _c.ConnectionService();
            SmtpClient smtpEmail = GetSmtlEmail(servicesModel);
            if (_tempData.Count > 0)
            {
                foreach (var _data in _tempData)
                {
                    SendMail(_data, servicesModel, smtpEmail,"Transection");
                }
            }
        }
        public void RejectProcessMail(List<RejectTypeAsset> dataAsset, List<RejectTypeOrganization> dataOrganization)
        {
            List<EmailModel> _tempData = SetDataReject(dataAsset, dataOrganization);
            List<ServiceModel> servicesModel = _c.ConnectionService();
            SmtpClient smtpEmail = GetSmtlEmail(servicesModel);
            if (_tempData.Count > 0)
            {
                foreach (var _data in _tempData)
                {
                    SendMail(_data, servicesModel, smtpEmail,"Transection");
                }
            }
        }
        public void ConsoleProcessMail(List<Consolidate_Transection> dataConsole)
        {
            List<EmailModel> _tempData = SetDataConsole(dataConsole);
            List<ServiceModel> servicesModel = _c.ConnectionService();
            SmtpClient smtpEmail = GetSmtlEmail(servicesModel);
            if (_tempData.Count > 0)
            {
                foreach (var _data in _tempData)
                {
                    SendMail(_data, servicesModel, smtpEmail,"Transection");
                }
            }
        }
        private List<EmailModel> SetDataSubmit(List<TypeAssetModel> dataAsset,List<TypeOrganizationModel> dataOrganization,string Role)
        {
            List<TempRawData> _temp = new List<TempRawData>();
            List<TempRawData> _mergeData = new List<TempRawData>();
            List<EmailModel> Template = new List<EmailModel>();
            string tempId = "";
            int countAssetId = 0;
            int countOrgId = 0;
            if (!_h.IsNullOrEmpty(dataAsset))
            {
                if (dataAsset.Count > 0)
                {
                    foreach (var _data in dataAsset)
                    {
                        TempRawData d = new TempRawData();
                        d.Status = _data.Risk_Status_Workflow;
                        d.To = _data.Risk_AssignTo;
                        d.Risk_Id = _data.Risk_Id;
                        tempId += _data.Risk_Id;
                        if (countAssetId != (dataAsset.Count - 1))//not last
                        {
                            tempId += ',';
                        }
                        countAssetId++;
                        //d.Level = _data.Risk_Business_Unit_WF_Level;
                        _temp.Add(d);
                    }
                }
            }
            if (!_h.IsNullOrEmpty(dataOrganization))
            {
                if (dataOrganization.Count > 0)
                {
                    foreach (var _data in dataOrganization)
                    {
                        TempRawData d = new TempRawData();
                        d.Status = _data.Risk_Status_Workflow;
                        d.To = _data.Risk_AssignTo;
                        d.Risk_Id = _data.Risk_Id;
                        tempId += _data.Risk_Id;
                        if (countOrgId != (dataOrganization.Count - 1))//not last
                        {
                            tempId += ',';
                        }
                        countOrgId++;
                        //d.Level = _data.Risk_Business_Unit_WF_Level;
                        _temp.Add(d);
                    }
                }
            }
            if (_temp.Count > 0)
            {
                var p = new DynamicParameters();
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@TempId", tempId);
                p.Add("@Role", Role);
                p.Add("@Bowtie", "RISK");
                CollectionRisk colRisk = _g.GetCollenctionRiskData(p, Role);
                if (colRisk.Risk.Count > 0)
                {
                    if (Role == "Transection") // is Role Co Submit
                    {
                        _mergeData = GetDataCcSubmit(colRisk, _temp);//get cc
                        _mergeData = MergeDataToSubmit(_mergeData);// merge data to status and to
                    }
                    else // is Staff Submit 
                    {
                        _mergeData = MergeDataToSubmit(_temp);// merge data to status and to
                    }
                    List<EmailModel> templateEmail = GetTemplateEmail();
                    List<Owner> OwnerModel = new List<Owner>();
                    Template = ReplaceTemplate(templateEmail, colRisk, _mergeData, OwnerModel, "Submit");
                    //render table
                }
            }
            return Template;
        }
        private List<EmailModel> SetDataApprove(List<ApproveTypeAsset> dataAsset, List<ApproveTypeOrganization> dataOrganization)
        {
            List<TempRawData> _temp = new List<TempRawData>();
            List<TempRawData> _mergeData = new List<TempRawData>();
            List<EmailModel> Template = new List<EmailModel>();
            string tempId = "";
            int countAssetId = 0;
            int countOrgId = 0;
            if (!_h.IsNullOrEmpty(dataAsset))
            {
                if (dataAsset.Count > 0)
                {
                    foreach (var _data in dataAsset)
                    {
                        TempRawData d = new TempRawData();
                        d.Status = _data.Risk_Status_Approve;
                        //d.To = _data.Risk_CC;
                        d.Risk_Id = _data.Risk_Id;
                        if (_data.Risk_Status_Workflow == "13")//Waiting ERM
                            _data.Risk_AssignTo = "";
                        if (!_h.CheckNull(_data.Risk_AssignTo))
                            d.Cc = _data.Risk_AssignTo + (!_h.CheckNull(_data.Risk_CC) ? "," + _data.Risk_CC : ""); // cc next to risk Co 
                        else
                            d.Cc = _data.Risk_CC; // cc next to risk Co 
                        d.Comment = _data.Comment;
                        tempId += _data.Risk_Id;
                        if (countAssetId != (dataAsset.Count - 1))//not last
                        {
                            tempId += ',';
                        }
                        countAssetId++;
                        //d.Level = _data.Risk_Business_Unit_WF_Level;
                        _temp.Add(d);
                    }
                }
            }
            if (!_h.IsNullOrEmpty(dataOrganization))
            {
                if (dataOrganization.Count > 0)
                {
                    foreach (var _data in dataOrganization)
                    {
                        TempRawData d = new TempRawData();
                        d.Status = _data.Risk_Status_Approve;
                        d.Risk_Id = _data.Risk_Id;
                        if (_data.Risk_Status_Workflow == "13")//Waiting ERM
                            _data.Risk_AssignTo = "";
                        if (!_h.CheckNull(_data.Risk_AssignTo))
                            d.Cc = _data.Risk_AssignTo + (!_h.CheckNull(_data.Risk_CC) ? "," + _data.Risk_CC : ""); // cc next to risk Co 
                        else
                            d.Cc = _data.Risk_CC; // cc next to risk Co 
                        d.Comment = _data.Comment;
                        tempId += _data.Risk_Id;
                        if (countOrgId != (dataOrganization.Count - 1))//not last
                        {
                            tempId += ',';
                        }
                        countOrgId++;
                        //.Level = _data.Risk_Business_Unit_WF_Level;
                        _temp.Add(d);
                    }
                }
            }
            if (_temp.Count > 0)
            {
                var p = new DynamicParameters();
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@TempId", tempId);
                p.Add("@Role", "Transection");
                p.Add("@Bowtie", "RISK");
                CollectionRisk colRisk  = _g.GetCollenctionRiskData(p, "Transection");
                if (colRisk.Risk.Count > 0)
                {
                    _mergeData = GetDataCcApproveRejecct(colRisk, _temp);//get cc
                    _mergeData = GetDataMitigation(colRisk, _mergeData);//get cc mitigation 
                    _mergeData = GetDataToApproveReject(colRisk, _mergeData);//get To
                    List<Owner> OwnerModel = new List<Owner>();
                    for(int i = 0; i < _mergeData.Count; i++)
                    { 
                        Owner o = new Owner();
                        o.ID = _mergeData[i].OwnerModel.ID;
                        o.Owner_Name = _mergeData[i].OwnerModel.Owner_Name;
                        OwnerModel.Add(o);
                    }
                    _mergeData = MergeDataToSubmit(_mergeData);//merge cc and to
                    List<EmailModel> templateEmail = GetTemplateEmail();//get template all
                    Template = ReplaceTemplate(templateEmail, colRisk, _mergeData, OwnerModel, "Approve");//replace template
                }
            }
            return Template;
        }
        private List<EmailModel> SetDataReject(List<RejectTypeAsset> dataAsset, List<RejectTypeOrganization> dataOrganization)
        {
            List<TempRawData> _temp = new List<TempRawData>();
            List<TempRawData> _mergeData = new List<TempRawData>();
            List<EmailModel> Template = new List<EmailModel>();
            string tempId = "";
            int countAssetId = 0;
            int countOrgId = 0;
            if (!_h.IsNullOrEmpty(dataAsset))
            {
                if (dataAsset.Count > 0)
                {
                    foreach (var _data in dataAsset)
                    {
                        TempRawData d = new TempRawData();
                        d.Status = _data.Risk_Status_Workflow;
                        d.To = _data.Risk_AssignTo;
                        d.Risk_Id = _data.Risk_Id;
                        d.Comment = _data.Comment;
                        //d.Cc = _data.Risk_AssignTo; // cc next to risk Co 
                        tempId += _data.Risk_Id;
                        if (countAssetId != (dataAsset.Count - 1))//not last
                        {
                            tempId += ',';
                        }
                        countAssetId++;
                        //d.Level = _data.Risk_Business_Unit_WF_Level;
                        _temp.Add(d);
                    }
                }
            }
            if (!_h.IsNullOrEmpty(dataOrganization))
            {
                if (dataOrganization.Count > 0)
                {
                    foreach (var _data in dataOrganization)
                    {
                        TempRawData d = new TempRawData();
                        d.Status = _data.Risk_Status_Workflow;
                        d.Risk_Id = _data.Risk_Id;
                        d.To = _data.Risk_AssignTo;
                        d.Comment = _data.Comment;
                        //d.Cc = _data.Risk_AssignTo; // cc next to risk Co 
                        tempId += _data.Risk_Id;
                        if (countOrgId != (dataOrganization.Count - 1))//not last
                        {
                            tempId += ',';
                        }
                        countOrgId++;
                        //.Level = _data.Risk_Business_Unit_WF_Level;
                        _temp.Add(d);
                    }
                }
            }
            if (_temp.Count > 0)
            {
                var p = new DynamicParameters();
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@TempId", tempId);
                p.Add("@Role", "Transection");
                p.Add("@Bowtie", "RISK");
                CollectionRisk colRisk = _g.GetCollenctionRiskData(p, "Transection");
                if (colRisk.Risk.Count > 0)
                {
                    _mergeData = GetDataCcApproveRejecct(colRisk, _temp);//get cc 
                    //_mergeData = GetDataMitigation(colRisk, _mergeData);//get mitigation 
                    _mergeData = GetDataToApproveReject(colRisk, _mergeData);//get To
                    _mergeData = MergeDataToSubmit(_mergeData);//merge cc and to
                    List<EmailModel> templateEmail = GetTemplateEmail();//get template all
                    List<Owner> OwnerModel = new List<Owner>();
                    Template = ReplaceTemplate(templateEmail, colRisk, _mergeData, OwnerModel, "Reject");//replace template
                }
            }
            return Template;
        }
        private List<EmailModel> SetDataConsole(List<Consolidate_Transection> dataConsole)
        {
            List<TempRawData> _temp = new List<TempRawData>();
            List<TempRawData> _mergeData = new List<TempRawData>();
            List<EmailModel> Template = new List<EmailModel>();
            string tempId = "";
            int countAssetId = 0;
            int countOrgId = 0;
            if (!_h.IsNullOrEmpty(dataConsole))
            {
                if (dataConsole.Count > 0)
                {
                    foreach (var _data in dataConsole)
                    {
                        if (_data.Risk_Status_Workflow == "14") // ERM Console
                        {
                            TempRawData d = new TempRawData();
                            d.Status = _data.Risk_Status_Workflow;
                            d.Risk_Id = _data.Risk_Id;
                            //d.To = _data.Risk_AssignTo;
                            //d.Cc = _data.Risk_AssignTo; // cc next to risk Co 
                            tempId += _data.Risk_Id;
                            if (countOrgId != (dataConsole.Count - 1))//not last
                            {
                                tempId += ',';
                            }
                            countOrgId++;
                            //.Level = _data.Risk_Business_Unit_WF_Level;
                            _temp.Add(d);
                        }
                    }
                }
            }
            if (_temp.Count > 0)
            {
                var p = new DynamicParameters();
                p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@TempId", tempId);
                p.Add("@Role", "Transection");
                p.Add("@Bowtie", "RISK");
                CollectionRisk colRisk = _g.GetCollenctionRiskData(p, "Transection");
                if (colRisk.Risk.Count > 0)
                {
                    _mergeData = GetDataCcApproveRejecct(colRisk, _temp);//get cc 
                    //_mergeData = GetDataMitigation(colRisk, _mergeData);//get mitigation 
                    _mergeData = GetDataToApproveReject(colRisk, _mergeData);//get To
                    _mergeData = MergeDataToSubmit(_mergeData);//merge cc and to
                    List<EmailModel> templateEmail = GetTemplateEmail();//get template all
                    //Template = ReplaceTemplate(templateEmail, colRisk, _mergeData,"Console");//replace template
                }
            }
            return Template;
        }
        private List<TempRawData> GetDataCcSubmit(CollectionRisk colRisk, List<TempRawData> _temp)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                var p = new DynamicParameters();
                p.Add("@Table_Name", "Master_ContactUs");
                var _tempContactUs = conn.Query<Contact>("sp_Get_Table", p, commandType: CommandType.StoredProcedure).ToList();
                
                //loop add data To in _temp
                foreach (var item in _temp)
                {
                    //check match Risk id
                    var risk = colRisk.Risk.Where(a => a.Risk_Id == item.Risk_Id).ToList();
                    if (risk.Count > 0)
                    {
                        if (risk[0].Risk_Type == "Organization") //Org
                        {
                            //get parent org
                            var paramsOrg = new List<KeyValuePair<string, string>>() {
                                new KeyValuePair<string, string>("OrganizetionID", risk[0].Risk_Business_Unit),
                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                            };
                            List<Organizations> org = new List<Organizations>();
                            List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                            if (orgparentInfo.Count > 0)
                            {

                                //find org unit in contract us
                                #region getOrgLevel for contract us
                                //sort Code
                                orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                var keyOrgLevel = "";
                                foreach (var orgParent in orgparentInfo)
                                {
                                    if (orgParent.ORGANIZATION_LEVEL == "Group")
                                    {
                                        keyOrgLevel = orgParent.ORGANIZATION_LEVEL;
                                        break;
                                    }
                                    else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                    {
                                        keyOrgLevel = orgParent.ORGANIZATION_LEVEL;
                                        break;
                                    }
                                    else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                    {
                                        keyOrgLevel = orgParent.ORGANIZATION_LEVEL;
                                        break;
                                    }
                                }
                                #endregion
                                if (_tempContactUs.Count > 0)
                                {
                                    var contract = _tempContactUs.Where(x => orgparentInfo.Any(r => r.ORGANIZATION_LEVEL == keyOrgLevel && x.GroupMap != null && x.GroupMap.Split(',').Contains(r.ORGANIZATION_ID))).ToList();
                                    if (contract.Count > 0)
                                    {
                                        //get Emial from contract us
                                        int index = 0;
                                        if (!_h.CheckNull(item.Cc))
                                            item.Cc += ',';
                                        foreach (var d in contract)
                                        {
                                            item.Cc += d.Emp_Id;
                                            if (index != (contract.Count - 1))//not last
                                            {
                                                item.Cc += ',';
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                        else // Asset
                        {
                            if (_tempContactUs.Count > 0)
                            {
                                var contract = _tempContactUs.Where(x => risk.Any(r => x.GroupMap != null && x.GroupMap.Split(',').Contains(r.Risk_Business_Unit))).ToList();
                                if (contract.Count > 0)
                                {
                                    //get Emial from contract us
                                    int index = 0;
                                    if (!_h.CheckNull(item.Cc))
                                        item.Cc += ',';
                                    foreach (var d in contract)
                                    {
                                        item.Cc += d.Emp_Id;
                                        if (index != (contract.Count - 1))//not last
                                        {
                                            item.Cc += ',';
                                        }
                                        index++;
                                    }
                                }
                            }
                        }
                        var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : "";
                        var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? risk[0].DI_Approve : "";
                        var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : "";
                        var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? risk[0].BU_Approve : "";
                        var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : "";
                        var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? risk[0].Asset_Approve : "";
                        var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : "";
                        var Risk_Register_By = !_h.CheckNull(risk[0].Risk_Register_By) ? risk[0].Risk_Register_By : "";
                        string[] strJoin = { FG_Console, DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console, Risk_Register_By };
                        if (_h.CheckNull(FG_Console)
                            && _h.CheckNull(DI_Approve)
                            && _h.CheckNull(DI_Console)
                            && _h.CheckNull(BU_Approve)
                            && _h.CheckNull(BU_Console)
                            && _h.CheckNull(Asset_Approve)
                            && _h.CheckNull(Asset_Console)
                            && _h.CheckNull(Risk_Register_By))
                        {
                            if (!_h.CheckNull(item.Cc))
                                item.Cc += ",";
                            item.Cc += risk[0].Risk_Register_By;
                        }
                        else
                        {
                            if (!_h.CheckNull(item.Cc))
                                item.Cc += ",";
                            item.Cc += string.Join(",", strJoin.Where(s => !string.IsNullOrEmpty(s)));
                        }
                    }
                }
            }
            return _temp;
        }
        private List<TempRawData> GetDataCcApproveRejecct(CollectionRisk colRisk, List<TempRawData> _temp)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                #region CC ERM Staff
                var p = new DynamicParameters();
                p.Add("@Table_Name", "Master_ContactUs");
                var _tempContactUs = conn.Query<Contact>("sp_Get_Table", p, commandType: CommandType.StoredProcedure).ToList();
                if (_tempContactUs.Count > 0)
                {
                    //loop add data To in _temp
                    foreach (var item in _temp)
                    {
                        //check match Risk id
                        var risk = colRisk.Risk.Where(a => a.Risk_Id == item.Risk_Id).ToList();
                        if (risk.Count > 0)
                        {
                            if (risk[0].Risk_Type == "Organization") // Org
                            {
                                //get parent org
                                var paramsOrg = new List<KeyValuePair<string, string>>() {
                                    new KeyValuePair<string, string>("OrganizetionID", risk[0].Risk_Business_Unit),
                                    new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                };
                                List<Organizations> org = new List<Organizations>();
                                List<Organizations> orgparentInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                                if (orgparentInfo.Count > 0)
                                {
                                    #region getOrgLevel for contract us
                                    //sort Code
                                    orgparentInfo = orgparentInfo.OrderBy(o => o.CODE).ToList();
                                    var keyOrgLevel = "";
                                    foreach (var orgParent in orgparentInfo)
                                    {
                                        if (orgParent.ORGANIZATION_LEVEL == "Group")
                                        {
                                            keyOrgLevel = orgParent.ORGANIZATION_LEVEL;
                                            break;
                                        }
                                        else if (orgParent.ORGANIZATION_LEVEL == "Division")
                                        {
                                            keyOrgLevel = orgParent.ORGANIZATION_LEVEL;
                                            break;
                                        }
                                        else if (orgParent.ORGANIZATION_LEVEL == "Department")
                                        {
                                            keyOrgLevel = orgParent.ORGANIZATION_LEVEL;
                                            break;
                                        }
                                    }
                                    #endregion
                                    //find org unit in contract us
                                    var contract = _tempContactUs.Where(x => orgparentInfo.Any(r => r.ORGANIZATION_LEVEL == keyOrgLevel && x.GroupMap != null && x.GroupMap.Split(',').Contains(r.ORGANIZATION_ID))).ToList();
                                    if (contract.Count > 0)
                                    {
                                        //get Emial from contract us
                                        int index = 0;
                                        if (!_h.CheckNull(item.Cc))
                                            item.Cc += ',';
                                        foreach (var d in contract)
                                        {
                                            item.Cc += d.Emp_Id;
                                            if (index != (contract.Count - 1))//not last
                                            {
                                                item.Cc += ',';
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }
                            else //Asset
                            {
                                if (_tempContactUs.Count > 0)
                                {
                                    var contract = _tempContactUs.Where(x => risk.Any(r => x.GroupMap != null && x.GroupMap.Split(',').Contains(r.Risk_Business_Unit))).ToList();
                                    if (contract.Count > 0)
                                    {
                                        //get Emial from contract us
                                        int index = 0;
                                        if (!_h.CheckNull(item.Cc))
                                            item.Cc += ',';
                                        foreach (var d in contract)
                                        {
                                            item.Cc += d.Emp_Id;
                                            if (index != (contract.Count - 1))//not last
                                            {
                                                item.Cc += ',';
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                #region CC Risk Co , Risk Owner is Console
                foreach (var item in _temp)
                {
                    //check match Risk id
                    var risk = colRisk.Risk.Where(a => a.Risk_Id == item.Risk_Id).ToList();
                    if (risk.Count > 0)
                    {
                        if (item.Status == "19" || item.Status == "15")
                        {
                            if (!_h.CheckNull(item.Cc))
                                item.Cc += ',';
                            item.Cc += !_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By;
                        }
                        else if (item.Status == "20" || item.Status == "16")
                        {
                            if (!_h.CheckNull(item.Cc))
                                item.Cc += ',';
                            item.Cc += !_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By;
                        }
                        else if (item.Status == "21" || item.Status == "17")
                        {
                            if (!_h.CheckNull(item.Cc))
                                item.Cc += ',';
                            item.Cc += !_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By;
                        }
                        else if (item.Status == "22" || item.Status == "18")//Owner FG Approve
                        {
                            if (!_h.CheckNull(item.Cc))
                                item.Cc += ',';
                            var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : "";
                            var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? risk[0].DI_Approve : "";
                            var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : "";
                            var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? risk[0].BU_Approve : "";
                            var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : "";
                            var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? risk[0].Asset_Approve : "";
                            var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : "";
                            string[] strJoin = { FG_Console, DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                            if (_h.CheckNull(FG_Console)
                                && _h.CheckNull(DI_Approve)
                                && _h.CheckNull(DI_Console)
                                && _h.CheckNull(BU_Approve)
                                && _h.CheckNull(BU_Console)
                                && _h.CheckNull(Asset_Approve)
                                && _h.CheckNull(Asset_Console))
                            {
                                item.Cc += risk[0].Risk_Register_By;
                            }
                            else
                            {
                                item.Cc += string.Join(",", strJoin.Where(s => !string.IsNullOrEmpty(s)));
                            }
                        }
                        else if (item.Status == "14")//ERM Console
                        {
                            var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? risk[0].FG_Approve : "";
                            var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : "";
                            string[] strJoin = { FG_Approve, FG_Console };
                            if (_h.CheckNull(FG_Approve)
                                && _h.CheckNull(FG_Console))
                            {

                                item.Cc += risk[0].Risk_Register_By;
                            }
                            else
                            {
                                item.Cc += string.Join(",", strJoin.Where(s => !string.IsNullOrEmpty(s)));
                            }
                        }
                    }
                }
                #endregion
            }
            return _temp;
        }
        private List<TempRawData> GetDataToApproveReject(CollectionRisk colRisk, List<TempRawData> _temp)
        {
            //List<TempRawData> _temp = new List<TempRawData>();
            //Get unique records if ListA has duplicate records
            //var listUnique = data.GroupBy(i => i.To).Select(g => g.First()).ToList();
            //loop add data To in _temp
            foreach (var item in _temp)
            {
                //check match Risk id
                var risk = colRisk.Risk.Where(a => a.Risk_Id == item.Risk_Id).ToList();
                if (risk.Count > 0)
                {
                    if (item.Status == "19" || item.Status == "15")// Asset
                        item.To = !_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : risk[0].Risk_Register_By;
                    else if (item.Status == "20" || item.Status == "16") // Department
                        item.To = !_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : risk[0].Risk_Register_By;
                    else if (item.Status == "21" || item.Status == "17") // Division
                        item.To = !_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : risk[0].Risk_Register_By;
                    else if (item.Status == "22" || item.Status == "18")//Owner FG Approve
                    {
                        item.To = !_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : risk[0].Risk_Register_By;
                        //var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? _h.GetUserEmailFromId(risk[0].FG_Approve) : "";
                        /*var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : "";
                        var DI_Approve = !_h.CheckNull(risk[0].DI_Approve) ? risk[0].DI_Approve : "";
                        var DI_Console = !_h.CheckNull(risk[0].DI_Console) ? risk[0].DI_Console : "";
                        var BU_Approve = !_h.CheckNull(risk[0].BU_Approve) ? risk[0].BU_Approve : "";
                        var BU_Console = !_h.CheckNull(risk[0].BU_Console) ? risk[0].BU_Console : "";
                        var Asset_Approve = !_h.CheckNull(risk[0].Asset_Approve) ? risk[0].Asset_Approve : "";
                        var Asset_Console = !_h.CheckNull(risk[0].Asset_Console) ? risk[0].Asset_Console : "";
                        string[] strJoin = { FG_Console, DI_Approve, DI_Console, BU_Approve, BU_Console, Asset_Approve, Asset_Console };
                        if (_h.CheckNull(FG_Console)
                            && _h.CheckNull(DI_Approve)
                            && _h.CheckNull(DI_Console)
                            && _h.CheckNull(BU_Approve)
                            && _h.CheckNull(BU_Console)
                            && _h.CheckNull(Asset_Approve)
                            && _h.CheckNull(Asset_Console))
                        {
                            item.To = risk[0].Risk_Register_By;
                        }
                        else
                        {
                            item.To = string.Join(",", strJoin.Where(s => !string.IsNullOrEmpty(s)));
                        }*/
                    }
                    else if (item.Status == "14")//ERM Console
                    {
                        var FG_Approve = !_h.CheckNull(risk[0].FG_Approve) ? risk[0].FG_Approve : "";
                        var FG_Console = !_h.CheckNull(risk[0].FG_Console) ? risk[0].FG_Console : "";
                        string[] strJoin = { FG_Approve, FG_Console};
                        if (_h.CheckNull(FG_Approve)
                            && _h.CheckNull(FG_Console))
                        {

                            item.To = risk[0].Risk_Register_By;
                        }
                        else
                        {
                            item.To = string.Join(",", strJoin.Where(s => !string.IsNullOrEmpty(s)));
                        }
                    }
                }
            }
            return _temp;
        }
        private List<TempRawData> GetDataMitigation(CollectionRisk colRisk, List<TempRawData> _temp)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                //get asset
                var p_masterasset = new DynamicParameters();
                p_masterasset.Add("@Table_Name", "Master_Asset");
                var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterasset, commandType: CommandType.StoredProcedure).ToList();

                var p_masterDepartment = new DynamicParameters();
                p_masterDepartment.Add("@Table_Name", "Master_Department");
                var mDepartment = conn.Query<Master_Department>("sp_Get_Table", p_masterDepartment, commandType: CommandType.StoredProcedure).ToList();
                //loop add data To in _temp
                foreach (var item in _temp)
                {
                    //check match Risk id
                    var risk = colRisk.Risk.Where(a => a.Risk_Id == item.Risk_Id).ToList();
                    if (risk.Count > 0)
                    {
                        #region get rootcause mitigation owner
                        if (!_h.IsNullOrEmpty(risk[0].RootCause))
                        {
                            //loop rootcause
                            for (int r = 0; r < risk[0].RootCause.Count; r++)
                            {
                                if (!_h.IsNullOrEmpty(risk[0].RootCause[r].RootCause_Mitigation))
                                {
                                    //loop mitigation
                                    int index = 0;
                                    if (!_h.CheckNull(item.Cc))
                                        item.Cc += ",";
                                    if (!_h.CheckNull(item.Owner))
                                        item.Owner += ",";
                                    for (int m = 0; m < risk[0].RootCause[r].RootCause_Mitigation.Count; m++)
                                    {
                                        if (!_h.CheckNull(risk[0].RootCause[r].RootCause_Mitigation[m].RootCause_Mitigation_Owner))
                                        {
                                            //get org owner
                                            /*var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", risk[0].RootCause[r].RootCause_Mitigation[m].RootCause_Mitigation_Owner),
                                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                            };
                                            var org = _o.GetOrganization(paramsOrg);*/
                                            var org = mDepartment.Where(d => d.DeptCode == risk[0].RootCause[r].RootCause_Mitigation[m].RootCause_Mitigation_Owner).ToList();
                                            if (org.Count > 0)
                                            {
                                                if (!_h.CheckNull(org[0].HeadID))
                                                {
                                                    item.Cc += org[0].HeadID;
                                                    item.Owner += org[0].Abbreviation;
                                                    if (index != (risk[0].RootCause[r].RootCause_Mitigation.Count - 1))//not last
                                                    {
                                                        item.Cc += ',';
                                                        item.Owner += ',';
                                                    }
                                                }

                                            }
                                            else // is asset or can 't find org
                                            {
                                                if (mAsset.Count > 0)
                                                {
                                                    //check match asset in config
                                                    var _result = mAsset.Where(o => o.Asset_Code == risk[0].RootCause[r].RootCause_Mitigation[m].RootCause_Mitigation_Owner).ToList();
                                                    if (_result.Count() > 0)
                                                    {
                                                        //loop get asset
                                                        int indexAsset = 0;
                                                        foreach (var asset in _result)
                                                        {
                                                            if (!_h.CheckNull(asset.Asset_Org))
                                                            {
                                                                //get org asset owner
                                                                /*var paramsOrgAsset = new List<KeyValuePair<string, string>>() {
                                                                    new KeyValuePair<string, string>("OrganizetionID", asset.Asset_Org),
                                                                    new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                                                };
                                                                var orgAsset = _o.GetOrganization(paramsOrgAsset);*/
                                                                var orgAsset = mDepartment.Where(d => d.DeptCode == asset.Asset_Org).ToList();
                                                                if (orgAsset.Count > 0)
                                                                {
                                                                    item.Cc += orgAsset[0].HeadID;
                                                                    item.Owner += orgAsset[0].Abbreviation;
                                                                    if (indexAsset != (_result.Count - 1))//not last
                                                                    {
                                                                        item.Cc += ',';
                                                                        item.Owner += ',';
                                                                    }
                                                                }
                                                            }
                                                            indexAsset++;
                                                        }
                                                    }
                                                }
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region get impact mitigation owner
                        if (!_h.IsNullOrEmpty(risk[0].Impact))
                        {
                            //loop impact
                            for (int r = 0; r < risk[0].Impact.Count; r++)
                            {
                                if (!_h.IsNullOrEmpty(risk[0].Impact[r].Impact_Mitigation))
                                {
                                    //loop mitigation
                                    var index = 0;
                                    if (!_h.CheckNull(item.Cc))
                                        item.Cc += ",";
                                    if (!_h.CheckNull(item.Owner))
                                        item.Owner += ",";
                                    for (int m = 0; m < risk[0].Impact[r].Impact_Mitigation.Count; m++)
                                    {
                                        if (!_h.CheckNull(risk[0].Impact[r].Impact_Mitigation[m].Impact_Mitigation_Owner))
                                        {
                                            //get org owner
                                            /*var paramsOrg = new List<KeyValuePair<string, string>>() {
                                                new KeyValuePair<string, string>("OrganizetionID", risk[0].Impact[r].Impact_Mitigation[m].Impact_Mitigation_Owner),
                                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                            };
                                            var org = _o.GetOrganization(paramsOrg);*/
                                            var org = mDepartment.Where(d => d.DeptCode == risk[0].Impact[r].Impact_Mitigation[m].Impact_Mitigation_Owner).ToList();
                                            if (org.Count > 0)
                                            {
                                                if (!_h.CheckNull(org[0].HeadID))
                                                {
                                                    item.Cc += org[0].HeadID;
                                                    item.Owner += org[0].Abbreviation;
                                                    if (index != (risk[0].Impact[r].Impact_Mitigation.Count - 1))//not last
                                                    {
                                                        item.Cc += ',';
                                                        item.Owner += ',';
                                                    }
                                                }
                                            }
                                            else // is asset or can 't find org
                                            {
                                                if (mAsset.Count > 0)
                                                {
                                                    //check match asset in config
                                                    var _result = mAsset.Where(o => o.Asset_Code == risk[0].Impact[r].Impact_Mitigation[m].Impact_Mitigation_Owner).ToList();
                                                    if (_result.Count() > 0)
                                                    {
                                                        //loop get asset
                                                        int indexAsset = 0;
                                                        foreach (var asset in _result)
                                                        {
                                                            if (!_h.CheckNull(asset.Asset_Org))
                                                            {
                                                                //get org asset owner
                                                                /*var paramsOrgAsset = new List<KeyValuePair<string, string>>() {
                                                                    new KeyValuePair<string, string>("OrganizetionID", asset.Asset_Org),
                                                                    new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                                                };
                                                                var orgAsset = _o.GetOrganization(paramsOrgAsset);*/
                                                                var orgAsset = mDepartment.Where(d => d.DeptCode == asset.Asset_Org).ToList();
                                                                if (orgAsset.Count > 0)
                                                                {
                                                                    item.Cc += orgAsset[0].HeadID;
                                                                    item.Owner += orgAsset[0].Abbreviation;
                                                                    if (indexAsset != (_result.Count - 1))//not last
                                                                    {
                                                                        item.Cc += ',';
                                                                        item.Owner += ',';
                                                                    }
                                                                }
                                                            }
                                                            indexAsset++;
                                                        }
                                                    }
                                                }
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        item.OwnerModel = new Owner { ID = item.Risk_Id, Owner_Name = item.Owner };
                    }
                    
                }

            }
            return _temp;
        }
        private List<TempRawData> MergeDataToSubmit(List<TempRawData> data)
        {
            List<TempRawData> _temp = new List<TempRawData>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            //Get unique records if ListA has duplicate records
            //var listUnique = data.GroupBy(i => i.To).Select(g => g.First()).ToList();
            for (int i = 0;i < data.Count;i++)
            {
                //Check if record already exists in ListC
                var chkDup = _temp.Where(t => data.Any(d => d.To == t.To) && t.Status == data[i].Status).ToList();
                if (chkDup.Count == 0)
                {
                    _temp.Add(data[i]);
                }
                else
                {
                    for (int j = 0; j < _temp.Count; j++)
                    {
                        //find index in _temp
                        if (chkDup.Where(t => data.Any(d => d.To == t.To) && t.Status == _temp[j].Status).Count() > 0)
                        {
                            _temp[j].Risk_Id = _temp[j].Risk_Id + "," + data[i].Risk_Id;
                            if (!_h.CheckNull(data[i].Cc))
                                if(!_h.CheckNull(_temp[j].Cc))
                                    _temp[j].Cc = _temp[j].Cc + "," + data[i].Cc;
                                else
                                    _temp[j].Cc =  data[i].Cc;
                            if (!_h.CheckNull(data[i].To))
                                if (!_h.CheckNull(_temp[j].To))
                                    _temp[j].To = _temp[j].To + "," + data[i].To;
                                else
                                    _temp[j].To = data[i].To;
                            
                        }
                    }
                }
            }
            if (_temp.Count > 0)
            {
                foreach (var d in _temp)
                {
                    if (!_h.CheckNull(d.Cc))
                    {
                        var listCc = new List<string>();
                        if (d.Cc.Contains(','))
                        {
                            var Cc = d.Cc.Split(',').Distinct().ToArray();
                            for (int i = 0; i < Cc.Length; i++)
                            {
                                if (!_h.CheckNull(Cc[i]))
                                {
                                    //Get User From Service
                                    var paramsEmp = new List<KeyValuePair<string, string>>() {
                                        new KeyValuePair<string, string>("EmployeeID", Cc[i]),
                                        new KeyValuePair<string, string>("FirstName", ""),
                                        new KeyValuePair<string, string>("LastName", ""),
                                        new KeyValuePair<string, string>("EmailAddress", ""),
                                        new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                                        new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                    };
                                    var user = _u.SearchEmployee(paramsEmp);
                                    if (user.Count > 0)
                                    {
                                        listCc.Add(user[0].EMAIL_ID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var paramsEmp = new List<KeyValuePair<string, string>>() {
                                new KeyValuePair<string, string>("EmployeeID", d.Cc),
                                new KeyValuePair<string, string>("FirstName", ""),
                                new KeyValuePair<string, string>("LastName", ""),
                                new KeyValuePair<string, string>("EmailAddress", ""),
                                new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                            };
                            var user = _u.SearchEmployee(paramsEmp);
                            if (user.Count > 0)
                            {
                                listCc.Add(user[0].EMAIL_ID);
                            }
                        }
                        d.Cc = string.Join(",", listCc.Where(s => !string.IsNullOrEmpty(s)));
                    }
                    if (!_h.CheckNull(d.To))
                    {
                        var listTo = new List<string>();
                        if (d.To.Contains(','))
                        {
                            var To = d.To.Split(',').Distinct().ToArray();
                            for (int i = 0; i < To.Length; i++)
                            {
                                if (!_h.CheckNull(To[i]))
                                {
                                    //Get User From Service
                                    var paramsEmp = new List<KeyValuePair<string, string>>() {
                                        new KeyValuePair<string, string>("EmployeeID", To[i]),
                                        new KeyValuePair<string, string>("FirstName", ""),
                                        new KeyValuePair<string, string>("LastName", ""),
                                        new KeyValuePair<string, string>("EmailAddress", ""),
                                        new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                                        new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                    };
                                    var user = _u.SearchEmployee(paramsEmp);
                                    if (user.Count > 0)
                                    {
                                        listTo.Add(user[0].EMAIL_ID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var paramsEmp = new List<KeyValuePair<string, string>>() {
                                new KeyValuePair<string, string>("EmployeeID", d.To),
                                new KeyValuePair<string, string>("FirstName", ""),
                                new KeyValuePair<string, string>("LastName", ""),
                                new KeyValuePair<string, string>("EmailAddress", ""),
                                new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                            };
                            var user = _u.SearchEmployee(paramsEmp);
                            if (user.Count > 0)
                            {
                                listTo.Add(user[0].EMAIL_ID);
                            }
                        }
                        d.To = string.Join(",", listTo.Where(s => !string.IsNullOrEmpty(s)));
                    }
                }
            }
            return _temp;
        }
        private List<EmailModel> ReplaceTemplate(List<EmailModel> templateEmail, CollectionRisk colRisk, List<TempRawData> _temp,List<Owner> owners,string Type)
        {
            List<EmailModel> EmailTemplate = new List<EmailModel>();
            
            foreach (var tdata in _temp)
            {
                var template = templateEmail.Where(t => t.Status == tdata.Status).ToList();
                if (template.Count > 0)
                {
                    EmailModel e = new EmailModel();
                    e.Status = template[0].Status;
                    e.Subject = template[0].Subject;
                    StringBuilder sb1 = new StringBuilder(template[0].Body1);
                    StringBuilder sb2 = new StringBuilder(template[0].Body2);
                    StringBuilder sb3 = new StringBuilder(template[0].Body3);
                    if (!_h.CheckNull(template[0].Body1))//check is config body 1
                    {
                        sb1 = ReplaceBody1(sb1, colRisk, tdata);
                    }
                    if (!_h.CheckNull(template[0].Body2))//check is config body 2
                    {
                        sb2 = ReplaceBody2(sb2, colRisk, tdata);
                    }
                    if (!_h.CheckNull(template[0].Body3))//check is config body 3 
                    {

                        sb3 = ReplaceRisk(sb3, colRisk, tdata, owners, Type);
                        //sb3 = ReplaceRootCause(sb3, colRisk, tdata);
                        //sb3 = ReplaceImpact(sb3, colRisk, tdata);

                    }
                    e.Body1 = sb1.ToString();
                    e.Body2 = sb2.ToString();
                    e.Body3 = sb3.ToString();
                    e.To = tdata.To;
                    e.Cc = tdata.Cc;
                    e.Risk_Id = tdata.Risk_Id;
                    EmailTemplate.Add(e);
                }
            }
            return EmailTemplate;
        }
        private StringBuilder ReplaceBody1(StringBuilder sbTable, CollectionRisk colRisk, TempRawData tdata)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            
            sbTable.Replace("[LinkConsolidate]", "<a href=" + (!_h.CheckNull(servicesModel[0].Link) ? servicesModel[0].Link + "/Consolidate" : "") + ">Link</a>");
            sbTable.Replace("[LinkRiskmap]", "<a href=" + (!_h.CheckNull(servicesModel[0].Link) ? servicesModel[0].Link + "/Riskmap" : "") + ">Link</a>");
            sbTable.Replace("[LinkTodo]", "<a href=" + (!_h.CheckNull(servicesModel[0].Link) ? servicesModel[0].Link + "/Todo" : "") + ">Link</a>");
            sbTable.Replace("[LinkMyrisk]", "<a href=" + (!_h.CheckNull(servicesModel[0].Link) ? servicesModel[0].Link + "/Myrisk" : "") + ">Link</a>");

            return sbTable;
        }
        private StringBuilder ReplaceBody2(StringBuilder sbTable, CollectionRisk colRisk, TempRawData tdata)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            sbTable.Replace("[Link]", "<a href=" + (!_h.CheckNull(servicesModel[0].Link) ? servicesModel[0].Link + "/Approval/1" : "") + ">Link</a>");
            return sbTable;
        }
        private StringBuilder ReplaceRisk(StringBuilder sbTable,CollectionRisk colRisk, TempRawData tdata ,List<Owner> owners,string Type)
        {
            var listRisk = new List<string>();
            bool chkRow = false;
            StringBuilder sbRisk = new StringBuilder();
            if (tdata.Risk_Id.Contains(','))//if multiple rsik
            {
                var risk = tdata.Risk_Id.Split(',').Distinct().ToArray();
                sbRisk.Append("<br/>");
                sbRisk.Append("<table style='border-collapse: collapse;'>");
                sbRisk.Append("<tr style='border:1px solid black'>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#bd9e5e;color:#FFFFFF;text-align: center;'>&nbsp; Risk ID &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#00aeef;color:#FFFFFF;text-align: center;'>&nbsp; Risk Name &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Business Unit &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Risk Level &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Risk Type &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Risk Status &nbsp;</th>");
                if (Type == "Reject")
                    sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Comments &nbsp;</th>");
                if (Type == "Approve")
                    sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Mitigation Owner &nbsp;</th>");
                sbRisk.Append("</tr>");
                for (int i = 0; i < risk.Length; i++)
                {
                    var dataRisk = colRisk.Risk.Where(r => r.Risk_Id == risk[i]).ToList();//get data risk from colRisk
                    if (dataRisk.Count > 0)
                    {
                        sbRisk.Append("<tr style='border:1px solid black;'>");
                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Co_Id) ? dataRisk[0].Risk_Co_Id : !_h.CheckNull(dataRisk[0].Risk_Staff_Id) ? dataRisk[0].Risk_Staff_Id : "");
                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Name) ? dataRisk[0].Risk_Name : "");
                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].ABBREVIATION) ? dataRisk[0].ABBREVIATION : "");
                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Rating) ? dataRisk[0].Risk_Rating : "");
                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].WPBID) ? "WPB" : "Normal");
                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Status) ? dataRisk[0].Risk_Status == "2" ? "Close (Invalid)" : dataRisk[0].Risk_Status == "3" ? "Close (Mitigation Completed)" : "Active" : "");
                        if (Type == "Reject")
                            sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(tdata.Comment) ? tdata.Comment : "");
                        if (Type == "Approve")
                        {
                            var Owner = owners.Where(a => a.ID == dataRisk[0].Risk_Id).ToList();
                            if (Owner.Count > 0)
                            {
                                if (!_h.CheckNull(Owner[0].Owner_Name))
                                {
                                    if (Owner[0].Owner_Name.Contains(','))
                                    {
                                        var dupOwner = Owner[0].Owner_Name.Split(',').Distinct().ToArray();
                                        if (dupOwner.Length > 0)
                                        {
                                            string strOwner = string.Join(",", dupOwner.Where(s => !string.IsNullOrEmpty(s)));
                                            /*int index = 0;
                                            string strOwner = "";
                                            foreach (var d in dupOwner)
                                            {
                                                strOwner += d[index];
                                                if (index != (dupOwner.Length - 1))//not last
                                                {
                                                    strOwner += ',';
                                                }
                                                index++;
                                            }*/
                                            sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", strOwner);
                                        }
                                        else
                                            sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", "");
                                    }
                                    else
                                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(Owner[0].Owner_Name) ? Owner[0].Owner_Name : "");
                                }
                                else
                                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(Owner[0].Owner_Name) ? Owner[0].Owner_Name : "");
                            }
                            else
                                sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", "");
                        }
                        sbRisk.Append("</tr>");
                        chkRow = true;
                    }
                }
                sbRisk.Append("</table>");
            }
            else // if one risk
            {
                sbRisk.Append("<br/>");
                sbRisk.Append("<table style='border-collapse: collapse;'>");
                sbRisk.Append("<tr style='border:1px solid black'>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#bd9e5e;color:#FFFFFF;text-align: center;'>&nbsp; Risk ID &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#00aeef;color:#FFFFFF;text-align: center;'>&nbsp; Risk Name &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Business Unit &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Risk Level &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Risk Type &nbsp;</th>");
                sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Risk Status &nbsp;</th>");
                if (Type == "Reject")
                    sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Comments &nbsp;</th>");
                if (Type == "Approve")
                    sbRisk.Append("<th style='border:1px solid black;background-color:#033153;color:#FFFFFF;text-align: center;'>&nbsp; Mitigation Owner &nbsp;</th>");
                sbRisk.Append("</tr>");
                var dataRisk = colRisk.Risk.Where(r => r.Risk_Id == tdata.Risk_Id).ToList();//get data risk from colRisk
                if (dataRisk.Count > 0)
                {
                    sbRisk.Append("<tr style='border:1px solid black;'>");
                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Co_Id) ? dataRisk[0].Risk_Co_Id : !_h.CheckNull(dataRisk[0].Risk_Staff_Id) ? dataRisk[0].Risk_Staff_Id : "");
                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Name) ? dataRisk[0].Risk_Name : "");
                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].ABBREVIATION) ? dataRisk[0].ABBREVIATION : "");
                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Rating) ? dataRisk[0].Risk_Rating : "");
                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].WPBID) ? "WPB" : "Normal");
                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(dataRisk[0].Risk_Status) ? dataRisk[0].Risk_Status == "2" ? "Close (Invalid)" : dataRisk[0].Risk_Status == "3" ? "Close (Mitigation Completed)" : "Active" : "");
                    if (Type == "Reject")
                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(tdata.Comment) ? tdata.Comment : "");
                    if (Type == "Approve")
                    {
                        var Owner = owners.Where(a => a.ID == dataRisk[0].Risk_Id).ToList();
                        if (Owner.Count > 0)
                        {
                            if (!_h.CheckNull(Owner[0].Owner_Name))
                            {
                                if (Owner[0].Owner_Name.Contains(','))
                                {
                                    var dupOwner = Owner[0].Owner_Name.Split(',').Distinct().ToArray();
                                    if (dupOwner.Length > 0)
                                    {
                                        string strOwner = string.Join(",", dupOwner.Where(s => !string.IsNullOrEmpty(s)));
                                        /*int index = 0;
                                        string strOwner = "";
                                        foreach (var d in dupOwner)
                                        {
                                            strOwner += d[index];
                                            if (index != (dupOwner.Length - 1))//not last
                                            {
                                                strOwner += ',';
                                            }
                                            index++;
                                        }*/
                                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", strOwner);
                                    }
                                    else
                                        sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", "");
                                }
                                else
                                    sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(Owner[0].Owner_Name) ? Owner[0].Owner_Name : "");
                            }
                            else
                                sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", !_h.CheckNull(Owner[0].Owner_Name) ? Owner[0].Owner_Name : "");
                        }
                        else
                            sbRisk.AppendFormat("<td style='border:1px solid black;text-align: left;vertical-align: text-top;'>&nbsp; {0} &nbsp;</td>", "");
                    }
                    sbRisk.Append("</tr>");
                    chkRow = true;
                }
                sbRisk.Append("</table>");
                
            }
            if (chkRow)
                sbTable.Replace("[TABLE_RISK]", sbRisk.ToString());
            else
                sbTable.Replace("[TABLE_RISK]", "");
            return sbTable;
        }
        private StringBuilder ReplaceRootCause(StringBuilder sbTable, CollectionRisk colRisk, TempRawData tdata)
        {
            var listRisk = new List<string>();
            bool chkRow = false;
            StringBuilder sbRisk = new StringBuilder();
            if (tdata.Risk_Id.Contains(','))//if multiple rsik
            {
                var risk = tdata.Risk_Id.Split(',').Distinct().ToArray();
                sbRisk.Append("<table style='border:1px solid black;'>");
                sbRisk.Append("<tr style='border:1px solid black;background-color:#338AFF'>");
                sbRisk.Append("<th style='border:1px solid black;'>Risk ID</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Root Cause Name</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Plan</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Owner</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Due Date</th>");
                sbRisk.Append("</tr>");
                for (int i = 0; i < risk.Length; i++)
                {
                    var dataRisk = colRisk.Risk.Where(r => r.Risk_Id == risk[i]).ToList();//get data risk from colRisk
                    if (dataRisk.Count > 0)
                    {
                        for (int root = 0; root < dataRisk[0].RootCause.Count; root++)//loop root cause
                        {
                            var rootName = dataRisk[0].RootCause[root].RootCause_Category;
                            if (!_h.IsNullOrEmpty(dataRisk[0].RootCause[root].RootCause_Mitigation))//check have mitigation
                            {
                                for (int mitigation = 0; mitigation < dataRisk[0].RootCause[root].RootCause_Mitigation.Count; mitigation++)//loop mitigation
                                {
                                    sbRisk.Append("<tr style='border:1px solid black;'>");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Risk_Co_Id) ? dataRisk[0].Risk_Co_Id : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(rootName) ? rootName : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Name) ? dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Name : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Owner_ABBREVIATION) ? dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Owner_ABBREVIATION : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_DueDate) ? dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_DueDate : "");
                                    sbRisk.Append("</tr>");
                                    chkRow = true;
                                }
                            }
                        }
                    }
                }
                sbRisk.Append("</table>");
            }
            else // if one risk
            {
                sbRisk.Append("<table style='border:1px solid black;'>");
                sbRisk.Append("<tr style='border:1px solid black;background-color:#338AFF'>");
                sbRisk.Append("<th style='border:1px solid black;'>Risk ID</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Root Cause Name</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Plan</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Owner</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Due Date</th>");
                sbRisk.Append("</tr>");
                var dataRisk = colRisk.Risk.Where(r => r.Risk_Id == tdata.Risk_Id).ToList();//get data risk from colRisk
                if (dataRisk.Count > 0)
                {
                    for (int root = 0; root < dataRisk[0].RootCause.Count; root++)//loop root cause
                    {
                        var rootName = dataRisk[0].RootCause[root].RootCause_Category;
                        if (!_h.IsNullOrEmpty(dataRisk[0].RootCause[root].RootCause_Mitigation))
                        {
                            for (int mitigation = 0; mitigation < dataRisk[0].RootCause[root].RootCause_Mitigation.Count; mitigation++)//loop mitigation
                            {
                                sbRisk.Append("<tr style='border:1px solid black;'>");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Risk_Co_Id) ? dataRisk[0].Risk_Co_Id : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(rootName) ? rootName : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Name) ? dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Name : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Owner_ABBREVIATION) ? dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_Owner_ABBREVIATION : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_DueDate) ? dataRisk[0].RootCause[root].RootCause_Mitigation[mitigation].RootCause_Mitigation_DueDate : "");
                                sbRisk.Append("</tr>");
                                chkRow = true;
                            }
                        }
                    }
                }
                sbRisk.Append("</table>");

            }
            if(chkRow)
                sbTable.Replace("[TABLE_ROOTCAUSE]", "<p>Root Cause Table</p>" + sbRisk.ToString());
            else
                sbTable.Replace("[TABLE_ROOTCAUSE]", "");
            return sbTable;
        }
        private StringBuilder ReplaceImpact(StringBuilder sbTable, CollectionRisk colRisk, TempRawData tdata)
        {
            var listRisk = new List<string>();
            bool chkRow = false;
            StringBuilder sbRisk = new StringBuilder();
            if (tdata.Risk_Id.Contains(','))//if multiple rsik
            {
                var risk = tdata.Risk_Id.Split(',').Distinct().ToArray();
                sbRisk.Append("<table style='border:1px solid black;'>");
                sbRisk.Append("<tr style='border:1px solid black;background-color:#338AFF'>");
                sbRisk.Append("<th style='border:1px solid black;'>Risk ID</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Impact Name</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Plan</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Owner</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Due Date</th>");
                sbRisk.Append("</tr>");
                for (int i = 0; i < risk.Length; i++)
                {
                    var dataRisk = colRisk.Risk.Where(r => r.Risk_Id == risk[i]).ToList();//get data risk from colRisk
                    if (dataRisk.Count > 0)
                    {
                        for (int impact = 0; impact < dataRisk[0].Impact.Count; impact++)//loop Impact
                        {
                            var impactName = dataRisk[0].Impact[impact].Impact_Description;
                            if (!_h.IsNullOrEmpty(dataRisk[0].Impact[impact].Impact_Mitigation))
                            {
                                for (int mitigation = 0; mitigation < dataRisk[0].Impact[impact].Impact_Mitigation.Count; mitigation++)//loop mitigation
                                {
                                    sbRisk.Append("<tr style='border:1px solid black;'>");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Risk_Co_Id) ? dataRisk[0].Risk_Co_Id : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(impactName) ? impactName : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Name) ? dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Name : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Owner_ABBREVIATION) ? dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Owner_ABBREVIATION : "");
                                    sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_DueDate) ? dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_DueDate : "");
                                    sbRisk.Append("</tr>");
                                    chkRow = true;
                                }
                            }
                        }
                    }
                }
                sbRisk.Append("</table>");
            }
            else // if one risk
            {
                sbRisk.Append("<table style='border:1px solid black;'>");
                sbRisk.Append("<tr style='border:1px solid black;background-color:#338AFF'>");
                sbRisk.Append("<th style='border:1px solid black;'>Risk ID</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Impact Name</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Plan</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Mitigation Owner</th>");
                sbRisk.Append("<th style='border:1px solid black;'>Due Date</th>");
                sbRisk.Append("</tr>");
                var dataRisk = colRisk.Risk.Where(r => r.Risk_Id == tdata.Risk_Id).ToList();//get data risk from colRisk
                if (dataRisk.Count > 0)
                {
                    for (int impact = 0; impact < dataRisk[0].Impact.Count; impact++)//loop Impact
                    {
                        var impactName = dataRisk[0].Impact[impact].Impact_Category;
                        if (!_h.IsNullOrEmpty(dataRisk[0].Impact[impact].Impact_Mitigation))
                        {
                            for (int mitigation = 0; mitigation < dataRisk[0].Impact[impact].Impact_Mitigation.Count; mitigation++)//loop mitigation
                            {
                                sbRisk.Append("<tr>");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Risk_Co_Id) ? dataRisk[0].Risk_Co_Id : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(impactName) ? impactName : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Name) ? dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Name : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Owner_ABBREVIATION) ? dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_Owner_ABBREVIATION : "");
                                sbRisk.AppendFormat("<td style='border:1px solid black;'>{0}</td>", !_h.CheckNull(dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_DueDate) ? dataRisk[0].Impact[impact].Impact_Mitigation[mitigation].Impact_Mitigation_DueDate : "");
                                sbRisk.Append("</tr>");
                                chkRow = true;
                            }
                        }
                    }
                }
                sbRisk.Append("</table>");

            }
            if (chkRow)
                sbTable.Replace("[TABLE_IMPACT]", "<p>Impact Table</p>" + sbRisk.ToString());
            else
                sbTable.Replace("[TABLE_IMPACT]","");
            return sbTable;
        }
        private SmtpClient GetSmtlEmail(List<ServiceModel> servicesModel)
        {
            //PTT
            if (servicesModel[0].SendFrom == "PTT")
            {
                SmtpClient SmtpServer = new SmtpClient();
                SmtpServer.EnableSsl = false;
                SmtpServer.Credentials = CredentialCache.DefaultNetworkCredentials;
                SmtpServer.Host = servicesModel[0].Email_HostName;
                SmtpServer.Port = !_h.CheckNull(servicesModel[0].Port) ? Convert.ToInt32(servicesModel[0].Port) : 0;
                SmtpServer.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                return SmtpServer;
            }
            else
            {
                //Fusion
                SmtpClient SmtpServer = new SmtpClient(servicesModel[0].Email_HostName);
                SmtpServer.Credentials = new System.Net.NetworkCredential(servicesModel[0].Email_FromUser, servicesModel[0].Email_FromPassword);
                SmtpServer.Host = servicesModel[0].Email_HostName;
                SmtpServer.EnableSsl = true;
                SmtpServer.TargetName = "STARTTLS/smtp.office365.com";
                SmtpServer.Port = !_h.CheckNull(servicesModel[0].Port) ? Convert.ToInt32(servicesModel[0].Port) : 0;
                return SmtpServer;
            }
        }
        private List<EmailModel> GetTemplateEmail()
        {
            List<EmailModel> EmailModel = new List<EmailModel>();
            try
            {

                var p = new DynamicParameters();
                p.Add("@Table_Name", "Master_Email");
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    conn.Open();
                    var emailTemplate = conn.Query<EmailModel>("sp_Get_Table", p, commandType: CommandType.StoredProcedure).ToList();
                    if (emailTemplate.Count > 0)
                    {
                        //var _result = emailTemplate.Where(e => e.Status == status).ToList();
                        //if (_result.Count > 0)
                        //{
                        foreach (var _result in emailTemplate)
                        {
                            EmailModel Email = new EmailModel();
                            Email.Status = _result.Status;
                            Email.Subject = _result.Subject;
                            Email.Body1 = _result.Body1;
                            Email.Body2 = _result.Body2;
                            Email.Body3 = _result.Body3;
                            EmailModel.Add(Email);
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return EmailModel;
        }
        private void SendMail(EmailModel _dataEmail, List<ServiceModel> servicesModel, SmtpClient smtpEmail,string Table)
        {
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                try
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(servicesModel[0].Email_FromAddress);
                    #region set to cc
                    if (servicesModel[0].SentToUsers)
                    {
                        /*PRD*/
                        if (!_h.CheckNull(_dataEmail.To))
                        {
                            if (_dataEmail.To.Contains(','))
                            {
                                string[] userTo = _dataEmail.To.Split(',');
                                foreach (string MultiTo in userTo)
                                {
                                    if (!_h.CheckNull(MultiTo))
                                        mail.To.Add(new MailAddress(MultiTo));
                                }
                            }
                            else
                            {
                                mail.To.Add(_dataEmail.To);
                            }
                        }
                        if (!_h.CheckNull(_dataEmail.Cc))
                        {
                            if (_dataEmail.Cc.Contains(','))
                            {
                                string[] userCC = _dataEmail.Cc.Split(',');
                                foreach (string MultiCC in userCC)
                                {
                                    if (!_h.CheckNull(MultiCC))
                                        mail.CC.Add(new MailAddress(MultiCC));
                                }
                            }
                            else
                            {
                                mail.CC.Add(_dataEmail.Cc);
                            }
                        }
                    }
                    else
                    {
                        /*UAT*/
                        //To
                        mail.To.Add(servicesModel[0].Email_ToAddress);
                        //CC
                        if (servicesModel[0].Email_CC.Contains(','))
                        {
                            string[] userCC = servicesModel[0].Email_CC.Split(',');
                            foreach (string MultiCC in userCC)
                            {
                                if (!_h.CheckNull(MultiCC))
                                    mail.CC.Add(new MailAddress(MultiCC));
                            }
                        }
                        else
                        {
                            mail.CC.Add(servicesModel[0].Email_CC);
                        }
                        //mail.CC.Add(servicesModel[0].Email_CC);
                    }
                    #endregion
                    mail.Subject = !_h.CheckNull(_dataEmail.Subject) ? _dataEmail.Subject : "";
                    if (servicesModel[0].SentToUsers)
                        mail.Body = String.Format("{0}{1}{2}", _dataEmail.Body1, _dataEmail.Body2, _dataEmail.Body3);
                    else
                        mail.Body = String.Format("{0}{1}{2}<br/>TO : {3}<br/>CC :{4}", _dataEmail.Body1, _dataEmail.Body2, _dataEmail.Body3, _dataEmail.To, _dataEmail.Cc);
                    mail.IsBodyHtml = true;
                    smtpEmail.Send(mail);

                    //insert Log Email
                    var p_Log = new DynamicParameters();
                    p_Log.Add("@Module", "Insert");
                    p_Log.Add("@Risk_Id", _dataEmail.Risk_Id);
                    p_Log.Add("@Status", _dataEmail.Status);
                    p_Log.Add("@Email_To", _dataEmail.To);
                    p_Log.Add("@Email_Cc", _dataEmail.Cc);
                    p_Log.Add("@Table_Type", Table);
                    p_Log.Add("@Description", "Success");
                    var result = conn.Query<object>("sp_Log_Email", p_Log, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex)
                {
                    //insert Log Email
                    var p_Log = new DynamicParameters();
                    p_Log.Add("@Module", "Insert");
                    p_Log.Add("@Risk_Id", _dataEmail.Risk_Id);
                    p_Log.Add("@Status", _dataEmail.Status);
                    p_Log.Add("@Email_To", _dataEmail.To);
                    p_Log.Add("@Email_Cc", _dataEmail.Cc);
                    p_Log.Add("@Table_Type", Table);
                    p_Log.Add("@Description", "Fail : " + ex.Message +"<br/>"+ ex.StackTrace );
                    var result = conn.Query<object>("sp_Log_Email", p_Log, commandType: CommandType.StoredProcedure).ToList();
                }
            }
        }
    }
}
