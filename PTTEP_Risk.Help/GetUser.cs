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

namespace PTTEP_Risk.Help
{
    public class GetUser
    {
        Helper _helper = new Helper();
        ConfigurationService _c = new ConfigurationService();

        public List<UserEmployee> SearchEmployee(List<KeyValuePair<string, string>> Params)
        {
            List<UserEmployee> userModel = new List<UserEmployee>();
            UserEmployee user = new UserEmployee();
            List<ServiceModel> servicesModel = _c.ConnectionService();

            try
            {
                IRestResponse response = _helper.CallService(servicesModel[0].SearchEmployee, Params);
                XDocument doc = XDocument.Parse(response.Content);
                XNamespace ns = "http://v1.workflow2009.pttep.com/";
                IEnumerable<XElement> responses = doc.Descendants(ns + "Employees_MastersDATA");
                foreach (XElement itemH in responses)
                {
                    
                    user.EMPLOYEE_ID = (string)itemH.Element(ns + "EMPLOYEE_ID");
                    user.FIRSTNAME = (string)itemH.Element(ns + "FIRSTNAME");
                    user.LASTNAME = (string)itemH.Element(ns + "LastName");
                    user.THAINAME = (string)itemH.Element(ns + "THAINAME");
                    user.POSITION = (string)itemH.Element(ns + "POSITION");
                    user.SECTION_ID = (string)itemH.Element(ns + "SECTION_ID");
                    user.DEPARTMENT_ID = (string)itemH.Element(ns + "DEPARTMENT_ID");
                    user.DIVISION_ID = (string)itemH.Element(ns + "DIVISION_ID");
                    user.Groupdivision_ID = (string)itemH.Element(ns + "Groupdivision_ID");
                    user.ORGUNIT = (string)itemH.Element(ns + "ORGUNIT");
                    user.EMAIL_ID = (string)itemH.Element(ns + "EMAIL_ID");
                    user.GROUP = (string)itemH.Element(ns + "GROUP");
                    IEnumerable<XElement> itemHD = itemH.Descendants(ns + "DepartmentInfo");
                    if (itemHD.Count() > 0)
                    {
                        List<DepartmentInfo> departmentModel = new List<DepartmentInfo>();
                        foreach (var iitemHD in itemHD)
                        {
                            DepartmentInfo department = new DepartmentInfo();
                            department.ORGANIZATION_ID = (string)iitemHD.Element(ns + "ORGANIZATION_ID");
                            department.NAME = (string)iitemHD.Element(ns + "NAME");
                            department.ABBREVIATION = (string)iitemHD.Element(ns + "ABBREVIATION");
                            department.ORGANIZATION_LEVEL = (string)iitemHD.Element(ns + "ORGANIZATION_LEVEL");
                            department.CODE = (string)iitemHD.Element(ns + "CODE");
                            IEnumerable<XElement> itemHDA = iitemHD.Descendants(ns + "HeadActing");
                            if (itemHDA.Count() > 0)
                            {
                                List<HeadActing> HeadModel = new List<HeadActing>();
                                foreach (var iitemHDA in itemHDA)
                                {
                                    HeadActing head = new HeadActing();
                                    head.ORGANIZATION_ID = (string)iitemHDA.Element(ns + "ORGANIZATION_ID");
                                    head.HEAD_ID = (string)iitemHDA.Element(ns + "HEAD_ID");
                                    head.HeadEmail = (string)iitemHDA.Element(ns + "HeadEmail");
                                    HeadModel.Add(head);
                                    department.HeadActing = HeadModel;
                                }  
                            }
                            departmentModel.Add(department);
                            user.DepartmentInfo = departmentModel;
                        }
                    }
                    IEnumerable<XElement> itemHDI = itemH.Descendants(ns + "DivisionInfo");
                    if (itemHDI.Count() > 0)
                    {
                        List<DivisionInfo> divisionModel = new List<DivisionInfo>();
                        foreach (var iitemHD in itemHDI)
                        {
                            DivisionInfo division = new DivisionInfo();
                            division.ORGANIZATION_ID = (string)iitemHD.Element(ns + "ORGANIZATION_ID");
                            division.NAME = (string)iitemHD.Element(ns + "NAME");
                            division.ABBREVIATION = (string)iitemHD.Element(ns + "ABBREVIATION");
                            division.ORGANIZATION_LEVEL = (string)iitemHD.Element(ns + "ORGANIZATION_LEVEL");
                            division.CODE = (string)iitemHD.Element(ns + "CODE");
                            IEnumerable<XElement> itemHDIA = iitemHD.Descendants(ns + "HeadActing");
                            if (itemHDIA.Count() > 0)
                            {
                                List<HeadActing> HeadModel = new List<HeadActing>();
                                foreach (var iitemHDA in itemHDIA)
                                {
                                    HeadActing head = new HeadActing();
                                    head.ORGANIZATION_ID = (string)iitemHDA.Element(ns + "ORGANIZATION_ID");
                                    head.HEAD_ID = (string)iitemHDA.Element(ns + "HEAD_ID");
                                    head.HeadEmail = (string)iitemHDA.Element(ns + "HeadEmail");
                                    HeadModel.Add(head);
                                    division.HeadActing = HeadModel;
                                }
                            }
                            divisionModel.Add(division);
                            user.DivisionInfo = divisionModel;
                        }
                    }
                    IEnumerable<XElement> itemHG = itemH.Descendants(ns + "GroupDivisionInfo");
                    if (itemHG.Count() > 0)
                    {
                        List<GroupDivisionInfo> groupDivisionModel = new List<GroupDivisionInfo>();
                        foreach (var iitemHG in itemHG)
                        {
                            GroupDivisionInfo groupDivision = new GroupDivisionInfo();
                            groupDivision.ORGANIZATION_ID = (string)iitemHG.Element(ns + "ORGANIZATION_ID");
                            groupDivision.NAME = (string)iitemHG.Element(ns + "NAME");
                            groupDivision.ABBREVIATION = (string)iitemHG.Element(ns + "ABBREVIATION");
                            groupDivision.ORGANIZATION_LEVEL = (string)iitemHG.Element(ns + "ORGANIZATION_LEVEL");
                            groupDivision.CODE = (string)iitemHG.Element(ns + "CODE");
                            IEnumerable<XElement> itemHDG = iitemHG.Descendants(ns + "HeadActing");
                            if (itemHDG.Count() > 0)
                            {
                                List<HeadActing> HeadModel = new List<HeadActing>();
                                foreach (var iitemHGA in itemHDG)
                                {
                                    HeadActing head = new HeadActing();
                                    head.ORGANIZATION_ID = (string)iitemHGA.Element(ns + "ORGANIZATION_ID");
                                    head.HEAD_ID = (string)iitemHGA.Element(ns + "HEAD_ID");
                                    head.HeadEmail = (string)iitemHGA.Element(ns + "HeadEmail");
                                    HeadModel.Add(head);
                                    groupDivision.HeadActing = HeadModel;
                                }
                            }
                            groupDivisionModel.Add(groupDivision);
                            user.GroupDivisionInfo = groupDivisionModel;
                        }
                    }
                    userModel.Add(user);
                }
            }
            catch (Exception ex)
            {
            }
            return userModel;
            
        }

