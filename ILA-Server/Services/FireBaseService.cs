using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using ILA_Server.Data;
using ILA_Server.Models;
using Newtonsoft.Json;

namespace ILA_Server.Services
{
    public interface IFireBaseService
    {
        Task SendPushNotificationMessageToSingleUser(ILAUser user, string title, string body, Dictionary<string, string> data = null);
        Task SendPushNotificationMessage(List<ILAUser> users, string title, string body, Dictionary<string, string> data = null);
    }
    public class FireBaseService : IFireBaseService
    {
        private readonly FirebaseApp _firebaseApp;

        public FireBaseService()
        {
            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("./firebase_credentials.json")
            });
        }

        public Task SendPushNotificationMessageToSingleUser(ILAUser user, string title, string body, Dictionary<string, string> data = null) =>
        SendPushNotificationMessage(new List<ILAUser> { user }, title, body, data);

        public async Task SendPushNotificationMessage(List<ILAUser> users, string title, string body, Dictionary<string, string> data = null)
        {
            if (title.Length > 200)
                title = title.Substring(0, 196) + " ...";

            if (body.Length > 200)
                body = body.Substring(0, 196) + " ...";

            await SendPushNotificationMessages(users.SelectMany(x => x.PushTokens.Select(y => y.Token)).ToList(), title, body,
                data);
        }

        private async Task SendPushNotificationMessages(List<string> tokens, string title, string body, Dictionary<string, string> data = null)
        {
            int tokenCount = tokens.Count <= 100 ? tokens.Count : 100;
            if (tokenCount == 0)
                return;

            MulticastMessage message = new MulticastMessage
            {
                Tokens = tokens.Take(tokenCount).ToList(),
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                },
                Data = data,
            };

            BatchResponse response = await FirebaseMessaging.GetMessaging(_firebaseApp).SendMulticastAsync(message);
            // TODO: handle response

            if (tokens.Count > 100)
                await SendPushNotificationMessages(tokens.Skip(100).ToList(), title, body, data);
        }
    }
}
