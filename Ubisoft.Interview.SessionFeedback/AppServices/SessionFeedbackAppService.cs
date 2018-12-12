using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ubisoft.Interview.SessionFeedback.BL;
using Ubisoft.Interview.SessionFeedback.Controllers;

namespace Ubisoft.Interview.SessionFeedback.AppServices
{
    public interface ISessionFeedbackAppService
    {
        Task<ReadUserFeedbackDTO> GetUserFeedback(Guid sessionId, Guid userId);
        Task<IEnumerable<ReadUserFeedbackDTO>> GetLastFeedbacks(UserRate? rate);
        Task<IEnumerable<ReadUserFeedbackDTO>> GetLastFeedbacksForSession(Guid sessionId, UserRate? rate);
        Task<bool> CreateFeedback(Guid sessionId, Guid userId, CreateUserFeedbackDTO feedback);
    }
    public class SessionFeedbackAppService : ISessionFeedbackAppService
    {
        private readonly ISessionFeedbackRepository _sessionFeedbackRepository;
        private readonly ILastFeedbacksWriterService _lastFeedbacksWriterService;
        public SessionFeedbackAppService(ISessionFeedbackRepository sessionFeedbackRepository, ILastFeedbacksWriterService lastFeedbacksWriterService)
        {
            _sessionFeedbackRepository = sessionFeedbackRepository;
            _lastFeedbacksWriterService = lastFeedbacksWriterService;
        }
        public Task CreateFeedback(UserFeedbackBO userFeedback)
        {
            return this._sessionFeedbackRepository.CreateFeedback(userFeedback);
        }

        public async Task<bool> CreateFeedback(Guid sessionId, Guid userId, CreateUserFeedbackDTO feedback)
        {
            var userFeedback = UserFeedbackBO.CreateUserFeedback(sessionId, userId, DateTime.UtcNow, feedback.Comment, feedback.Rate);
            if (await _sessionFeedbackRepository.CreateFeedback(userFeedback))
            {
                _lastFeedbacksWriterService.AddNewFeedback(userFeedback);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<ReadUserFeedbackDTO>> GetLastFeedbacks(UserRate? rate)
        {
            if (!rate.HasValue)
            {
                return (await _sessionFeedbackRepository.GetLastFeedbacks()).Select(bo => ReadUserFeedbackDTO.CreateDTOFromBO(bo));
            }

            return (await _sessionFeedbackRepository.GetLastFeedbacksByRate(rate.Value)).Select(bo => ReadUserFeedbackDTO.CreateDTOFromBO(bo));

        }

        public async Task<IEnumerable<ReadUserFeedbackDTO>> GetLastFeedbacksForSession(Guid sessionId, UserRate? rate)
        {
            if (!rate.HasValue)
            {
                return (await _sessionFeedbackRepository.GetLastFeedbacksForSession(sessionId)).Select(bo => ReadUserFeedbackDTO.CreateDTOFromBO(bo));
            }
            return (await _sessionFeedbackRepository.GetLastFeedbacksForSessionByRate(sessionId, rate.Value)).Select(bo => ReadUserFeedbackDTO.CreateDTOFromBO(bo));

        }

        public async Task<ReadUserFeedbackDTO> GetUserFeedback(Guid sessionId, Guid userId)
        {
            return ReadUserFeedbackDTO.CreateDTOFromBO(await _sessionFeedbackRepository.GetUserFeedback(sessionId, userId));
        }
    }
}
