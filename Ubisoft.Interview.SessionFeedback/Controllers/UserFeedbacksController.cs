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
    public class UserFeedbacksController : ControllerBase
    {
        private readonly ISessionFeedbackAppService _sessionFeedbackAppService;
        public UserFeedbacksController(ISessionFeedbackAppService sessionFeedbackAppService)
        {
            this._sessionFeedbackAppService = sessionFeedbackAppService;
        }

        /// <summary>
        /// Get the last 15 feedbacks across all sessions.If you provide a rate you will get the last 15 feedbacks for a specific session with that rate.
        /// </summary>
        /// <param name="rate">The optional rate to get last 15 feedbacks with a specific rate, needs to be provided as query string.</param>
        /// <returns></returns>
        [HttpGet("lastfeedbacks")]
        public Task<IEnumerable<ReadUserFeedbackDTO>> Get([Range(1, 5)]int? rate)
        {
            return _sessionFeedbackAppService.GetLastFeedbacks((UserRate?)rate);

        }
        /// <summary>
        /// Get a sepecific feedback for a specific session
        /// </summary>
        /// <param name="sessionId">SessionId needs to a valid Guid.</param>
        /// <param name="userId">UserId needs to a valid Guid.</param>
        /// <returns></returns>
        [HttpGet("{sessionId}")]
        public async Task<ReadUserFeedbackDTO> Get(
            [RequiredGuid][FromRoute]string sessionId,
            [RequiredGuid][FromHeader(Name = "Ubi-UserId")] string userId)
        {
            var result = await  this._sessionFeedbackAppService.GetUserFeedback(Guid.Parse(sessionId), Guid.Parse(userId));
            if (result != null)
                return result;
            throw new  ApiException($"Feedback from the user {userId} for the session {sessionId} could not be found", System.Net.HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Create a user feedback for a specific session..
        /// SessionId should be provided as part of route, UserId as header with name Ubi-UserId.
        /// Rate and Comment should be provided as body in json format.
        /// If you a user has already submitted a feedback for a session it will fail and the result will be 403 .
        /// </summary>
        /// <param name="sessionId">SessionId needs to a valid Guid.</param>
        /// <param name="userId">UserId needs to a valid Guid.</param>
        /// <param name="feedback">Comment and Rate in a json document.</param>
        /// <returns></returns>
        [HttpPost("{sessionId}")]
        public async Task Post([FromRoute(Name ="sessionId")][RequiredGuid]string sessionId, [RequiredGuid][FromHeader(Name = "Ubi-UserId")] string userId,[FromBody] CreateUserFeedbackDTO feedback)
        {
           // string userId = Request.Headers["Ubi-UserId"];
            var isSucceeded = await this._sessionFeedbackAppService.CreateFeedback(Guid.Parse( sessionId), Guid.Parse(userId), feedback);

            if (!isSucceeded)
            {
                throw new ApiException($"The user {userId} for the session {sessionId} has laready posted a feedback.", System.Net.HttpStatusCode.Forbidden);
            }
            return;
        }


    }
}
