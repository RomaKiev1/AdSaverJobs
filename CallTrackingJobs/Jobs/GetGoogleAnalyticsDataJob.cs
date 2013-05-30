using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Analytics;
using Quartz.Server.AdditionalClasses;
using CallTracking.DB;
using CallTracking.Repository;

namespace Quartz.Server.Jobs
{
    public class GetGoogleAnalyticsDataJob:IJob
    {
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
  
        private VisitsRepository _visitsRep = new VisitsRepository();

        public ClientRepository _clientRep = new ClientRepository();

        public SourceRepository _sourceRep = new SourceRepository();

        public SourceKeywordsRepository _sourcekeywordsRep = new SourceKeywordsRepository();

        public SourceSiteRepository _sourcesiteRep = new SourceSiteRepository();

        public SourceTypeRepository _sourcetypeRep = new SourceTypeRepository();

        JobsInfoRepository _jobsinfoRep;
        

        public GetGoogleAnalyticsDataJob()
            : base()
        {
            _jobsinfoRep = new JobsInfoRepository();
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
              //  Log.Info("GetGoogleAnalyticsDataJob запущен!");

                Console.WriteLine("GetGoogleAnalyticsDataJob is started....." + DateTime.Now.ToString());

                IQueryable<Client> clients = _clientRep.GetAll();
                DateTime StartDate = new DateTime();
                DateTime FinishDate = new DateTime();

                JobsInfo JobsInfoQuery = _jobsinfoRep.GetLastActionDateForJob(Constants.GetGoogleAnalyticsDataJob);
                if (JobsInfoQuery != null) 
                {
                    StartDate = /*DateTime.Parse("2013-04-01");*/JobsInfoQuery.lastactiondate.AddDays(-1);//AddDays(-1) для учета часовых поясов
                    FinishDate = DateTime.Now.ToUniversalTime().AddDays(2);//AddDays(1) для учета часовых поясов
                }
                else
                {
                    if (_visitsRep.GetAll().Count() > 0)
                    {
                        StartDate = DateTime.Parse("2012-05-01");
                        FinishDate = DateTime.Now.ToUniversalTime().AddDays(2);//AddDays(1) для учета часовых поясов
                    }
                }

                foreach (Client item in clients)
                {
                    Console.WriteLine("GetGoogleAnalyticsDataJob Клиент " + item.Name + " Время старта: " + DateTime.Now); 
                        if (item.Id != 6)
                        {
                            if (!(String.IsNullOrEmpty(item.LoginGoogleAnalitics)) && (!String.IsNullOrEmpty(item.PasswordGoogleAnalitics)) && (!String.IsNullOrEmpty(item.ids.ToString())) && (item.ids != 0))
                            {
                                Execute_NonPaidSearchTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);

                                Execute_PaidSearchTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);

                                Execute_ReferralTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);

                                Execute_DirectTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);
                            }
                        }
                        else
                        {
                            if (StartDate >= DateTime.Parse("2013-05-25"))//для клиента 6 выполняем после 25-го числа 
                            {
                                if (!(String.IsNullOrEmpty(item.LoginGoogleAnalitics)) && (!String.IsNullOrEmpty(item.PasswordGoogleAnalitics)) && (!String.IsNullOrEmpty(item.ids.ToString())) && (item.ids != 0))
                                {
                                    Execute_NonPaidSearchTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);

                                    Execute_PaidSearchTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);

                                    Execute_ReferralTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);

                                    Execute_DirectTraffic_Query(StartDate, FinishDate, item.LoginGoogleAnalitics, item.PasswordGoogleAnalitics, item.ids.ToString(), item.Id);
                                }
                            }
                        }
                        Console.WriteLine("GetGoogleAnalyticsDataJob Клиент " + item.Name + " Время финиша: " + DateTime.Now); 
                }

                _jobsinfoRep.Add(new JobsInfo() { jobId = Constants.GetGoogleAnalyticsDataJob, lastactiondate = DateTime.Now.AddDays(-1).ToUniversalTime(), status = true });
                _jobsinfoRep.Save();

                       //   Log.Info("GetGoogleAnalyticsDataJob успешно выполнен!");
                          Console.WriteLine("GetGoogleAnalyticsDataJob is finished....." + DateTime.Now.ToString());
             }
            catch(Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                //Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                //  Console.WriteLine("GetGoogleAnalyticsDataJob is running with error....." + DateTime.Now.ToString());
                //  Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                throw ex;
            }
        }

        public void Execute_NonPaidSearchTraffic_Query(DateTime StartDate, DateTime FinishDate, string LoginGoogleAnalitics, string PasswordGoogleAnalitics, string ids, int ClientId_)
        {
            try
            {
                List<GoogleAnalyticsNonPaidSearchTrafficDataRow> GAData = new List<GoogleAnalyticsNonPaidSearchTrafficDataRow>();
                DataFeed_NonPaidSearchTraffic DFE = new DataFeed_NonPaidSearchTraffic(StartDate, FinishDate, LoginGoogleAnalitics, PasswordGoogleAnalitics, ids);
                GAData = DFE.GetData();

                GoogleAnalyticsNonPaidSearchTrafficDataRow r = GAData.Where(t => t.eventLabel == "40885").FirstOrDefault();

                int? sourcekeywordId = null;
                int? sourcesiteId = null;
                int count = 0;
                foreach (GoogleAnalyticsNonPaidSearchTrafficDataRow row in GAData)
                {
                    string[] noresult = { "(none)", "(not set)", "(not provided)" };

                    int VisitsId = int.Parse(row.eventLabel);

                    if (VisitsId > -1)
                    {
                        int? Source_Id = null;
                        if (!noresult.Contains(row.keyword.Trim()))
                            sourcekeywordId = AddSourceKeywordIfNotExist(row.keyword.Trim(), null, ClientId_, out Source_Id);

                        if (!noresult.Contains(row.sourcesite.Trim()))
                            sourcesiteId = AddSourceSiteIfExist(row.sourcesite, ClientId_);

                        Visit curent_visit = _visitsRep.FindBy(t => t.Id == VisitsId).FirstOrDefault();
                        if (curent_visit != null)
                        {
                            curent_visit.SourceId = Source_Id;
                            curent_visit.sourcekeywordId = sourcekeywordId;
                            curent_visit.sourcetypeId = Constants.SeoSourceId;
                            curent_visit.sourcesiteId = sourcesiteId;
                           // curent_visit.TimeOnSite = Convert.ToInt32(double.Parse(row.timeOnSite.Replace('.', ',')));
                           // curent_visit.IsNewVisitor = bool.Parse((row.newVisits == "0") ? "false" : "true");
                            _visitsRep.Edit(curent_visit);
                            _visitsRep.Save();
                        }

                        sourcekeywordId = null;
                        sourcesiteId = null;
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                Console.WriteLine("Message: " + ex.Message + " Source: " + ex.Source + " TargetSite: " + ex.TargetSite);
                throw ex;
            }

        }

        public void Execute_PaidSearchTraffic_Query(DateTime StartDate, DateTime FinishDate, string LoginGoogleAnalitics, string PasswordGoogleAnalitics, string ids, int ClientId_)
        {
            try
            {
                List<GoogleAnalyticsPaidSearchTrafficDataRow> GAData = new List<GoogleAnalyticsPaidSearchTrafficDataRow>();
                DataFeed_PaidSearchTraffic DFE = new DataFeed_PaidSearchTraffic(StartDate, FinishDate, LoginGoogleAnalitics, PasswordGoogleAnalitics, ids);
                GAData = DFE.GetData();

                int? sourcekeywordId = null;
                int? sourcesiteId = null;
                int? sourceId = null;

                foreach (GoogleAnalyticsPaidSearchTrafficDataRow row in GAData)
                {
                    string[] noresult = { "(none)", "(not set)", "(not provided)" };
                    int VisitsId = int.Parse(row.eventLabel);
                    if (VisitsId > -1)
                    {
                        if (!noresult.Contains(row.sourcesite.Trim()))
                            sourcesiteId = AddSourceSiteIfExist(row.sourcesite.Trim(), ClientId_);

                        if (!noresult.Contains(row.adGroup.Trim()))
                            sourceId = AddSourceIfNotExist(ClientId_, row.adGroup, Constants.AddwordsSourceId);

                        if (!noresult.Contains(row.keyword.Trim()))
                        {
                            int? Source_Id = null;
                            sourcekeywordId = AddSourceKeywordIfNotExist(row.keyword.Trim(), sourceId, ClientId_, out Source_Id);
                        }
                    }

                    Visit curent_visit = _visitsRep.FindBy(t => t.Id == VisitsId).FirstOrDefault();
                    if (curent_visit != null)
                    {
                        curent_visit.SourceId = sourceId;
                        curent_visit.sourcekeywordId = sourcekeywordId;
                        curent_visit.sourcetypeId = Constants.AddwordsSourceId;
                        curent_visit.sourcesiteId = sourcesiteId;
                      //  curent_visit.TimeOnSite = Convert.ToInt32(double.Parse(row.timeOnSite.Replace('.', ',')));
                      //  curent_visit.IsNewVisitor = bool.Parse((row.newVisits == "0") ? "false" : "true");
                        _visitsRep.Edit(curent_visit);
                        _visitsRep.Save();
                    }

                    sourcekeywordId = null;
                    sourcesiteId = null;
                    sourceId = null;
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                Console.WriteLine("Message: " + ex.Message + " Source: " + ex.Source + " TargetSite: " + ex.TargetSite);
                throw ex;
            }
        }

        //DataFeed_ReferralTraffic

        public void Execute_ReferralTraffic_Query(DateTime StartDate, DateTime FinishDate, string LoginGoogleAnalitics, string PasswordGoogleAnalitics, string ids, int ClientId_)
        {
            try
            {
                List<GoogleAnalyticsReferralTrafficDataRow> GAData = new List<GoogleAnalyticsReferralTrafficDataRow>();
                DataFeed_ReferralTraffic DFE = new DataFeed_ReferralTraffic(StartDate, FinishDate, LoginGoogleAnalitics, PasswordGoogleAnalitics, ids);
                GAData = DFE.GetData();

                int? sourcesiteId = null;

                foreach (GoogleAnalyticsReferralTrafficDataRow row in GAData)
                {
                    string[] noresult = { "(none)", "(not set)", "(not provided)" };
                    int VisitsId = int.Parse(row.eventLabel);
                    if (VisitsId > -1)
                    {
                        if (!noresult.Contains(row.sourcesite.Trim()))
                            sourcesiteId = AddSourceSiteIfExist(row.sourcesite.Trim(), ClientId_);

                        Visit curent_visit = _visitsRep.FindBy(t => t.Id == VisitsId).FirstOrDefault();
                        if (curent_visit != null)
                        {
                            curent_visit.SourceId = null;
                            curent_visit.sourcekeywordId = null;
                            curent_visit.sourcetypeId = Constants.ReferralTrafficSourceId;
                            curent_visit.sourcesiteId = sourcesiteId;
                           // curent_visit.TimeOnSite = Convert.ToInt32(double.Parse(row.timeOnSite.Replace('.', ',')));
                           // curent_visit.IsNewVisitor = bool.Parse((row.newVisits == "0") ? "false" : "true");
                            _visitsRep.Edit(curent_visit);
                            _visitsRep.Save();
                        }

                        sourcesiteId = null;
                    }
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                Console.WriteLine("Message: " + ex.Message + " Source: " + ex.Source + " TargetSite: " + ex.TargetSite);
                throw ex;
            }
        }


        public void Execute_DirectTraffic_Query(DateTime StartDate, DateTime FinishDate, string LoginGoogleAnalitics, string PasswordGoogleAnalitics, string ids, int ClientId_)
        {
            try
            {
                List<GoogleAnalyticsDirectTrafficDataRow> GAData = new List<GoogleAnalyticsDirectTrafficDataRow>();
                DataFeed_DirectTraffic DFE = new DataFeed_DirectTraffic(StartDate, FinishDate, LoginGoogleAnalitics, PasswordGoogleAnalitics, ids);
                GAData = DFE.GetData();

                foreach (GoogleAnalyticsDirectTrafficDataRow row in GAData)
                {
                    int VisitsId = int.Parse(row.eventLabel);
                    if (VisitsId > -1)
                    {
                        Visit curent_visit = _visitsRep.FindBy(t => t.Id == VisitsId).FirstOrDefault();
                        if (curent_visit != null)
                        {
                            curent_visit.SourceId = null;
                            curent_visit.sourcekeywordId = null;
                            curent_visit.sourcetypeId = Constants.DirectTraffikSourceId;
                            curent_visit.sourcesiteId = null;
                          //  curent_visit.TimeOnSite = Convert.ToInt32(double.Parse(row.timeOnSite.Replace('.', ',')));
                        //    curent_visit.IsNewVisitor = bool.Parse((row.newVisits == "0") ? "false" : "true");
                            _visitsRep.Edit(curent_visit);
                            _visitsRep.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                Console.WriteLine("Message: " + ex.Message + " Source: " + ex.Source + " TargetSite: " + ex.TargetSite);
                throw ex;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private int AddSourceKeywordIfNotExist(string keyword, int? sourceId, int Client_Id, out int? Source_Id)
        {
            IQueryable<SourceKeyword> sourcekeyword = null;
            if (sourceId == null)//случай для organic
                sourcekeyword = from sk in _sourcekeywordsRep.GetAll()//_sourcekeywordsRep.FindBy(t=>t.ClientId==Client_Id && t.Keyword==keyword && t.)
                                where sk.Keyword == keyword
                                      && sk.ClientId == Client_Id
                                      && (sk.Source.SourceTypeId==Constants.SeoSourceId || sk.Source.SourceTypeId==null)
                                select sk;
            else//случай для cpc
                sourcekeyword = _sourcekeywordsRep.FindBy(t => t.Keyword == keyword && (sourceId == null ? t.SourceId == null : t.SourceId == sourceId) /*&& (sourcetype == null ? t.SourceTypeId == null : t.SourceTypeId == sourcetype)*/ && t.ClientId == Client_Id);

            if (sourcekeyword.Count() == 0)
            {
                SourceKeyword new_sourcekeyword = new SourceKeyword() { Keyword = keyword, ClientId = Client_Id, SourceId = sourceId };
                _sourcekeywordsRep.Add(new_sourcekeyword);
                _sourcekeywordsRep.Save();
                Source_Id = null;
                return new_sourcekeyword.id;
            }
            Source_Id = sourcekeyword.First().SourceId;
            return sourcekeyword.First().id;
        }

        private int AddSourceIfNotExist(int ClientId, string sourcename, int? sourcetypeid)
        {
            Source new_source = _sourceRep.FindBy(t => t.Name == sourcename && t.ClientId == ClientId && (sourcetypeid == null ? t.SourceTypeId == null : t.SourceTypeId == sourcetypeid)).FirstOrDefault();

            if (new_source == null)
            {
                new_source = new Source() { Name = sourcename, ClientId = ClientId, SourceTypeId = sourcetypeid };
                _sourceRep.Add(new_source);
                _sourceRep.Save();
            }

            return new_source.Id;
        }

        private int AddSourceSiteIfExist(string source, int? Client_Id)
        {
            SourceSite new_sourcesite = _sourcesiteRep.FindBy(t => t.Site == source && t.ClientId==Client_Id).FirstOrDefault();
            if (new_sourcesite == null)
            {
                new_sourcesite = new SourceSite() { Site = source, ClientId=Client_Id };
                _sourcesiteRep.Add(new_sourcesite);
                _sourcesiteRep.Save();
            }
            return new_sourcesite.Id;
        }

        private int? AddSourceTypeIfNotExist(string type)
        {
            SourceType sourcetype = _sourcetypeRep.FindBy(t => t.Name == type).FirstOrDefault();
            if (sourcetype == null)
            {
                sourcetype = new SourceType() { Name = type };
                _sourcetypeRep.Add(sourcetype);
                _sourcetypeRep.Save();
            }
            return sourcetype.id;
        }
    }
}
