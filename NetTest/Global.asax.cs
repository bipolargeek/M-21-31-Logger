using log4net;
using M_21_31.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace NetTest
{
    public class Global : HttpApplication
    {
        private static readonly ILog _logger = Log4Net_LogManager.GetM_21_31_Logger(typeof(Global));
        
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //_logger.Info("TEST");
            var loggerContext = new M_21_31.Logger.M_21_31_LoggerContext();
            loggerContext.BeginCapture();
            HttpContext.Current.Items["LoggerContext"] = loggerContext;

            var requestBody = loggerContext.GetRequestBody().Result;

            _logger.LogEvent(M_21_31.Logger.EventType.Request, M_21_31.Logger.EventStatus.Success, null, null, null, null, requestBody, null);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Items["LoggerContext"] is M_21_31.Logger.M_21_31_LoggerContext loggerContext)
            {
                loggerContext.EndCapture();
                var responseBody = loggerContext.GetResponseBody().Result;
                var duration = loggerContext.GetElapsedMilliseconds();
                // log or use responseBody/duration

                _logger.LogEvent(M_21_31.Logger.EventType.Response, M_21_31.Logger.EventStatus.Success, null, null, null, long.Parse(duration.ToString()), null, responseBody);
            }
        }
    }
}