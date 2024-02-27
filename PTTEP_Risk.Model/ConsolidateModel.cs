using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class ConsolidateModel
    {
        
        public string Risk_Catagory { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Register_By { get; set; }
        public string Risk_Register_Date_From { get; set; }
        public string Risk_Register_Date_To { get; set; }
        public string Risk_Running { get; set; }
        public string Consolidate_By { get; set; }
        public string Risk_Escalation { get; set; }
        public string Risk_Rating { get; set; }
        public string QuarterID { get; set; }
        public string WPBID { get; set; }
        public string QuaterMaster { get; set; }
        //public string IdCollection { get; set; }
        public List<Consolidate_Transection> Consolidate_Transection { get; set; }
        public List<Consolidate_Staff> Consolidate_Staff { get; set; }
    }

    public class Consolidate_Transection
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
    }

    public class Consolidate_Staff
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
    }

    public class Consolidate_Transfer
    {
        public string Risk_Id { get; set; }
        public string Risk_Co_Id { get; set; }
        public string Risk_Co_Id_New { get; set; }
        public string Risk_Register_By { get; set; }
        public string Risk_Modified_By { get; set; }
        public string Risk_Category { get; set; }
        public string Risk_Status { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Name { get; set; }
        public string Risk_Rating { get; set; }
        public string Risk_Running { get; set; }
        public string Risk_Escalation { get; set; }
        public string QuarterID { get; set; }
    }

    public class ReConsolidate
    {
        public string ReConsolidateBy { get; set; }
        public List<ReConsoleTransection> ReConsoleTransection { get; set; }
    }
    public class ReConsoleTransection
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Comment { get; set; }

    }
}
