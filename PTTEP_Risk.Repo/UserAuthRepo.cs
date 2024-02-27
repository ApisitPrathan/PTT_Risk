using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PTTEP_Risk.Help;
using PTTEP_Risk.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using RestSharp;
using System.Xml.Linq;

namespace PTTEP_Risk.Repo
{
    public class UserAuthRepo
    {
        Helper _h = new Helper();
        GetUser _s = new GetUser();
        GetOrganizations _o = new GetOrganizations();
        ConfigurationService _c = new ConfigurationService();
        string error = "Start";
        public ResponseMessage<List<UserEmployee>> API_GetUserlogin(UserAuthLogin request)
        {
            error = "Start GET";
            ResponseMessage<List<UserEmployee>> response = new ResponseMessage<List<UserEmployee>>();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            using (var conn = new SqlConnection(ConnectionStrings.ConnectionStringss()))
            {
                conn.Open();
                try
                {
                    //Get Financial Table
                    var p_FinancialImpact = new DynamicParameters();
                    p_FinancialImpact.Add("@Table_Name", "Master_FinancialImpact");
                    var dataFinancial = conn.Query<ResFinancialImpact>("sp_Get_Table", p_FinancialImpact, commandType: CommandType.StoredProcedure).ToList();

                    //get master asset 
                    var p_masterAsset = new DynamicParameters();
                    p_masterAsset.Add("@Table_Name", "Master_Asset");
                    var mAsset = conn.Query<Master_Assset>("sp_Get_Table", p_masterAsset, commandType: CommandType.StoredProcedure).ToList();
                    //if (dataFinancial.Count > 0)
                    //{
                    #region Get Info User
                    //Get User From Service
                    var paramsEmp = new List<KeyValuePair<string, string>>() {
                        new KeyValuePair<string, string>("EmployeeID", ""),
                        new KeyValuePair<string, string>("FirstName", ""),
                        new KeyValuePair<string, string>("LastName", ""),
                        new KeyValuePair<string, string>("EmailAddress", request.Email),
                        new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                        new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                    };
                    //get information employee xml from web service GetEmployee
                    error = "GET USER";
                    var userInfo = _s.GetEmployee(paramsEmp);
                    if (userInfo.Count > 0)
                    {

                        error = "GetEmployee";
                        userInfo[0].S_PERMISSSION_LEVEL = "Staff";
                        response.Status = true;
                        response.body = userInfo;
                        string org = "";

                        //Check Org Level
                        var p_CheckOrg = new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string, string>("OrganizetionID", userInfo[0].ORGUNIT),
                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                        };
                        var orgInfo = _o.GetOrganization(p_CheckOrg);
                        if (orgInfo.Count > 0)
                        {
                            #region check Section Or BU
                            error = "GetOrganization";
                            if (orgInfo.Count > 0)
                            {
                                if (orgInfo[0].ORGANIZATION_LEVEL == "Section")// is Level Section
                                {
                                    //Get parent Org
                                    var p_ParentOrg = new List<KeyValuePair<string, string>>() {
                                        new KeyValuePair<string, string>("OrganizetionID", orgInfo[0].ORGANIZATION_ID),
                                        new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                                    };
                                    var orgParentInfo = _o.GetParentOrganization(p_ParentOrg);
                                    if (orgParentInfo.Count > 0)
                                    {

                                        //check bu have asset 
                                        var _resultAsset = mAsset.Where(o => o.Asset_Org == orgParentInfo[0].ORGANIZATION_ID).ToList();
                                        if (_resultAsset.Count > 0)
                                        {
                                            List<Asset> _aModal = new List<Asset>();
                                            foreach (var _asset in _resultAsset)
                                            {

                                                //Check Bu have Financial master
                                                var _resultAssetfi = dataFinancial.Where(o => o.BusinessCode == _asset.Asset_Code).ToList();
                                                if (_resultAssetfi.Count > 0)//have financial
                                                {
                                                    Asset _a = new Asset();
                                                    _a.Asset_Code = _asset.Asset_Code;
                                                    _a.Asset_Coordinators = _asset.Asset_Coordinators;
                                                    _a.Asset_Level = _asset.Asset_Level;
                                                    _a.Asset_Name = _asset.Asset_Name;
                                                    _a.Asset_Org = _asset.Asset_Org;
                                                    _a.Asset_Short = _asset.Asset_Short;
                                                    _aModal.Add(_a);
                                                }
                                            }
                                            if (_h.IsNullOrEmpty(userInfo[0].AssetInfo))
                                                userInfo[0].AssetInfo = _aModal;
                                            else
                                                userInfo[0].AssetInfo.AddRange(_aModal);
                                        }
                                        //Check Bu have Financial master
                                        var _result = dataFinancial.Where(o => o.BusinessCode == orgParentInfo[0].ORGANIZATION_ID).ToList();
                                        if (_result.Count > 0)//have financial
                                        {
                                            error = "GetParentOrganization";
                                            //set parent org in have register
                                            userInfo[0].ChildOrganizationInfo = orgParentInfo;
                                            org = orgParentInfo[0].ORGANIZATION_ID;
                                            response.Status = true;
                                        }
                                        else
                                        {
                                            org = orgParentInfo[0].ORGANIZATION_ID;
                                            response.ErrorMessage += "[API_GetUserlogin]" + "BU " + orgParentInfo[0].ORGANIZATION_ID + "is not have financialimpact" + "<br/>";
                                        }
                                    }
                                }
                                else//is Level FG,Division,Department
                                {
                                    //check bu have asset 
                                    var _resultAsset = mAsset.Where(o => o.Asset_Org == orgInfo[0].ORGANIZATION_ID).ToList();
                                    if (_resultAsset.Count > 0)
                                    {
                                        List<Asset> _aModal = new List<Asset>();
                                        foreach (var _asset in _resultAsset)
                                        {

                                            //Check Bu have Financial master
                                            var _resultAssetfi = dataFinancial.Where(o => o.BusinessCode == _asset.Asset_Code).ToList();
                                            if (_resultAssetfi.Count > 0)//have financial
                                            {
                                                Asset _a = new Asset();
                                                _a.Asset_Code = _asset.Asset_Code;
                                                _a.Asset_Coordinators = _asset.Asset_Coordinators;
                                                _a.Asset_Level = _asset.Asset_Level;
                                                _a.Asset_Name = _asset.Asset_Name;
                                                _a.Asset_Org = _asset.Asset_Org;
                                                _a.Asset_Short = _asset.Asset_Short;
                                                _aModal.Add(_a);
                                                response.Status = true;
                                            }
                                        }
                                        if (_h.IsNullOrEmpty(userInfo[0].AssetInfo))
                                            userInfo[0].AssetInfo = _aModal;
                                        else
                                            userInfo[0].AssetInfo.AddRange(_aModal);
                                    }
                                    //Check Bu have Financial master
                                    var _result = dataFinancial.Where(o => o.BusinessCode == orgInfo[0].ORGANIZATION_ID).ToList();
                                    if (_result.Count > 0)
                                    {
                                        //set org in have register
                                        userInfo[0].ChildOrganizationInfo = orgInfo;
                                        org = orgInfo[0].ORGANIZATION_ID;
                                        response.Status = true;
                                    }
                                    else
                                    {
                                        org = orgInfo[0].ORGANIZATION_ID;
                                        response.ErrorMessage += "[API_GetUserlogin]" + "BU " + orgInfo[0].ORGANIZATION_ID + "is not have financialimpact" + "<br/>";
                                    }
                                }
                            }
                            #endregion
                        }
                        #region Check Co and asset
                        //Check Co
                        var p_role = new DynamicParameters();
                        p_role.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                        p_role.Add("@Type", "Check");
                        p_role.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        //Check User request is CO

                        //check co from table Master_Cordinator
                        var dataCheck = conn.Query<UserEmployee>("sp_Check_Role_Co", p_role, commandType: CommandType.StoredProcedure).ToList();
                        if (dataCheck.Count > 0) // is Co
                        {
                            error = "Check sp_Check_Role_Co";
                            response.Status = true;
                            if (!_h.CheckNull(userInfo[0].S_PERMISSSION_LEVEL))
                            {
                                userInfo[0].S_PERMISSSION_LEVEL = userInfo[0].S_PERMISSSION_LEVEL + "," + dataCheck[0].S_PERMISSSION_LEVEL;
                                var p_co = new DynamicParameters();
                                p_co.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                                p_co.Add("@Type", "Get");
                                p_co.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                //check co from table Master_Cordinator
                                var dataCo = conn.Query<Co>("sp_Check_Role_Co", p_co, commandType: CommandType.StoredProcedure).ToList();
                                if (dataCo.Count > 0)
                                {
                                    List<Co> _tempCo = new List<Co>();
                                    //loop multiple bu
                                    foreach (var co in dataCo)
                                    {
                                        //Check Bu have Financial master
                                        var _result = dataFinancial.Where(o => o.BusinessCode == co.ORGANIZATION_ID).ToList();
                                        if (_result.Count > 0)
                                        {
                                            error = "GET sp_Check_Role_Co";
                                            _tempCo.Add(co);
                                        }
                                        else
                                        {
                                            response.ErrorMessage += "[API_GetUserlogin]" + "BU " + co.ORGANIZATION_ID + "is not have financialimpact" + "<br/>";
                                        }
                                    }
                                    if (_h.IsNullOrEmpty(userInfo[0].CoInfo))
                                        userInfo[0].CoInfo = _tempCo;
                                    else
                                        userInfo[0].CoInfo.AddRange(_tempCo);
                                }
                            }
                            else
                            {
                                userInfo[0].S_PERMISSSION_LEVEL = dataCheck[0].S_PERMISSSION_LEVEL;
                            }

                            //Get Asset by Co ID
                            var p_asset = new DynamicParameters();
                            p_asset.Add("@Module", "Asset");
                            p_asset.Add("@TextSearch1", "CO");
                            p_asset.Add("@TextSearch2", userInfo[0].EMPLOYEE_ID);
                            var dataAsset = conn.Query<Asset>("sp_Get_DDL", p_asset, commandType: CommandType.StoredProcedure).ToList();
                            if (dataAsset.Count > 0)
                            {
                                List<Asset> _tempAsset = new List<Asset>();
                                //loop multiple asset
                                foreach (var asset in dataAsset)
                                {
                                    //Check Bu have Financial master
                                    var _result = dataFinancial.Where(o => o.BusinessCode == asset.Asset_Code).ToList();
                                    if (_result.Count > 0)
                                    {
                                        error = "GET ASSET CO sp_Get_DDL";
                                        response.Status = true;
                                        _tempAsset.Add(asset);
                                    }
                                    else
                                    {
                                        response.ErrorMessage += "[API_GetUserlogin]" + "BU " + asset.Asset_Code + "is not have financialimpact" + "<br/>";
                                    }
                                }
                                if (_h.IsNullOrEmpty(userInfo[0].AssetInfo))
                                    userInfo[0].AssetInfo = _tempAsset;
                                else
                                    userInfo[0].AssetInfo.AddRange(_tempAsset);
                            }
                        }
                        else //is Staff Or Ownner
                        {
                            //Get Asset by BU
                            var p_asset = new DynamicParameters();
                            p_asset.Add("@Module", "Asset");
                            p_asset.Add("@TextSearch1", "STAFF");
                            p_asset.Add("@TextSearch2", org);
                            var dataAsset = conn.Query<Asset>("sp_Get_DDL", p_asset, commandType: CommandType.StoredProcedure).ToList();
                            if (dataAsset.Count > 0)
                            {
                                List<Asset> _tempAsset = new List<Asset>();
                                //loop multiple bu
                                foreach (var asset in dataAsset)
                                {
                                    //Check Bu have Financial master
                                    var _result = dataFinancial.Where(o => o.BusinessCode == asset.Asset_Code).ToList();
                                    if (_result.Count > 0)
                                    {
                                        error = "GET ASSET Staff Or Ownner OR sp_Get_DDL";
                                        response.Status = true;
                                        _tempAsset.Add(asset);
                                    }
                                    else
                                    {
                                        response.ErrorMessage += "[API_GetUserlogin]" + "BU " + asset.Asset_Code + "is not have financialimpact" + "<br/>";
                                    }
                                }
                                if (_h.IsNullOrEmpty(userInfo[0].AssetInfo))
                                    userInfo[0].AssetInfo = _tempAsset;
                                else
                                    userInfo[0].AssetInfo.AddRange(_tempAsset);
                            }
                        }
                        #endregion

                        #region Check Owner and acting
                        if (orgInfo.Count > 0)
                        {
                            error = "CHECK OWNER";
                            //Check Owner Emp Id
                            var p_CheckActingEmpId = new List<KeyValuePair<string, string>>() {
                                new KeyValuePair<string, string>("EmployeeID", userInfo[0].EMPLOYEE_ID),
                                new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                            };
                            var empOwnerActing = _o.GetActingOrganization(p_CheckActingEmpId);
                            if (empOwnerActing.Count > 0)
                            {
                                #region Check Owner
                                List<Organizations> _tempOwner = new List<Organizations>();
                                //loop multiple Owner
                                foreach (var owner in empOwnerActing)
                                {
                                    if (owner.ORGANIZATION_LEVEL != "Section")
                                    {
                                        if (owner.ORGANIZATION_LEVEL == "Department")// is Owner - Bu
                                        {
                                            response.Status = true;
                                            if (!_h.CheckNull(userInfo[0].S_PERMISSSION_LEVEL))
                                                userInfo[0].S_PERMISSSION_LEVEL = userInfo[0].S_PERMISSSION_LEVEL + "," + "Owner-BU";
                                            else
                                                userInfo[0].S_PERMISSSION_LEVEL = "Owner-BU";
                                        }
                                        else if (owner.ORGANIZATION_LEVEL == "Division")// is Owner - Division
                                        {
                                            response.Status = true;
                                            if (!_h.CheckNull(userInfo[0].S_PERMISSSION_LEVEL))
                                                userInfo[0].S_PERMISSSION_LEVEL = userInfo[0].S_PERMISSSION_LEVEL + "," + "Owner-DI";
                                            else
                                                userInfo[0].S_PERMISSSION_LEVEL = "Owner-DI";
                                        }
                                        else if (owner.ORGANIZATION_LEVEL == "Group")//is Owner - Fg
                                        {
                                            response.Status = true;
                                            if (!_h.CheckNull(userInfo[0].S_PERMISSSION_LEVEL))
                                                userInfo[0].S_PERMISSSION_LEVEL = userInfo[0].S_PERMISSSION_LEVEL + "," + "Owner-FG";
                                            else
                                                userInfo[0].S_PERMISSSION_LEVEL = "Owner-FG";
                                        }
                                        //add owner to model
                                        _tempOwner.Add(owner);
                                        //}
                                    }
                                }
                                if (_h.IsNullOrEmpty(userInfo[0].OwnerInfo))
                                    userInfo[0].OwnerInfo = _tempOwner;
                                else
                                    userInfo[0].OwnerInfo.AddRange(_tempOwner);
                                #endregion
                                #region Check Acting
                                //Check Acting
                                var p_Check_Acting = new DynamicParameters();
                                p_Check_Acting.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                                p_Check_Acting.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                //Check User request is CO

                                //check co from table Master_Cordinator
                                var headActings = conn.Query<Acting>("sp_Check_Acting", p_Check_Acting, commandType: CommandType.StoredProcedure).ToList();
                                if (headActings.Count > 0) // is Acting
                                {
                                    List<Acting> _tempActing = new List<Acting>();
                                    //loop multiple bu
                                    foreach (var acting in headActings)
                                    {
                                        //Check Bu have Financial master
                                        var _result = dataFinancial.Where(o => o.BusinessCode == acting.DeptCode).ToList();
                                        if (_result.Count > 0)
                                        {
                                            _tempActing.Add(acting);

                                        }
                                        else
                                        {
                                            response.ErrorMessage += "[API_GetUserlogin]" + "BU " + acting.DeptCode + "is not have financialimpact" + "<br/>";
                                        }
                                    }
                                    //have head acting Bu
                                    if (_tempActing.Count > 0)
                                    {
                                        userInfo[0].HeadActing = _tempActing;
                                        if (!_h.CheckNull(userInfo[0].S_PERMISSSION_LEVEL))
                                            userInfo[0].S_PERMISSSION_LEVEL = userInfo[0].S_PERMISSSION_LEVEL + "," + "Owner-Acting";
                                        else
                                            userInfo[0].S_PERMISSSION_LEVEL = "Owner-Acting";

                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion

                        #region check erm
                        //Check ERM
                        var p_Check_ERM = new DynamicParameters();
                        p_Check_ERM.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                        p_Check_ERM.Add("@Type", "Check");
                        p_Check_ERM.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                        //Check User request is CO

                        //check co from table Master_Cordinator
                        var dataCheckERM = conn.Query<UserEmployee>("sp_Check_Role_ERM", p_Check_ERM, commandType: CommandType.StoredProcedure).ToList();
                        if (dataCheckERM.Count > 0)
                        {
                            error = "CHECK ERM";
                            response.Status = true;
                            if (!_h.CheckNull(userInfo[0].S_PERMISSSION_LEVEL))
                            {
                                var p_Erm = new DynamicParameters();
                                p_Erm.Add("@Emp_Id", userInfo[0].EMPLOYEE_ID);
                                p_Erm.Add("@Type", "Get");
                                p_Erm.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                                var dataERM = conn.Query<ERM>("sp_Check_Role_ERM", p_Erm, commandType: CommandType.StoredProcedure).ToList();
                                if (dataERM.Count > 0)
                                {
                                    List<ERM> _tempERM = new List<ERM>();
                                    //loop multiple bu
                                    foreach (var erm in dataERM)
                                    {
                                        //Check Bu have Financial master
                                        var _result = dataFinancial.Where(o => o.BusinessCode == erm.ORGANIZATION_ID).ToList();
                                        if (_result.Count > 0)
                                        {
                                            _tempERM.Add(erm);
                                        }
                                        else
                                        {
                                            response.ErrorMessage += "[API_GetUserlogin]" + "BU " + erm.ORGANIZATION_ID + "is not have financialimpact" + "<br/>";
                                        }
                                    }
                                    userInfo[0].ERMInfo = _tempERM;
                                }
                                userInfo[0].S_PERMISSSION_LEVEL = userInfo[0].S_PERMISSSION_LEVEL + "," + dataCheckERM[0].S_PERMISSSION_LEVEL;
                            }
                            else
                            {
                                userInfo[0].S_PERMISSSION_LEVEL = dataCheckERM[0].S_PERMISSSION_LEVEL;
                            }
                        }
                        #endregion

                    }
                    else//  No User Azure
                    {
                        //Get Dummy Table
                        var p_role = new DynamicParameters();
                        p_role.Add("@Table_Name", "Master_Dummy_User");
                        var dataDummy = conn.Query<UserDummy>("sp_Get_Table", p_role, commandType: CommandType.StoredProcedure).ToList();
                        if (dataDummy.Count > 0) // is Dummy
                        {
                            error = "Check Dummy User";
                            //response.Status = true;
                            var _result = dataDummy.Where(o => o.Email.ToUpper() == request.Email.ToUpper()).ToList();
                            if (_result.Count > 0)//is user Dummy
                            {
                                List<UserEmployee> usersDummy = new List<UserEmployee>();
                                UserEmployee user = new UserEmployee();
                                user.EMPLOYEE_ID = !_h.CheckNull(_result[0].Dummy_Id) ? _result[0].Dummy_Id : "";
                                user.FIRSTNAME = !_h.CheckNull(_result[0].E_FirstName) ? _result[0].E_FirstName : "";
                                user.LASTNAME = !_h.CheckNull(_result[0].E_LastName) ? _result[0].E_LastName : "";
                                user.THAINAME = (!_h.CheckNull(_result[0].T_FirstName) ? _result[0].T_FirstName : "") + " " + (!_h.CheckNull(_result[0].T_LastName) ? _result[0].T_LastName : "");
                                user.EMAIL_ID = !_h.CheckNull(_result[0].Email) ? _result[0].Email : "";
                                usersDummy.Add(user);
                                response.body = usersDummy;
                                response.Status = true;
                            }
                            else//Not user Dummy
                            {
                                response.ErrorMessage = "[API_GetUserlogin]" + "Can't find employee in webservice";
                                response.Status = false;
                            }

                        }
                        else
                        {
                            response.ErrorMessage = "[API_GetUserlogin]" + "Can't find employee in webservice";
                            response.Status = false;
                        }
                    }
                    #endregion
                    //}
                }
                catch (Exception ex)
                {
                    response.Status = false;
                    response.ErrorMessage = "[Check_Role_Employee] " + error + "||" + ex.Message;
                }
            }
            return response;
        }

        public ResponseMessage<object> CheckUserlogin(UserAuthLogin request)
        {
            ResponseMessage<object> response = new ResponseMessage<object>();
            try
            {
                List<ServiceModel> servicesModel = _c.ConnectionService();
                if (!_h.CheckNull(servicesModel[0].PasswordAPI))
                {
                    if (request.Password == servicesModel[0].PasswordAPI)
                        response.Status = true;
                    else
                        response.Status = false;
                }
                else
                {
                    response.Status = false;
                    response.ErrorMessage = "Password in config is null!";
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.ErrorMessage = "[CheckUserlogin]" + ex.Message;
            }

            return response;
        }
    }
}
