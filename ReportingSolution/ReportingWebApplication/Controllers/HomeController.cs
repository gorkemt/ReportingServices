using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using ReportingWebApplication.WSReportService2010;

//using ReportService2010;

namespace ReportingWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private ReportingServiceClient rs { get; set; }
        
        public ActionResult Index()
        {
            if (HttpContext.Session["user"] == null)
            {
                return View("Login");
            }
                 return View("Index");
        }

        public ActionResult Reports(string username, string password, string domain)
        {
            if (HttpContext.Session["user"] == null)
            {
                rs = new ReportingServiceClient(username, password, domain);
            }
            else
            {
                var credential = (NetworkCredential)HttpContext.Session["user"];
                rs = new ReportingServiceClient(credential.UserName, credential.Password, credential.Domain);
            }
            
            return ReportViewAction();
        }

        private ActionResult ReportViewAction()
        {
            try
            {
                if (rs.IsCredentialsValid())
                {
                    HttpContext.Session["user"] = rs.Credentials;
                    ProcessReportingView();

                    return View("Reports");
                }
                else
                {
                    return View("Login");
                }
            }
            catch (Exception e)
            {
               return  View("Error");
            }
        }

        private void ProcessReportingView()
        {
            var items = rs.GetAllReports();
            ViewBag.Message = "Your application reports page.";
            ViewBag.ReportNames = items;
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }

    public class ReportingServiceClient : ReportingService2010
    {
        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _domain;

        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        public NetworkCredential NetworkCredentials { get; set; }
        public ReportingServiceClient(string username, string password, string domain)
        {
            this._domain = domain;
            this._username = username;
            this._password = password;
            if (!string.IsNullOrEmpty(username))
            {
                var sepInd = username.IndexOf("\\", StringComparison.Ordinal);
                if (sepInd >= 0)
                {
                    _domain = username.Substring(0, sepInd);
                    _username = username.Substring(sepInd + 1);
                }
            }
            Credentials = new NetworkCredential(_username, _password, _domain);
        }
        /*
        public ReportingServiceClient(string username, string password)
        {
            _password = password;
            if (!string.IsNullOrEmpty(username))
            {
                var sepInd = username.IndexOf("\\", StringComparison.Ordinal);
                if (sepInd >= 0)
                {
                    _domain = username.Substring(0, sepInd);
                    _username = username.Substring(sepInd + 1);
                }
            }
            Credentials= new NetworkCredential(_username, _password, _domain);
        }
        */
        public bool IsCredentialsValid()
        {
            var isValid = false;
            try
            {
                base.GetItemType("/");
                isValid = true;
            }
            catch (WebException ex)
            {
                isValid = false;
            }
            catch (InvalidOperationException e)
            {
                throw;
            }
            return isValid;
        }

        public CatalogItem[] GetAllReports()
        {
            return ListChildren("/", true);
        }
    }
}