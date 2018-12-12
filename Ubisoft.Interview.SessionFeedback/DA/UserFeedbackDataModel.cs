using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ubisoft.Interview.SessionFeedback.BL;
using Ubisoft.Interview.SessionFeedback.Helpers;

namespace Ubisoft.Interview.SessionFeedback.DA
{

    [DynamoDBTable("LastUserFeedbacks")]
    public class UserFeedbackDataModel
    {
        public static UserFeedbackDataModel GenerateFromUserFeedbackBO(UserFeedbackBO bo, int lastUserFeedbacksCount)
        {
            return new UserFeedbackDataModel()
            {
                LastUserFeedbacksCount = lastUserFeedbacksCount,
                SessionId_UserId = $"{bo.SessionId}_{bo.UserId}",
                SessionId = bo.SessionId.ToString(),
                UserId = bo.UserId.ToString(),
                SubmitDate = DateTimeHelper.DateTime2IsoUtcTime(bo.SubmitDate),
                Comment = bo.Comment,
                Rate = bo.Rate
            };
        }
        [DynamoDBGlobalSecondaryIndexHashKey]
        public int LastUserFeedbacksCount { get; set; }
        [DynamoDBHashKey]
        public string SessionId_UserId { get; set; }
        public UserRate Rate { get; set; } 
        [DynamoDBGlobalSecondaryIndexRangeKey]
        public string SubmitDate { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
    }

}
