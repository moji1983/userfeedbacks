using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ubisoft.Interview.SessionFeedback.AppServices;
using Ubisoft.Interview.SessionFeedback.BL;
using Ubisoft.Interview.SessionFeedback.Controllers;

namespace Ubisoft.Interview.SessionFeedback.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionFeedbacksController : ControllerBase
    {
        private readonly ISessionFeedbackAppService _sessionFeedbackAppService;
        public SessionFeedbacksController(ISessionFeedbackAppService sessionFeedbackAppService)
        {
            this._sessionFeedbackAppService = sessionFeedbackAppService;
        }


        /// <summary>
        ///Get the last 15 feedbacks for a specific session. If you provide a rate you will get the last 15 feedbacks for a specific session with that rate.
        /// </summary>
        /// <param name="sessionId">SessionId needs to be a valid Guid and needs to be provoided as part of url. </param>
        /// <param name="rate">The optional rate to get last 15 feedbacks with a specific rate, needs to be provided as query string.</param>
        /// <returns></returns>
        [HttpGet("lastfeedbacks/{sessionId}")]
        public Task<IEnumerable<ReadUserFeedbackDTO>> Get([RequiredGuid][FromRoute] string sessionId, [Range(1, 5)]int? rate)
        {
            return _sessionFeedbackAppService.GetLastFeedbacksForSession(Guid.Parse(sessionId),(UserRate?)rate);
        }

    }
}
