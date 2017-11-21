using NostreetsExtensions.Interfaces;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace NostreetsExtensions.Helpers
{
    public abstract class BaseService
    {
        public BaseService()
        {
            _connectionKey = "DefaultConnection";
        }

        public BaseService(string connectionKey)
        {

            _connectionKey = connectionKey;

        }

        private string _connectionKey;

        public SqlConnection Connection { get { return new SqlConnection(WebConfigurationManager.ConnectionStrings[_connectionKey].ConnectionString); } }

        protected static IDao DataProvider
        {

            get { return Helpers.DataProvider.SqlInstance; }
        }

    }
}