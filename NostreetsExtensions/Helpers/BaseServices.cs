using NostreetsExtensions.Interfaces;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace NostreetsExtensions.Helpers
{

    public abstract class SqlService
    {
        public SqlService()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        }

        public SqlService(string connectionKey)
        {

            _connectionString = WebConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
        }


        private string _connectionString;

        public SqlConnection Connection { get { return new SqlConnection(_connectionString); } }

        protected static ISqlDao DataProvider
        {

            get { return Helpers.DataProvider.SqlInstance; }
        }

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

        protected static IOleDbDao DataProvider
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