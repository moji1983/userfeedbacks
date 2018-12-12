using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ubisoft.Interview.SessionFeedback.Controllers;

namespace Ubisoft.Interview.SessionFeedback.BL
{
    public interface ISessionFeedbackRepository
    {
        Task<bool> CreateFeedback(UserFeedbackBO userFeedback);
        Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacks();
        Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacksByRate(UserRate rate);
        Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacksForSession(Guid sessionId);
        Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacksForSessionByRate(Guid sessionId, UserRate rate);
        Task StoreLastFeedbacks(IEnumerable<UserFeedbackBO> enumerable);
        Task<UserFeedbackBO> GetUserFeedback(Guid sessionId, Guid userId);
    }
}
