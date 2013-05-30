using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Common.Logging;
using CallTracking.DB;
using CallTracking.Repository;
using log4net;
using MySql.Data.MySqlClient;
using System.Data;

namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    public class SetCallInfoJob :  IJob
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        JobsInfoRepository _jobsinfoRep;
        CdrRepository _cdrRep;

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public SetCallInfoJob()
            : base()
        {
            _jobsinfoRep = new JobsInfoRepository();
            _cdrRep = new CdrRepository();
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                using (CallTrackingDBContext _db = new CallTrackingDBContext())
                {
                      Log.Info("SetCallInfoJob запущен!");
                    //получаем дату и время последнего запуска Job-а
                    string date = "";
                    JobsInfo JobsInfoQuery = _jobsinfoRep.GetLastActionDateForJob(Constants.SetCallInfoJob);//_db.jobsInfo.FirstOrDefault(t => t.job.Id == Constants.SetCallInfoJob);
                    if (JobsInfoQuery !=null)
                    {
                        //date = JobsInfoQuery.lastactiondate.ToString("YYYY-M-dd hh:");
                        date = JobsInfoQuery.lastactiondate.Year + "-" + JobsInfoQuery.lastactiondate.Month + "-"
                                + JobsInfoQuery.lastactiondate.Day + " " + JobsInfoQuery.lastactiondate.Hour + ":"
                                + JobsInfoQuery.lastactiondate.Minute + ":" + JobsInfoQuery.lastactiondate.Second;
                    }
                    else //если Job не запускался, то делаем работу за весь период
                    {
                       DateTime calldate = DateTime.Parse("2012-05-01");
                       date = calldate.Year + "-" + calldate.Month + "-"
                           + calldate.Day + " " + calldate.Hour + ":"
                            + calldate.Minute + ":" + calldate.Second;
                    }

                    //

                    using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
                    {
                        objConn.Open();
                        MySqlCommand objSetCallInfo = new MySqlCommand("SetCallInfo", objConn);
                        objSetCallInfo.CommandType = CommandType.StoredProcedure;
                        objSetCallInfo.CommandTimeout = int.MaxValue;
                        objSetCallInfo.Parameters.Add("@date_", MySqlDbType.Datetime).Value = DateTime.Parse(date);
                        objSetCallInfo.ExecuteNonQuery();
                        objConn.Close();
                    }


                    /*_db.Database.ExecuteSqlCommand(String.Format(@"update Cdr c, Visits v
                                                                   set c.VisitsId=v.Id
                                                                   where
                                                                   c.VisitsId is null
                                                                   and 
                                                                   c.calldate>Date('{0}')
                                                                   and
                                                                   c.dcontext='frw_in'
                                                                   and
                                                                   c.dst=v.UserNumber
                                                                   and
                                                                   c.calldate between v.datetime and v.lastupdate", date));// + interval 3 minute */

                    JobsInfo new_jobs_info = new JobsInfo() { status = true, lastactiondate = DateTime.Now.ToUniversalTime(), jobId = Constants.SetCallInfoJob };//поставить еще jobId! ( также в классе JobsInfo )
                    _jobsinfoRep.Add(new_jobs_info);
                    _jobsinfoRep.Save();

                    Console.WriteLine("SetCallInfoJob is running....." + DateTime.Now.ToString());
                    Log.Info("SetCallInfoJob успешно выполнен!");
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
               //JobsInfo new_jobs_info = new JobsInfo() { status = false, lastactiondate = DateTime.Now.ToUniversalTime(), ExceptionText = String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace) };//поставить еще jobId! ( также в классе JobsInfo )
               // _jobsinfoRep.Add(new_jobs_info);
               // _jobsinfoRep.Save();
                 Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                 Console.WriteLine("SetCallInfoJob is running with error....." + DateTime.Now.ToString());

                 Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
            }
        }
    }
}