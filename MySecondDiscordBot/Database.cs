using System;
using System.Data.SqlClient;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace MySecondDiscordBot
{
    class Database
    {
        private string Table { get; set; }
        private const string server = "127.0.0.1, 59224";
        private const string database = "discord";
        private const string username = "sa";
        private const string password = "123";

        private SqlConnection dbConnection;

        public Database(string table)
        {
            this.Table = table;

            //Standard SQL connection string
            //Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;

            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();

            sb.UserID = username;
            sb.Password = password;
            sb.DataSource = server; //this is where the server name goes
            sb.InitialCatalog = database; //inital catalog and database can be used interchangably

            var connectionString = sb.ToString(); //construts connection string

            dbConnection = new SqlConnection(connectionString);

            dbConnection.Open();
            //try
            //{
            //    using (dbConnection = new SqlConnection(connectionString))
            //    {
            //        dbConnection.Open();
            //    }
            //}
            //catch 
            //{
            //    Console.WriteLine("Error opening connection");
            //}

            //Console.WriteLine("Execution of {0} is {1}", connectionString, dbConnection);
        }

        public SqlDataReader ExecuteQuery(string query) //reads stuff through an input query
        {
            if (dbConnection == null)
            {
                return null;
            }

            SqlCommand command = new SqlCommand(query, dbConnection);

            //sends the command text (query) to the connection and builds a SqlDataReader

            var SqlReader = command.ExecuteReader(); 

            return SqlReader;
        }

        public void CloseConnection() //closes connection
        {
            if(dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        //check if user is in table discorduser
        public static List<string> CheckExistingUser(IUser user)
        {
            var result = new List<string>();

            //database name goes in
            var database = new Database("discord"); //why does a new database have to be instantiated?

            //data reading query goes here
            var str = string.Format("SELECT * FROM discorduser WHERE user_id ='{0}'", user.Id); 

            //tableName is the data (output) from which the query str provides
            var tableName = database.ExecuteQuery(str);

            //I guess SqlDataReader.Read() goes through all the lines of the output before going null
            while (tableName.Read())
            {
                //reads the data from column "user_id"
                var userId = (string)tableName["user_id"]; 

                //and does something to it
                result.Add(userId);
            }

            return result;
        }

        public static List<string> CheckExistingUser()
        {
            var result = new List<string>();

            //database name goes in
            var database = new Database("discord"); //why does a new database type have to be instantiated?

            //data reading query goes here
            var str = string.Format("SELECT user_id FROM discorduser");
            //tableName is the data (output) from which the query str provides
            var table = database.ExecuteQuery(str);

            //I guess SqlDataReader.Read() goes through all the lines of the output before going null
            while (table.Read())
            {
                //reads the data from column "user_id"
                var userId = (string)table["user_id"];

                //and does something to it
                result.Add(userId);
            }

            return result;
        }

        

        public static void EnterUser(IUser user)
        {
            var database = new Database("discord");

            var str = string.Format("Insert INTO discorduser (user_id, user_ign, user_joindate, user_leftdate, userisjoin), ") +
            string.Format("({0}, {1}, {2}, {3}, {4}) ,",
                            "'" + user.Id + "'",
                            "'" + user.Username + "'",
                            "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'",
                            "''",
                            '1');



            var table = database.ExecuteQuery(str);

            database.CloseConnection();
        }
    }
}
