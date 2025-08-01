using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace ajesha
{
    public class DBConnection
    {
        private const string ConnectionString = "server=127.0.0.1;port=3306;user=ajesha;password=ControladorTotalitario_123;database=ajesha;";
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
