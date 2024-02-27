using System;
using System.Collections.Generic;
using System.Text;
using PTTEP_Risk.Model;

namespace PTTEP_Risk.Model
{

    public class UserAuthLogin
    {
        public string SessionEmpId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public string Email { get; set; }
        public string SecurityCode { get; set; }
        public int SystemId { get; set; }
        public int StatusId { get; set; }
    }

    public class UserAuthSession
    {
        public string Employee_Id { get; set; }
        public string E_FirstName { get; set; }
        public string E_LastName { get; set; }
        public string T_FirstName { get; set; }
        public string T_LastName { get; set; }
        public string Email { get; set; }
        public string E_Position { get; set; }
        public string T_Position { get; set; }
        public string Section_Id { get; set; }
        public string Department_Id { get; set; }
        public string Division_Id { get; set; }
        public string Group_Division { get; set; }
        public string Organization_Id { get; set; }
        public string StatusId { get; set; }
        public string expireDate { get; set; }
        public string GroupID { get; set; }
        public string GroupName { get; set; }
        public string PermissionLevel { get; set; }
    }

    public class UserAuthChangePassword
    {
        public int EmpID { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string IPAddress { get; set; }
        public int SystemId { get; set; }

    }

    public class UserEmployee
    {
        public string EMPLOYEE_ID { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string THAINAME { get; set; }
        public string POSITION { get; set; }
        public string SECTION_ID { get; set; }
        public string DEPARTMENT_ID { get; set; }
        public string DIVISION_ID { get; set; }
        public string Groupdivision_ID { get; set; }
        public string ORGUNIT { get; set; }
        public string EMAIL_ID { get; set; }
        public string GROUP { get; set; }
        public string D_Group_ID { get; set; }
        public string D_GROUP { get; set; }
        public string D_PERMISSSION_LEVEL { get; set; }
        public string S_PERMISSSION_LEVEL { get; set; }
        public string Error { get; set; }
        public List<DepartmentInfo> DepartmentInfo { get; set; }
        public List<DivisionInfo> DivisionInfo { get; set; }
        public List<GroupDivisionInfo> GroupDivisionInfo { get; set; }
        public List<Organizations> ChildOrganizationInfo { get; set; }
        public List<Asset> AssetInfo { get; set; }
        public List<Co> CoInfo { get; set; }
        public List<Organizations> OwnerInfo { get; set; }
        public List<ERM> ERMInfo { get; set; }
        public List<Acting> HeadActing { get; set; }
    }
    public class UserDummy
    {
        public string Dummy_Id { get; set; }
        public string E_FirstName { get; set; }
        public string E_LastName { get; set; }
        public string T_FirstName { get; set; }
        public string T_LastName { get; set; }
        public string Email { get; set; }
    }

    /*public class Organization
    {
        public string ORGANIZATION_ID { get; set; }
        public string NAME { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
        public List<HeadActing> HeadActing { get; set; }
    }*/

    public class Asset
    {
        public string Asset_Code { get; set; }
        public string Asset_Name { get; set; }
        public string Asset_Short { get; set; }
        public string Asset_Level { get; set; }
        public string Asset_Org { get; set; }
        public string Asset_Coordinators { get; set; }
    }

