using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ubisoft.Interview.SessionFeedback.Helpers
{
    public class DateTimeHelper
    {
        public static string DateTime2IsoUtcTime(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("o");
        }

        public static DateTime IsoUtcTime2UtcDateTime(string isoUTCTime)
        {
            return DateTime.Parse(isoUTCTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }
    }
}
