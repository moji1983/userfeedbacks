using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ubisoft.Interview.SessionFeedback.BL
{
    /// <summary>
    /// This class is used as a batch-writer to store last 15 feedbacks 
    /// The timespan is every 10 seconds.
    /// The design is every server will get a bunch of UserFeedbacks to store, this class will store the requests in a collection in the memory,
    /// then every 10 seconds it will write last 15 ones to the database.
    /// </summary>
    public interface ILastFeedbacksWriterService
    {
        void AddNewFeedback(UserFeedbackBO userFeedback);
    }

    public class LastFeedbacksWriterService : ILastFeedbacksWriterService, IHostedService
    {
        private static ConcurrentBag<UserFeedbackBO> _lastFeedbacks = new ConcurrentBag<UserFeedbackBO>();
        private readonly ILogger _logger;
        private readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
        private readonly ISessionFeedbackRepository _sessionFeedbackRepository;
        public const int NumberOfLastFeedbacks = 15;

        public LastFeedbacksWriterService(ISessionFeedbackRepository sessionFeedbackRepository, ILogger<LastFeedbacksWriterService> logger)
        {
            _logger = logger;
            _sessionFeedbackRepository = sessionFeedbackRepository;
        }
        public void AddNewFeedback(UserFeedbackBO userFeedback)
        {
            _lastFeedbacks.Add(userFeedback);
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background LastFeedbacksWriterService is starting.");

            while (true)
            {
                await StoreFeedbacks();
                await Task.Delay(Interval, cancellationToken);
            }
        }
        private async Task StoreFeedbacks()
        {
            var snapshot = _lastFeedbacks.ToList();
            try
            {
                await _sessionFeedbackRepository.StoreLastFeedbacks(snapshot.OrderByDescending(item => item.SubmitDate).Take(NumberOfLastFeedbacks));
                foreach (var item in snapshot)
                {
                    _lastFeedbacks.TryTake(out var dummy);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Error storing last feedbacks in storage", ex);
            }

        }
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background LastFeedbacksWriterService is stopping.");

            return Task.CompletedTask;
        }
    }
}
