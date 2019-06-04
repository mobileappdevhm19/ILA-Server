using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using ILA_Server.Data;
using ILA_Server.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ILA_Server.Services
{
    public interface IFireBaseService
    {
        void SendPushNotificationMessageToSingleUser(ILAUser user, string title, string body, Dictionary<string, string> data = null);
        void SendPushNotificationMessage(List<ILAUser> users, string title, string body, Dictionary<string, string> data = null);
    }
    public interface IFireBaseTaskQueue
    {
        void QueuePushMessage(List<string> tokens, string title, string body,
            Dictionary<string, string> data = null);

        Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class FireBaseService : IFireBaseService
    {
        private readonly IFireBaseTaskQueue _pushQueue;

        public FireBaseService(IFireBaseTaskQueue pushQueue)
        {
            _pushQueue = pushQueue;
        }

        public void SendPushNotificationMessageToSingleUser(ILAUser user, string title, string body, Dictionary<string, string> data = null) =>
        SendPushNotificationMessage(new List<ILAUser> { user }, title, body, data);

        public void SendPushNotificationMessage(List<ILAUser> users, string title, string body, Dictionary<string, string> data = null)
        {
            if (title.Length > 200)
                title = title.Substring(0, 196) + " ...";

            if (body.Length > 200)
                body = body.Substring(0, 196) + " ...";

            _pushQueue.QueuePushMessage(users.SelectMany(x => x.PushTokens.Select(y => y.Token)).ToList(), title, body,
                data);
        }
    }

    public class FireBaseTaskQueue : IFireBaseTaskQueue
    {
        private readonly FirebaseApp _firebaseApp;

        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems =
            new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public FireBaseTaskQueue()
        {
            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("./firebase_credentials.json")
            });
        }

        public void QueuePushMessage(List<string> tokens, string title, string body, Dictionary<string, string> data = null)
        {
            Func<CancellationToken, Task> workItem = token => SendPushNotificationMessages(tokens, title, body, data, token);

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }

        private async Task SendPushNotificationMessages(List<string> tokens, string title, string body, Dictionary<string, string> data, CancellationToken cancellationToken)
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

            BatchResponse response = await FirebaseMessaging.GetMessaging(_firebaseApp)
                .SendMulticastAsync(message, cancellationToken);
            // TODO: handle response

            if (tokens.Count > 100)
                await SendPushNotificationMessages(tokens.Skip(100).ToList(), title, body, data, cancellationToken);
        }
    }

    public class FireBaseHostedService : BackgroundService
    {
        private readonly ILogger _logger;

        public FireBaseHostedService(IFireBaseTaskQueue taskQueue,
            ILoggerFactory loggerFactory)
        {
            TaskQueue = taskQueue;
            _logger = loggerFactory.CreateLogger<FireBaseHostedService>();
        }

        public IFireBaseTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(cancellationToken);

                try
                {
                    await workItem(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"Error occurred executing {nameof(workItem)}.");
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
