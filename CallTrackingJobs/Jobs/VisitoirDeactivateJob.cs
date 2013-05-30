using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CallTracking.Repository;
using CallTracking.DB;
using MySql.Data.MySqlClient;
using System.Data;

namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
   public class VisitoirDeactivateJob:IJob
    {
       VisitsRepository _visitsRep = new VisitsRepository();
       Phone2ClientRepository _phone2clientrepository = new Phone2ClientRepository();
       private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

       ///<Summary>
       /// Gets the answer
       ///</Summary>
       public void Execute(IJobExecutionContext context)
       {
           try
           {
               DateTime DT_test = DateTime.Now.ToUniversalTime();

               CallTrackingDBContext _db = new CallTrackingDBContext();

               string interval = System.Configuration.ConfigurationManager.AppSettings["interval"].ToString();

               using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
               {
                   objConn.Open();
                   MySqlCommand objVisitorDeactivate = new MySqlCommand("VisitorDeactivate", objConn);
                   objVisitorDeactivate.CommandType = CommandType.StoredProcedure;
                   objVisitorDeactivate.Parameters.Add("@interval_", MySqlDbType.String).Value = interval;
                   objVisitorDeactivate.ExecuteNonQuery();
                   objConn.Close();
               }

               using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
               {
                   objConn.Open();
                   MySqlCommand objVisitorDeactivate = new MySqlCommand("VisitorDeactivate", objConn);
                   objVisitorDeactivate.CommandType = CommandType.StoredProcedure;
                   objVisitorDeactivate.Parameters.Add("@interval_", MySqlDbType.String).Value = interval; 
                   objVisitorDeactivate.ExecuteNonQuery();
                   objConn.Close();
               }

               _db.Database.Connection.Close();
               Log.Info("VisitoirDeactivateJob запущен!");

               Log.Info("VisitoirDeactivateJob успешно выполнен!");
               Console.WriteLine("VisitoirDeactivateJob is running....." + DateTime.Now.ToString());
           }
           catch (Exception ex)
           {
               Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
               Console.WriteLine("VisitoirDeactivateJob is running with error....." + DateTime.Now.ToString());

               Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
           }
       }
    }
}
