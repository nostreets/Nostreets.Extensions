using Hangfire.Dashboard;
using NostreetsExtensions.Extend.Basic;
using NostreetsExtensions.Extend.Web;
using System.Web;

namespace NostreetsExtensions.DataControl.Attributes
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            bool ipsMatch = false;
            string[] ipsToCheck = new string[] { };
            string[] seperators = new[] { ",", ", ", " , ", " ,", "  ,  " };
            string allowedIPs = Web.GetValueFromWebConfig("Hangfire.AllowedIps");


            foreach (string s in seperators)
                if (allowedIPs.Contains(s))
                {
                    ipsToCheck = allowedIPs.Split(s);
                    break;
                }

            if (ipsToCheck.Length == 0)
                ipsToCheck = new[] { allowedIPs };

            string ip = HttpContext.Current.GetIPAddress();
            string ip2 = HttpContext.Current.GetIP4Address();

            foreach (string _ip in ipsToCheck)
                ipsMatch = _ip == ip || _ip == ip2;

            return ipsMatch;//HttpContext.Current.User.Identity.IsAuthenticated;
        }
    }
}
