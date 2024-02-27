using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

using System.Reflection;
using Dapper;
using PTTEP_Risk.Model;
using RestSharp;

namespace PTTEP_Risk.Help
{
    public class Helper
    {
        
        #region Datatable Function
        public DataTable ToDataTable<T>(List<T> list)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in list)
            {
                for (int i = 0; i < values.Length; i++)
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                table.Rows.Add(values);
            }
            return table;
        }

        public DataTable ImportCSV(string filePath, string fileExt, string SheetName)
        {
            //Upload and save the file  
            //string csvPath = Server.MapPath("~/Files/") + Path.GetFileName(FileUpload1.PostedFile.FileName);
            //FileUpload1.SaveAs(csvPath);

            //string csvData = File.ReadAllText(filePath);

            //Create a DataTable.  
            DataTable dt = new DataTable();

            //dt.Columns.AddRange(new DataColumn[5] {
            //new DataColumn("Id", typeof(string)),
            //new DataColumn("Name", typeof(string)),
            //new DataColumn("Technology", typeof(string)),
            //new DataColumn("Company", typeof(string)),
            //new DataColumn("Country",typeof(string)) });

            //dt.Columns.AddRange(new DataColumn[5] { new DataColumn("Country", typeof(string)) });
            //dt.Columns.Add("NewColumn", typeof(string));

            //Read the contents of CSV file.  
            string csvData = File.ReadAllText(filePath);
            int numHead = 0;
            //Execute a loop over the rows.  
            foreach (string row in csvData.Split('\n'))
            {
                if (numHead == 0)
                {
                    foreach (string cell in row.Split(','))
                    {
                        dt.Columns.Add(cell, typeof(string));
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                        dt.Rows.Add();
                        int i = 0;

                        //Execute a loop over the columns.  
                        foreach (string cell in row.Split(','))
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell;
                            i++;
                        }
                    }
                }
                numHead++;
            }

            return dt;

        }
        #endregion

        #region Convert Data

        public string ConvertstringtoDatetime(string date)
        {
            string sdate = string.Empty;
            if (date != null && date.Trim() != string.Empty)
            {
                if (date.IndexOf('/') > 0)
                {
                    // DateTime dt = Convert.ToDateTime(date);
                    DateTime dt = DateTime.ParseExact(date, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US"));
                    sdate = String.Format("{0:0000}", dt.Year) + "-" + String.Format("{0:00}", dt.Month) + "-" + String.Format("{0:00}", dt.Day);
                }
                else
                {
                    sdate = date;
                }
            }
            else if (date.Trim() == string.Empty)
            {
                sdate = null;
            }
            return sdate;
        }
        public bool CheckNull(string sNull)
        {
            bool c = true;
            if (sNull == null)
            {
                c = true;
            }
            else
            {
                if (sNull.Trim() == "")
                {
                    c = true;
                }
                else
                {
                    c = false;
                }

            }
            return c;
        }
        public bool IsNullOrEmpty<T>(IList<T> List)
        {
            return (List == null || List.Count < 1);
        }
        public string ConvertDatetimeThaiFormat(string date)
        {
            string sdate = string.Empty;
            if (date != null && date.Trim() != string.Empty)
            {
                if (date.IndexOf('/') > 0)
                {
                    DateTime dt = DateTime.ParseExact(date, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US"));
                    sdate = String.Format("{0:00}", dt.Day) + "\t" + ConvertMonthToString(String.Format("{0:00}", dt.Month)) + "\t" + ConvertYearToThaiFormat(dt.Year);
                }
                else
                {
                    sdate = date;
                }
            }
            else
            {
                sdate = "";
            }
            return sdate;
        }
        public string ConvertMonthToString(string strMonthCode)
        {
            string strResult = string.Empty;

            switch (strMonthCode)
            {
                case "01": strResult = "มกราคม"; break;
                case "02": strResult = "กุมภาพันธ์"; break;
                case "03": strResult = "มีนาคม"; break;
                case "04": strResult = "เมษายน"; break;
                case "05": strResult = "พฤษภาคม"; break;
                case "06": strResult = "มิถุนายน"; break;
                case "07": strResult = "กรกฎาคม"; break;
                case "08": strResult = "สิงหาคม"; break;
                case "09": strResult = "กันยายน"; break;
                case "10": strResult = "ตุลาคม"; break;
                case "11": strResult = "พฤศจิกายน"; break;
                case "12": strResult = "ธันวาคม"; break;
            }

            return strResult;
        }
        public string ConvertYearToThaiFormat(int iYear)
        {
            string strResult = string.Empty;

            strResult = (iYear + 543).ToString();

            return strResult;
        }
        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public string Getimgtolink(string FilePath, string FileName)
        {
            //string url = System.Configuration.ConfigurationManager.AppSettings["HostUrl"];
            string s = string.Empty;
            //if (FilePath != string.Empty)
            //{
            //    s = url + "Pages\\Download.aspx?path=" + HttpContext.Current.Server.UrlEncode(Base64Encode(FilePath))
            //            + "&&filename=" + HttpContext.Current.Server.UrlEncode(Base64Encode(FileName));
            //}
            return s;
        }
        public string Getimgtobase64(string imgFile, string type)
        {
            string s = string.Empty;
            if (imgFile != string.Empty)
            {
                s = "data:image/"
                    + Path.GetExtension(imgFile).Replace(".", "")
                    + ";base64,"
                    + Convert.ToBase64String(File.ReadAllBytes(imgFile));
            }
            return s;
        }
        public string ThaiBahtText(string strNumber, bool IsTrillion = false)
        {
            string BahtText = "";
            string strTrillion = "";
            string[] strThaiNumber = { "ศูนย์", "หนึ่ง", "สอง", "สาม", "สี่", "ห้า", "หก", "เจ็ด", "แปด", "เก้า", "สิบ" };
            string[] strThaiPos = { "", "สิบ", "ร้อย", "พัน", "หมื่น", "แสน", "ล้าน" };

            decimal decNumber = 0;
            decimal.TryParse(strNumber, out decNumber);

            if (decNumber == 0)
            {
                return "ศูนย์บาทถ้วน";
            }

            strNumber = decNumber.ToString("0.00");
            string strInteger = strNumber.Split('.')[0];
            string strSatang = strNumber.Split('.')[1];

            if (strInteger.Length > 13)
                throw new Exception("รองรับตัวเลขได้เพียง ล้านล้าน เท่านั้น!");

            bool _IsTrillion = strInteger.Length > 7;
            if (_IsTrillion)
            {
                strTrillion = strInteger.Substring(0, strInteger.Length - 6);
                BahtText = ThaiBahtText(strTrillion, _IsTrillion);
                strInteger = strInteger.Substring(strTrillion.Length);
            }

            int strLength = strInteger.Length;
            for (int i = 0; i < strInteger.Length; i++)
            {
                string number = strInteger.Substring(i, 1);
                if (number != "0")
                {
                    if (i == strLength - 1 && number == "1" && strLength != 1)
                    {
                        BahtText += "เอ็ด";
                    }
                    else if (i == strLength - 2 && number == "2" && strLength != 1)
                    {
                        BahtText += "ยี่";
                    }
                    else if (i != strLength - 2 || number != "1")
                    {
                        BahtText += strThaiNumber[int.Parse(number)];
                    }

                    BahtText += strThaiPos[(strLength - i) - 1];
                }
            }

            if (IsTrillion)
            {
                return BahtText + "ล้าน";
            }

            if (strInteger != "0")
            {
                BahtText += "บาท";
            }

            if (strSatang == "00")
            {
                BahtText += "ถ้วน";
            }
            else
            {
                strLength = strSatang.Length;
                for (int i = 0; i < strSatang.Length; i++)
                {
                    string number = strSatang.Substring(i, 1);
                    if (number != "0")
                    {
                        if (i == strLength - 1 && number == "1" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "เอ็ด";
                        }
                        else if (i == strLength - 2 && number == "2" && strSatang[0].ToString() != "0")
                        {
                            BahtText += "ยี่";
                        }
                        else if (i != strLength - 2 || number != "1")
                        {
                            BahtText += strThaiNumber[int.Parse(number)];
                        }

                        BahtText += strThaiPos[(strLength - i) - 1];
                    }
                }

                BahtText += "สตางค์";
            }

            return BahtText;
        }
        public string GetAssignToFromConfig<T>(List<T> tempAssign)
        {
            //var propertier = tempAssign.GetType().GetProperties();
            string assignTo = "",prop = "";
            int count = tempAssign.Count;
            int index = 0;
            string ListName = tempAssign.GetType().GetGenericArguments().Single().Name;

            if (ListName == "Master_Assset")
                prop = "Asset_Coordinators";
            else if (ListName == "Master_CO")
                prop = "Coordinator_Employee_Id";
            else if (ListName == "Master_ERM")
                prop = "EmpCode";
            foreach (var items in tempAssign)
            {
                var temp = GetPropertyValue(items, prop);
                assignTo += temp;
                if (index != (count - 1))//last
                    assignTo += ",";
                index++;
            }

            return assignTo;
        }
        //Function to get the Property Value
        public static object GetPropertyValue(object SourceData, string propName)
        {
            return SourceData.GetType().GetProperty(propName).GetValue(SourceData, null);
        }
        #endregion

        #region Service
        public IRestResponse CallService(string serviceName, List<KeyValuePair<string, string>> Parameters)
        {
            var client = new RestClient(serviceName);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            foreach (var parameter in Parameters)
                request.AddParameter(parameter.Key, parameter.Value, ParameterType.GetOrPost);
            IRestResponse response = client.Execute(request);
            return response;
        }
        #endregion

        public string GetUserEmailFromId(string colId)
        {
            ConfigurationService _c = new ConfigurationService();
            GetUser _u = new GetUser();
            List<ServiceModel> servicesModel = _c.ConnectionService();
            var splUser = colId.Split(',');
            string allUser = "";
            int index = 0;
            foreach (var user in splUser)
            {
                //Get User From Service
                var paramsAssignTo = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("EmployeeID", user),
                    new KeyValuePair<string, string>("FirstName", ""),
                    new KeyValuePair<string, string>("LastName", ""),
                    new KeyValuePair<string, string>("EmailAddress", ""),
                    new KeyValuePair<string, string>("OrganizationAbbreviation", ""),
                    new KeyValuePair<string, string>("SecurityCode", servicesModel[0].TokenService),
                };
                var userInfo = _u.SearchEmployee(paramsAssignTo);
                if (userInfo.Count > 0)
                {
                    allUser += userInfo[0].EMAIL_ID;
                    if (index != (splUser.Count() - 1))//last
                        allUser += ";";
                }
                index++;
            }
            return allUser;
        }

        public bool CheckCurrentWorklfow(string itemId,string status,string Table,string Step)
        {
            bool isCurrentStatus = true;
            GetData _g = new GetData();
            var p = new DynamicParameters();
            p.Add("@StatusText", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
            p.Add("@StatusChange", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@TempId", itemId);
            p.Add("@Role", Table);
            p.Add("@Bowtie", "RISK");
            CollectionRisk colRisk = _g.GetCollenctionRiskData(p, Table);
            if (colRisk.Risk.Count > 0)
            {
                foreach (var risk in colRisk.Risk)
                {
                    if (Step == "Console")
                    {
                        if (risk.Risk_Status_Workflow != status)//is Current Status nnot eq Input Status
                        {
                            isCurrentStatus = false;
                            break;
                        }
                    }
                    else // other step
                    {
                        if (Convert.ToInt32(risk.Risk_Status_Workflow) < Convert.ToInt32(status))//is Current Status less Input Status ex. submit after action re console
                        {
                            isCurrentStatus = false;
                            break;
                        }
                        if (Step == "ReConsole" && status == "5" && risk.BU_Flag == "Y") // check Bu Approve case Re console
                        {
                            isCurrentStatus = false;
                            break;
                        }
                        if (Step == "ReConsole" && status == "8" && risk.DI_Flag == "Y") // check DI Approve case Re console
                        {
                            isCurrentStatus = false;
                            break;
                        }
                        if (Step == "ReConsole" && status == "11" && risk.FG_Flag == "Y") // check FG Approve case Re console
                        {
                            isCurrentStatus = false;
                            break;
                        }
                        if (risk.Risk_ViewMode == "Y") // check view mode
                        {
                            isCurrentStatus = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                isCurrentStatus = false;
            }
            return isCurrentStatus;
        }
    }
}
