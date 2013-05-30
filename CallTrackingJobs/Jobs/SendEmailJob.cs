using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using CallTracking.DB;
using CallTracking.Repository;

namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    public class SendEmailJob : IJob
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly NameValueCollection AppConfig = ConfigurationSettings.AppSettings;
        SendEmailJobInfoRepository _sendemailjobinfoRep;

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public SendEmailJob()
            : base()
        {
            _sendemailjobinfoRep = new SendEmailJobInfoRepository();
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Info("SendEmailJob запущен!");
                //IQueryable<SendEmailJobInfo> SendEmailJobInfoQuery = _sendemailjobinfoRep.GetAll();
                IQueryable<SendEmailJobInfo> SendEmailJobInfoQuery = _sendemailjobinfoRep.FindBy(t => t.status == false && t.attemptcount < 10);
                if (SendEmailJobInfoQuery.Count() > 0)
                {
                    System.Net.Mail.SmtpClient Smtp = new SmtpClient(/*"smtp.mail.ru", 25*/);
                    MailMessage Message = new MailMessage();

                    bool IsDebug = bool.Parse(System.Configuration.ConfigurationSettings.AppSettings["Debug"].ToString());

                    foreach (SendEmailJobInfo item in SendEmailJobInfoQuery)
                    {
                        try
                        {
                            string MessageTo = "";
                            if (IsDebug)
                            {
                                MessageTo = System.Configuration.ConfigurationSettings.AppSettings["AdminEmail"].ToString();
                            }
                            else
                            {
                                MessageTo = item.To;
                            }

                            string from = System.Configuration.ConfigurationManager.AppSettings["from"].ToString().Trim();

                            SendEmail(from, MessageTo, "CallTracking", "HTML", item.message);

                            item.attemptcount = item.attemptcount + 1;
                            item.status = true;
                            _sendemailjobinfoRep.Edit(item);
                        }
                        catch
                        {
                            item.attemptcount = item.attemptcount + 1;
                            _sendemailjobinfoRep.Edit(item);
                        }
                    }
                    _sendemailjobinfoRep.Save();

                    Log.Info("SendEmailJob успешно выполнен!");
                }
                Console.WriteLine("SendEmailJob is running....." + DateTime.Now.ToString());
            }
            catch(Exception ex)
            {
                Log.Error(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
                Console.WriteLine("SendEmailJob is running with error! " + DateTime.Now.ToString());

                Console.WriteLine(String.Format(@"Message: {0}, Source: {1}, TargetSite: {2}, Trace: {3} ", ex.Message, ex.Source, ex.TargetSite, ex.StackTrace));
            }
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public static Boolean SendEmail(String From, String To, String Subject, String Text = null, String HTML = null, String emailReplyTo = null, String returnPath = null)
        {
            if (Text != null && HTML != null)
            {
                String from = From;

                List<String> to
                    = To
                    .Replace(", ", ",")
                    .Split(',')
                    .ToList();

                Destination destination = new Destination();
                destination.WithToAddresses(to);
                //destination.WithCcAddresses(cc);
                //destination.WithBccAddresses(bcc);

                Amazon.SimpleEmail.Model.Content subject = new Amazon.SimpleEmail.Model.Content();
                subject.WithCharset("UTF-8");
                subject.WithData(Subject);

                Amazon.SimpleEmail.Model.Content html = new Amazon.SimpleEmail.Model.Content();
                html.WithCharset("UTF-8");
                html.WithData(HTML);

                Amazon.SimpleEmail.Model.Content text = new Amazon.SimpleEmail.Model.Content();
                text.WithCharset("UTF-8");
                text.WithData(Text);

                Body body = new Body();
                body.WithHtml(html);
                body.WithText(text);

                Amazon.SimpleEmail.Model.Message message = new Amazon.SimpleEmail.Model.Message();
                message.WithBody(body);
                message.WithSubject(subject);

                AmazonSimpleEmailService ses = AWSClientFactory.CreateAmazonSimpleEmailServiceClient(AppConfig["AWSAccessKey"], AppConfig["AWSSecretKey"]);

                SendEmailRequest request = new SendEmailRequest();
                request.WithDestination(destination);
                request.WithMessage(message);
                request.WithSource(from);

                if (emailReplyTo != null)
                {
                    List<String> replyto
                        = emailReplyTo
                        .Replace(", ", ",")
                        .Split(',')
                        .ToList();

                    request.WithReplyToAddresses(replyto);
                }

                if (returnPath != null)
                {
                    request.WithReturnPath(returnPath);
                }

                try
                {
                    SendEmailResponse response = ses.SendEmail(request);

                    SendEmailResult result = response.SendEmailResult;

                    return true;
                }
                catch 
                {
                    return false;
                }
            }

            return false;
        }
    }
}
