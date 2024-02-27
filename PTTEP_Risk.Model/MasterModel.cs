using System;
using System.Collections.Generic;
using System.Text;

namespace PTTEP_Risk.Model
{
    #region Quarter
    public class ReqQuarter
    {
        public string QuarterID { get; set; }
        public string QuarterCode { get; set; }
        public string Year { get; set; }
        public string QuarterName { get; set; }
        public string BuCoodinator { get; set; }
        public string BusinessUniteValue { get; set; }
        public string ImpactRating { get; set; }
        public string LikelihoodRating { get; set; }
        public string RiskCategory { get; set; }
        public string RiskRating { get; set; }
        public string StartQuarter { get; set; }
        public string EndQuarter { get; set; }
        public string LockDate { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResQuarter
    {
        public string QuarterID { get; set; }
        public string QuarterCode { get; set; }
        public string Year { get; set; }
        public string QuarterName { get; set; }
        public string BuCoodinator { get; set; }
        public string BusinessUniteValue { get; set; }
        public string ImpactRating { get; set; }
        public string LikelihoodRating { get; set; }
        public string RiskCategory { get; set; }
        public string RiskRating { get; set; }
        public string StartQuarter { get; set; }
        public string EndQuarter { get; set; }
        public string LockDate { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region BUCoordinator
    public class ReqBUCoordinator
    {
        public string CoordinatorId { get; set; }
        public string QuarterID { get; set; }
        public string EmpID { get; set; }
        public string DeptID { get; set; }
        public string Level { get; set; }
        public string CoorBU { get; set; }
        public string BULevel { get; set; }
        public string DelId { get; set; }
        public List<sBUCoorEx> sBUCoorEx { get; set; }
    }

    public class sBUCoorEx
    {
        public string BU_ID { get; set; }
        public string BU_NAME { get; set; }
        public string CO_EMAIL { get; set; }
        public string CO_NAME { get; set; }
    }

    public class ResBUCoordinator
    {
        public string CoordinatorId { get; set; }
        public string QuarterYear { get; set; }
        public string QuarterName { get; set; }
        public string QuarterID { get; set; }
        public string EmpID { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string DeptID { get; set; }
        public string DeptName { get; set; }
        public string Level { get; set; }
        public string CoorBU { get; set; }
        public string BULevel { get; set; }
        public List<sBUCoorEx> sBUCoorEx { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
    }
    #endregion

    #region FinancialImpact
    public class ReqFinancialImpact
    {
        public string BusinessID { get; set; }
        public string QuarterID { get; set; }
        public string BusinessCode { get; set; }
        public string BusinessUnit { get; set; }
        public string NI { get; set; }
        public string NPV_EMV { get; set; }
        public string DelFlag { get; set; }
        public List<FinancialEx> sFinancialEx { get; set; }
    }

    public class FinancialEx
    {
        public string BusinessCode { get; set; }
        public string BusinessUnit { get; set; }
        public string NI { get; set; }
        public string NPV_EMV { get; set; }
    }

    public class ResFinancialImpact
    {
        public string BusinessID { get; set; }
        public string QuarterID { get; set; }
        public string QuarterYear { get; set; }
        public string QuarterName { get; set; }
        public string BusinessCode { get; set; }
        public string BusinessUnit { get; set; }
        public decimal NI { get; set; }
        public decimal NPV_EMV { get; set; }
        public int DelFlag { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
    }
    #endregion

    #region RiskCategory
    public class ReqRiskCategory
    {
        public string RiskCategoryID { get; set; }
        public string QuarterID { get; set; }
        public string RiskCategoryCode { get; set; }
        public string RiskCategoryName { get; set; }
        public string ErmFlag { get; set; }
        public string OrderNum { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResRiskCategory
    {
        public string RiskCategoryID { get; set; }
        public string QuarterID { get; set; }
        public string QuarterYear { get; set; }
        public string QuarterName { get; set; }
        public string RiskCategoryCode { get; set; }
        public string RiskCategoryName { get; set; }
        public string ErmFlag { get; set; }
        public string OrderNum { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region RiskRating
    public class ReqRiskRating
    {
        public string RiskRatingID { get; set; }
        public string QuarterID { get; set; }
        public string RiskRatingCode { get; set; }
        public string Likelihood { get; set; }
        public string Impact { get; set; }
        public string LikelihoodAndImpact { get; set; }
        public string RiskRating { get; set; }
        public string EscalationLevel { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResRiskRating
    {
        public string RiskRatingID { get; set; }
        public string QuarterID { get; set; }
        public string QuarterYear { get; set; }
        public string QuarterName { get; set; }
        public string RiskRatingCode { get; set; }
        public string Likelihood { get; set; }
        public string Impact { get; set; }
        public string LikelihoodAndImpact { get; set; }
        public string RiskRating { get; set; }
        public string EscalationLevel { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region Likelihood
    public class ReqLikelihood
    {
        public string LikelihoodID { get; set; }
        public string QuarterID { get; set; }
        public string LikelihoodCode { get; set; }
        public string LikelihoodName { get; set; }
        public string DelFlag { get; set; }
        public List<ReqLikelihoodItem> sReqLikelihoodItem { get; set; }
    }

    public class ReqLikelihoodItem
    {
        public string LikelihoodItemID { get; set; }
        public string LikelihoodItemCode { get; set; }
        public string LikelihoodItemName { get; set; }
        public string Description { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResLikelihood
    {
        public string LikelihoodID { get; set; }
        public string QuarterID { get; set; }
        public string LikelihoodCode { get; set; }
        public string LikelihoodName { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public List<ResLikelihoodItem> sResLikelihoodItem { get; set; }
    }

    public class ResLikelihoodItem
    {
        public string LikelihoodItemID { get; set; }
        public string LikelihoodID { get; set; }
        public string QuarterID { get; set; }
        public string LikelihoodItemCode { get; set; }
        public string LikelihoodItemName { get; set; }
        public string Description { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region ImpactCategory
    public class ReqImpactCate
    {
        public string ImpactCateID { get; set; }
        public string QuarterID { get; set; }
        public string ImpactCateCode { get; set; }
        public string ImpactCateName { get; set; }
        public string DelFlag { get; set; }
        public List<ReqImpactCateItem> sReqImpactCateItem { get; set; }
    }

    public class ReqImpactCateItem
    {
        public string ImpactCateItemID { get; set; }
        public string ImpactCateItemCode { get; set; }
        public string ImpactCateItemName { get; set; }
        public string Description { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResImpactCate
    {
        public string ImpactCateID { get; set; }
        public string QuarterID { get; set; }
        public string ImpactCateCode { get; set; }
        public string ImpactCateName { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public List<ResImpactCateItem> sResImpactCateItem { get; set; }
    }

    public class ResImpactCateItem
    {
        public string ImpactCateItemID { get; set; }
        public string ImpactCateID { get; set; }
        public string QuarterID { get; set; }
        public string ImpactCateItemCode { get; set; }
        public string ImpactCateItemName { get; set; }
        public string Description { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region PerManagement
    public class ReqPerManagement
    {
        public string GroupID { get; set; }
        public string GroupName { get; set; }
        public string SearchBox { get; set; }
        public string PermissionLevel { get; set; }
        public string DelFlag { get; set; }
        public List<ReqPerManagementItem> sReqPerManagementItem { get; set; }
    }

    public class ReqPerManagementItem
    {
        public string GroupItemID { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string SearchBox { get; set; }
        public string Email { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResPerManagement
    {
        public string GroupID { get; set; }
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public string PermissionLevel { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string Email { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public List<ResPerManagementItem> sResPerManagementItem { get; set; }
    }

    public class ResPerManagementItem
    {
        public string GroupItemID { get; set; }
        public string GroupName { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string Email { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region TopMenu
    public class ReqTopMenu
    {
        public string EmpID { get; set; }
        public string MenuID { get; set; }
        public string MenuCode { get; set; }
        public string MenuName { get; set; }
        public string Parents { get; set; }
        public string Link { get; set; }
        public string FlagTag { get; set; }
        public string OrderBy { get; set; }
        public string[] PermissionGroup { get; set; }
        public string MenuIcon { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResTopMenu
    {
        public string MenuID { get; set; }
        public string MenuCode { get; set; }
        public string MenuName { get; set; }
        public string Parents { get; set; }
        public string ParentsName { get; set; }
        public string Link { get; set; }
        public string FlagTag { get; set; }
        public string OrderBy { get; set; }
        public string PermissionGroup { get; set; }
        public string[] arrPermissionGroup { get; set; }
        public string MenuIcon { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public List<ResTopMenuItem> sResTopMenuItem { get; set; }
    }

    public class ResTopMenuItem
    {
        public string MenuID { get; set; }
        public string MenuCode { get; set; }
        public string MenuName { get; set; }
        public string Parents { get; set; }
        public string ParentsName { get; set; }
        public string Link { get; set; }
        public string FlagTag { get; set; }
        public string OrderBy { get; set; }
        public string PermissionGroup { get; set; }
        public string[] arrPermissionGroup { get; set; }
        public string MenuIcon { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }

    }
    #endregion

    #region Instruction
    public class ReqInstruction
    {
        public string InstructionID { get; set; }
        public string Area { get; set; }
        public string Page { get; set; }
        public string Field { get; set; }
        public string InstructionName { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResInstruction
    {
        public string InstructionID { get; set; }
        public string Page { get; set; }
        public string Field { get; set; }
        public string Area { get; set; }
        public string InstructionName { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region ContactUs
    public class ReqContactUs
    {
        public string ContactID { get; set; }
        public string ContactName { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PicPath { get; set; }
        public string FirstRow { get; set; }
        public string[] GroupMap { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResContactUs
    {
        public string ContactID { get; set; }
        public string ContactName { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PicPath { get; set; }
        public string GroupMap { get; set; }
        public string FirstRow { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public string[] arrGroupMap { get; set; }
        public List<UploadFileModel> itemAttSeq1 { get; set; }
        public List<UploadFileModel> itemAttSeq2 { get; set; }
    }
    #endregion

    #region Banner
    public class ReqBanner
    {
        public string BannerId { get; set; }
        public string BannerName { get; set; }
        public string BusinessId { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResBanner
    {
        public string BannerId { get; set; }
        public string BannerName { get; set; }
        public string BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public List<UploadFileModel> itemAttSeq1 { get; set; }
        public List<UploadFileModel> itemAttSeq2 { get; set; }
    }
    #endregion

    #region CorpTarget
    public class ReqCorpTarget
    {
        public string CorpTargetID { get; set; }
        public string CorpTargetCode { get; set; }
        public string CorpTargetName { get; set; }
        public string CorpTargetYear { get; set; }
        public string DelFlag { get; set; }
        public List<ReqCorpTargetItem> sReqCorpTargetItem { get; set; }
    }

    public class ReqCorpTargetItem
    {
        public string CorpTargetItemID { get; set; }
        public string CorpTargetItemCode { get; set; }
        public string CorpTargetItemName { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResCorpTarget
    {
        public string CorpTargetID { get; set; }
        public string CorpTargetCode { get; set; }
        public string CorpTargetName { get; set; }
        public string CorpTargetYear { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public List<ResCorpTargetItem> sResCorpTargetItem { get; set; }
    }

    public class ResCorpTargetItem
    {
        public string CorpTargetItemID { get; set; }
        public string CorpTargetItemCode { get; set; }
        public string CorpTargetItemName { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region RiskCatalog
    public class ReqRiskCatalog
    {
        public string RiskCatalogID { get; set; }
        public string RiskCatalogTitle { get; set; }
        public string RiskCatalogDesc { get; set; }
        public string DelFlag { get; set; }
    }

    public class ResRiskCatalog
    {
        public string RiskCatalogID { get; set; }
        public string RiskCatalogTitle { get; set; }
        public string RiskCatalogDesc { get; set; }
        public string CreateBy { get; set; }
        public string CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDateTime { get; set; }
        public string DelFlag { get; set; }
        public List<UploadFileModel> itemAttSeq1 { get; set; }
        public List<UploadFileModel> itemAttSeq2 { get; set; }
    }
    #endregion

    #region Master Asset
    public class ReqMaster_Assset
    {
        public string Asset_Id { get; set; }
        public string QuarterYear { get; set; }
        public string QuarterID { get; set; }
        public string Asset_Name { get; set; }
        public string Asset_Short { get; set; }
        public string Asset_Code { get; set; }
        public string Asset_Coordinators { get; set; }
        public string Asset_Level { get; set; }
        public string Asset_Org { get; set; }
        public string ActveDate { get; set; }
        public string DelFlag { get; set; }
    }
    public class Master_Assset
    {
        public string Asset_Id { get; set; }
        public string QuarterYear { get; set; }
        public string QuarterID { get; set; }
        public string Asset_Name { get; set; }
        public string Asset_Short { get; set; }
        public string Asset_Code { get; set; }
        public string Asset_Coordinators { get; set; }
        public string Asset_Coordinators_EName { get; set; }
        public string Asset_Level { get; set; }
        public string Asset_Org { get; set; }
        public string DelFlag { get; set; }
    }

    public class Master_Department
    {
        public string DeptCode { get; set; }
        public string Abbreviation { get; set; }
        public string DeptLevel { get; set; }
        public string Parents { get; set; }
        public string HeadID { get; set; }
    }
    #endregion

    #region Master WPB
    public class ReqMaster_WPB
    {
        public string WpbId { get; set; }
        public string Year { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string DelFlag { get; set; }
    }
    public class Master_WPB
    {
        public string WpbId { get; set; }
        public string wpbCode { get; set; }
        public string Year { get; set; }
        public string StartDate { get; set; }
        public string StartDateText { get; set; }
        public string EndDate { get; set; }
        public string EndDateText { get; set; }
        public string CreateBy { get; set; }
        public string CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateDate { get; set; }
        public string DelFlag { get; set; }
    }
    #endregion

    #region Master CO
    public class Master_CO
    {
        public string CoordinatorId { get; set; }
        public string QuarterYear { get; set; }
        public string QuarterID { get; set; }
        public string Coordinator_Employee_Id { get; set; }
        public string Coordinator_EName { get; set; }
        public string Coordinator_Department_Id { get; set; }
        public string Coordinator_Level { get; set; }
    }
    #endregion
    #region Owner
    public class Master_Owner
    {
        public string Emp_Id { get; set; }
        public string Dept_Id { get; set; }
        public string Dept_Name { get; set; }
        public string Dept_Short { get; set; }
        public string Emp_FullName { get; set; }
        public string Emp_Email { get; set; }
    }
    #endregion
    #region Master ERM
    public class Master_ERM
    {
        public string EmpCode { get; set; }
    }


    #endregion

    #region Master ReplaceCordinator
    public class Master_ReplaceCordinator
    {
        public string Risk_Business_Unit { get; set; }
        public string Co_Old { get; set; }
        public string Co_New { get; set; }
        public string Type_Co { get; set; }
    }
    #endregion
    public class DropDownList_Master
    {
        public string value { set; get; }
        public string text { get; set; }
        public string text1 { get; set; }
        public string text2 { get; set; }
        public string text3 { get; set; }
        public string text4 { get; set; }
        public string text5 { get; set; }
        public string text6 { get; set; }

        public string Module { set; get; }
        public string TextSearch1 { set; get; }
        public string TextSearch2 { set; get; }
        public string TextSearch3 { set; get; }
        public string TextSearch4 { set; get; }
        public string TextSearch5 { set; get; }
        public string TextSearch6 { set; get; }
        public string TextSearch7 { set; get; }
    }

    public class UploadFileModel
    {
        public string SessionEmpID { get; set; }
        public string AttachFileID { get; set; }
        public string Form { get; set; }
        public string ReqId { get; set; }
        public string SeqNo { get; set; }
        public string FileName { get; set; }
        public string RootPath { get; set; }
        public string PathFile { get; set; }
        public string DelFlag { get; set; }
    }
}
