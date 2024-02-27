using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class RiskMap_Impact
    {
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public string QuaterMaster { get; set; }
        public string BusinessCode { get; set; }
        public string Year { get; set; }
        public string TypeMitigate { get; set; }
        public string Risk_Level { get; set; }
        public string Risk_Escalation { get; set; }
    }

    public class RiskMap_Unit
    {
        public string FilterUser { get; set; }
        public string Module { get; set; }
        public string BUCode { get; set; }
        public string Role { get; set; }
        public string QuarterID { get; set; }
        public string Filter_Type { get; set; }
    }

    public class RiskMap_Unit_Result
    {
        public string Risk_Id { get; set; }
        public string Risk_Co_Id { get; set; }
        public string Risk_Staff_Id { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_BusinessUnit_Code { get; set; }
        public string Risk_BusinessUnit_Name { get; set; }
        public string Risk_Status_Code { get; set; }
        public string Risk_Status_Name { get; set; }
        public string Risk_Type { get; set; }
        public string Impact_Category { get; set; }
        public string Impact_NPT_EMV { get; set; }
        public string Impact_Total_Amont { get; set; }
        public string Impact_Level { get; set; }
        public string Table_Type { get; set; }
        public string Y_Impact { get; set; }
        public string X_Likelihood { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
    }

    public class Risk_Unit_Result
    {
        public string Risk_Id { get; set; }
        public string Risk_Co_Id { get; set; }
        public string Risk_Staff_Id { get; set; }
        public string Risk_Register_Date { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Max_RootCause_Id { get; set; }
        public string Risk_Max_Impact_Id { get; set; }
        public string Risk_BusinessUnit_Code { get; set; }
        public string Risk_BusinessUnit_Name { get; set; }
        public string Risk_Status_Code { get; set; }
        public string Risk_Status_Name { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_Source { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Register_By { get; set; }
        public string Table_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public string TransectionLevel { get; set; }
        public string FG_Unit { get; set; }
        public string FG_Flag { get; set; }
        public string FG_Console { get; set; }
        public string DI_Unit { get; set; }
        public string DI_Flag { get; set; }
        public string DI_Console { get; set; }
        public string BU_Unit { get; set; }
        public string BU_Flag { get; set; }
        public string BU_Console { get; set; }
        public string Asset_Unit { get; set; }
        public string Asset_Flag { get; set; }
        public string Asset_Console { get; set; }
        public string Close_Invalid { get; set; }
        public string Close_Invalid_Approve { get; set; }
        public string Close_Mitigation { get; set; }
        public string Close_Mitigation_Approve { get; set; }
        public string Risk_ViewMode { get; set; }
        public string Risk_Status_Workflow_ReConsole { get; set; }
        public string Risk_AssignTo_ReConsole { get; set; }
        public List<History> History { get; set; }
    }

    public class RiskMap_Menu
    {
        public string QuarterID { get; set; }
        public List<RiskMap_MenuGroup> GroupDivision_Level { get; set; }
        public List<RiskMap_MenuDivision> Division_Level { get; set; }
        public List<RiskMap_MenuDepartment> Department_Level { get; set; }
    }

    public class RiskMap_MenuGroup
    {
        /*public string Organization_Name { get; set; }
        public string Organization_Level { get; set; }
        public string Organization_Code { get; set; }
        public string CODE { get; set; }
        public string ABBREVIATION { get; set; }
        public List<Asset_Menu> Asset_Level { get; set; }*/
        public List<GroupDivision_Menu> Org_Menu { get; set; }
        //public List<Division_Menu> Division_Level { get; set; }
        //public List<Department_Menu> Department_Level { get; set; }
    }

    public class RiskMap_MenuDivision
    {
        /*public string Organization_Name { get; set; }
        public string Organization_Level { get; set; }
        public string Organization_Code { get; set; }
        public string CODE { get; set; }
        public string ABBREVIATION { get; set; }
        public List<Asset_Menu> Asset_Level { get; set; }*/
        public List<Division_Menu> Org_Menu { get; set; }
        //public List<Division_Menu> Division_Level { get; set; }
        //public List<Department_Menu> Department_Level { get; set; }
    }

    public class RiskMap_MenuDepartment
    {
        /*public string Organization_Name { get; set; }
        public string Organization_Level { get; set; }
        public string Organization_Code { get; set; }
        public string CODE { get; set; }
        public string ABBREVIATION { get; set; }
        public List<Asset_Menu> Asset_Level { get; set; }*/
        public List<Department_Menu> Org_Menu { get; set; }
        //public List<Division_Menu> Division_Level { get; set; }
        //public List<Department_Menu> Department_Level { get; set; }
    }

    public class GroupDivision_Menu
    {
        public string Organization_Name { get; set; }
        public string Organization_Level { get; set; }
        public string Organization_Code { get; set; }
        public string CODE { get; set; }
        public string ABBREVIATION { get; set; }
        public List<Master_CO> CoInfo { get; set; }
        public List<Asset_Menu> Asset_Level { get; set; }
        public List<Division_Menu> Division_Level { get; set; }
        public List<ResFinancialImpact> Financial { get; set; }
    }

    public class Division_Menu
    {
        public string Organization_Name { get; set; }
        public string Organization_Level { get; set; }
        public string Organization_Code { get; set; }
        public string CODE { get; set; }
        public string ABBREVIATION { get; set; }
        public List<Master_CO> CoInfo { get; set; }
        public List<Asset_Menu> Asset_Level { get; set; }
        public List<Department_Menu> Department_Level { get; set; }
        public List<ResFinancialImpact> Financial { get; set; }
    }

    public class Department_Menu
    {
        public string Organization_Name { get; set; }
        public string Organization_Level { get; set; }
        public string Organization_Code { get; set; }
        public string CODE { get; set; }
        public string ABBREVIATION { get; set; }
        public List<Master_CO> CoInfo { get; set; }
        public List<Asset_Menu> Asset_Level { get; set; }
        public List<ResFinancialImpact> Financial { get; set; }
    }

    public class Asset_Menu
    {
        public string Asset_Name { get; set; }
        public string ABBREVIATION { get; set; }
        public string Organization_Level { get; set; }
        public string Organization_Code { get; set; }
        public List<Master_Assset> CoInfo { get; set; }
        public List<ResFinancialImpact> Financial { get; set; }
    }
}
