using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.IO;


namespace Individual_Activity
{
    public static class Database
    {
        // Database para sa Users/Login
        public static readonly string UserDBPath =
            Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\Pawfect.accdb"
                )
            );

        // Database para sa Products/Inventory
        public static readonly string ProductDBPath =
            Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\PawfectItems.accdb"
                )
            );

        public static OleDbConnection GetUserConnection()
        {
            string connStr =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                UserDBPath +
                ";Persist Security Info=False;";

            return new OleDbConnection(connStr);
        }

        public static OleDbConnection GetProductConnection()
        {
            string connStr =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                ProductDBPath +
                ";Persist Security Info=False;";

            return new OleDbConnection(connStr);
        }
    }
}

