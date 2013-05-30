using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Quartz;

namespace Quartz.Server.Jobs
{
    class CollectNumbersStatisticJob:IJob
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Info("CollectNumbersStatisticJob запущен!");
                using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
                {
                    objConn.Open();
                    MySqlCommand objGetHighlightedNumbersStatistic = new MySqlCommand("GetHighlightedNumbersStatistic", objConn);
                    objGetHighlightedNumbersStatistic.CommandType = CommandType.StoredProcedure;
                    objGetHighlightedNumbersStatistic.CommandTimeout = int.MaxValue;
                    objGetHighlightedNumbersStatistic.ExecuteNonQuery();
                    objConn.Close();

                }

                Console.WriteLine("CollectNumbersStatisticJob is running....." + DateTime.Now.ToString());
            }
            catch(Exception ex)
            {
                Console.WriteLine("CollectNumbersStatisticJob is running with error....." + DateTime.Now.ToString());
                Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
            }
        }
    }
}
