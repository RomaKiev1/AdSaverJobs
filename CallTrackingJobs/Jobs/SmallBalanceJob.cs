using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Common.Logging;
using CallTracking.DB;
using System.Net.Mail;
using CallTracking.Repository;

namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    public class SmallBalanceJob : IJob
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        JobsInfoRepository _jobsinfoRep;
        PaymantsRepository _paymantsRep;
        ClientRepository _clientRep;
        SendEmailJobInfoRepository _sendemailjobinfoRep;

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public SmallBalanceJob()
            : base()
        {
            _jobsinfoRep = new JobsInfoRepository();
            _paymantsRep = new PaymantsRepository();
            _clientRep = new ClientRepository();
            _sendemailjobinfoRep = new SendEmailJobInfoRepository();
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Info("SmallBalanceJob запущен!");

                bool IsDebug = bool.Parse(System.Configuration.ConfigurationSettings.AppSettings["Debug"].ToString());

                var PaymantsQuery = _paymantsRep.GetCurentBalanceForAllClients();//неправильно работает с balancestaterepository

                var RegularExspences = _paymantsRep.GetCurentRemoveMoneyAverageForAllClients();

                Dictionary<int, double> RExspenses = new Dictionary<int, double>();
                foreach (var item in RegularExspences)
                {
                    RExspenses.Add(item.ClientId, item.RExpenses * (-1));
                }

                int WarningPeriod = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["WarningPeriod"]);

                foreach (var item in PaymantsQuery)
                {
                    if (RExspenses.ContainsKey(item.ClientId) && item.amount < RExspenses[item.ClientId] * WarningPeriod)//для тестирования в БД должны хранится реальные email !!!!!
                    {

                        string ClientsEmail = _clientRep.Find(item.ClientId).Email;
                        double Curent_Balance = item.amount;
                        string ToEmail = Constants.DebugEmail;
                        if (!IsDebug)
                            ToEmail = ClientsEmail;
                        string Message = String.Format("На вашем счету недостаточно средств. Баланс на {0} составляет {1}", DateTime.Now.ToShortDateString(), Curent_Balance);
                        _sendemailjobinfoRep.Add(new SendEmailJobInfo() { date = DateTime.Now.ToUniversalTime(), subject = "CallTracking", status = false, To = ToEmail, message = Message });
                        _sendemailjobinfoRep.Save();

                        //Send Message ( Begin )

                        /*MailMessage Message = new MailMessage();
                        Message.Subject = "subject";
                        Message.Body = String.Format("На вашем счету недостаточно средств. Баланс на {0} составляет {1}", DateTime.Now.ToShortDateString(), Curent_Balance);
                        Message.To.Add(new MailAddress(ClientsEmail));
                        System.Net.Mail.SmtpClient Smtp = new SmtpClient();
                        Smtp.EnableSsl = false;
                        Smtp.Send(Message);*/

                        //Send Message ( End )
                    }
                }

                //JobsInfo new_jobs_info = new JobsInfo() { status = true, lastactiondate = DateTime.Now, jobId=Constants.SmallbalanceJob };//поставить еще jobId! ( также в классе JobsInfo )
                //_jobsinfoRep.Add(new_jobs_info);
                //_jobsinfoRep.Save();

                Log.Info("SmallBalanceJob успешно выполнен!");
                Console.WriteLine("SmallBalanceJob is running....." + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                //    ex = ex.InnerException;
                //JobsInfo new_jobs_info = new JobsInfo() { status = false, lastactiondate = DateTime.Now.ToUniversalTime(), ExceptionText = String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace), jobId=Constants.SmallbalanceJob };//поставить еще jobId! ( также в классе JobsInfo )
                //_jobsinfoRep.Add(new_jobs_info);
                //_jobsinfoRep.Save();
                Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                Console.WriteLine("SmallBalanceJob is running with error....." + DateTime.Now.ToString());

                Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
            }
        }
    }
}