using NostreetsExtensions.Helpers.SqlTranslator;
using NostreetsExtensions.Interfaces;
using NostreetsExtensions.Utilities;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;

namespace NostreetsExtensions.Helpers
{
    public abstract class SqlService : Disposable
    {
        public SqlService()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _queryProvider = new LinqProvider(Connection);
        }

        public SqlService(string connectionKey)
        {

            _connectionString = WebConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            _queryProvider = new LinqProvider(Connection);
        }


        private string _connectionString = null;
        private LinqProvider _queryProvider = null;

        public SqlConnection Connection => new SqlConnection(_connectionString);
        public static ISqlExecutor Instance => DataProvider.SqlInstance;
        public LinqProvider QueryProvider => _queryProvider;


        public SqlConnection ChangeSqlConnection(string connectionKey)
        {
            _connectionString = WebConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            return Connection;
        }

    }

    public abstract class OleDbService
    {
        public OleDbService(string filePath)
        {
            string[] splitPath = filePath.Split('.');

            if (splitPath[splitPath.Length - 1].Contains("xlsx"))

                _connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0; Data Source='{0}'; Extended Properties=\"Excel 12.0;HDR=YES;\"", filePath);
            else
                _connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source = '{0}'; Extended Properties=\"Excel 8.0;HDR=YES;\"", filePath);
        }

        private string _connectionString;

        public OleDbConnection Connection { get { return new OleDbConnection(_connectionString); } }

        protected static IOleDbExecutor DataProvider
        {

            get { return Helpers.DataProvider.OleDbInstance; }
        }

        public OleDbConnection ChangeSqlConnection(string connectionKey)
        {
            _connectionString = WebConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            return Connection;

        }
    }
}