using System;
using MySql.Data.MySqlClient;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            DotNetEnv.Env.Load("../../../.env");
            string conStr = DotNetEnv.Env.GetString("MYSQL_CON");

            using var con = new MySqlConnection(conStr);
            con.Open();
            string version = con.ServerVersion;
            con.Close();
            // Console.WriteLine("MySQL version : " + version);
            
        }
    }
}