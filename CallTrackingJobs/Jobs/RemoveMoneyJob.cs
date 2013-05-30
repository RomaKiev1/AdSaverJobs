using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using CallTracking.DB;
using Common.Logging;
//using System.Transactions;
using CallTracking.Repository;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;

namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    ///
    public class BillSecForClientsInfo
    {
        public int ClientId { get; set; }
        public DateTime date { get; set; }
        public List<billsec_count_for_code> codes_billsec { get; set; }
    }

    public class ActivedClientsNumbersCountInfo
    {
        public int ClientId { get; set; }
        public DateTime date { get; set; }
        public int count { get; set; }
    }

    public class InfoForPaymants
    {
        public int ClientId { get; set; }
        public DateTime date { get; set; }
        public List<billsec_count_for_code> codes_billsec { get; set; }
        public int count { get; set; }
        public double sum { get; set; } 
    }

    public class billsec_count_for_code
    {
        public int code { get; set; }
        public int billsec { get; set; }
    }

    public class RemoveMoneyJob: IJob
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CdrRepository _cdrRep;
        JobsInfoRepository _jobsinfoRep;
        MinutesCostRepository _minutescostRep;
        NumberCostRepository _numbercostRep;
        Phone2ClientRepository _phone2clientRep;
        PaymantsRepository _paymantsRep;
        ClientRepository _clientRep;
        SendEmailJobInfoRepository _sendemailjobinfoRep;
        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public RemoveMoneyJob() : base()
        {
            _cdrRep = new CdrRepository();
            _jobsinfoRep = new JobsInfoRepository();
            _minutescostRep = new MinutesCostRepository();
            _numbercostRep = new NumberCostRepository();
            _paymantsRep = new PaymantsRepository();
            _phone2clientRep = new Phone2ClientRepository();
            _sendemailjobinfoRep = new SendEmailJobInfoRepository();
            _clientRep = new ClientRepository();
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        ///

        public DateTime [] GetDateFromFile()
        {
            DateTime [] result = new DateTime [2];
            System.IO.StreamReader file =
                    new System.IO.StreamReader(@"C:\Users\roma\Desktop\TestDateTimeFile.txt");
            string line = "";
            List<string> lol = new List<string>();
            while (file.Peek() >= 0)
            {
                line = file.ReadLine();
                lol.Add(line);
            }

            file.Close();

            DateTime start = DateTime.Parse(lol[0]);
            DateTime finish = DateTime.Parse(lol[1]);

            result[0] = start;
            result[1] = finish;

            return result;
        }

        public void AddOneHourToDatesFromFile()
        {
            System.IO.StreamReader file =
            new System.IO.StreamReader(@"C:\Users\roma\Desktop\TestDateTimeFile.txt");
            string line = "";
            List<string> lol = new List<string>();
            while (file.Peek() >= 0)
            {
                line = file.ReadLine();
                lol.Add(line);
            }

            file.Close();

            DateTime start = DateTime.Parse(lol[0]);
            DateTime finish = DateTime.Parse(lol[1]);

            DateTime startnext = start.AddHours(1);
            DateTime finishnext = finish.AddHours(1);

            lol.Clear();
            lol.Add(startnext.ToString());
            lol.Add(finishnext.ToString());

            StreamWriter NewFile = new StreamWriter(@"C:\Users\roma\Desktop\TestDateTimeFile.txt",false);
            foreach (string item in lol)
                NewFile.WriteLine(item);
            NewFile.Close();
        }

        public void Execute(IJobExecutionContext context)
        {
                try
                {
                    Console.WriteLine("RemoveMoneyJob is starting....." + DateTime.Now.ToString());
                      
                    Log.Info("RemoveMoneyJob запущен!");

                        List<GetBillsecForClientInfo> BillsecForClientInfoList_Curent = new List<GetBillsecForClientInfo>();
                        List<GetPhonesCountForClientInfo> PhonesCountForClientInfoList_Curent = new List<GetPhonesCountForClientInfo>();

                        ////выбираем дату последнего успешного запуска Job-а ( Начало )
                        DateTime lastactiondate = new DateTime();
                        JobsInfo JobsInfoQuery = _jobsinfoRep.GetLastActionDateForJob(Constants.RemoveMoneyJob);
                        if (JobsInfoQuery != null)
                            lastactiondate = JobsInfoQuery.lastactiondate;
                        // выбираем дату последнего успешного запуска Job-а ( Конец )
                        else
                            lastactiondate = DateTime.Parse("2013-03-15");

                        double numbercostforday = 0;

                        DateTime FinishDate = /*GetDateFromFile()[1];*/DateTime.Now.AddHours(-1).ToUniversalTime(); 
                        DateTime StartDate = /*GetDateFromFile()[0];*/ lastactiondate;
                        DateTime CurentDate = StartDate;

                        MinutesCost lastminutecostdate = null;
                        NumberCost lastnumbercostdate = null;

                        //StartDate = DateTime.Parse("29.04.2013 23:00:00");
                        //CurentDate = StartDate;
                        //FinishDate = DateTime.Parse("30.04.2013 00:00:00").AddHours(-1);

                        while (CurentDate <= FinishDate)//условие <= приводит к двойному выполнению Job-а для одного и того же клиента в сутки
                        {
                            string clients = GetClientsForRemoveMoney_(CurentDate/*DateTime.Parse("30.04.2013 0:00:00")*/);//получаем всех клиентов, для которых нужно списать деньги

                            if (!String.IsNullOrEmpty(clients))
                            {
                                        Console.WriteLine("Диапазон между startdate и finishdate: " + StartDate.ToString() + " - " + FinishDate.ToString());
                                        Console.WriteLine("Грабеж!!!!!!!!! Время грабежа: " + CurentDate.ToString() + " Клиенты: " + clients);
                                        List<BillSecForClientsInfo> billsecforclientsinfo = Call_GetBillSecForClients(CurentDate, clients);
                                        List<ActivedClientsNumbersCountInfo> activedclientsnumberscountinfo = Call_GetActivedClientsNumbers(CurentDate, clients);

                                        lastminutecostdate = _minutescostRep.GetAll().Where(t => t.lastupdate <= CurentDate).OrderByDescending(t => t.lastupdate).FirstOrDefault();
                                        lastnumbercostdate = _numbercostRep.GetAll().Where(t => t.lastupdate <= CurentDate).OrderByDescending(t => t.lastupdate).FirstOrDefault();

                                        List<InfoForPaymants> infoforpaymants = new List<InfoForPaymants>();

                                        var commondata = from b in billsecforclientsinfo
                                                         from a in activedclientsnumberscountinfo
                                                         where b.date == a.date && b.ClientId == a.ClientId
                                                         select new
                                                         {
                                                             ClientId = b.ClientId,
                                                             date = b.date,
                                                             count = a.count,
                                                             codes_billsec = b.codes_billsec
                                                         };

                                        var differentdata = from a in activedclientsnumberscountinfo
                                                            where !billsecforclientsinfo.Any(t => t.ClientId == a.ClientId && t.date == a.date)
                                                            select a;

                                        if (commondata.Count() > 0)
                                            foreach (var item in commondata)
                                                infoforpaymants.Add(new InfoForPaymants() { codes_billsec = item.codes_billsec, ClientId = item.ClientId, count = item.count, date = item.date });

                                        if (differentdata.Count() > 0)
                                            foreach (var item in differentdata)
                                                infoforpaymants.Add(new InfoForPaymants() { codes_billsec = null, count = item.count, date = item.date, ClientId = item.ClientId });

                                        int allbillsec = 0;//общее колическво секунд разговоров для клиента для всех кодов

                                        if (infoforpaymants.Count() > 0)
                                        {
                                            foreach (InfoForPaymants item in infoforpaymants)
                                            {
                                                numbercostforday = _numbercostRep.GetLastNumberCostToThisDate(item.date);//последняя стоимость номера за день
                                                if (item.codes_billsec != null)
                                                    item.sum = Math.Round(GetCommonBillsecCostForAllCodes(item.codes_billsec) + item.count * numbercostforday, 2);
                                                else
                                                    item.sum = Math.Round(item.count * numbercostforday, 2);

                                                if (item.codes_billsec != null)
                                                    allbillsec = item.codes_billsec.Sum(t => t.billsec);

                                                Paymant new_paymant = new Paymant() { ClientId = item.ClientId, date = item.date, Type = false, sum = -1 * item.sum, coment = "Снятие денег со счета. Время разговоров: " + allbillsec + " сек; Количество арендуемых номеров: " + item.count + " Время: " + DateTime.Now.ToString() };
                                                _paymantsRep.Add(new_paymant);
                                                _paymantsRep.Save();

                                                Client curent_client = _clientRep.Find(item.ClientId);
                                                if (curent_client != null)
                                                {
                                                    string message_ = "Уважаемый " + curent_client.Name + ", " + item.date.ToShortDateString() + " с вашего счета было снято " + item.sum + " грн за " + allbillsec + " секунд разговоров и за аренду телефонов в количестве " + item.count + ".";
                                                    _sendemailjobinfoRep.Add(new SendEmailJobInfo() { attemptcount = 0, date = DateTime.Now.ToUniversalTime(), message = message_, status = false, subject = "CallTracking Info", To = curent_client.Email });
                                                    _sendemailjobinfoRep.Save();
                                                }
                                                allbillsec = 0;
                                            }
                                }
                            }
                            CurentDate = CurentDate.AddHours(1);
                        }

                       // AddOneHourToDatesFromFile();

                        JobsInfo new_jobs_info = new JobsInfo() { status = true, lastactiondate = DateTime.Now.ToUniversalTime(), jobId = Constants.RemoveMoneyJob };//поставить еще jobId! ( также в классе JobsInfo )
                        _jobsinfoRep.Add(new_jobs_info);
                        _jobsinfoRep.Save();

                        Log.Info("RemoveMoneyJob успешно выполнен!");
                        Console.WriteLine("RemoveMoneyJob is finished....." + DateTime.Now.ToString());
                    }
                catch (Exception ex)
                {

                    while (ex.InnerException != null)
                        ex = ex.InnerException;
                   /* JobsInfo new_jobs_info = new JobsInfo() { status = false, lastactiondate = DateTime.Now.ToUniversalTime(), jobId = Constants.RemoveMoneyJob, ExceptionText = String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace) };//поставить еще jobId! ( также в классе JobsInfo )
                    _jobsinfoRep.Add(new_jobs_info);
                    _jobsinfoRep.Save();

                    Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                    Console.WriteLine("RemoveMoneyJob is running with error....." + DateTime.Now.ToString());
                    Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));*/
                    throw ex;
                }
        }

        public double GetCommonBillsecCostForAllCodes(List<billsec_count_for_code> codes_billsec)
        {
            double CommonBillsecCost = 0;
            double CurentCodeCost = 0;

            foreach (billsec_count_for_code item in codes_billsec)
            {
                CurentCodeCost = _minutescostRep.GetCodesCost(item.code);
                CommonBillsecCost += CurentCodeCost * item.billsec;
            }

            return CommonBillsecCost;
        }

        public List<BillSecForClientsInfo> Call_GetBillSecForClients(DateTime start, string clients)
        {
            List<BillSecForClientsInfo> result = new List<BillSecForClientsInfo>();

            using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
            {
                objConn.Open();
                MySqlCommand objComGetBillSecForClients = new MySqlCommand();
                objComGetBillSecForClients.Connection = objConn;
                objComGetBillSecForClients.CommandType = CommandType.StoredProcedure;
                objComGetBillSecForClients.CommandText = "GetBillSecForClients";
                objComGetBillSecForClients.Parameters.Add("@curentdate", MySqlDbType.DateTime).Value = start.Date;
                objComGetBillSecForClients.Parameters.Add("@clients", MySqlDbType.VarChar).Value = clients;
                MySqlDataReader reader = objComGetBillSecForClients.ExecuteReader();
                if (reader.HasRows)
                {
                    int CurentClientId = 0;
                    DateTime CurentDate = new DateTime();
                    int PrevClientId = 0;
                    DateTime PrevDate = new DateTime();
                    BillSecForClientsInfo Curent_BillSecForClientsInfo = new BillSecForClientsInfo();
                    bool isfirstelement = true;

                    while (reader.Read())
                    {
                        CurentDate = reader.GetDateTime(0);
                        CurentClientId = reader.GetInt32(2);
                        if ((CurentDate == PrevDate) && (CurentClientId == PrevClientId))// заполняем список стоимости по кодам, если тот же самый клиент и та же дата
                        {
                            Curent_BillSecForClientsInfo.codes_billsec.Add(new billsec_count_for_code() { code = reader.GetInt32(3), billsec = reader.GetInt32(1) });
                        }
                        else//если новый клиент и новая дата
                        {
                            if (!isfirstelement)//если это не первый создаваемый обьект BillSecForClientsInfo
                            {
                                result.Add(Curent_BillSecForClientsInfo);
                            }
                            Curent_BillSecForClientsInfo = new BillSecForClientsInfo() { date = reader.GetDateTime(0), ClientId = reader.GetInt32(2) };
                            Curent_BillSecForClientsInfo.codes_billsec = new List<billsec_count_for_code>();
                            Curent_BillSecForClientsInfo.codes_billsec.Add(new billsec_count_for_code() { code = reader.GetInt32(3), billsec = reader.GetInt32(1) });
                            PrevDate = CurentDate;
                            PrevClientId = CurentClientId;
                            isfirstelement = false;
                        }
                    }
                    result.Add(Curent_BillSecForClientsInfo);//добавляем в список последний созданный обьект
                }
                objConn.Close();
            }

            return result;
        }

        public List<ActivedClientsNumbersCountInfo> Call_GetActivedClientsNumbers(DateTime start, string clients)
        {
            List<ActivedClientsNumbersCountInfo> result = new List<ActivedClientsNumbersCountInfo>();

            using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
            {
                objConn.Open();
                MySqlCommand objComGetActivedClientsNumbers = new MySqlCommand();
                objComGetActivedClientsNumbers.Connection = objConn;
                objComGetActivedClientsNumbers.CommandType = CommandType.StoredProcedure;
                objComGetActivedClientsNumbers.CommandText = "GetActivedClientsNumbers";
                objComGetActivedClientsNumbers.Parameters.Add("@curentdate", MySqlDbType.DateTime).Value = start;
                objComGetActivedClientsNumbers.Parameters.Add("@clients", MySqlDbType.VarChar).Value = clients;
                MySqlDataReader reader = objComGetActivedClientsNumbers.ExecuteReader();
                if (reader.HasRows)
                    while (reader.Read())
                        result.Add(new ActivedClientsNumbersCountInfo() { ClientId = reader.GetInt32(0), date = reader.GetDateTime(1), count = reader.GetInt32(2) });
            }

            return result;
        }

        public string GetClientsForRemoveMoney_(DateTime curent_date)
        {
           List<Client> allclientslist = new List<Client>();
                IQueryable<Client> allclientsquery = _clientRep.GetAll();

                if (allclientsquery.Count() > 0)
                    allclientslist = allclientsquery.ToList<Client>();

                string result = "";
                bool isfirstelement = true;

          //  int [] mass_temp = {9,10,11};

                foreach (Client client in allclientslist)
                {
                   // if (mass_temp.Contains(client.Id))
                   // {
                        if (ExecuteRemoveMoneyForThisClient_(client.timezone.ZoneId, curent_date))
                            if (isfirstelement)
                            {
                                result += client.Id;
                                isfirstelement = false;
                            }
                            else
                                result += "," + client.Id;
                   // }
                }
                return result;
        }

        public bool ExecuteRemoveMoneyForThisClient_(string ZoneId, DateTime curent_date)
        {
            DateTime clientsdatetimenow = TimeZoneInfo.ConvertTimeFromUtc(curent_date, TimeZoneInfo.FindSystemTimeZoneById(ZoneId));
           // if ((clientsdatetimenow.Hour == 0) && (clientsdatetimenow.Minute >= 0) && (clientsdatetimenow.Minute <=59))
            if (clientsdatetimenow.Hour == 0)
            {
                return true;
            }
            else
                return false;
        }
    }
}