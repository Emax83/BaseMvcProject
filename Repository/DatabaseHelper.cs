using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BaseMvcProject.Repository
{
    public class DatabaseHelper
    {

        private string ConnectionString;
        private SqlConnection sqlConnection;

        public DatabaseHelper()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(ConnectionString);
        }

        public System.Data.DataTable GetDataTableFromStoredProcedure(string spName, System.Data.SqlClient.SqlParameter[] args, string dtName="table")
        {
            DataTable dt = new DataTable(dtName);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(spName, ConnectionString);
            sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlDataAdapter.SelectCommand.Parameters.AddRange(args);
            sqlDataAdapter.FillSchema(dt, SchemaType.Source);
            sqlDataAdapter.Fill(dt);
            return dt;
        }


        public int GetExecuteQueryFromStoredProcedure(string spName, System.Data.SqlClient.SqlParameter[] args)
        {
            int count = 0;
            SqlCommand sqlCommand = new SqlCommand(spName, sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddRange(args);
            sqlCommand.Connection.Open();
            count = sqlCommand.ExecuteNonQuery();
            sqlCommand.Connection.Close();
            return count;
        }

        public IEnumerable<object> GetListOfbjectFromDataTable(DataTable dataTable, Type destinationType) {
            List<object> instances = new List<object>();
            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    instances.Add(GetObjectFromDataRow(row, destinationType));
                }
                catch(Exception ex)
                {

                }
             
            }
            if (dataTable.Rows.Count != instances.Count)
            {
                throw new Exception("");
            }
            return instances;
        }

        public object GetObjectFromDataRow(DataRow dataRow, Type destinationType)
        {
            object instance = Activator.CreateInstance(destinationType);
            foreach (DataColumn col in dataRow.Table.Columns)
            {
                try
                {
                    System.Reflection.PropertyInfo property = destinationType.GetProperty(col.ColumnName);
                    if (property != null && property.CanWrite)
                    {
                        object value = Convert.ChangeType(dataRow[col.ColumnName],property.PropertyType);
                        property.SetValue(instance, value);
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception("GetObjectFromDataRow", ex);
                }
              
            }
            return instance;
        }

        public bool TestConnection()
        {
            try
            {
                sqlConnection.Open();
                sqlConnection.Close();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
