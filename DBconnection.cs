using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace PrivateGradeDeliverySystem1
{
    class DBconnection
    {
        // Local connection string
        private static string localConnectionString =
            "server=localhost;database=universitydb;user=root;password=12345678;SslMode=none;";

        // Online connection string (AlwaysData)
        private static string onlineConnectionString =
            "server=mysql-janah123.alwaysdata.net;database=janah123_universitydb;user=janah123_uni;password=universitydb_2025@;SslMode=none;";

        private bool useOnline;

        public DBconnection(bool useOnline = false)
        {
            this.useOnline = useOnline;
        }

        public MySqlConnection GetConnection()
        {
            if (useOnline)
                return new MySqlConnection(onlineConnectionString);
            else
                return new MySqlConnection(localConnectionString);
        }

        // اختبار الاتصال
        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    conn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Connection failed: " + ex.Message);
                return false;
            }
        }

        public DataTable GetData(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

    }
}
