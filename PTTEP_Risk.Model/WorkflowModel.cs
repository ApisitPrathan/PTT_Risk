using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    public class WorkflowModel
    {
        public string Risk_Id { get; set; }
        public string Risk_Assign_To { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Modified_By { get; set; }
        public string Risk_Submit_Workflow { get; set; }
        public string IdCollection { get; set; }
        public string SecurityCode { get; set; }
    }

    public class SubmitModel
    {
        public string Risk_Modified_By { get; set; }
        public string Risk_Submit_Workflow { get; set; }
        public List<TypeAssetModel> SubmitTypeAsset{ get; set; }
        public List<TypeOrganizationModel> SubmitTypeOrganization { get; set; }
        public List<TypeCorporateModel> SubmitTypeCorporate { get; set; }
    }

    public class TypeAssetModel
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Business_Unit_WF_Level { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Risk_Modify_By { get; set; }
        public string Risk_Submit_Action { get; set; }
        public string Risk_Submit_Reason { get; set; }
    }

    public class TypeOrganizationModel
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Business_Unit_WF_Level { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Risk_Modify_By { get; set; }
        public string Risk_Submit_Action { get; set; }
        public string Risk_Submit_Reason { get; set; }
    }

    public class TypeCorporateModel
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Business_Unit_WF_Level { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Risk_Modify_By { get; set; }
    }

    public class ApproveModel
    { 
        public string Approve_By { get; set; }
        public List<ApproveTypeAsset> ApproveTypeAsset { get; set; }
        public List<ApproveTypeOrganization> ApproveTypeOrganization { get; set; }
    }

    public class ApproveTypeAsset
    {

        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Status_Approve { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Comment { get; set; }
        public string Risk_CC { get; set; }
        public string Action { get; set; }
    }

    public class ApproveTypeOrganization
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Status_Approve { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Comment { get; set; }
        public string Risk_CC { get; set; }
        public string Action { get; set; }
    }

    public class RejectModel
    {
        public string Reject_By { get; set; }
        public List<RejectTypeAsset> RejectTypeAsset { get; set; }
        public List<RejectTypeOrganization> RejectTypeOrganization { get; set; }
    }

    public class RejectTypeAsset
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Status_Reject { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }
    }

    public class RejectTypeOrganization
    {
        public string Risk_Id { get; set; }
        public string Risk_Business_Unit { get; set; }
        public string Risk_Business_Unit_WF { get; set; }
        public string Risk_Status_Workflow { get; set; }
        public string Risk_Status_Reject { get; set; }
        public string Risk_Type { get; set; }
        public string Risk_AssignTo { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }
    }
    public class Contact
    {
        public string ContactID { get; set; }
        public string ContactName { get; set; }
        public string Position { get; set; }
        public string Emp_Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PicPath { get; set; }
        public string GroupMap { get; set; }
    }

}
