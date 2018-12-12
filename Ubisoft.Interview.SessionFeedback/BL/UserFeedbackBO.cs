using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ubisoft.Interview.SessionFeedback.BL
{
    public class UserFeedbackBO
    {
        public static UserFeedbackBO CreateUserFeedback(Guid sessionId, Guid userId,
            DateTime submitDate, string comment, UserRate rate)
        {

            return new UserFeedbackBO()
            {
                SessionId = sessionId,
                UserId = userId,
                SubmitDate = submitDate,
                Rate = rate,
                Comment = comment
            };
        }
        public UserRate Rate { get; set; }
        public DateTime SubmitDate { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string Comment { get; set; }

    }
    public enum UserRate
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5
    }
}
