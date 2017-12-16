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
        public ExcelService(string filePath) : base(filePath)
        {
            string[] splitPath = filePath.Split('\\', '.');
            _fileName = splitPath[splitPath.Length - 2];
        }

        private string _fileName = null;

        public List<object> GetAll(string sheetName)
        {
            List<object> result = null;
            string[] excelSchema = null;
            Type[] schemaTypes = null;
            Type dynamicType = null;

            DataProvider.ExecuteCmd(() => Connection, string.Format("Select * From [{0}$]", sheetName), null,
                (reader, set) =>
                {
                    if (result == null)
                        result = new List<object>();

                    if (excelSchema == null)
                        excelSchema = reader.GetSchema().ToArray();

                    if (schemaTypes == null)
                        schemaTypes = reader.GetSchemaTypes().ToArray();

                    if (dynamicType == null)
                        dynamicType = ClassBuilder.CreateType("DynamicModel", excelSchema, schemaTypes);


                    object stat = DataMapper.MapToObject(reader, dynamicType);


                    result.Add(stat);
                });

            return result;
        }

        public List<string> GetAllInColumn(string sheetName, string columnName)
        {
            List<string> result = null;

            DataProvider.ExecuteCmd(() => Connection, string.Format("SELECT `{0}$`.{1} FROM [{0}$]", sheetName, columnName), null,
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
