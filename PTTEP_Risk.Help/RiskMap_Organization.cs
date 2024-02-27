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
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PTTEP_Risk.Help
{

    public class RiskMap_Organization
    {
        Helper _h = new Helper();
        GetUser _s = new GetUser();
        GetOrganizations _o = new GetOrganizations();
        ConfigurationService _c = new ConfigurationService();

        public List<RiskMap_Menu> Get_RiskMap_Organization(List<Organizations> colOrg,string QuarterID)
        {

            List<RiskMap_Menu> GroupMenu = new List<RiskMap_Menu>();
            List<RiskMap_MenuGroup> RiskMap_MenuGroup = new List<RiskMap_MenuGroup>();
            List<RiskMap_MenuDivision> RiskMap_MenuDivision = new List<RiskMap_MenuDivision>();
            List<RiskMap_MenuDepartment> RiskMap_MenuDepartment = new List<RiskMap_MenuDepartment>();
            RiskMap_Menu RiskMap_Menu = new RiskMap_Menu();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                var p_masterAsset = new DynamicParameters();
                p_masterAsset.Add("@Table_Name", "Master_Asset");
                p_masterAsset.Add("@TextSearch1", QuarterID);
                var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterAsset, commandType: CommandType.StoredProcedure).ToList();

                var p_masterCo = new DynamicParameters();
                p_masterCo.Add("@Table_Name", "Master_Cordinator");
                p_masterCo.Add("@TextSearch1", QuarterID);
                var mCo = conn.Query<Master_CO>("sp_Get_Table", p_masterCo, commandType: CommandType.StoredProcedure).ToList();

                var p_masterFinancial = new DynamicParameters();
                p_masterFinancial.Add("@Table_Name", "Master_FinancialImpact");
                p_masterFinancial.Add("@TextSearch1", QuarterID);
                var mfinan = conn.Query<ResFinancialImpact>("sp_Get_Table", p_masterFinancial, commandType: CommandType.StoredProcedure).ToList();
                foreach (var _org in colOrg)
                {
                    List<Organizations> orgInfo = new List<Organizations>();
                    if (_org.ORGANIZATION_LEVEL != "Group") //if ORGANIZATION_LEVEL not Group find FG
                    {
                        //get parent org
                        /*var paramsOrg = new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string, string>("OrganizetionID", _org.ORGANIZATION_ID),
                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                        };
                        List<Organizations> org = new List<Organizations>();*/
                        //orgInfo = _o.GetRecursiveOrganization(paramsOrg, org);
                        orgInfo = _o.GetRecursiveOrganization_DB(_org, QuarterID);
                    }
                    else//if ORGANIZATION_LEVEL is group find chide org
                    {
                        //get chide org
                        /*var paramsOrg = new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string, string>("OrganizetionID", _org.ORGANIZATION_ID),
                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                        };*/
                        orgInfo = _o.GetOrgChildByOrgID_DB(_org, QuarterID);
                        //orgInfo = _o.GetOrgChildByOrgID(paramsOrg);
                        orgInfo.Add(_org);
                    }
                    if (orgInfo.Count > 0)
                    {
                        //check Bu parent ceo
                        var _temp = orgInfo.Where(o => o.ORGANIZATION_LEVEL == "Group" || o.ORGANIZATION_LEVEL == "Division" || o.ORGANIZATION_LEVEL == "Department").OrderBy(x => x.CODE).ToList();
                        if (_temp.Count > 0)
                        {
                            if (_temp[0].ORGANIZATION_LEVEL == "Group")
                            {
                                //Get Menu Organization

                                RiskMap_MenuGroup Menu = new RiskMap_MenuGroup();
                                Menu.Org_Menu = Get_GroupDivision_Organization(_temp[0], mAsset, mCo, mfinan,QuarterID);
                                RiskMap_MenuGroup.Add(Menu);
                                RiskMap_Menu.GroupDivision_Level = RiskMap_MenuGroup;

                            }
                            else if (_temp[0].ORGANIZATION_LEVEL == "Division")
                            {
                                //Get Menu Division
                                RiskMap_MenuDivision Menu = new RiskMap_MenuDivision();
                                Menu.Org_Menu = Get_Division_Organization(_temp[0], mAsset, mCo, mfinan, QuarterID);
                                RiskMap_MenuDivision.Add(Menu);
                                RiskMap_Menu.Division_Level = RiskMap_MenuDivision;
                            }
                            else if (_temp[0].ORGANIZATION_LEVEL == "Department")
                            {
                                //Get Menu Department
                                RiskMap_MenuDepartment Menu = new RiskMap_MenuDepartment();
                                Menu.Org_Menu = Get_Department_Organization(_temp[0], mAsset, mCo, mfinan, QuarterID);
                                RiskMap_MenuDepartment.Add(Menu);
                                RiskMap_Menu.Department_Level = RiskMap_MenuDepartment;
                            }
                            
                        }
                    }
                }
                GroupMenu.Add(RiskMap_Menu);
            }
            return GroupMenu;
        }
        public string Get_Childe_Organization_To_OrgLevel(List<RiskMap_Menu> riskmapOrg)
        {
            var colStr = "";
            if (!_h.IsNullOrEmpty(riskmapOrg[0].GroupDivision_Level))//if FG org parent is ceo
            {
                var strFG = Get_Childe_Level_FG(riskmapOrg[0].GroupDivision_Level);
                if (!_h.CheckNull(strFG))
                    colStr += ",";
                colStr += strFG;
            }
            if (!_h.IsNullOrEmpty(riskmapOrg[0].Division_Level))//if Di org parent is ceo
            {
                var strDI = Get_Childe_Level_DI(riskmapOrg[0].Division_Level);
                if (!_h.CheckNull(strDI))
                    colStr += ",";
                colStr += strDI;
            }
            if (!_h.IsNullOrEmpty(riskmapOrg[0].Department_Level))//if BU org parent is ceo
            {
                var strDEP = Get_Childe_Level_DEP(riskmapOrg[0].Department_Level);
                if (!_h.CheckNull(strDEP))
                    colStr += ",";
                colStr += strDEP;
            }
            return colStr;
        }
        private string Get_Childe_Level_FG(List<RiskMap_MenuGroup> listFG)
        {
            var str = "";
            foreach (var _org in listFG)
            {
                //Get Org Division Level
                if (!_h.IsNullOrEmpty(_org.Org_Menu[0].Division_Level))
                {
                    str += Get_StrChilde_Division(_org.Org_Menu[0].Division_Level);
                }
                //Get Asset of BU
                if (!_h.IsNullOrEmpty(_org.Org_Menu[0].Asset_Level))
                {
                    str += Get_Asset(_org.Org_Menu[0].Asset_Level);
                }
                if (!_h.CheckNull(str))
                    str += ",";
                str += _org.Org_Menu[0].Organization_Code;
            }
            return str;
        }
        private string Get_Childe_Level_DI(List<RiskMap_MenuDivision> listDI)
        {
            var str = "";
            foreach (var _org in listDI)
            {  
                if (!_h.IsNullOrEmpty(_org.Org_Menu[0].Department_Level))
                {
                    str += Get_StrChilde_Department(_org.Org_Menu[0].Department_Level);
                }
                //Get Asset of BU
                if (!_h.IsNullOrEmpty(_org.Org_Menu[0].Asset_Level))
                {
                    str += Get_Asset(_org.Org_Menu[0].Asset_Level);
                }
                if (!_h.CheckNull(str))
                    str += ",";
                str += _org.Org_Menu[0].Organization_Code;
            }
            return str;
        }
        private string Get_Childe_Level_DEP(List<RiskMap_MenuDepartment> listDEP)
        {
            var str = "";
            foreach (var _org in listDEP)
            {
                //Get Asset of BU
                if (!_h.IsNullOrEmpty(_org.Org_Menu[0].Asset_Level))
                {
                    str += Get_Asset(_org.Org_Menu[0].Asset_Level);
                }
                if (!_h.CheckNull(str))
                    str += ",";
                str += _org.Org_Menu[0].Organization_Code;
            }
            return str;
        }
        private string Get_StrChilde_Division(List<Division_Menu> DI)
        {
            var str = "";
            foreach (var _org in DI)
            {
                if (!_h.IsNullOrEmpty(_org.Department_Level))
                {
                    str += Get_StrChilde_Department(_org.Department_Level);
                }
                //Get Asset of BU
                if (!_h.IsNullOrEmpty(_org.Asset_Level))
                {
                    str += Get_Asset(_org.Asset_Level);
                }
                if (!_h.CheckNull(str))
                    str += ",";
                str += _org.Organization_Code;
            }
            return str;
        }
        private string Get_StrChilde_Department(List<Department_Menu> DE)
        {
            var str = "";
            foreach (var _org in DE)
            {
                //Get Asset of BU
                if (!_h.IsNullOrEmpty(_org.Asset_Level))
                {
                    str += Get_Asset(_org.Asset_Level);
                }
                if (!_h.CheckNull(str))
                    str += ",";
                str += _org.Organization_Code;
            }
            return str;
        }
        private string Get_Asset(List<Asset_Menu> Asset)
        {
            var str = "";
            foreach (var _org in Asset)
            {
                if (!_h.CheckNull(str))
                    str += ",";
                str += _org.Organization_Code;
            }
            return str;
        }
        private List<GroupDivision_Menu> Get_GroupDivision_Organization(Organizations org, List<Master_Assset> asset , List<Master_CO> co,List<ResFinancialImpact> finan,string QuarterID)
        {
            List<GroupDivision_Menu> groupDivision_Model = new List<GroupDivision_Menu>();
            GroupDivision_Menu groupDivision = new GroupDivision_Menu();
            groupDivision.Organization_Name = org.NAME;
            groupDivision.Organization_Code = org.ORGANIZATION_ID;
            groupDivision.Organization_Level = org.ORGANIZATION_LEVEL;
            groupDivision.CODE = org.CODE;
            groupDivision.ABBREVIATION = org.ABBREVIATION;
            groupDivision.CoInfo = Get_Co_BU(org.ORGANIZATION_ID,co);
            groupDivision.Asset_Level = Get_Asset_Menu(asset, finan, org.ORGANIZATION_ID);
            groupDivision.Division_Level = Get_Division(asset,co, finan, org, QuarterID);
            groupDivision.Financial = Get_FinancialImpact(finan,org.ORGANIZATION_ID);
            groupDivision_Model.Add(groupDivision);


            return groupDivision_Model;
        }
        private List<Division_Menu> Get_Division_Organization(Organizations org, List<Master_Assset> asset, List<Master_CO> co, List<ResFinancialImpact> finan, string QuarterID)
        {
            List<Division_Menu> Division_Model = new List<Division_Menu>();
            Division_Menu Division = new Division_Menu();
            Division.Organization_Name = org.NAME;
            Division.Organization_Code = org.ORGANIZATION_ID;
            Division.Organization_Level = org.ORGANIZATION_LEVEL;
            Division.CODE = org.CODE;
            Division.ABBREVIATION = org.ABBREVIATION;
            Division.CoInfo = Get_Co_BU(org.ORGANIZATION_ID, co);
            Division.Asset_Level = Get_Asset_Menu(asset, finan, org.ORGANIZATION_ID);
            Division.Department_Level = Get_Department(asset,co, finan, org, QuarterID);
            Division.Financial = Get_FinancialImpact(finan, org.ORGANIZATION_ID);
            Division_Model.Add(Division);
            return Division_Model;
        }
        private List<Department_Menu> Get_Department_Organization(Organizations org, List<Master_Assset> asset, List<Master_CO> co, List<ResFinancialImpact> finan, string QuarterID)
        {
            List<Department_Menu> Department_Model = new List<Department_Menu>();
            Department_Menu Department = new Department_Menu();
            Department.Organization_Name = org.NAME;
            Department.Organization_Code = org.ORGANIZATION_ID;
            Department.Organization_Level = org.ORGANIZATION_LEVEL;
            Department.CODE = org.CODE;
            Department.ABBREVIATION = org.ABBREVIATION;
            Department.CoInfo = Get_Co_BU(org.ORGANIZATION_ID, co);
            Department.Asset_Level = Get_Asset_Menu(asset, finan, org.ORGANIZATION_ID);
            Department.Financial = Get_FinancialImpact(finan, org.ORGANIZATION_ID);
            Department_Model.Add(Department);
            return Department_Model;
        }
        private List<Division_Menu> Get_Division(List<Master_Assset> asset, List<Master_CO> co, List<ResFinancialImpact> finan, Organizations org, string QuarterID)
        {
            List<Division_Menu> Division_Model = new List<Division_Menu>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            //Get Division Organization
            /*var paramsChildOrg = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("OrganizetionID", org),
                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
            };
            var orgChildDivisionInfo = _o.GetOrgChildByOrgID(paramsChildOrg);*/
            var orgChildDivisionInfo = _o.GetOrgChildByOrgID_DB(org, QuarterID);
            if (orgChildDivisionInfo.Count > 0)
            {
                foreach (var items in orgChildDivisionInfo)
                {
                    if (items.ORGANIZATION_LEVEL != "Section")
                    {
                        Division_Menu Division = new Division_Menu();
                        Division.Organization_Name = items.NAME;
                        Division.Organization_Code = items.ORGANIZATION_ID;
                        Division.Organization_Level = items.ORGANIZATION_LEVEL;
                        Division.CODE = items.CODE;
                        Division.ABBREVIATION = items.ABBREVIATION;
                        Division.CoInfo = Get_Co_BU(items.ORGANIZATION_ID, co);
                        Division.Asset_Level = Get_Asset_Menu(asset, finan, items.ORGANIZATION_ID);
                        Division.Department_Level = Get_Department(asset, co, finan, items, QuarterID);
                        Division.Financial = Get_FinancialImpact(finan, items.ORGANIZATION_ID);
                        Division_Model.Add(Division);
                    }
                }
            }
            return Division_Model;
        }
        private List<Department_Menu> Get_Department(List<Master_Assset> asset, List<Master_CO> co, List<ResFinancialImpact> finan, Organizations org, string QuarterID)
        {
            List<Department_Menu> Department_Model = new List<Department_Menu>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            //Get Division Organization
            /*var paramsChildOrg = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("OrganizetionID", org),
                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
            };
            var orgChildDepartmentInfo = _o.GetOrgChildByOrgID(paramsChildOrg);*/
            var orgChildDepartmentInfo = _o.GetOrgChildByOrgID_DB(org, QuarterID);
            if (orgChildDepartmentInfo.Count > 0)
            {
                foreach (var items in orgChildDepartmentInfo)
                {
                    if (items.ORGANIZATION_LEVEL != "Section")
                    {
                        Department_Menu Department = new Department_Menu();
                        Department.Organization_Name = items.NAME;
                        Department.Organization_Code = items.ORGANIZATION_ID;
                        Department.Organization_Level = items.ORGANIZATION_LEVEL;
                        Department.CODE = items.CODE;
                        Department.ABBREVIATION = items.ABBREVIATION;
                        Department.CoInfo = Get_Co_BU(items.ORGANIZATION_ID, co);
                        Department.Asset_Level = Get_Asset_Menu(asset, finan, items.ORGANIZATION_ID);
                        Department.Financial = Get_FinancialImpact(finan, items.ORGANIZATION_ID);
                        Department_Model.Add(Department);
                    }
                }
            }
            return Department_Model;
        }
        private List<Asset_Menu> Get_Asset_Menu(List<Master_Assset> Asssets, List<ResFinancialImpact> finan, string org)
        {
            List<Asset_Menu> asset_Model = new List<Asset_Menu>();
            var _resultAsset = Asssets.Where(o => o.Asset_Org == org).ToList();
            var _distictAsset =  _resultAsset.GroupBy(i => i.Asset_Code).Select(i => i.FirstOrDefault()).ToList();
            if (_resultAsset.Count > 0)
            {
                foreach (var items in _distictAsset)
                {
                    if (items.Asset_Level != "Section")
                    {
                        Asset_Menu Asset = new Asset_Menu();
                        Asset.Asset_Name = items.Asset_Name;
                        Asset.ABBREVIATION = items.Asset_Short;
                        Asset.Organization_Code = items.Asset_Code;
                        Asset.Organization_Level = items.Asset_Level;
                        Asset.CoInfo = Get_Co_Asset(items.Asset_Code, Asssets);
                        Asset.Financial = Get_FinancialImpact(finan, items.Asset_Code);
                        asset_Model.Add(Asset);
                    }
                }
            }
            return asset_Model;
        }
        private List<Master_CO> Get_Co_BU(string orgId,List<Master_CO> co)
        {
            List<Master_CO> Co_Model = new List<Master_CO>();
            var _resultCo = co.Where(o => o.Coordinator_Department_Id == orgId).ToList();
            if (_resultCo.Count > 0)
            {
                foreach (var items in _resultCo)
                {
                    if (items.Coordinator_Level != "Section")
                    {
                        Master_CO Co = new Master_CO();
                        Co.CoordinatorId = items.CoordinatorId;
                        Co.QuarterID = items.QuarterID;
                        Co.QuarterYear = items.QuarterYear;
                        Co.Coordinator_Department_Id = items.Coordinator_Department_Id;
                        Co.Coordinator_Employee_Id = items.Coordinator_Employee_Id;
                        Co.Coordinator_EName = items.Coordinator_EName;
                        Co.Coordinator_Level = items.Coordinator_Level;
                        Co_Model.Add(Co);
                    }
                }
            }
            return Co_Model;
        }
        private List<Master_Assset> Get_Co_Asset(string orgId, List<Master_Assset> asset)
        {
            List<Master_Assset> Asset_Model = new List<Master_Assset>();
            var _resultAsset = asset.Where(o => o.Asset_Code == orgId).ToList();
            if (_resultAsset.Count > 0)
            {
                foreach (var items in _resultAsset)
                {
                    Master_Assset Co_Asset = new Master_Assset();
                    Co_Asset.Asset_Id = items.Asset_Id;
                    Co_Asset.QuarterYear = items.QuarterYear;
                    Co_Asset.QuarterID = items.QuarterID;
                    Co_Asset.Asset_Name = items.Asset_Name;
                    Co_Asset.Asset_Short = items.Asset_Short;
                    Co_Asset.Asset_Code = items.Asset_Code;
                    Co_Asset.Asset_Org = items.Asset_Org;
                    Co_Asset.Asset_Coordinators = items.Asset_Coordinators;
                    Co_Asset.Asset_Coordinators_EName = items.Asset_Coordinators_EName;
                    Co_Asset.Asset_Level = items.Asset_Level;
                    Co_Asset.DelFlag = items.DelFlag;
                    Asset_Model.Add(Co_Asset);
                }
            }
            return Asset_Model;
        }
        private List<ResFinancialImpact> Get_FinancialImpact(List<ResFinancialImpact> financialImpacts, string orgId)
        {
            List<ResFinancialImpact> financialImpacts_Model = new List<ResFinancialImpact>();
            var _resultFinancial = financialImpacts.Where(o => o.BusinessCode == orgId).ToList();
            if (_resultFinancial.Count > 0)
            {
                foreach (var items in _resultFinancial)
                {
                    ResFinancialImpact f = new ResFinancialImpact();
                    f.BusinessID = items.BusinessID;
                    f.QuarterID = items.QuarterID;
                    f.BusinessCode = items.BusinessCode;
                    f.BusinessUnit = items.BusinessUnit;
                    f.NI = items.NI;
                    f.NPV_EMV = items.NPV_EMV;
                    f.DelFlag = items.DelFlag;
                    financialImpacts_Model.Add(f);
                }
            }
            return financialImpacts_Model;
        }
        public List<RiskMap_Menu> Get_Childe_Organization(List<UserEmployee> userInfo, string QuarterID)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            List<RiskMap_Menu> MenuOrg = new List<RiskMap_Menu>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                var p_Check = new DynamicParameters();
                p_Check.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                p_Check.Add("@Type", "Check");
                p_Check.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                var dataERM = conn.Query<ERM>("sp_Check_Role_ERM", p_Check, commandType: CommandType.StoredProcedure).ToList();
                if (dataERM.Count > 0)
                {
                    var p_Get = new DynamicParameters();
                    p_Get.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                    p_Get.Add("@Type", "GETORG");
                    p_Get.Add("@QuarterID", QuarterID);
                    p_Get.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    var dataOrg = conn.Query<Organizations>("sp_Check_Role_ERM", p_Get, commandType: CommandType.StoredProcedure).ToList();
                    if (dataOrg.Count > 0)
                    {
                        //get information employee xml from web service GetEmployee

                        MenuOrg = Get_RiskMap_Organization(dataOrg, QuarterID);
                    }
                }
                else
                {
                    //Check Owner Emp Id
                    var p_CheckActingEmpId = new List<KeyValuePair<string, string>>() {
                        new KeyValuePair<string, string>("EmployeeID", userInfo[0].EMPLOYEE_ID),
                        new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                    };
                    var empOwnerActing = _o.GetActingOrganization(p_CheckActingEmpId);
                    if (empOwnerActing.Count > 0)
                    {
                        //check is owner section
                        if (empOwnerActing[0].ORGANIZATION_LEVEL == "Section")
                        {
                            var p_Org = new List<KeyValuePair<string, string>>() {
                                new KeyValuePair<string, string>("OrganizetionID", userInfo[0].ORGUNIT),
                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService)
                            };
                            var orgInfo = _o.GetOrganization(p_Org);
                            if (orgInfo.Count > 0)
                            {
                                MenuOrg = Get_RiskMap_Organization(orgInfo, QuarterID);
                            }
                        }
                        else // is not owner section 
                        {
                            var p_Get = new DynamicParameters();
                            p_Get.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                            p_Get.Add("@Type", "GETORG");
                            p_Get.Add("@QuarterID", QuarterID);
                            p_Get.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                            var dataOrg = conn.Query<Organizations>("sp_Check_Role_ERM", p_Get, commandType: CommandType.StoredProcedure).ToList();
                            if (dataOrg.Count > 0)
                            {
                                //get information employee xml from web service GetEmployee

                                MenuOrg = Get_RiskMap_Organization(dataOrg, QuarterID);
                            }
                        }
                    }
                    else // User is not Owner
                    {
                        //is't Erm
                        //Check Org Level
                        var p_Org = new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string, string>("OrganizetionID", userInfo[0].ORGUNIT),
                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService)
                        };
                        var orgInfo = _o.GetOrganization(p_Org);
                        if (orgInfo.Count > 0)
                        {
                            MenuOrg = Get_RiskMap_Organization(orgInfo, QuarterID);
                        }
                    }
                }
            }
            return MenuOrg;
        }

    }
}
