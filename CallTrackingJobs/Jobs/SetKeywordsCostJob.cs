using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Server.AdditionalClasses;
using CallTracking.DB;
using CallTracking.Repository;
using log4net;
using System.Threading;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Data;

namespace Quartz.Server.Jobs
{
    class SetKeywordsCostJob : IJob
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private VisitsRepository _visitsRep = new VisitsRepository();

        private ClientRepository _clientRep = new ClientRepository();

        private JobsInfoRepository _jobsinfoRep = new JobsInfoRepository();

        private SourceRepository _sourceRep = new SourceRepository();

        private SourceKeywordsRepository _sourcekeywordsRep = new SourceKeywordsRepository();

        public void Execute(IJobExecutionContext context)
        {
            Log.Info("SetKeywordsCostJob запущен!");

            Console.WriteLine("SetKeywordsCostJob was started! "+DateTime.Now);

            IQueryable<Client> clients = _clientRep.GetAll();
            DateTime StartDate = new DateTime();
            DateTime FinishDate = new DateTime();

            JobsInfo JobsInfoQuery = _jobsinfoRep.GetLastActionDateForJob(Constants.SetKeywordsCostJob);
            if (JobsInfoQuery != null)
            {
                StartDate = JobsInfoQuery.lastactiondate.AddDays(-1);//AddDays(-1) для учета часовых поясов
                FinishDate = DateTime.Now.ToUniversalTime().AddDays(2);//AddDays(1) для учета часовых поясов
            }
            else
            {
                if (_visitsRep.GetAll().Count() > 0)
                {
                    StartDate = DateTime.Parse("2013-04-28");
                    FinishDate =DateTime.Now.ToUniversalTime().AddDays(2);//AddDays(1) для учета часовых поясов
                }
            }

            foreach (Client item in clients)
            {
                Console.WriteLine("SetKeywordsCostJob Клиент " + item.Name + " Время старта: " + DateTime.Now); 
                    Execute_KeywordsCost_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);
                    Console.WriteLine("SetKeywordsCostJob Клиент " + item.Name + " Время финиша: " + DateTime.Now); 
            }

            _jobsinfoRep.Add(new JobsInfo() { jobId = Constants.SetKeywordsCostJob, lastactiondate = DateTime.Now.AddDays(-1).ToUniversalTime(), status = true });
            _jobsinfoRep.Save();

            Console.WriteLine("SetKeywordsCostJob is finished! "+DateTime.Now);

        }
        
       public void Execute_KeywordsCost_Query(DateTime StartDate, DateTime FinishDate, string LoginGoogleAnalitics, string PasswordGoogleAnalitics, string ids, int ClientId_)
        {
            try
            {
                int p = 0;
                DateTime curent_datetime = StartDate;
                DateTime next_datetime = new DateTime();
                while (curent_datetime < FinishDate)
                {
                    next_datetime = curent_datetime.AddDays(1);
                    List<GoogleAnalyticsKeywordsCostDataRow> GAData = new List<GoogleAnalyticsKeywordsCostDataRow>();
                    DataFeed_KeywordsCost DFE = new DataFeed_KeywordsCost(curent_datetime, next_datetime, LoginGoogleAnalitics, PasswordGoogleAnalitics, ids);
                    GAData = DFE.GetData();
                    List<Visit> list = new List<Visit>();
                    foreach (GoogleAnalyticsKeywordsCostDataRow row in GAData)
                    {
                        Execute_SetVisitsCost(curent_datetime.Date, next_datetime.Date, ids, ClientId_, row);
                    }
                    curent_datetime = curent_datetime.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       public void Execute_SetVisitsCost(DateTime StartDate, DateTime FinishDate, string ids, int ClientId_, GoogleAnalyticsKeywordsCostDataRow row)
       {
           try
           {
               using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
               {
                   //Source source = _sourceRep.FindBy(t => t.Name == row.adGroup && t.ClientId == ClientId_).FirstOrDefault();
                   //SourceKeyword sourcekeyword = _sourcekeywordsRep.FindBy(t => t.ClientId == ClientId_ && t.Keyword == row.keyword && t.SourceId==source.Id).FirstOrDefault();

                   /* objConn.Open();
                    MySqlCommand objSetVisitsCost = new MySqlCommand();
                    objSetVisitsCost.Connection = objConn;
                    objSetVisitsCost.CommandType = CommandType.StoredProcedure;
                    objSetVisitsCost.CommandText = "SetVisitsCost";
                    objSetVisitsCost.Parameters.Add("@startdate", MySqlDbType.DateTime).Value = StartDate;
                    objSetVisitsCost.Parameters.Add("@finishdate", MySqlDbType.VarChar).Value = FinishDate;
                    objSetVisitsCost.Parameters.Add("@ClientId_", MySqlDbType.Int32).Value = ClientId_;
                    objSetVisitsCost.Parameters.Add("@sourcekeyword", MySqlDbType.VarChar).Value = row.keyword.Trim();
                    objSetVisitsCost.Parameters.Add("@sourcename", MySqlDbType.VarChar).Value = row.adGroup.Trim();
                    objSetVisitsCost.Parameters.Add("@VisitsCost_", MySqlDbType.Double).Value = row.cost;

                    objSetVisitsCost.ExecuteNonQuery();
                    objConn.Close();*/
                   
                   //IQueryable<Visit> visits = _visitsRep.FindBy(t => t.sourcekeyword.Keyword == row.keyword && t.Source.Name == row.adGroup && t.clientId == ClientId_ && t.datetime >=StartDate && t.datetime<FinishDate);
                   //if (visits.Count() > 0)
                   //{
                   //    foreach (Visit item in visits)
                   //    {
                   //        item.VisitsCost = row.cost;
                   //        _visitsRep.Edit(item);
                   //    }
                   //    _visitsRep.Save();
                   //}

                   objConn.Open();
                   MySqlCommand objSetVisitsCost = new MySqlCommand();
                   objSetVisitsCost.Connection = objConn;

                   objSetVisitsCost.CommandText = @"update Visits v left join sources s on s.Id=v.SourceId
left join sourcekeywords sk on sk.Id=v.sourcekeywordId
set VisitsCost = @VisitsCost_
where  v.clientId=@clientId_
and sk.keyword = @keyword_
and s.name=@name_
and v.SourceId is not null
and v.sourcekeywordId is not null
and Date(datetime)=Date(@start_)";

                   objSetVisitsCost.Parameters.Add("@VisitsCost_", MySqlDbType.Double).Value = row.cost;
                   objSetVisitsCost.Parameters.Add("@clientId_", MySqlDbType.Int32).Value = ClientId_;
                   objSetVisitsCost.Parameters.Add("@keyword_", MySqlDbType.VarChar).Value = row.keyword.Trim();
                   objSetVisitsCost.Parameters.Add("@name_", MySqlDbType.VarChar).Value = row.adGroup.Trim();
                   objSetVisitsCost.Parameters.Add("@start_", MySqlDbType.DateTime).Value = StartDate;

                   objSetVisitsCost.ExecuteNonQuery();
                   objConn.Close();
               }
            }
           catch (Exception ex)
           {
               while (ex.InnerException != null)
                   ex = ex.InnerException;
               Console.WriteLine("Message: " + ex.Message + "   TargetSite: " + ex.TargetSite + "   StackTrace: " + ex.StackTrace);
           }
        }
      }


    }
