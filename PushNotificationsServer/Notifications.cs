using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace PushNotificationsServer
{
    public class NotificationsMessage
    {
        public string Message { get; set; }
    }

    public class Notifications
    {
        private readonly ILogger logger;

        public Notifications(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<Notifications>();
        }

        [Function("SendNotification")]
        public async Task<HttpResponseData>
    Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications")] HttpRequestData req)
        {
            this.logger.LogInformation("Preparando el envío del mensaje.");

            StreamReader sr = new(req.Body);
            var notificationMessage = JsonConvert.DeserializeObject<NotificationsMessage>(sr.ReadToEnd());

            var message = new Message()
            {
                Topic = "Bienvenido",
                Notification = new Notification()
                {
                    Title = "Notificación de bienvenida",
                    Body = notificationMessage.Message ?? "¡Bienvenido a nuestra aplicación!",
                }
            };

            string result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            this.logger.LogInformation("Respuesta: " + result);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Mensaje enviado correctamente");
            return response;
        }

    }
}
