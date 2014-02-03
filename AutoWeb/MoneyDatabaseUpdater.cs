using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace AutoWeb
{
    class MoneyDatabaseUpdater
    {
        private MySqlConnection myConnection;
        private MySqlCommand myCommand;

        public MoneyDatabaseUpdater()
        {
            string myConnectionString = "Host=192.168.1.101;user id=dev;password=fEvvGsSc2b6pa2Ms;database=life";

            this.myConnection = new MySqlConnection(myConnectionString);
            this.myConnection.Open();

            this.myCommand = new MySqlCommand();
            this.myCommand.Connection = this.myConnection;
        }

        ~MoneyDatabaseUpdater()
        {
            this.myConnection.Close();
        }

        public void AddTotal(DateTime date, double earnTotal, double earnLeft)
        {
            if (this.IsTotalLogged(date, earnTotal, earnLeft))
            {
                return;
            }

            string SQLString = string.Format("INSERT INTO moneysnap (datetime,earntotal, lefttotal) VALUES(?date,?earn,?left)");

            this.myCommand.CommandText = SQLString;
            this.myCommand.Parameters.Clear();
            this.myCommand.Parameters.AddWithValue("date", date);
            this.myCommand.Parameters.AddWithValue("earn", earnTotal);
            this.myCommand.Parameters.AddWithValue("left", earnLeft);

            this.myCommand.ExecuteNonQuery();
        }

        public void AddDaily(DateTime date, double change, byte type)
        {
            if(this.IsDailyLogged(date, change, type))
            {
                return;
            }

            string SQLString = string.Format("INSERT INTO dailymoney (datetime,money,type) VALUES(?date,?money,?type)");

            this.myCommand.CommandText = SQLString;
            this.myCommand.Parameters.Clear();
            this.myCommand.Parameters.AddWithValue("date", date);
            this.myCommand.Parameters.AddWithValue("money", change);
            this.myCommand.Parameters.AddWithValue("type", type);

            this.myCommand.ExecuteNonQuery();
        }

        private bool IsTotalLogged(DateTime date, double earnTotal, double earnLeft)
        {
            string SQLString = "SELECT MAX(`datetime`) AS `latest` FROM `moneysnap`";

            this.myCommand.CommandText = SQLString;
            this.myCommand.Parameters.Clear();

            MySqlDataReader myDatareader;
            myDatareader = this.myCommand.ExecuteReader();
            myDatareader.Read();

            DateTime latestDate;
            try
            {
                latestDate = myDatareader.GetDateTime(myDatareader.GetOrdinal("latest"));
            }
            catch
            {
                latestDate = DateTime.MinValue;
            }

            this.myCommand.Connection.Close();
            this.myCommand.Connection.Open();

            return latestDate >= date.Date;
        }

        private bool IsDailyLogged(DateTime date, double change, byte type)
        {
            string SQLString = "SELECT * FROM `dailymoney` WHERE `datetime`=?date AND `money`=?money AND `type`=?type";

            this.myCommand.CommandText = SQLString;
            this.myCommand.Parameters.Clear();
            this.myCommand.Parameters.AddWithValue("date", date);
            this.myCommand.Parameters.AddWithValue("money", change);
            this.myCommand.Parameters.AddWithValue("type", type);

            MySqlDataReader myDatareader;
            myDatareader = this.myCommand.ExecuteReader();
            myDatareader.Read();

            bool isLogged;
            isLogged = myDatareader.HasRows;

            this.myCommand.Connection.Close();
            this.myCommand.Connection.Open();

            return isLogged;
        }
    }
}