        public List<UserEmployee> GetEmployee(List<KeyValuePair<string, string>> Params)
        {
            List<UserEmployee> userModel = new List<UserEmployee>();
            UserEmployee user = new UserEmployee();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            try
            {
                var Email = Params.First(kvp => kvp.Key == "EmailAddress").Value;
                if (Email.Length > 0)
                {
                    if (Email.Contains('@'))
                    {
                        var indexchar = Email.IndexOf('@');
                        var splEmail = Email.Substring(0, Email.Length - (Email.Length - indexchar));
                        //Get User From Service
                        var paramsEmp = new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string, string>("LogonName", splEmail),
                            new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                        };
                        IRestResponse response = _helper.CallService(servicesModel[0].GetEmployee, paramsEmp);
                        XDocument doc = XDocument.Parse(response.Content);
                        XNamespace ns = "http://v1.workflow2009.pttep.com/";
                        IEnumerable<XElement> responses = doc.Descendants(ns + "Employees_MastersDATA");
                        foreach (XElement itemH in responses)
                        {

                            user.EMPLOYEE_ID = (string)itemH.Element(ns + "EMPLOYEE_ID");
                            user.FIRSTNAME = (string)itemH.Element(ns + "FIRSTNAME");
                            user.LASTNAME = (string)itemH.Element(ns + "LastName");
                            user.THAINAME = (string)itemH.Element(ns + "THAINAME");
                            user.POSITION = (string)itemH.Element(ns + "POSITION");
                            user.SECTION_ID = (string)itemH.Element(ns + "SECTION_ID");
                            user.DEPARTMENT_ID = (string)itemH.Element(ns + "DEPARTMENT_ID");
                            user.DIVISION_ID = (string)itemH.Element(ns + "DIVISION_ID");
                            user.Groupdivision_ID = (string)itemH.Element(ns + "Groupdivision_ID");
                            user.ORGUNIT = (string)itemH.Element(ns + "ORGUNIT");
                            user.EMAIL_ID = (string)itemH.Element(ns + "EMAIL_ID");
                            user.GROUP = (string)itemH.Element(ns + "GROUP");
                            IEnumerable<XElement> itemHD = itemH.Descendants(ns + "DepartmentInfo");
                            if (itemHD.Count() > 0)
                            {
                                List<DepartmentInfo> departmentModel = new List<DepartmentInfo>();
                                foreach (var iitemHD in itemHD)
                                {
                                    DepartmentInfo department = new DepartmentInfo();
                                    department.ORGANIZATION_ID = (string)iitemHD.Element(ns + "ORGANIZATION_ID");
                                    department.NAME = (string)iitemHD.Element(ns + "NAME");
                                    department.ABBREVIATION = (string)iitemHD.Element(ns + "ABBREVIATION");
                                    department.ORGANIZATION_LEVEL = (string)iitemHD.Element(ns + "ORGANIZATION_LEVEL");
                                    department.CODE = (string)iitemHD.Element(ns + "CODE");
                                    IEnumerable<XElement> itemHDA = iitemHD.Descendants(ns + "HeadActing");
                                    if (itemHDA.Count() > 0)
                                    {
                                        List<HeadActing> HeadModel = new List<HeadActing>();
                                        foreach (var iitemHDA in itemHDA)
                                        {
                                            HeadActing head = new HeadActing();
                                            head.ORGANIZATION_ID = (string)iitemHDA.Element(ns + "ORGANIZATION_ID");
                                            head.HEAD_ID = (string)iitemHDA.Element(ns + "HEAD_ID");
                                            head.HeadEmail = (string)iitemHDA.Element(ns + "HeadEmail");
                                            HeadModel.Add(head);
                                            department.HeadActing = HeadModel;
                                        }
                                    }
                                    departmentModel.Add(department);
                                    user.DepartmentInfo = departmentModel;
                                }
                            }
                            IEnumerable<XElement> itemHDI = itemH.Descendants(ns + "DivisionInfo");
                            if (itemHDI.Count() > 0)
                            {
                                List<DivisionInfo> divisionModel = new List<DivisionInfo>();
                                foreach (var iitemHD in itemHDI)
                                {
                                    DivisionInfo division = new DivisionInfo();
                                    division.ORGANIZATION_ID = (string)iitemHD.Element(ns + "ORGANIZATION_ID");
                                    division.NAME = (string)iitemHD.Element(ns + "NAME");
                                    division.ABBREVIATION = (string)iitemHD.Element(ns + "ABBREVIATION");
                                    division.ORGANIZATION_LEVEL = (string)iitemHD.Element(ns + "ORGANIZATION_LEVEL");
                                    division.CODE = (string)iitemHD.Element(ns + "CODE");
                                    IEnumerable<XElement> itemHDIA = iitemHD.Descendants(ns + "HeadActing");
                                    if (itemHDIA.Count() > 0)
                                    {
                                        List<HeadActing> HeadModel = new List<HeadActing>();
                                        foreach (var iitemHDA in itemHDIA)
                                        {
                                            HeadActing head = new HeadActing();
                                            head.ORGANIZATION_ID = (string)iitemHDA.Element(ns + "ORGANIZATION_ID");
                                            head.HEAD_ID = (string)iitemHDA.Element(ns + "HEAD_ID");
                                            head.HeadEmail = (string)iitemHDA.Element(ns + "HeadEmail");
                                            HeadModel.Add(head);
                                            division.HeadActing = HeadModel;
                                        }
                                    }
                                    divisionModel.Add(division);
                                    user.DivisionInfo = divisionModel;
                                }
                            }
                            IEnumerable<XElement> itemHG = itemH.Descendants(ns + "GroupDivisionInfo");
                            if (itemHG.Count() > 0)
                            {
                                List<GroupDivisionInfo> groupDivisionModel = new List<GroupDivisionInfo>();
                                foreach (var iitemHG in itemHG)
                                {
                                    GroupDivisionInfo groupDivision = new GroupDivisionInfo();
                                    groupDivision.ORGANIZATION_ID = (string)iitemHG.Element(ns + "ORGANIZATION_ID");
                                    groupDivision.NAME = (string)iitemHG.Element(ns + "NAME");
                                    groupDivision.ABBREVIATION = (string)iitemHG.Element(ns + "ABBREVIATION");
                                    groupDivision.ORGANIZATION_LEVEL = (string)iitemHG.Element(ns + "ORGANIZATION_LEVEL");
                                    groupDivision.CODE = (string)iitemHG.Element(ns + "CODE");
                                    IEnumerable<XElement> itemHDG = iitemHG.Descendants(ns + "HeadActing");
                                    if (itemHDG.Count() > 0)
                                    {
                                        List<HeadActing> HeadModel = new List<HeadActing>();
                                        foreach (var iitemHGA in itemHDG)
                                        {
                                            HeadActing head = new HeadActing();
                                            head.ORGANIZATION_ID = (string)iitemHGA.Element(ns + "ORGANIZATION_ID");
                                            head.HEAD_ID = (string)iitemHGA.Element(ns + "HEAD_ID");
                                            head.HeadEmail = (string)iitemHGA.Element(ns + "HeadEmail");
                                            HeadModel.Add(head);
                                            groupDivision.HeadActing = HeadModel;
                                        }
                                    }
                                    groupDivisionModel.Add(groupDivision);
                                    user.GroupDivisionInfo = groupDivisionModel;
                                }
                            }
                            userModel.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return userModel;

        }

        public void addOrReplace<T, U>(List<KeyValuePair<T, U>> list, KeyValuePair<T, U> item)
        where T : IEquatable<T>
        {
            var target_idx = list.FindIndex(n => n.Key.Equals(item.Key));

            if (target_idx != -1)
            {
                list[target_idx] = item;
            }
            else
            {
                list.Add(item);
            }
        }
    }
}
