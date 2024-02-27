using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class Report_Risk_Items
    {
        public string Risk_Business_Unit { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_KRI_Status { get; set; }
        public string Risk_Catagory { get; set; }
        public string Risk_Register_Date_From { get; set; }
        public string Risk_Register_Date_To { get; set; }
        public string Risk_Rating { get; set; }
        public string Risk_Name { get; set; }
        public string Filter_Table { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public string QuaterMaster { get; set; }
    }

    public class Report_Dashboard_Category
    {
        public string Risk_Business_Level { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Child_Node { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public string QuaterMaster { get; set; }
    }

    public class Report_Dashboard_Status
    {
        public string QuarterID { get; set; }
        public string QuaterMaster { get; set; }
        public string QuarterMasterID { get; set; }
        public string WPBID { get; set; }
    }

    /*public class Risk_Items_View
    { 
        public List<Risk_items> Risk_items { get; set; }
    }
    public class Risk_items
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
        public List<RootCause_items> RootCause_items { get; set; }
        public List<Impact_items> Impact_items { get; set; }
    }

    public class RootCause_items
    {
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
        public List<RootCause_Mitication_items> RootCause_Mitication_items { get; set; }
        
    }

    public class Impact_items
    {
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
        public List<Impact_Mitication_items> Impact_Mitication_items { get; set; }
    }

    public class RootCause_Mitication_items
    {
        public string Risk_Id { get; set; }
        public string RootCause_Id { get; set; }
        public string RootCause_Mitigation_Id { get; set; }
        public string RootCause_Mitigation_Name { get; set; }
        public string RootCause_Mitigation_Owner { get; set; }
        public string RootCause_Mitigation_DueDate { get; set; }
        public string RootCause_Mitigation_Progress { get; set; }
        public string RootCause_Mitigation_Justification { get; set; }
    }

    public class Impact_Mitication_items
    {
        public string Risk_Id { get; set; }
        public string Impact_Id { get; set; }
        public string Impact_Mitigation_Id { get; set; }
        public string Impact_Mitigation_Name { get; set; }
        public string Impact_Mitigation_Owner { get; set; }
        public string Impact_Mitigation_DueDate { get; set; }
        public string Impact_Mitigation_Progress { get; set; }
        public string Impact_Mitigation_Justification { get; set; }
    }*/
}
