using NostreetsExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.Utilities
{
    public class ExcelService : OleDbService
    {
        public ExcelService(string filePath) : base(filePath) { }

        public List<string> GetAllInColumn(string sheetName, int column)
        {
            List<string> result = null;

            DataProvider.ExecuteCmd(() => Connection, "Select F{0} From [{1}$]", null, 
                (reader, set) => result = DataMapper<List<string>>.Instance.MapToObject(reader));

            return result;
        }


        public List<string> GetAll(string sheetName)
        {
            List<string> result = null;

            DataProvider.ExecuteCmd(() => Connection, "Select * From [{1}$]", null,
                (reader, set) =>
                {
                    if (result == null)
                        result = new List<string>();

                    result.Add(reader.GetString(0));
                });

            return result;
        }
    }
}
