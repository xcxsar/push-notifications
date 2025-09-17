using Microsoft.Extensions.Logging;
using FirebaseAdmin.Messaging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using System.Net;
using PushNotificationsServer.Services;

namespace PushNotificationsServer
{
    public class Notifications(ILogger logger, IDatabaseService databaseService)
    {

        [Function("SendNotification")]
        public async Task<HttpResponseData> SendNotification([HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications")] HttpRequestData req)
        {
            logger.LogInformation("Processing single notification request.");

            try
            {
                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync();

                var notificationMessage = JsonConvert.DeserializeObject<NotificationsMessage>(requestBody);
                
                if (notificationMessage == null || string.IsNullOrEmpty(notificationMessage.Message))
                {
                    return await CreateErrorResponse(req, "Message is required", HttpStatusCode.BadRequest);
                }

                var notification = new Notification()
                {
                    Title = notificationMessage.Title ?? "Notification",
                    Body = notificationMessage.Message,
                };

                Message message;
                string targetType;

                if (!string.IsNullOrEmpty(notificationMessage.Token))
                {
                    message = new Message()
                    {
                        Token = notificationMessage.Token,
                        Notification = notification,
                        Data = new Dictionary<string, string>
                        {
                            {"sent_time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")},
                            {"message_type", "direct"}
                        }
                    };
                    targetType = "token";
                }
                else
                {
                    var topic = notificationMessage.Topic ?? "general";
                    message = new Message()
                    {
                        Topic = topic,
                        Notification = notification,
                        Data = new Dictionary<string, string>
                        {
                            {"sent_time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")},
                            {"message_type", "topic"}
                        }
                    };
                    targetType = "topic";
                }

                var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                logger.LogInformation($"Single notification sent successfully. ID: {result}");

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                var successResponse = new NotificationResponse
                {
                    Success = true,
                    MessageId = result,
                    TargetType = targetType
                };

                await response.WriteStringAsync(JsonConvert.SerializeObject(successResponse));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending single notification");
                return await CreateErrorResponse(req, "Internal server error", HttpStatusCode.InternalServerError);
            }
            
        }
        private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, string message, HttpStatusCode statusCode)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var errorResponse = new NotificationResponse
            {
                Success = false,
                Error = message
            };

            await response.WriteStringAsync(JsonConvert.SerializeObject(errorResponse));
            return response;
        }
        public class NotificationsMessage
        {
            public required string Message { get; set; }
            public string? Title { get; set; }
            public string? Token { get; set; }
            public string? Topic { get; set; }
        }

        public class NotificationResponse
        {
            public bool Success { get; set; }
            public string? MessageId { get; set; }
            public string? Error { get; set; }
            public string? TargetType { get; set; }
        }
    }
}
