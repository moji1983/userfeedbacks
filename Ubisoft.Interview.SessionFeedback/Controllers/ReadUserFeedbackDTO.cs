using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ubisoft.Interview.SessionFeedback.BL;
using Ubisoft.Interview.SessionFeedback.Helpers;

namespace Ubisoft.Interview.SessionFeedback.Controllers
{
    public class ReadUserFeedbackDTO
    {
        public static ReadUserFeedbackDTO CreateDTOFromBO(UserFeedbackBO bo)
        {
            if (bo == null)
                return null;

            return new ReadUserFeedbackDTO()
            {
                SessionId = bo.SessionId.ToString(),
                UserId = bo.UserId.ToString(),
                SubmitDate = DateTimeHelper.DateTime2IsoUtcTime(bo.SubmitDate),
                Comment = bo.Comment,
                Rate = bo.Rate
            };
        }
        public UserRate Rate { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
        public string SubmitDate { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
    }
}
