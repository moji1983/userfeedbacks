using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ubisoft.Interview.SessionFeedback.BL;
using Ubisoft.Interview.SessionFeedback.Controllers;
using Ubisoft.Interview.SessionFeedback.Helpers;

namespace Ubisoft.Interview.SessionFeedback.DA
{
    //please refer to databse document to see all databse design beside together
    public class SessionFeedbackRepository : ISessionFeedbackRepository
    {

        private readonly IAmazonDynamoDB _amazonDynamoDB;
        private readonly IDynamoDBContext _context;

        private const string _SessionFeedbacksTableName = "SessionFeedbacks";
        private const string _LastUserFeedbacksTableName = "LastUserFeedbacks";
        private readonly ILogger _logger;



        public SessionFeedbackRepository(IDynamoDBConnectionFactory dynamoDBConnectionFactory, ILogger<LastFeedbacksWriterService> logger)
        {
            (_amazonDynamoDB, _context) = dynamoDBConnectionFactory.Create();
            _logger = logger;
        }

        /// <summary>
        /// SessionFeedbacks table is used to store feedbacks 
        ///To satisfy the requirement to get the last feedbacks for a specific session we had to choose SessionId as our hash key 
        /// but since we want to be able to do order on date for a session we had to choose SessionId as hash
        /// not the best index since it might be the case that there are many users in one session so the best index would be using userId as hash and sessionId as index 
        /// While creating the feedback we use the uniqueness of sessionId and userId as a condition to preventing multiple feedback
        /// </summary>
        /// <param name="userFeedback"></param>
        /// <returns>True if it succeeded to add a feedback for the user, and false if user has already added a feedback for the session.</returns>
        public async Task<bool> CreateFeedback(UserFeedbackBO userFeedback)
        {
            try
            {
                var request = new PutItemRequest
                {
                    TableName = _SessionFeedbacksTableName,
                    Item = new Dictionary<string, AttributeValue>()
                    {
                        { nameof(UserFeedbackBO.SessionId), new AttributeValue {S = userFeedback.SessionId.ToString()}},
                        { $"{nameof(UserFeedbackBO.SessionId) }_{nameof(UserFeedbackBO.Rate) }", new AttributeValue {S = $"{userFeedback.SessionId.ToString()}_{((int)userFeedback.Rate).ToString()}"} },
                        { nameof(UserFeedbackBO.UserId), new AttributeValue() {S = userFeedback.UserId.ToString()} },
                        { nameof(UserFeedbackBO.SubmitDate), new AttributeValue() { S = DateTimeHelper.DateTime2IsoUtcTime(userFeedback.SubmitDate) } },
                        { nameof(UserFeedbackBO.Comment), new AttributeValue() { S = userFeedback.Comment } },
                        { nameof(UserFeedbackBO.Rate), new AttributeValue() { N = ((int) userFeedback.Rate).ToString() } },

                        },
                    //This condition lets us prevent having more than one feedback from a user for a specific session 
                    //https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/WorkingWithItems.html#WorkingWithItems.ConditionalUpdate
                    ConditionExpression = "attribute_not_exists(UserId)"
                };
                await ExecuteCommand(request);
            }
            catch (ConditionalCheckFailedException)
            {
                //when this exception happens means already there is a feedback from the user with UserId
                //for the session with SessionId
                return false;
            }
            return true;
        }

