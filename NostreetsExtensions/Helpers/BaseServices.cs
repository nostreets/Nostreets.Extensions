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
        }

        public SqlService(string connectionKey)
        {
            _connectionString = WebConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
        }


        private string _connectionString = null;
        private IQueryProvider _queryProvider = null;

        public SqlConnectionStringBuilder Builder => new SqlConnectionStringBuilder(_connectionString);
        public SqlConnection Connection => new SqlConnection(_connectionString);
        public static ISqlExecutor Instance => DataProvider.SqlInstance;
        public IQueryProvider QueryProvider { get => _queryProvider; set => _queryProvider = value; }


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
        private IQueryProvider _queryProvider = null;

        public OleDbConnection Connection => new OleDbConnection(_connectionString); 
        public IQueryProvider QueryProvider { get => _queryProvider; set => _queryProvider = value; }
        protected static IOleDbExecutor Instance => DataProvider.OleDbInstance;

        public OleDbConnection ChangeSqlConnection(string connectionKey)
        {
            _connectionString = WebConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            return Connection;

        }
    }
}