    public class Co
    {
        public string ORGANIZATION_ID { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_NANE { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
        public string Coordinator_Employee_Id { get; set; }
    }
    public class ERM
    {
        public string ORGANIZATION_ID { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_NANE { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
    }

    public class DepartmentInfo
    {
        public string ORGANIZATION_ID { get; set; }
        public string NAME { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
        public string CODE { get; set; }

        public List<HeadActing> HeadActing { get; set; }
    }

    public class DivisionInfo
    {
        public string ORGANIZATION_ID { get; set; }
        public string NAME { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
        public string CODE { get; set; }
        public List<HeadActing> HeadActing { get; set; }
    }

    public class GroupDivisionInfo
    {
        public string ORGANIZATION_ID { get; set; }
        public string NAME { get; set; }
        public string ABBREVIATION { get; set; }
        public string ORGANIZATION_LEVEL { get; set; }
        public string CODE { get; set; }
        public List<HeadActing> HeadActing { get; set; }
    }

    public class HeadActing
    {
        public string ORGANIZATION_ID { get; set; }
        public string HEAD_ID { get; set; }
        public string HeadEmail { get; set; }
    }
    public class Acting
    {
        public string DeptCode { get; set; }
        public string Abbreviation { get; set; }
        public string DeptName { get; set; }
        public string DeptLevel { get; set; }
    }

    public class UserAuthCutomer
    {
        public string EmpID { get; set; }
        public string UserTypeID { get; set; }
        public string pDefAppID { get; set; }
        public string pDefAppName { get; set; }
        public string pDefCostID { get; set; }
        public string pDefCostName { get; set; }
        public string Fname { get; set; }
        public string pTel { get; set; }
        public string pFax { get; set; }
        public string pLocation { get; set; }
        public string pEmail { get; set; }
        public string pCCEmail { get; set; }
        public string pRemark { get; set; }
        public string pActive { get; set; }

        public bool service_add { get; set; }
        public bool service_search { get; set; }
        public bool service_return { get; set; }
        public bool service_insert { get; set; }
        public bool service_destroy { get; set; }
        public bool service_withdraw { get; set; }
        public bool service_buyProduct { get; set; }
        public bool approve_add { get; set; }
        public bool approve_search { get; set; }
        public bool approve_return { get; set; }
        public bool approve_insert { get; set; }
        public bool approve_destroy { get; set; }
        public bool approve_withdraw { get; set; }
        public bool approve_buyProduct { get; set; }
        public bool other_editDocDetail { get; set; }
        public bool other_viewReport { get; set; }
    }

    public class UserAuthCutomerCostCenter
    {
        public string EmpID { get; set; }
        public string pCostCenterID { get; set; }
        public string pDeptID { get; set; }
        public string pCusID { get; set; }
        public string pMode { get; set; }
        //public string DeFlag { get; set; }
    }

    public class UserAuthCutomerApprove
    {
        public string EmpID { get; set; }
        public string pApprover { get; set; }
        public string pMode { get; set; }
        //public string DeFlag { get; set; }

    }

    public class UserAuthNew
    {
        public string EmpID { get; set; }
        public string EmpCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string UserTypeID { get; set; }
        public string ActiveID { get; set; }
        public string RightsTypeID { get; set; }
        public int ID_Area { get; set; }
        public UserAuthNewTeam[] itemsTeam { get; set; }
        public UserAuthNewRight[] itemsRight { get; set; }

    }

    public class UserAuthNewTeam
    {
        public string TeamID { get; set; }
        public string TeamName { get; set; }
    }

    public class UserAuthNewRight
    {
        public string UserSystemID { get; set; }
        public string UserSystemName { get; set; }
        public string RightID { get; set; }
        public string RightName { get; set; }
    }

    public class UserGetlist
    {
        public string pEmpID { get; set; }
        public string pCusID { get; set; }
        public string pSystemID { get; set; }
        public string pSystemTypeID { get; set; }
        public string EmpName { get; set; }
        public string Username { get; set; }
        public string UserTypeID { get; set; }
        public string ActiveID { get; set; }
        public string Module { get; set; }
    }

    public class UserGetGridlist
    {
        public long No { get; set; }
        public string EmpID { get; set; }
        public string EmpCode { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Fullname { get; set; }
        public string Authen { get; set; }
        public string CostCenter { get; set; }
        public string DeptID { get; set; }
        public string UserTypeID { get; set; }
        public string TypeDescription { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int ID_Area { get; set; }
        public string AreaName { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public byte Password { get; set; }
        public string CreateDate { get; set; }
        public string ExpireDate { get; set; }
        public string CustomerName { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterIFZ { get; set; }
        public string ActiveID { get; set; }
        public string ActiveStatus { get; set; }
        public int TotalCount { get; set; }
    }

    public class UserMenuGetlist
    {
        public int MenuId { get; set; }
        public string ParentId { get; set; }
        public string MenuName { get; set; }
        public string IsMaster { get; set; }
        public string Handler { get; set; }
        public string OrderRank { get; set; }
    }

    public class UserChangePassword
    {
        public int EmpID { get; set; }
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string Module { get; set; }
    }

    public class test
    {
        public string test1 { get; set; }
        public string test2 { get; set; }
        public string test3 { get; set; }
    }
}
