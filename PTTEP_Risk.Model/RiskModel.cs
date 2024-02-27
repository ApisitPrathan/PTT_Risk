using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class CollectionRisk
    {
        public List<Risk> Risk { get; set; }
    }
    public class Risk
    {
        public string Role { get; set; }
        public string Risk_Id { get; set; }
        public string Risk_Co_Id { get; set; }
        public string Risk_Staff_Id { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_Abbreviation { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Likelihood { get; set; }
        public string Risk_Impact { get; set; }
        public string Risk_Rating { get; set; }
        public string Risk_Category { get; set; }
        public string Risk_Objective { get; set; }
        public string Risk_Objective_Parent { get; set; }
        public string Risk_Unit_KPI { get; set; }
        public string Risk_Context { get; set; }
        public string Risk_Register_By { get; set; }
        public string Risk_Register_Date { get; set; }
        public string Risk_Modified_By { get; set; }
        public string Risk_Modified_Date { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_Max_RootCause_Id { get; set; }
        public string Risk_Max_Impact_Id { get; set; }
        public string Risk_Max_RootCause_Ater_Mitigated_Id { get; set; }
        public string Risk_Max_Impact_Ater_Mitigated_Id { get; set; }
        public string Risk_Source { get; set; }
        public string Table_Type { get; set; }
        public string Risk_Copy { get; set; }
        public string FG_Unit { get; set; }
        public string FG_Flag { get; set; }
        public string FG_Console { get; set; }
        public string FG_Approve { get; set; }
        public string DI_Unit { get; set; }
        public string DI_Flag { get; set; }
        public string DI_Console { get; set; }
        public string DI_Approve { get; set; }
        public string BU_Unit { get; set; }
        public string BU_Flag { get; set; }
        public string BU_Console { get; set; }
        public string BU_Approve { get; set; }
        public string Asset_Unit { get; set; }
        public string Asset_Flag { get; set; }
        public string Asset_Console { get; set; }
        public string Asset_Approve { get; set; }
        public string Risk_Level { get; set; }
        public string Risk_ContactUs { get; set; }
        public string Close_Invalid { get; set; }
        public string Close_Mitigation { get; set; }
        public string Close_Invalid_Approve { get; set; }
        public string Close_Mitigation_Approve { get; set; }
        public string Delete_Flag { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public string ABBREVIATION { get; set; }
        public string Risk_Quarter { get; set; }
        public string ReCall { get; set; }
        public string Risk_ViewMode { get; set; }
        /*
        public string Role { get; set; }
        public string Risk_Id { get; set; }
        public string Risk_Co_Id { get; set; }
        public string Risk_Staff_Id { get; set; }
        public string Risk_Register_Date { get; set; }
        public string Risk_Quarter { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string ABBREVIATION { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Likelihood { get; set; }
        public string Risk_Impact { get; set; }
        public string Risk_Category { get; set; }
        public string Risk_Objective { get; set; }
        public string Risk_Objective_Parent { get; set; }
        public string Risk_Unit_KPI { get; set; }
        public string Risk_Context { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Register_By { get; set; }
        public string Risk_Modified_By { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Risk_Rating { get; set; }
        public string FG_Approve { get; set; }
        public string FG_Console { get; set; }
        public string DI_Approve { get; set; }
        public string DI_Console { get; set; }
        public string BU_Approve { get; set; }
        public string BU_Console { get; set; }
        public string Asset_Approve { get; set; }
        public string Asset_Console { get; set; }
        public string Delete_Flag { get; set; }*/
        public List<RootCause> RootCause { get; set; }
        public List<Impact> Impact { get; set; }

    }
    public class RootCause
    {
        public string Role { get; set; }
        public string Risk_Id { get; set; }
        public string RootCause_Id { get; set; }
        public string RootCause_Category { get; set; }
        public string RootCause_Likelihood { get; set; }
        public string RootCause_Mitigation_Type { get; set; }
        public string RootCause_After_Mitigated { get; set; }
        public string RootCause_KRI_Name { get; set; }
        public string RootCause_KRI_Threshold_Green { get; set; }
        public string RootCause_KRI_Threshold_Red { get; set; }
        public string RootCause_KRI_Status { get; set; }
        public string RootCause_KRI_Justification { get; set; }
        public string DeleteFag { get; set; }
        public string Delete_RootCause_Id { get; set; }
        public string Delete_RootCause_Mitigation_Id { get; set; }
        public List<RootCause_Mitigation> RootCause_Mitigation { get; set; }
    }
    public class Impact
    {
        public string Role { get; set; }
        public string Risk_Id { get; set; }
        public string Impact_Id { get; set; }
        public string Impact_Category { get; set; }
        public string Impact_NPT_EMV { get; set; }
        public string Impact_Total_Amont { get; set; }
        public string Impact_Description { get; set; }
        public string Impact_Level { get; set; }
        public string Impact_Rating { get; set; }
        public string Impact_Mitigation_Type { get; set; }
        public string Impact_After_Mitigated { get; set; }
        public string DeleteFag { get; set; }
        public string Delete_Impact_Id { get; set; }
        public string Delete_Impact_Mitigation_Id { get; set; }

        public List<Impact_Mitigation> Impact_Mitigation { get; set; }
    }
    public class RootCause_Mitigation
    {
        public string Risk_Id { get; set; }
        public string RootCause_Id { get; set; }
        public string RootCause_Mitigation_Id { get; set; }
        public string RootCause_Mitigation_Name { get; set; }
        public string RootCause_Mitigation_Owner { get; set; }
        public string RootCause_Mitigation_Owner_ABBREVIATION { get; set; }
        public string RootCause_Mitigation_DueDate { get; set; }
        public string RootCause_Mitigation_Progress { get; set; }
        public string RootCause_Mitigation_Justification { get; set; }
    }

    public class Impact_Mitigation
    {
        public string Risk_Id { get; set; }
        public string Impact_Id { get; set; }
        public string Impact_Mitigation_Id { get; set; }
        public string Impact_Mitigation_Name { get; set; }
        public string Impact_Mitigation_Owner { get; set; }
        public string Impact_Mitigation_Owner_ABBREVIATION { get; set; }
        public string Impact_Mitigation_DueDate { get; set; }
        public string Impact_Mitigation_Progress { get; set; }
        public string Impact_Mitigation_Justification { get; set; }
    }

    public class History
    {
        public string History_Id { get; set; }
        public string Risk_Id { get; set; }
        public string Status_Id { get; set; }
        public string Bu_Level { get; set; }
        public string Status_Name { get; set; }
        public string CreateDate { get; set; }
        public string CreateBy { get; set; }
        public string Business_Name { get; set; }
        public string ActionBy { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }
    }


    public class RiskView
    {
        public string Risk_Id { get; set; }
        public string Risk_Co_Id { get; set; }
        public string Risk_Staff_Id { get; set; }
        public string Risk_Quarter { get; set; }
        public string Risk_Register_Date { get; set; }
        public string Risk_Register_By { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string ABBREVIATION { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Likelihood { get; set; }
        public string Risk_Impact { get; set; }
        public string Risk_Category { get; set; }
        public string Risk_Objective { get; set; }
        public string Risk_Objective_Parent { get; set; }
        public string Risk_Unit_KPI { get; set; }
        public string Risk_Context { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_Rating { get; set; }
        public string FG_Approve { get; set; }
        public string FG_Console { get; set; }
        public string DI_Approve { get; set; }
        public string DI_Console { get; set; }
        public string BU_Approve { get; set; }
        public string BU_Console { get; set; }
        public string Asset_Approve { get; set; }
        public string Asset_Console { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public List<RootCause> RootCause { get; set; }
        public List<Impact> Impact { get; set; }
        public List<RootCause_Mitigation> RootCause_Mitigation { get; set; }
        public List<Impact_Mitigation> Impact_Mitigation { get; set; }
        public List<History> Risk_History { get; set; }

    }
}
