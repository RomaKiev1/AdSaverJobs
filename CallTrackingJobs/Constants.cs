using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quartz.Server.Jobs
{
    ///<Summary>
    /// Gets the answer
    ///</Summary>
    public static class Constants
    {
        public const int RemoveMoneyJob = 1;

        public const int ReserveNumbersJob = 2;

        public const int SetCallInfoJob = 3;

        public const int SmallbalanceJob = 4;

        public const int SetSourceIdJob = 5;

        public const int SendEmailJob = 6;

        public const int GetGoogleAnalyticsDataJob = 7;

        public const int SetKeywordsCostJob = 8;

        public const string DebugEmail = "RomaKiev1@bigmir.net";

        //типы источников (Begin)
        public const int SeoSourceId = 1;

        public const int AddwordsSourceId = 2;

        public const int ReferralTrafficSourceId = 3;

        public const int DirectTraffikSourceId = 4;
        //типы источников (End)
    }
}