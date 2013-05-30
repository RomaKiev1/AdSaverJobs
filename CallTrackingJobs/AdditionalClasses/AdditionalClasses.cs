using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.GData.Analytics;
using System.Threading;
using System.Globalization;

namespace Quartz.Server.AdditionalClasses
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    public class Asterisk
    {
        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public static bool LinkNumbers(List<string> ClientsNumbers, List<string> SystemNumbers)
        {
            return true;
        }

        ///<Summary>
        /// Gets the answer
        ///</Summary>
        public static bool RelieveNumbers(List<string> Numbers)
        {
            return true;
        }
    }

    //public class GoogleAnalyticsDataRow
    //{
    //    public string eventLabel { get; set; }

    //    public string keyword { get; set; }

    //    public string campaign { get; set; }

    //    public string source { get; set; }

    //    public string medium { get; set; }

    //    public string adGroup { get; set; }

    //    public string timeOnSite { get; set; }

    //    public string newVisits { get; set; }
    //}

    public class GoogleAnalyticsNonPaidSearchTrafficDataRow
    {
        public string eventLabel { get; set; }
        public string keyword { get; set; }
        public string sourcesite { get; set; }
      //  public string timeOnSite { get; set; }
     //   public string newVisits { get; set; }
    }

    public class GoogleAnalyticsPaidSearchTrafficDataRow
    {
        public string eventLabel { get; set; }
        public string keyword { get; set; }
        public string sourcesite { get; set; }
        public string adGroup { get; set; }
     //   public string timeOnSite { get; set; }
      //  public string newVisits { get; set; }
    }

    public class GoogleAnalyticsReferralTrafficDataRow
    {
        public string eventLabel { get; set; }
        public string sourcesite { get; set; }
      //  public string timeOnSite { get; set; }
      //  public string newVisits { get; set; }
    }

    public class GoogleAnalyticsDirectTrafficDataRow
    {
        public string eventLabel { get; set; }
      //  public string timeOnSite { get; set; }
     //   public string newVisits { get; set; }
    }

    public class GoogleAnalyticsKeywordsCostDataRow
    {
        public string adGroup { get; set; }
        public string keyword { get; set; }
        public double cost { get; set; }
    }

    public class DataFeed_NonPaidSearchTraffic
    {
        private const String CLIENT_USERNAME = "roman@svitsoft.com";
        private const String CLIENT_PASS = "Sheva53608286_";
        private const String TABLE_ID = "ga:37386554";

        public DataFeed feed;

        public DataFeed feed2;


        public DataFeed_NonPaidSearchTraffic(DateTime Start, DateTime Finish, string GALogin, string GAPassword, string ids)
        {

            string start = Start.ToString("yyyy-MM-dd");
            AnalyticsService asv = new AnalyticsService("gaExportAPI_acctSample_v2.0");

            asv.setUserCredentials(GALogin, GAPassword);

            String baseUrl = "https://www.google.com/analytics/feeds/data";

            DataQuery query = new DataQuery(baseUrl);
            query.Ids = "ga:" + ids;
            query.Dimensions = "ga:eventLabel, ga:keyword, ga:source";
           // query.Metrics = "ga:timeOnSite, ga:newVisits";
            query.Metrics = "ga:visits";
            query.Segment = "gaid::-5";
            query.GAStartDate = Start.ToString("yyyy-MM-dd");
            query.GAEndDate = Finish.ToString("yyyy-MM-dd");
            query.NumberToRetrieve = 10000;
            feed = asv.Query(query);
        }

        public List<GoogleAnalyticsNonPaidSearchTrafficDataRow> GetData()
        {
            List<GoogleAnalyticsNonPaidSearchTrafficDataRow> GoogleAnalyticsDataRowList = new List<GoogleAnalyticsNonPaidSearchTrafficDataRow>();

            foreach (DataEntry entry in this.feed.Entries)
            {
                GoogleAnalyticsNonPaidSearchTrafficDataRow new_GoogleAnalyticsDataRow = new GoogleAnalyticsNonPaidSearchTrafficDataRow();
                new_GoogleAnalyticsDataRow.eventLabel = entry.Dimensions[0].Value;
                new_GoogleAnalyticsDataRow.keyword = entry.Dimensions[1].Value;
                new_GoogleAnalyticsDataRow.sourcesite = entry.Dimensions[2].Value;
               // new_GoogleAnalyticsDataRow.timeOnSite = entry.Metrics[0].Value;
               // new_GoogleAnalyticsDataRow.newVisits = entry.Metrics[1].Value;
                GoogleAnalyticsDataRowList.Add(new_GoogleAnalyticsDataRow);
            }

            return GoogleAnalyticsDataRowList;
        }

    }

    public class DataFeed_PaidSearchTraffic
    {
        private const String CLIENT_USERNAME = "roman@svitsoft.com"; //"INSERT_LOGIN_EMAIL_HERE";
        private const String CLIENT_PASS = "Sheva53608286_"; //"INSERT_PASSWORD_HERE";
        private const String TABLE_ID = "ga:37386554"; //"INSERT_TABLE_ID_HERE";

        public DataFeed feed;

        public DataFeed feed2;

        // google.gdata.analytics.GwoAnalyticsAccountId

        public DataFeed_PaidSearchTraffic(DateTime Start, DateTime Finish, string GALogin, string GAPassword, string ids)
        {

            string start = Start.ToString("yyyy-MM-dd");
            AnalyticsService asv = new AnalyticsService("gaExportAPI_acctSample_v2.0");
            asv.setUserCredentials(GALogin, GAPassword);

            String baseUrl = "https://www.google.com/analytics/feeds/data";

            DataQuery query = new DataQuery(baseUrl);
            query.Ids = "ga:" + ids;
            query.Dimensions = "ga:eventLabel, ga:keyword, ga:source, ga:adGroup";
           // query.Metrics = "ga:timeOnSite, ga:newVisits";
            query.Metrics = "ga:visits";
            query.Segment = "gaid::-4";
            query.GAStartDate = Start.ToString("yyyy-MM-dd");
            query.GAEndDate = Finish.ToString("yyyy-MM-dd");
            query.NumberToRetrieve = 10000;
            feed = asv.Query(query);
        }

        public List<GoogleAnalyticsPaidSearchTrafficDataRow> GetData()
        {
            List<GoogleAnalyticsPaidSearchTrafficDataRow> GoogleAnalyticsDataRowList = new List<GoogleAnalyticsPaidSearchTrafficDataRow>();

            foreach (DataEntry entry in this.feed.Entries)
            {
                GoogleAnalyticsPaidSearchTrafficDataRow new_GoogleAnalyticsDataRow = new GoogleAnalyticsPaidSearchTrafficDataRow();
                new_GoogleAnalyticsDataRow.eventLabel = entry.Dimensions[0].Value;
                new_GoogleAnalyticsDataRow.keyword = entry.Dimensions[1].Value;
                new_GoogleAnalyticsDataRow.sourcesite = entry.Dimensions[2].Value;
                new_GoogleAnalyticsDataRow.adGroup = entry.Dimensions[3].Value;
               // new_GoogleAnalyticsDataRow.timeOnSite = entry.Metrics[0].Value;
               // new_GoogleAnalyticsDataRow.newVisits = entry.Metrics[1].Value;
                GoogleAnalyticsDataRowList.Add(new_GoogleAnalyticsDataRow);
            }

            return GoogleAnalyticsDataRowList;
        }

    }

    public class DataFeed_ReferralTraffic
    {
        private const String CLIENT_USERNAME = "roman@svitsoft.com"; //"INSERT_LOGIN_EMAIL_HERE";
        private const String CLIENT_PASS = "Sheva53608286_"; //"INSERT_PASSWORD_HERE";
        private const String TABLE_ID = "ga:37386554"; //"INSERT_TABLE_ID_HERE";

        public DataFeed feed;

        public DataFeed feed2;


        public DataFeed_ReferralTraffic(DateTime Start, DateTime Finish, string GALogin, string GAPassword, string ids)
        {
            string start = Start.ToString("yyyy-MM-dd");
            AnalyticsService asv = new AnalyticsService("gaExportAPI_acctSample_v2.0");

            asv.setUserCredentials(GALogin, GAPassword);

            String baseUrl = "https://www.google.com/analytics/feeds/data";

            DataQuery query = new DataQuery(baseUrl);
            query.Ids = "ga:" + ids;
            query.Dimensions = "ga:eventLabel,ga:source";
           // query.Metrics = "ga:timeOnSite, ga:newVisits";
            query.Metrics = "ga:visits";
            query.Segment = "gaid::-8";
            query.NumberToRetrieve = 10000;
            query.GAStartDate = Start.ToString("yyyy-MM-dd");
            query.GAEndDate = Finish.ToString("yyyy-MM-dd");
            feed = asv.Query(query);
        }


        public List<GoogleAnalyticsReferralTrafficDataRow> GetData()
        {
            List<GoogleAnalyticsReferralTrafficDataRow> GoogleAnalyticsDataRowList = new List<GoogleAnalyticsReferralTrafficDataRow>();

            foreach (DataEntry entry in this.feed.Entries)
            {
                GoogleAnalyticsReferralTrafficDataRow new_GoogleAnalyticsDataRow = new GoogleAnalyticsReferralTrafficDataRow();
                new_GoogleAnalyticsDataRow.eventLabel = entry.Dimensions[0].Value;
                new_GoogleAnalyticsDataRow.sourcesite = entry.Dimensions[1].Value;
               // new_GoogleAnalyticsDataRow.timeOnSite = entry.Metrics[0].Value;
               // new_GoogleAnalyticsDataRow.newVisits = entry.Metrics[1].Value;
                GoogleAnalyticsDataRowList.Add(new_GoogleAnalyticsDataRow);
            }

            return GoogleAnalyticsDataRowList;
        }

    }

    public class DataFeed_DirectTraffic
    {
        private const String CLIENT_USERNAME = "roman@svitsoft.com"; //"INSERT_LOGIN_EMAIL_HERE";
        private const String CLIENT_PASS = "Sheva53608286_"; //"INSERT_PASSWORD_HERE";
        private const String TABLE_ID = "ga:37386554"; //"INSERT_TABLE_ID_HERE";

        public DataFeed feed;

        public DataFeed feed2;

        public DataFeed_DirectTraffic(DateTime Start, DateTime Finish, string GALogin, string GAPassword, string ids)
        {
            string start = Start.ToString("yyyy-MM-dd");
            AnalyticsService asv = new AnalyticsService("gaExportAPI_acctSample_v2.0");

            asv.setUserCredentials(GALogin, GAPassword);

            String baseUrl = "https://www.google.com/analytics/feeds/data";

            DataQuery query = new DataQuery(baseUrl);
            query.Ids = "ga:" + ids;
            query.Dimensions = "ga:eventLabel";
          //  query.Metrics = "ga:timeOnSite, ga:newVisits";
            query.Metrics = "ga:visits";
            query.Segment = "gaid::-7";
            query.GAStartDate = Start.ToString("yyyy-MM-dd");
            query.NumberToRetrieve = 10000;
            query.GAEndDate = Finish.ToString("yyyy-MM-dd");
            feed = asv.Query(query);
        }

        public List<GoogleAnalyticsDirectTrafficDataRow> GetData()
        {
            List<GoogleAnalyticsDirectTrafficDataRow> GoogleAnalyticsDataRowList = new List<GoogleAnalyticsDirectTrafficDataRow>();

            foreach (DataEntry entry in this.feed.Entries)
            {
                GoogleAnalyticsDirectTrafficDataRow new_GoogleAnalyticsDataRow = new GoogleAnalyticsDirectTrafficDataRow();
                new_GoogleAnalyticsDataRow.eventLabel = entry.Dimensions[0].Value;
               // new_GoogleAnalyticsDataRow.timeOnSite = entry.Metrics[0].Value;
               // new_GoogleAnalyticsDataRow.newVisits = entry.Metrics[1].Value;
                GoogleAnalyticsDataRowList.Add(new_GoogleAnalyticsDataRow);
            }

            return GoogleAnalyticsDataRowList;
        }
    }

    public class DataFeed_KeywordsCost
    {
        private const String CLIENT_USERNAME = "roman@svitsoft.com"; //"INSERT_LOGIN_EMAIL_HERE";
        private const String CLIENT_PASS = "Sheva53608286_"; //"INSERT_PASSWORD_HERE";
        private const String TABLE_ID = "ga:37386554"; //"INSERT_TABLE_ID_HERE";

        public DataFeed feed;

        public DataFeed_KeywordsCost(DateTime Start, DateTime Finish, string GALogin, string GAPassword, string ids)
        {
            string start = Start.ToString("yyyy-MM-dd");
            AnalyticsService asv = new AnalyticsService("gaExportAPI_acctSample_v2.0");

            asv.setUserCredentials(GALogin, GAPassword);

            String baseUrl = "https://www.google.com/analytics/feeds/data";

            DataQuery query = new DataQuery(baseUrl);
            query.Ids = "ga:" + ids;
            query.Dimensions = "ga:adGroup,ga:keyword";
            query.Metrics = "ga:CPC";
            query.GAStartDate = Start.ToString("yyyy-MM-dd");
            query.NumberToRetrieve = 10000;
            query.GAEndDate = Start.ToString("yyyy-MM-dd");
            feed = asv.Query(query);
        }


        public List<GoogleAnalyticsKeywordsCostDataRow> GetData()
        {
            List<GoogleAnalyticsKeywordsCostDataRow> GoogleAnalyticsDataRowList = new List<GoogleAnalyticsKeywordsCostDataRow>();

          //  Thread.CurrentThread.CurrentCulture = new CultureInfo("uk-UA");

            foreach (DataEntry entry in this.feed.Entries)
            {
                GoogleAnalyticsKeywordsCostDataRow new_GoogleAnalyticsDataRow = new GoogleAnalyticsKeywordsCostDataRow();
                new_GoogleAnalyticsDataRow.adGroup = entry.Dimensions[0].Value;
                new_GoogleAnalyticsDataRow.keyword = entry.Dimensions[1].Value;
               // string str_test = entry.Metrics[0].Value.Substring(0,6).Trim();
                new_GoogleAnalyticsDataRow.cost = double.Parse(entry.Metrics[0].Value, CultureInfo.InvariantCulture);
                GoogleAnalyticsDataRowList.Add(new_GoogleAnalyticsDataRow);
            }

            return GoogleAnalyticsDataRowList;
        }
    }

}