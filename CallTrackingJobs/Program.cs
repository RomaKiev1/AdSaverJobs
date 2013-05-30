using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Analytics;
using Google.GData.Analytics;
using Google.GData.Client;
using Google.GData.Extensions;
using Quartz;
using Quartz.Impl;


namespace CallTrackingJobs
{
    class Program
    {

        static void Main(string[] args)
        {

          //  System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;

            Process thisProc = Process.GetCurrentProcess();
            thisProc.PriorityClass = ProcessPriorityClass.Idle;

                log4net.Config.XmlConfigurator.Configure();

                // construct a scheduler factory
                ISchedulerFactory schedFact = new StdSchedulerFactory();

                //  double v = double.Parse("603,0");

                //IJobDetail jobDetail1 = JobBuilder.Create(typeof(Quartz.Server.Jobs.SendEmailJob))
                // .WithIdentity("job1", "group1")
                //.RequestRecovery(true)
                // .Build();

                //ITrigger trigger1 = TriggerBuilder.Create()
                //    .WithIdentity("trigger1", "group1")
                //    .WithCronSchedule(/*"0/60 * * * * ?"*/"*/30 * * * * ?")//каждые 30 секунд
                //    .Build();

                // get a scheduler
                IScheduler sched = schedFact.GetScheduler();
                // sched.ScheduleJob(jobDetail1, trigger1);
                sched.Start();
        }

    }
}
