using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PushNotificationsServer.Models;
using PushNotificationsServer.Services;
using System.Net;

namespace PushNotificationsServer
{
    public class UserRegistration(ILogger<UserRegistration> logger, IDatabaseService databaseService)
    {

        [Function("RegisterUser")]
        public async Task<HttpResponseData> RegisterUser([HttpTrigger(AuthorizationLevel.Function, "post", Route = "register")] HttpRequestData req)
        {
            logger.LogInformation("Processing user registration request.");

            try
            {
                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(requestBody))
                {
                    return await CreateErrorResponse(req, "Request body is empty.", HttpStatusCode.BadRequest);
                }

                var registerRequest = JsonConvert.DeserializeObject<RegisterUserRequest>(requestBody);

                if (registerRequest == null || string.IsNullOrEmpty(registerRequest.FcmToken))
                {
                    return await CreateErrorResponse(req, "Invalid request body or missing FCM token.", HttpStatusCode.BadRequest);
                }

                var user = await databaseService.RegisterUserAsync(registerRequest);

                if (user == null)
                {
                    return await CreateErrorResponse(req, "Failed to register user.", HttpStatusCode.InternalServerError);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                var successResponse = new RegisterUserResponse
                {
                    Success = true,
                    UserId = user.Id,
                    Message = "Device registered successfully"
                };

                await response.WriteStringAsync(JsonConvert.SerializeObject(successResponse));
                
                logger.LogInformation($"User {registerRequest.Id} registered successfully with user ID {user.Id}");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing user registration request.");
                return await CreateErrorResponse(req, "An error occurred while processing the request.", HttpStatusCode.InternalServerError);
            }
        }
        private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, string message, HttpStatusCode statusCode)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var errorResponse = new RegisterUserResponse
            {
                Success = false,
                Error = message
            };

            await response.WriteStringAsync(JsonConvert.SerializeObject(errorResponse));
            return response;
        }
    }
}
