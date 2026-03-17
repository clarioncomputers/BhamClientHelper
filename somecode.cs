using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Int.Semestry.Staff.Helper.Logging;

namespace Int.Semestry.Staff.Helper.RestSharp
{
    public class SemestryAPIServiceCaller : ISemestryAPI
    {
        private static readonly string appName = "Int.Semestry.Staff";
        private string _campus = "";
        private string _token = "";

        public string Campus
        {
            set { _campus = value; }
        }

        public string Token
        {
            set { _token = GetSSOValue("SemestryAPIToken" + value); }
        }

        private static string GetSSOValue(string key)
        {
            return SSOSettingsFileReader.ReadString(appName, key);
        }

        //********************************************************
        //
        // Modules below use SSO for URL and Authenication Values
        //
        //********************************************************
        public bool GetStaffFromId(string staffCode)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId: staffCode {0}", staffCode));
            string getStaffUri = GetSSOValue("StaffGetURLSemestry" + _campus);
            getStaffUri += staffCode;

            var client = new RestClient(getStaffUri);
            var request = new RestRequest(getStaffUri, Method.GET);
            request.AddHeader("Authorization", _token);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId: Execute Get Request, Uri {0}", getStaffUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId: Response {0}", response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId StaffCode {0}: EMPTY Response returned", staffCode));
            }
            return !string.IsNullOrEmpty(response.Content) && !response.Content.Contains("No object found") ? true : false;
        }

        public bool CreateUpdateStaff(string staffCode, string staffName, string departmentName, string staffEmail, string principalName, string user1, bool createStaff)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: staffCode {0}, staffName {1}, departmentName {2}", staffCode, staffName, departmentName));
            string createUpdateStaffUri = GetSSOValue("StaffCreateUpdateURLSemestry" + _campus);
            createUpdateStaffUri += staffCode;
            var queryString =
                $"?n={Uri.EscapeDataString(staffName)}" +
                $"&dc={Uri.EscapeDataString(departmentName)}" +
                $"&e1={Uri.EscapeDataString(staffEmail)}" +
                $"&pn={Uri.EscapeDataString(principalName)}" +
                $"&user1={Uri.EscapeDataString(user1)}";
            createUpdateStaffUri += queryString;

            var client = new RestClient(createUpdateStaffUri);
            if (createStaff)
            {
                var request = new RestRequest(createUpdateStaffUri, Method.POST);
                request.AddHeader("Authorization", _token);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Execute Create Request, Uri {0}", createUpdateStaffUri));
                var response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Response {0}", response.Content));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: EMPTY Response returned"));
                }
                return true;
            }
            else
            {
                createUpdateStaffUri += "&td=0";

                var request = new RestRequest(createUpdateStaffUri, Method.PUT);
                request.AddHeader("Authorization", _token);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Execute Update Request, Uri {0}", createUpdateStaffUri));
                var response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Response {0}", response.Content));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: EMPTY Response returned"));
                }
                return true;
            }
        }

        public bool CreateUpdateStaff(string staffCode, string staffName, string departmentName, string staffEmail, string principalName, string user1, bool createStaff, string staffGroup)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: staffCode {0}, staffName {1}, departmentName {2}", staffCode, staffName, departmentName));
            string createUpdateStaffUri = GetSSOValue("StaffCreateUpdateURLSemestry" + _campus);
            createUpdateStaffUri += staffCode;
            var queryString =
                $"?n={Uri.EscapeDataString(staffName)}" +
                $"&dc={Uri.EscapeDataString(departmentName)}" +
                $"&e1={Uri.EscapeDataString(staffEmail)}" +
                $"&pn={Uri.EscapeDataString(principalName)}" +
                $"&staffGroups={Uri.EscapeDataString(staffGroup)}" +
                $"&user1={Uri.EscapeDataString(user1)}";
            createUpdateStaffUri += queryString;

            var client = new RestClient(createUpdateStaffUri);
            if (createStaff)
            {
                var request = new RestRequest(createUpdateStaffUri, Method.POST);
                request.AddHeader("Authorization", _token);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Execute Create Request, Uri {0}", createUpdateStaffUri));
                var response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Response {0}", response.Content));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: EMPTY Response returned"));
                }
                return true;
            }
            else
            {
                createUpdateStaffUri += "&td=0";

                var request = new RestRequest(createUpdateStaffUri, Method.PUT);
                request.AddHeader("Authorization", _token);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Execute Update Request, Uri {0}", createUpdateStaffUri));
                debugLogger.LoggerProcess(string.Format("Uri {0}", createUpdateStaffUri));
                var response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Response {0}", response.Content));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: EMPTY Response returned"));
                }
                return true;
            }
        }

        public bool DeleteStaff(string staffCode)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: staffCode {0}", staffCode));
            // Soft delete only, so call with parameter softDelete = 1
            string deleteStaffUri = GetSSOValue("StaffDeleteURLSemestry" + _campus);
            deleteStaffUri += staffCode;
            deleteStaffUri += "?softDelete=1";

            var client = new RestClient(deleteStaffUri);
            var request = new RestRequest(deleteStaffUri, Method.DELETE);
            request.AddHeader("Authorization", _token);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: Execute Delete Request, Uri {0}", deleteStaffUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: Response {0}", response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: EMPTY Response returned"));
            }
            return true;
        }

        public string GetDepartment(string departmentName)
        {
            try
            {
                string departmentCode = "";

                var debugLogger = new Logger();
                debugLogger.Init();
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: departmentName {0}", departmentName));
                string getDepartmentsUri = GetSSOValue("DepartmentsGetURLSemestry" + _campus);

                var client = new RestClient(getDepartmentsUri);
                var request = new RestRequest(getDepartmentsUri, Method.GET);
                request.AddHeader("Authorization", _token);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: Execute Get Request, Uri {0}", getDepartmentsUri));
                var response = client.Execute(request);

                // Load JSON into array and retrieve matching value
                JArray jsonArray = JArray.Parse(response.Content);
                if (_campus == "Dubai")
                {
                    departmentCode = (string)jsonArray.Children().Single(p => (string)p["n"] == departmentName)["c"];
                }
                else
                {
                    departmentCode = (string)jsonArray.Children().Single(p => (string)p["d"] == departmentName)["c"];
                }

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: departmentCode {0}", departmentCode));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: EMPTY Response returned"));
                }
                return !string.IsNullOrEmpty(departmentCode) ? departmentCode : "UoBD";
            }
            catch
            {
                var debugLogger = new Logger();
                debugLogger.Init();
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: In CATCH return UoBD"));
                return "UoBD";
            }
        }

        public bool GetStaffGroupFromCode(string staffGroupCode)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: staffGroupCode {0}", staffGroupCode));
            string getStaffGroupUri = GetSSOValue("StaffGroupGetURLSemestry" + _campus);
            getStaffGroupUri += staffGroupCode;

            var client = new RestClient(getStaffGroupUri);
            var request = new RestRequest(getStaffGroupUri, Method.GET);
            request.AddHeader("Authorization", _token);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: Execute Get Request, Uri {0}", getStaffGroupUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: Response {0}", response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: EMPTY Response returned"));
            }
            return response.Content.Length > 2 ? true : false;
        }

        public bool CreateStaffGroup(string staffGroup)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: staffGroup {0}", staffGroup));
            string createStaffGroupUri = GetSSOValue("StaffGroupCreateUpdateURLSemestry" + _campus);
            createStaffGroupUri += staffGroup;
            var queryString =
                $"?name={Uri.EscapeDataString(staffGroup)}" +
                $"&roleType={Uri.EscapeDataString("User-defined")}" +
                $"&colour={Uri.EscapeDataString("ffffff")}" +
                $"&tt={Uri.EscapeDataString("0")}" +
                $"&ex={Uri.EscapeDataString("0")}" +
                $"&published={Uri.EscapeDataString("0")}";
            createStaffGroupUri += queryString;

            var client = new RestClient(createStaffGroupUri);
            var request = new RestRequest(createStaffGroupUri, Method.POST);
            request.AddHeader("Authorization", _token);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: Execute Create Request, Uri {0}", createStaffGroupUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: Response {0}", response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: EMPTY Response returned"));
            }
            return true;
        }

        //****************************************************************
        //
        // Modules below use Control API for URL and Authenication Values
        //
        //****************************************************************
        public bool GetStaffFromId(string staffCode, string apiURL, string apiReadKey)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId: staffCode {0}", staffCode));
            string getStaffUri = apiURL + "staff/";
            getStaffUri += staffCode;

            var client = new RestClient(getStaffUri);
            var request = new RestRequest(getStaffUri, Method.GET);
            request.AddHeader("Authorization", apiReadKey);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId: Execute Get Request, Uri {0}", getStaffUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId: Response {0}", response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffFromId: EMPTY Response returned"));
            }
            return !string.IsNullOrEmpty(response.Content) && !response.Content.Contains("No object found") ? true : false;
        }

        public bool CreateUpdateStaff(string staffCode, string staffName, string departmentName, string staffEmail, string principalName, string user1, bool createStaff, string staffGroup, string apiURL, string apiUpdateKey)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: staffCode {0}, staffName {1}, departmentName {2}", staffCode, staffName, departmentName));
            string createUpdateStaffUri = apiURL + "staff/";
            createUpdateStaffUri += staffCode;
            var queryString =
                $"?n={Uri.EscapeDataString(staffName)}" +
                $"&dc={Uri.EscapeDataString(departmentName)}" +
                $"&e1={Uri.EscapeDataString(staffEmail)}" +
                $"&pn={Uri.EscapeDataString(principalName)}" +
                $"&staffGroups={Uri.EscapeDataString(staffGroup)}" +
                $"&user1={Uri.EscapeDataString(user1)}";
            createUpdateStaffUri += queryString;

            var client = new RestClient(createUpdateStaffUri);
            if (createStaff)
            {
                var request = new RestRequest(createUpdateStaffUri, Method.POST);
                request.AddHeader("Authorization", apiUpdateKey);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Execute Create Request, Uri {0}", createUpdateStaffUri));
                var response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Response {0}", response.Content));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: EMPTY Response returned"));
                }
                return true;
            }
            else
            {
                createUpdateStaffUri += "&td=0";

                var request = new RestRequest(createUpdateStaffUri, Method.PUT);
                request.AddHeader("Authorization", apiUpdateKey);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Execute Update Request, Uri {0}", createUpdateStaffUri));
                debugLogger.LoggerProcess(string.Format("Uri {0}", createUpdateStaffUri));
                var response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: Response {0}", response.Content));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateUpdateStaff: EMPTY Response returned"));
                }
                return true;
            }
        }

        public string GetDepartment(string departmentName, string apiURL, string apiReadKey)
        {
            try
            {
                string departmentCode = "";

                var debugLogger = new Logger();
                debugLogger.Init();
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: departmentName {0}", departmentName));
                string getDepartmentsUri = apiURL + "departments/";

                var client = new RestClient(getDepartmentsUri);
                var request = new RestRequest(getDepartmentsUri, Method.GET);
                request.AddHeader("Authorization", apiReadKey);
                request.AddHeader("Content-Type", "application/json");

                // execute the request
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: Execute Get Request, Uri {0}", getDepartmentsUri));
                var response = client.Execute(request);

                // Load JSON into array and retrieve matching value
                JArray jsonArray = JArray.Parse(response.Content);
                if (_campus == "Dubai")
                {
                    departmentCode = (string)jsonArray.Children().Single(p => (string)p["n"] == departmentName)["c"];
                }
                else
                {
                    departmentCode = (string)jsonArray.Children().Single(p => (string)p["d"] == departmentName)["c"];
                }

                if (!string.IsNullOrEmpty(response.Content))
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: departmentCode {0}", departmentCode));
                }
                else
                {
                    debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: EMPTY Response returned"));
                }
                return !string.IsNullOrEmpty(departmentCode) ? departmentCode : "UoBD";
            }
            catch
            {
                var debugLogger = new Logger();
                debugLogger.Init();
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetDepartment: In CATCH return UoBD"));
                return "UoBD";
            }
        }

        public bool GetStaffGroupFromCode(string staffGroupCode, string apiURL, string apiReadKey)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: staffGroupCode {0}", staffGroupCode));
            string getStaffGroupUri = apiURL + "staffgroup/";
            getStaffGroupUri += staffGroupCode;

            var client = new RestClient(getStaffGroupUri);
            var request = new RestRequest(getStaffGroupUri, Method.GET);
            request.AddHeader("Authorization", apiReadKey);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: Execute Get Request, Uri {0}", getStaffGroupUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: Response {0}", response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.GetStaffGroupFromCode: EMPTY Response returned"));
            }
            return response.Content.Length > 2 ? true : false;
        }

        public bool CreateStaffGroup(string staffGroup, string apiURL, string apiUpdateKey)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: staffGroup {0}", staffGroup));
            string createStaffGroupUri = apiURL + "staffgroup/"; 
            createStaffGroupUri += staffGroup;
            var queryString =
                $"?name={Uri.EscapeDataString(staffGroup)}" +
                $"&roleType={Uri.EscapeDataString("User-defined")}" +
                $"&colour={Uri.EscapeDataString("ffffff")}" +
                $"&tt={Uri.EscapeDataString("0")}" +
                $"&ex={Uri.EscapeDataString("0")}" +
                $"&published={Uri.EscapeDataString("0")}";
            createStaffGroupUri += queryString;

            var client = new RestClient(createStaffGroupUri);
            var request = new RestRequest(createStaffGroupUri, Method.POST);
            request.AddHeader("Authorization", apiUpdateKey);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: Execute Create Request, Uri {0}", createStaffGroupUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: Response {0}", response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.CreateStaffGroup: EMPTY Response returned"));
            }
            return true;
        }







        public bool DeleteStaff(string staffCode, string apiURL, string apiDeleteKey)
        {
            var debugLogger = new Logger();
            debugLogger.Init();
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: staffCode {0}", staffCode));
            // Soft delete only, so call with parameter softDelete = 1
            string deleteStaffUri = apiURL + "staff/";
            deleteStaffUri += staffCode;
            deleteStaffUri += "?softDelete=1";

            var client = new RestClient(deleteStaffUri);
            var request = new RestRequest(deleteStaffUri, Method.DELETE);
            request.AddHeader("Authorization", apiDeleteKey);
            request.AddHeader("Content-Type", "application/json");

            // execute the request
            debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: Execute Delete Request, Uri {0}", deleteStaffUri));
            var response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: StaffCode {0}, Response {1}", staffCode, response.Content));
            }
            else
            {
                debugLogger.LoggerProcess(string.Format("SemestryAPIServiceCaller.DeleteStaff: StaffCode {0}: EMPTY Response returned", staffCode));
            }
            return true;
        }
    }
}