        /// <summary>
        /// Using LastUserFeedbacks table to get last 15 feedbacks across all sessions 
        /// For this query we are intereted in GlobalSecondaryIndex named LastUserFeedbacksCount-index  always  15 (const) as hash and Date as range lets to do an order by on dates for all session
        /// Main hash is just a way to satisfy dynamodb (it needs a unique value) and it is not used in our  
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacks()
        {

            var request = new QueryRequest
            {
                TableName = _LastUserFeedbacksTableName,
                KeyConditions = new Dictionary<string, Condition>()
                      {
                        {
                          nameof(UserFeedbackDataModel.LastUserFeedbacksCount),
                          new Condition()
                          {
                            ComparisonOperator = "EQ",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { N = LastFeedbacksWriterService.NumberOfLastFeedbacks.ToString()}
                            }
                          }
                        },
                        {
                          nameof(UserFeedbackDataModel.SubmitDate),
                          new Condition()
                          {
                            ComparisonOperator = "LE",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = DateTimeHelper.DateTime2IsoUtcTime(DateTime.UtcNow)}
                            }
                          }
                        }
                      },
                Limit = LastFeedbacksWriterService.NumberOfLastFeedbacks,
                ScanIndexForward = false,
                IndexName = "LastUserFeedbacksCount-index"
            };

            var response = await _amazonDynamoDB.QueryAsync(request);
            return ExtractQueryResult(response.Items);
        }

        /// <summary>
        /// Using LastUserFeedbacks table to get last 15 feedbacks across all sessions 
        /// For this query we are intereted in GlobalSecondaryIndex named LastUserFeedbacksCount-Rate-index (having value 1 to 5) as hash and Date as range lets to do an order by on dates for all session within a specific rate
        /// Main hash is just a way to satisfy dynamodb (it needs a unique value) and it is not used in our  
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacksByRate(UserRate rate)
        {
            var request = new QueryRequest
            {
                TableName = _LastUserFeedbacksTableName,
                KeyConditions = new Dictionary<string, Condition>()
                      {
                        {
                          nameof(UserFeedbackDataModel.Rate),
                          new Condition()
                          {
                            ComparisonOperator = "EQ",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { N = ((int)rate).ToString()}
                            }
                          }
                        },
                        {
                          nameof(UserFeedbackDataModel.SubmitDate),
                          new Condition()
                          {
                            ComparisonOperator = "LE",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = DateTimeHelper.DateTime2IsoUtcTime(DateTime.UtcNow)}
                            }
                          }
                        }
                      },
                Limit = LastFeedbacksWriterService.NumberOfLastFeedbacks,
                ScanIndexForward = false,
                IndexName = "LastUserFeedbacksCount-Rate-index"
            };

            var response = await _amazonDynamoDB.QueryAsync(request);
            return ExtractQueryResult(response.Items);
        }


        /// <summary>
        /// Mapper function from data values to our Business Object
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private IEnumerable<UserFeedbackBO> ExtractQueryResult(List<Dictionary<string, AttributeValue>> items)
        {
            var result = new List<UserFeedbackBO>();
            foreach (var item in items)
            {
                var feedback = UserFeedbackBO.CreateUserFeedback(
                     Guid.Parse(item[nameof(UserFeedbackBO.SessionId)].S),
                     Guid.Parse(item[nameof(UserFeedbackBO.UserId)].S),
                     DateTimeHelper.IsoUtcTime2UtcDateTime(item[nameof(UserFeedbackBO.SubmitDate)].S),
                     item[nameof(UserFeedbackBO.Comment)].S,
                     (UserRate)int.Parse(item[nameof(UserFeedbackBO.Rate)].N)
                     );
                result.Add(feedback);
            }
            return result;
        }


        /// <summary>
        /// Using SessionFeedbacks to retrieve the last feedbacks for a session
        /// To satisfy the requirement to get the last feedbacks for a specific session we had to choose SessionId as our hash key 
        /// not the best index since it might be the case that there are many users in one session so the best index would be using userId as hash and sessionId as index 
        /// but since we want to be able to do order on date for a session we had to choose SessionId as hash
        /// in this query we are using a local index SubmitDate-index to be able to do orderby on Date field for a specific session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacksForSession(Guid sessionId)
        {

            var request = new QueryRequest
            {
                TableName = _SessionFeedbacksTableName,
                KeyConditions = new Dictionary<string, Condition>()
                      {
                        {
                          nameof(UserFeedbackDataModel.SessionId),
                          new Condition()
                          {
                            ComparisonOperator = "EQ",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = sessionId.ToString()}
                            }
                          }
                        },
                        {
                          nameof(UserFeedbackDataModel.SubmitDate),
                          new Condition()
                          {
                            ComparisonOperator = "LE",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = DateTimeHelper.DateTime2IsoUtcTime(DateTime.UtcNow)}
                            }
                          }
                        }
                      },
                Limit = LastFeedbacksWriterService.NumberOfLastFeedbacks,
                ScanIndexForward = false,
                IndexName = "SubmitDate-index"
            };


            var response = await _amazonDynamoDB.QueryAsync(request);
            return ExtractQueryResult(response.Items);
        }

        /// <summary>
        /// Using SessionFeedbacks to retrieve the last feedbacks for a session
        /// To satisfy the requirement to get the last feedbacks for a specific session we had to choose SessionId as our hash key 
        /// not the best index since it might be the case that there are many users in one session so the best index would be using userId as hash and sessionId as index 
        /// but since we want to be able to do order on date for a session we had to choose SessionId as hash
        /// in this query we are using a Global SecondaryIndex index SessionId_Rate_SubmitDate-index (SessionId_rate as hash key and SubmitDate as range key)
        /// to be able to do orderby on Date field for specific session's feedbacks with a specified rate
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<UserFeedbackBO>> GetLastFeedbacksForSessionByRate(Guid sessionId, UserRate rate)
        {

            var request = new QueryRequest
            {
                TableName = _SessionFeedbacksTableName,
                KeyConditions = new Dictionary<string, Condition>()
                      {
                        {
                         $"{nameof(UserFeedbackDataModel.SessionId)}_{nameof(UserFeedbackDataModel.Rate)}",
                          new Condition()
                          {
                            ComparisonOperator = "EQ",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = $"{sessionId.ToString()}_{((int)rate).ToString()}" }
                            }
                          }
                        },
                        {
                          nameof(UserFeedbackDataModel.SubmitDate),
                          new Condition()
                          {
                            ComparisonOperator = "LE",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = DateTimeHelper.DateTime2IsoUtcTime(DateTime.UtcNow)}
                            }
                          }
                        }
                      },
                Limit = LastFeedbacksWriterService.NumberOfLastFeedbacks,
                ScanIndexForward = false,
                IndexName = "SessionId_Rate_SubmitDate-index"
            };

            var response = await _amazonDynamoDB.QueryAsync(request);
            return ExtractQueryResult(response.Items);
        }



        //This a way to do fallback if dynamo table was ProvisionedThroughputExceededException
        //We use polly library to retry with some pause
        private async Task ExecuteCommand(PutItemRequest request)
        {
            var executionPolicy = Policy.Handle<ProvisionedThroughputExceededException>().WaitAndRetryAsync(10000,
                            (retryCount) => TimeSpan.FromMilliseconds(250),
                            (e, t, i, c) => OnRetry(request, e, t, i));
            await executionPolicy.ExecuteAsync(async () => await _amazonDynamoDB.PutItemAsync(request));
        }
        private void OnRetry(PutItemRequest request, Exception e, TimeSpan t, int i)
        {
            if (i % 100 == 0 && i != 0)
            {
                _logger.LogWarning("Retried Put 100 times without success", e);
            }
        }

        /// <summary>
        /// Storing last 15 feedbacks during some seconds time periods
        /// A random unique key as main key
        /// </summary>
        /// <param name="feedbacks"></param>
        /// <returns></returns>
        public Task StoreLastFeedbacks(IEnumerable<UserFeedbackBO> feedbacks)
        {
            if (feedbacks.Count() < 1)
                return Task.CompletedTask;
            var bookBatch = _context.CreateBatchWrite<UserFeedbackDataModel>();
            bookBatch.AddPutItems(feedbacks.Select(item => UserFeedbackDataModel.GenerateFromUserFeedbackBO(item, LastFeedbacksWriterService.NumberOfLastFeedbacks)));
            return bookBatch.ExecuteAsync();
        }

        public async Task<UserFeedbackBO> GetUserFeedback(Guid sessionId, Guid userId)
        {
            var request = new QueryRequest
            {
                TableName = _SessionFeedbacksTableName,
                KeyConditions = new Dictionary<string, Condition>()
                      {
                        {
                          nameof(UserFeedbackDataModel.SessionId),
                          new Condition()
                          {
                            ComparisonOperator = "EQ",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = sessionId.ToString()}
                            }
                          }
                        },
                        {
                          nameof(UserFeedbackDataModel.UserId),
                          new Condition()
                          {
                            ComparisonOperator = "EQ",
                            AttributeValueList = new List<AttributeValue>()
                            {
                              new AttributeValue { S = userId.ToString()}
                            }
                          }
                        }
                      },

            };


            var response = await _amazonDynamoDB.QueryAsync(request);
            return ExtractQueryResult(response.Items).FirstOrDefault();
        }
    }
}
