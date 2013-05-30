using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz.Impl;
using Quartz;
using CallTracking.DB;
using Common.Logging;
using CallTracking.Repository;
using Quartz.Server.AdditionalClasses;
using MySql.Data.MySqlClient;
using System.Data;
//using Quartz.Server.AdditionalClasses;
//using log4net;


namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    public class ReserveNumbersJob : IJob
    {

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        UserNumberRepository _usernumberRep;
        Phone2ClientRepository _phone2clientrepository;
        PhoneRepository _phoneRep;
        SendEmailJobInfoRepository _sendemailjobinfoRep;
        NumberCostRepository _numbercostRep;
        BalanceStateRepository _balancestateRep;
        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public ReserveNumbersJob() :base ()
        {
            _usernumberRep = new UserNumberRepository();
            _phone2clientrepository = new Phone2ClientRepository();
            _phoneRep = new PhoneRepository();
            _sendemailjobinfoRep = new SendEmailJobInfoRepository();
            _numbercostRep = new NumberCostRepository();
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Info("ReserveNumbersJob запущен!");
                IQueryable<Phone> freephones = _phoneRep.GetFreePhones();//выбираем свободные телефоны для выделения

                Dictionary<int, int> EnabledClients = Call_GetClientsForNumbersReserve();//получаем количество телефонов, которые можно выделить каждому клиенту ( учитывая состояние его баланса )

                if (_phone2clientrepository.FindBy(t => t.PhoneId == 1 && t.status == 1 && t.IsLast == true).Count() > 0)//если номера нужно выделять
                    ReserveNumber(freephones, EnabledClients);

                Log.Info("ReserveNumbersJob успешно выполнен! " + DateTime.Now.ToString());
                Console.WriteLine("ReserveNumbersJob is running.....");
            }
            catch (Exception ex)
            {
                Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                Console.WriteLine("ReserveNumbersJob is running with error! " + DateTime.Now.ToString());
                Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
               // Common.WriteExceptionToLog(ex, Log);
            }   
        }

        public Dictionary<int, int> Call_GetClientsForNumbersReserve()
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            int WarningsPeriod = int.Parse(System.Configuration.ConfigurationManager.AppSettings["WarningPeriod"].ToString());

            using (MySqlConnection objConn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CallTrackingDBContext"].ConnectionString))
            {
                objConn.Open();
                MySqlCommand Com_GetClientsForNumbersReserve = objConn.CreateCommand();
                Com_GetClientsForNumbersReserve.CommandType = CommandType.StoredProcedure;
                Com_GetClientsForNumbersReserve.CommandText = "GetClientsForNumbersReserve";
                Com_GetClientsForNumbersReserve.Parameters.Add("@WarningPeriod", MySqlDbType.Int32).Value = WarningsPeriod;
                MySqlDataReader DR = Com_GetClientsForNumbersReserve.ExecuteReader();//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (DR.HasRows)
                {
                    while (DR.Read())
                    {
                        result.Add(DR.GetInt32(0), DR.GetInt32(1));
                    }
                }
            }
            return result;
        }

        private void ReserveNumber(IQueryable<Phone> freephones, Dictionary<int, int> EnabledClients)
        {
            List<Phone> FreePhonesList = freephones.ToList<Phone>();
            List<Phone> PhonesForClient = new List<Phone>();//список выделенных для клиента номеров
            List<string> ClientsPhones = new List<string>();//список номеров для переадресации

            foreach (KeyValuePair<int, int> item in EnabledClients)
            {
                if (FreePhonesList.Count < item.Value)
                {
                    // не хватает номеров
                    //string AdminEmail = System.Configuration.ConfigurationSettings.AppSettings["AdminEmail"].ToString();
                    //SendEmailJobInfo new_sendemailjobinfo = new SendEmailJobInfo() { message = "Не хватает телефонных номеров! Необходимо заказать " + (item.Value - freephones.Count()).ToString() + " номеров!", date = DateTime.Now.ToUniversalTime(), status = false, subject = "Calltracking. Warning!", To = AdminEmail };
                    //_sendemailjobinfoRep.Add(new_sendemailjobinfo);
                    //_sendemailjobinfoRep.Save();
                    break;
                }
                else
                {
                    PhonesForClient = FreePhonesList.GetRange(0, item.Value);
                    FreePhonesList = FreePhonesList.GetRange(item.Value, FreePhonesList.Count - item.Value);
                    ClientsPhones = _usernumberRep.FindBy(t => t.ClientId == item.Key).Select(t => t.number).ToList<string>();//!!!!!!!!!!! выбираем номера для переадресации
                    List<Phone2Client> Phone2ClientListForClient = _phone2clientrepository.FindBy(t=>t.PhoneId==1 && t.status==1 && t.IsLast==true && t.ClientId==item.Key).Take(item.Value).ToList<Phone2Client>();

                    int indexforPhonesForClient = 0;//индекс необходим для прохода по списку уже выделенных для текущего клиента телефонов
                    foreach (Phone2Client p2c in Phone2ClientListForClient)//редактируем записи в БД
                    {
                        p2c.PhoneId = PhonesForClient[indexforPhonesForClient].Id;
                        _phone2clientrepository.Edit(p2c);
                        indexforPhonesForClient++;
                    }

                }
            }

            _phone2clientrepository.Save();
        }
    }
}