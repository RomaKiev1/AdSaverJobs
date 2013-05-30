using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CallTracking.DB;
using CallTracking.Repository;
using Quartz.Server.AdditionalClasses;

namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    public class RelieveNumbersJob :IJob
    {
        Phone2ClientRepository _phone2clientrepository;

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public RelieveNumbersJob() : base() 
        {
            _phone2clientrepository = new Phone2ClientRepository();
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Info("RelieveNumbersJob запущен!");
                IQueryable<Phone2Client> P2CForRelieve = _phone2clientrepository.GetRowsForRelieveNumbers();
                List<Phone2Client> InfoForAsterisk = new List<Phone2Client>();
                if (P2CForRelieve.Count() > 0)
                {
                    InfoForAsterisk = P2CForRelieve.ToList<Phone2Client>();//список телефонов для Asterisk
                }

                if (InfoForAsterisk.Count() > 0)
                {
                    if (Asterisk.RelieveNumbers(InfoForAsterisk.Select(t => t.phone.Phone_Value).ToList<string>()))
                    {
                        foreach (Phone2Client item in InfoForAsterisk)
                        {
                            item.status = 0;
                            _phone2clientrepository.Edit(item);
                        }
                    }
                    else
                    {
                        Log.Error("Метод Asterisk RelieveNumbers вернул ошибку!");
                    }

                    _phone2clientrepository.Save();
                }

                Log.Info("RelieveNumbersJob успешно выполнен!");
                Console.WriteLine("RelieveNumbersJob is running....." + DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                Console.WriteLine("RelieveNumbersJob is running with error! " + DateTime.Now.ToString());
                Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
            }
        }
    }
}
