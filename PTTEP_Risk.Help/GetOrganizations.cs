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
    public class GetOrganizations
    {
        Helper _h = new Helper();
        ConfigurationService _c = new ConfigurationService();
        public List<Organizations> GetOrganization(List<KeyValuePair<string, string>> Params)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            List<Organizations> orgModel = new List<Organizations>();
            try
            {
                IRestResponse response = _h.CallService(servicesModel[0].GetOrganizations, Params);
                XDocument doc = XDocument.Parse(response.Content);
                XNamespace ns = "http://v1.workflow2009.pttep.com/";
                IEnumerable<XElement> responses = doc.Descendants(ns + "OrganizationsDATA");
                foreach (XElement itemH in responses)
                {
                    Organizations org = new Organizations();
                    org.ORGANIZATION_ID = (string)itemH.Element(ns + "ORGANIZATION_ID");
                    org.NAME = (string)itemH.Element(ns + "NAME");
                    org.ABBREVIATION = (string)itemH.Element(ns + "ABBREVIATION");
                    org.ORGANIZATION_LEVEL = (string)itemH.Element(ns + "ORGANIZATION_LEVEL");
                    org.CODE = (string)itemH.Element(ns + "CODE");
                    IEnumerable<XElement> itemHA = itemH.Descendants(ns + "HeadActing");
                    if (itemHA.Count() > 0)
                    {
                        List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                        foreach (XElement iitemHA in itemHA)
                        {
                            HeadActing_Organizations heads = new HeadActing_Organizations();
                            heads.ORGANIZATION_ID = (string)iitemHA.Element(ns + "ORGANIZATION_ID");
                            heads.HEAD_ID = (string)iitemHA.Element(ns + "HEAD_ID");
                            heads.HeadEmail = (string)iitemHA.Element(ns + "HeadEmail");
                            heads.ACTING_HEAD_ID = (string)iitemHA.Element(ns + "ACTING_HEAD_ID");
                            heads.ACTING_STATUS = (bool)iitemHA.Element(ns + "ACTING_STATUS");
                            heads.ActingEmail = (string)iitemHA.Element(ns + "ActingEmail");
                            HeadModel.Add(heads);
                            org.HeadActing = HeadModel;
                        }
                    }
                    orgModel.Add(org);
                }
            }
            catch (Exception ex)
            {
                
            }
            return orgModel;
        }

        public List<Organizations> GetActingOrganization(List<KeyValuePair<string, string>> Params)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            List<Organizations> orgModel = new List<Organizations>();
            try
            {
                IRestResponse response = _h.CallService(servicesModel[0].GetActingEmpId, Params);
                XDocument doc = XDocument.Parse(response.Content);
                XNamespace ns = "http://v1.workflow2009.pttep.com/";
                IEnumerable<XElement> responses = doc.Descendants(ns + "OrganizationsDATA");
                foreach (XElement itemH in responses)
                {
                    Organizations org = new Organizations();
                    org.ORGANIZATION_ID = (string)itemH.Element(ns + "ORGANIZATION_ID");
                    org.NAME = (string)itemH.Element(ns + "NAME");
                    org.ABBREVIATION = (string)itemH.Element(ns + "ABBREVIATION");
                    org.ORGANIZATION_LEVEL = (string)itemH.Element(ns + "ORGANIZATION_LEVEL");
                    org.CODE = (string)itemH.Element(ns + "CODE");
                    IEnumerable<XElement> itemHA = itemH.Descendants(ns + "HeadActing");
                    if (itemHA.Count() > 0)
                    {
                        List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                        foreach (XElement iitemHA in itemHA)
                        {
                            HeadActing_Organizations heads = new HeadActing_Organizations();
                            heads.ORGANIZATION_ID = (string)iitemHA.Element(ns + "ORGANIZATION_ID");
                            heads.HEAD_ID = (string)iitemHA.Element(ns + "HEAD_ID");
                            heads.HeadEmail = (string)iitemHA.Element(ns + "HeadEmail");
                            heads.ACTING_HEAD_ID = (string)iitemHA.Element(ns + "ACTING_HEAD_ID");
                            heads.ACTING_STATUS = (bool)iitemHA.Element(ns + "ACTING_STATUS");
                            heads.ActingEmail = (string)iitemHA.Element(ns + "ActingEmail");
                            HeadModel.Add(heads);
                            org.HeadActing = HeadModel;
                        }
                    }
                    orgModel.Add(org);
                }
            }
            catch (Exception ex)
            {

            }
            return orgModel;
        }

        public List<Organizations> GetParentOrganization(List<KeyValuePair<string, string>> Params)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            List<Organizations> orgModel = new List<Organizations>();
            try
            {
                IRestResponse response = _h.CallService(servicesModel[0].GetParentOrganizations, Params);
                XDocument doc = XDocument.Parse(response.Content);
                XNamespace ns = "http://v1.workflow2009.pttep.com/";
                IEnumerable<XElement> responses = doc.Descendants(ns + "OrganizationsDATA");
                foreach (XElement itemH in responses)
                {

                    Organizations org = new Organizations();
                    org.ORGANIZATION_ID = (string)itemH.Element(ns + "ORGANIZATION_ID");
                    org.NAME = (string)itemH.Element(ns + "NAME");
                    org.ABBREVIATION = (string)itemH.Element(ns + "ABBREVIATION");
                    org.ORGANIZATION_LEVEL = (string)itemH.Element(ns + "ORGANIZATION_LEVEL");
                    IEnumerable<XElement> itemHA = itemH.Descendants(ns + "HeadActing");
                    if (itemHA.Count() > 0)
                    {
                        List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                        foreach (XElement iitemHA in itemHA)
                        {
                            HeadActing_Organizations heads = new HeadActing_Organizations();
                            heads.ORGANIZATION_ID = (string)iitemHA.Element(ns + "ORGANIZATION_ID");
                            heads.HEAD_ID = (string)iitemHA.Element(ns + "HEAD_ID");
                            heads.HeadEmail = (string)iitemHA.Element(ns + "HeadEmail");
                            heads.ACTING_HEAD_ID = (string)iitemHA.Element(ns + "ACTING_HEAD_ID");
                            heads.ACTING_STATUS = (bool)iitemHA.Element(ns + "ACTING_STATUS");
                            heads.ActingEmail = (string)iitemHA.Element(ns + "ActingEmail");
                            HeadModel.Add(heads);
                            org.HeadActing = HeadModel;
                        }
                    }
                    orgModel.Add(org);
                }
            }
            catch (Exception ex)
            {

            }
            return orgModel;
        }

        public List<Organizations> GetOrgChildByOrgID(List<KeyValuePair<string, string>> Params)
        {
            List<ServiceModel> servicesModel = _c.ConnectionService();
            List<Organizations> orgModel = new List<Organizations>();
            try
            {
                IRestResponse response = _h.CallService(servicesModel[0].GetChildOrganizations, Params);
                XDocument doc = XDocument.Parse(response.Content);
                XNamespace ns = "http://v1.workflow2009.pttep.com/";
                IEnumerable<XElement> responses = doc.Descendants(ns + "OrganizationsDATA");
                foreach (XElement itemH in responses)
                {

                    Organizations org = new Organizations();
                    org.ORGANIZATION_ID = (string)itemH.Element(ns + "ORGANIZATION_ID");
                    org.NAME = (string)itemH.Element(ns + "NAME");
                    org.CODE = (string)itemH.Element(ns + "CODE");
                    org.ABBREVIATION = (string)itemH.Element(ns + "ABBREVIATION");
                    org.ORGANIZATION_LEVEL = (string)itemH.Element(ns + "ORGANIZATION_LEVEL");
                    IEnumerable<XElement> itemHA = itemH.Descendants(ns + "HeadActing");
                    if (itemHA.Count() > 0)
                    {
                        List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                        foreach (XElement iitemHA in itemHA)
                        {
                            HeadActing_Organizations heads = new HeadActing_Organizations();
                            heads.ORGANIZATION_ID = (string)iitemHA.Element(ns + "ORGANIZATION_ID");
                            heads.HEAD_ID = (string)iitemHA.Element(ns + "HEAD_ID");
                            heads.HeadEmail = (string)iitemHA.Element(ns + "HeadEmail");
                            heads.ACTING_HEAD_ID = (string)iitemHA.Element(ns + "ACTING_HEAD_ID");
                            heads.ACTING_STATUS = (bool)iitemHA.Element(ns + "ACTING_STATUS");
                            heads.ActingEmail = (string)iitemHA.Element(ns + "ActingEmail");
                            HeadModel.Add(heads);
                            org.HeadActing = HeadModel;
                        }
                    }
                    orgModel.Add(org);
                }
            }
            catch (Exception ex)
            {

            }
            return orgModel;
        }

        public List<Organizations> GetOrgChildByOrgID_DB(Organizations orgInfo,string QuarterID)
        {
            //List<ServiceModel> servicesModel = _c.ConnectionService();
            List<Organizations> orgModel = new List<Organizations>();
            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    var p = new DynamicParameters();
                    p.Add("@Risk_Business_Unit", orgInfo.ORGANIZATION_ID);
                    p.Add("@QuarterID", QuarterID);
                    var orgChilde = conn.Query<OrganizationsChildeDB>("sp_Get_DeptChildByParent", p, commandType: CommandType.StoredProcedure).ToList();
                    //find org childe in result childe sql
                    //var result = orgChilde.Where(x => orgInfo.ORGANIZATION_LEVEL == "Group" && x.ORGANIZATION_LEVEL == "Division" || orgInfo.ORGANIZATION_LEVEL == "Division" && x.ORGANIZATION_LEVEL == "Department").ToList();
                    var result = orgChilde.Where(x => x.Parent_ORGANIZATION_ID == orgInfo.ORGANIZATION_ID).ToList();
                    foreach (var _d in result)
                    {
                        Organizations org = new Organizations();
                        List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                        HeadActing_Organizations heads = new HeadActing_Organizations();
                        //org
                        org.ORGANIZATION_ID = !_h.CheckNull(_d.ORGANIZATION_ID) ? _d.ORGANIZATION_ID : "";
                        org.NAME = !_h.CheckNull(_d.Name) ? _d.Name : "";
                        org.CODE = !_h.CheckNull(_d.CODE) ? _d.CODE : "";
                        org.ABBREVIATION = !_h.CheckNull(_d.ABBREVIATION) ? _d.ABBREVIATION : "";
                        org.ORGANIZATION_LEVEL = !_h.CheckNull(_d.ORGANIZATION_LEVEL) ? _d.ORGANIZATION_LEVEL : "";
                        //head acting
                        heads.ORGANIZATION_ID = !_h.CheckNull(_d.HEAD_ORGANIZATION_ID) ? _d.HEAD_ORGANIZATION_ID : "";
                        heads.HEAD_ID = !_h.CheckNull(_d.HEAD_ID) ? _d.HEAD_ID : "";
                        heads.HeadEmail = !_h.CheckNull(_d.HeadEmail) ? _d.HeadEmail : "";
                        heads.ACTING_HEAD_ID = !_h.CheckNull(_d.ACTING_HEAD_ID) ? _d.ACTING_HEAD_ID : "";
                        heads.ACTING_STATUS = !_h.CheckNull(_d.ACTING_STATUS) ? _d.ACTING_STATUS.Equals("1") ? true : false : false;
                        heads.ActingEmail = !_h.CheckNull(_d.ActingEmail) ? _d.ActingEmail : "";
                        heads.HEAD_ID = !_h.CheckNull(_d.HEAD_ID) ? _d.HEAD_ID : "";
                        HeadModel.Add(heads);
                        org.HeadActing = HeadModel;
                        orgModel.Add(org);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return orgModel;
        }

        public List<Organizations> GetOrgALLChildByOrgID_DB(Organizations orgInfo, string QuarterID)
        {
            //List<ServiceModel> servicesModel = _c.ConnectionService();
            List<Organizations> orgModel = new List<Organizations>();
            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    var p = new DynamicParameters();
                    p.Add("@Risk_Business_Unit", orgInfo.ORGANIZATION_ID);
                    p.Add("@QuarterID", QuarterID);
                    var orgChilde = conn.Query<OrganizationsChildeDB>("sp_Get_DeptChildByParent", p, commandType: CommandType.StoredProcedure).ToList();
                    //find org childe in result childe sql
                    //var result = orgChilde.Where(x => orgInfo.ORGANIZATION_LEVEL == "Group" && x.ORGANIZATION_LEVEL == "Division" || orgInfo.ORGANIZATION_LEVEL == "Division" && x.ORGANIZATION_LEVEL == "Department").ToList();
                    //var result = orgChilde.Where(x => x.Parent_ORGANIZATION_ID == orgInfo.ORGANIZATION_ID).ToList();
                    foreach (var _d in orgChilde)
                    {
                        Organizations org = new Organizations();
                        List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                        HeadActing_Organizations heads = new HeadActing_Organizations();
                        //org
                        org.ORGANIZATION_ID = !_h.CheckNull(_d.ORGANIZATION_ID) ? _d.ORGANIZATION_ID : "";
                        org.NAME = !_h.CheckNull(_d.Name) ? _d.Name : "";
                        org.CODE = !_h.CheckNull(_d.CODE) ? _d.CODE : "";
                        org.ABBREVIATION = !_h.CheckNull(_d.ABBREVIATION) ? _d.ABBREVIATION : "";
                        org.ORGANIZATION_LEVEL = !_h.CheckNull(_d.ORGANIZATION_LEVEL) ? _d.ORGANIZATION_LEVEL : "";
                        //head acting
                        heads.ORGANIZATION_ID = !_h.CheckNull(_d.HEAD_ORGANIZATION_ID) ? _d.HEAD_ORGANIZATION_ID : "";
                        heads.HEAD_ID = !_h.CheckNull(_d.HEAD_ID) ? _d.HEAD_ID : "";
                        heads.HeadEmail = !_h.CheckNull(_d.HeadEmail) ? _d.HeadEmail : "";
                        heads.ACTING_HEAD_ID = !_h.CheckNull(_d.ACTING_HEAD_ID) ? _d.ACTING_HEAD_ID : "";
                        heads.ACTING_STATUS = !_h.CheckNull(_d.ACTING_STATUS) ? _d.ACTING_STATUS.Equals("1") ? true : false : false;
                        heads.ActingEmail = !_h.CheckNull(_d.ActingEmail) ? _d.ActingEmail : "";
                        heads.HEAD_ID = !_h.CheckNull(_d.HEAD_ID) ? _d.HEAD_ID : "";
                        HeadModel.Add(heads);
                        org.HeadActing = HeadModel;
                        orgModel.Add(org);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return orgModel;
        }

        public List<Organizations> GetRecursiveOrganization(List<KeyValuePair<string, string>> Params ,List<Organizations> Listorg)
        {
            try
            {
                if (Listorg.Count == 0)//get org Current
                {
                    List<Organizations> orgCurrent = GetOrganization(Params);
                    Listorg.Add(orgCurrent[0]);
                }
                List<ServiceModel> servicesModel = _c.ConnectionService();
                List<Organizations> orgModel = new List<Organizations>();
                IRestResponse response = _h.CallService(servicesModel[0].GetParentOrganizations, Params);
                XDocument doc = XDocument.Parse(response.Content);
                XNamespace ns = "http://v1.workflow2009.pttep.com/";
                IEnumerable<XElement> responses = doc.Descendants(ns + "OrganizationsDATA");
                
                foreach (XElement itemH in responses)
                {
                    if (itemH.FirstNode != null)//if have parent
                    {
                        Organizations o = new Organizations();
                        o.ORGANIZATION_ID = (string)itemH.Element(ns + "ORGANIZATION_ID");
                        o.NAME = (string)itemH.Element(ns + "NAME");
                        o.ABBREVIATION = (string)itemH.Element(ns + "ABBREVIATION");
                        o.ORGANIZATION_LEVEL = (string)itemH.Element(ns + "ORGANIZATION_LEVEL");
                        o.CODE = (string)itemH.Element(ns + "CODE");
                        IEnumerable<XElement> itemHA = itemH.Descendants(ns + "HeadActing");
                        if (itemHA.Count() > 0)
                        {
                            List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                            foreach (XElement iitemHA in itemHA)
                            {
                                HeadActing_Organizations heads = new HeadActing_Organizations();
                                heads.ORGANIZATION_ID = (string)iitemHA.Element(ns + "ORGANIZATION_ID");
                                heads.HEAD_ID = (string)iitemHA.Element(ns + "HEAD_ID");
                                heads.HeadEmail = (string)iitemHA.Element(ns + "HeadEmail");
                                heads.ACTING_HEAD_ID = (string)iitemHA.Element(ns + "ACTING_HEAD_ID");
                                heads.ACTING_STATUS = (bool)iitemHA.Element(ns + "ACTING_STATUS");
                                heads.ActingEmail = (string)iitemHA.Element(ns + "ActingEmail");
                                HeadModel.Add(heads);
                                o.HeadActing = HeadModel;
                            }
                        }
                        orgModel.Add(o);
                    }
                    else // not parent
                    {
                        break;
                    }
                }
                if (orgModel.Count > 0)
                {
                    //List<Organizations> parentOrganizations = new List<Organizations>();
                    foreach (Organizations orgparent in orgModel)
                    {
                        Listorg.Add(orgparent);
                        // this will internally add to result
                        //get parent org
                        var paramsOrg = new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string, string>("OrganizetionID", orgparent.ORGANIZATION_ID),
                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                        };
                        GetRecursiveOrganization(paramsOrg, Listorg);//recursive
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Listorg;
        }

        public List<Organizations> GetRecursiveOrganization_DB(Organizations orgInfo, string QuarterID)
        {
            List<Organizations> orgModel = new List<Organizations>();
            try
            {
                using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
                {
                    var p = new DynamicParameters();
                    p.Add("@Risk_Business_Unit", orgInfo.ORGANIZATION_ID);
                    p.Add("@QuarterID", QuarterID);
                    var orgChilde = conn.Query<OrganizationsChildeDB>("sp_Get_DeptParenByChild", p, commandType: CommandType.StoredProcedure).OrderBy(x => x.CODE).ToList();
                    foreach (var _d in orgChilde)
                    {
                        Organizations org = new Organizations();
                        List<HeadActing_Organizations> HeadModel = new List<HeadActing_Organizations>();
                        HeadActing_Organizations heads = new HeadActing_Organizations();
                        //org
                        org.ORGANIZATION_ID = !_h.CheckNull(_d.ORGANIZATION_ID) ? _d.ORGANIZATION_ID : "";
                        org.NAME = !_h.CheckNull(_d.Name) ? _d.Name : "";
                        org.CODE = !_h.CheckNull(_d.CODE) ? _d.CODE : "";
                        org.ABBREVIATION = !_h.CheckNull(_d.ABBREVIATION) ? _d.ABBREVIATION : "";
                        org.ORGANIZATION_LEVEL = !_h.CheckNull(_d.ORGANIZATION_LEVEL) ? _d.ORGANIZATION_LEVEL : "";
                        //head acting
                        heads.ORGANIZATION_ID = !_h.CheckNull(_d.HEAD_ORGANIZATION_ID) ? _d.HEAD_ORGANIZATION_ID : "";
                        heads.HEAD_ID = !_h.CheckNull(_d.HEAD_ID) ? _d.HEAD_ID : "";
                        heads.HeadEmail = !_h.CheckNull(_d.HeadEmail) ? _d.HeadEmail : "";
                        heads.ACTING_HEAD_ID = !_h.CheckNull(_d.ACTING_HEAD_ID) ? _d.ACTING_HEAD_ID : "";
                        heads.ACTING_STATUS = !_h.CheckNull(_d.ACTING_STATUS) ? _d.ACTING_STATUS.Equals("1") ? true : false : false;
                        heads.ActingEmail = !_h.CheckNull(_d.ActingEmail) ? _d.ActingEmail : "";
                        heads.HEAD_ID = !_h.CheckNull(_d.HEAD_ID) ? _d.HEAD_ID : "";
                        HeadModel.Add(heads);
                        org.HeadActing = HeadModel;
                        orgModel.Add(org);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return orgModel;
        }



        public string GetRecursiveOrganizationFromRiskMap(RiskMap_Menu org)
        {
            List<string> listDepartment = new List<string>();
            List<string> listDivision = new List<string>();
            List<string> listFG = new List<string>();
            if (!_h.IsNullOrEmpty(org.Department_Level))
            {
                listDepartment = RecursiveRiskMapMenuDepartment(org.Department_Level);
            }
            if (!_h.IsNullOrEmpty(org.Division_Level))
            {
                listDivision = RecursiveRiskMapMenuDivision(org.Division_Level);
            }
            if (!_h.IsNullOrEmpty(org.GroupDivision_Level))
            {
                listFG = RecursiveRiskMapMenuGroupDivision(org.GroupDivision_Level);
            }

            IEnumerable<string> union = listFG.Union(listDivision).Union(listDepartment);
            string mergeBUStr = "";
            if (union.Count() > 0)
                mergeBUStr = string.Join(",", union.Where(s => !string.IsNullOrEmpty(s)));
            return mergeBUStr;
        }
        private List<string> RecursiveRiskMapMenuDepartment(List<RiskMap_MenuDepartment> riskmapOrg)
        {
            List<string> list = new List<string>();
            if (riskmapOrg.Count > 0)
            {
                foreach (var _org in riskmapOrg)
                {
                    if (!_h.IsNullOrEmpty(_org.Org_Menu))
                    {
                        list.AddRange(getDepartment(_org.Org_Menu));
                    }
                }
                //if(riskmapOrg[0].Org_Menu[0].)
            }
            return list;
        }
        private List<string> RecursiveRiskMapMenuDivision(List<RiskMap_MenuDivision> riskmapOrg)
        {
            List<string> list = new List<string>();
            if (riskmapOrg.Count > 0)
            {
                foreach (var _org in riskmapOrg)
                {
                    if (!_h.IsNullOrEmpty(_org.Org_Menu))
                    {
                        list.AddRange(getDivision(_org.Org_Menu));
                    }
                }
                //if(riskmapOrg[0].Org_Menu[0].)
            }
            return list;
        }
        private List<string> RecursiveRiskMapMenuGroupDivision(List<RiskMap_MenuGroup> riskmapOrg)
        {
            List<string> list = new List<string>();
            if (riskmapOrg.Count > 0)
            {
                foreach (var _org in riskmapOrg)
                {
                    if (!_h.IsNullOrEmpty(_org.Org_Menu))
                    {
                        list.AddRange(getGroupDivision(_org.Org_Menu));
                    }
                }
                //if(riskmapOrg[0].Org_Menu[0].)
            }
            return list;
        }
        private List<string> getGroupDivision(List<GroupDivision_Menu> group)
        {
            List<string> list = new List<string>();
            //loop asset FG
            foreach (var data in group)
            {
                list.Add(data.Organization_Code);
                if (data.Division_Level.Count > 0)
                    list.AddRange(getDivision(data.Division_Level));
                if(data.Asset_Level.Count > 0)
                    list.AddRange(getAsset(data.Asset_Level));
                
            }
            return list;
        }
        private List<string> getDivision(List<Division_Menu> group)
        {
            List<string> list = new List<string>();
            //loop asset FG
            foreach (var data in group)
            {
                list.Add(data.Organization_Code);
                if (data.Department_Level.Count > 0)
                    list.AddRange(getDepartment(data.Department_Level));
                if (data.Asset_Level.Count > 0)
                    list.AddRange(getAsset(data.Asset_Level));
            }
            return list;
        }
        private List<string> getDepartment(List<Department_Menu> group)
        {
            List<string> list = new List<string>();
            //loop asset FG
            foreach (var data in group)
            {
                list.Add(data.Organization_Code);
                if (data.Asset_Level.Count > 0)
                    list.AddRange(getAsset(data.Asset_Level));
            }
            return list;
        }
        private List<string> getAsset(List<Asset_Menu> asset)
        {
            List<string> list = new List<string>();
            //loop asset FG
            foreach (var data in asset)
            {
                list.Add(data.Organization_Code);
            }
            return list;
        }
    }
}
