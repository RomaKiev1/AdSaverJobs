using CallTracking.DB;
using CallTracking.Repository;
using MySql.Data.MySqlClient;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Server.Jobs
{
    class NumbersNotEnoughMessageJob : IJob
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        SendEmailJobInfoRepository _sendemailjobinfoRep = new SendEmailJobInfoRepository();

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                int notenough_numbers_count = Call_GetNumbersNotEnoughCount();
                if (notenough_numbers_count > 0)
                {
                    string AdminEmail = System.Configuration.ConfigurationSettings.AppSettings["AdminEmail"].ToString();
                    SendEmailJobInfo new_sendemailjobinfo = new SendEmailJobInfo() { message = "Не хватает телефонных номеров! Необходимо заказать " + notenough_numbers_count.ToString() + " номеров!", date = DateTime.Now.ToUniversalTime(), status = false, subject = "Calltracking. Warning!", To = AdminEmail };
                    _sendemailjobinfoRep.Add(new_sendemailjobinfo);
                    _sendemailjobinfoRep.Save(); 
                }
                Console.WriteLine("NumbersNotEnoughMessageJob is running... "+DateTime.Now);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        int Call_GetNumbersNotEnoughCount()
        {
            int numberscount = 0;

            using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
            {
                objConn.Open();
                MySqlCommand objGetNumbersNotEnoughCount = new MySqlCommand();
                objGetNumbersNotEnoughCount.Connection = objConn;
                objGetNumbersNotEnoughCount.CommandType = CommandType.StoredProcedure;
                objGetNumbersNotEnoughCount.CommandText = "GetNumbersNotEnoughCount";
                MySqlDataReader reader = objGetNumbersNotEnoughCount.ExecuteReader();
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        numberscount = reader.GetInt32(0);
                    }

                objConn.Close();
            }

            return numberscount;
        }
    }
}
