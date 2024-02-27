using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class DelegateModel
    {
        public string Delegate_Type { get; set; }
        public string Delegate_To { get; set; }
        public string Delegate_Status { get; set; }
        public string Risk_Category { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Running { get; set; }
        public string QuarterID { get; set; }
        public string QuaterMaster { get; set; }
        public string WPBID { get; set; }
        public List<Delegate_Transection> Delegate_Transection { get; set; }
    }
    public class Delegate_Transection
    {
        public string Risk_Id { get; set; }
        public string Delegate_Type { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_DelegateTo { get; set; }
        public string Remark { get; set; }
    }

    public class DelegateResult
    {
        public string Risk_Id { get; set; }
        public string Risk_Co_Id { get; set; }
        public string Risk_Staff_Id { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string BusinessUnit { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Category { get; set; }
        public string RiskCategoryName { get; set; }
        public string Risk_Register_Name { get; set; }
        public string Risk_Modified_Name { get; set; }
        public string Risk_Register_Date { get; set; }
        public string Risk_Modified_Date { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Rating { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Table_Type { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public string FG_Unit { get; set; }
        public string FG_Flag { get; set; }
        public string DI_Unit { get; set; }
        public string DI_Flag { get; set; }
        public string BU_Unit { get; set; }
        public string BU_Flag { get; set; }
        public string Asset_Unit { get; set; }
        public string Asset_Flag { get; set; }
    }
}
