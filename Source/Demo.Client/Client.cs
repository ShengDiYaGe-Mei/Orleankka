﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Demo
{
    public class Client
    {
        readonly IActorSystem system;
        readonly ClientObservable observable;

        public Client(IActorSystem system, ClientObservable observable)
        {
            this.system = system;
            this.observable = observable;
        }

        public async void Run()
        {
            await MonitorAvailabilityChanges("facebook");
            await MonitorAvailabilityChanges("twitter");

            observable.Subscribe(LogToConsole);

            foreach (var i in Enumerable.Range(1, 25))
            {
                var topic = system.ActorOf<Topic>(i.ToString());

                await topic.Send(new CreateTopic("[" + i + "]", new Dictionary<string, TimeSpan>
                {
                    {"facebook", TimeSpan.FromMinutes(1)},
                    {"twitter", TimeSpan.FromMinutes(1)},
                }));
            }
        }

        async Task MonitorAvailabilityChanges(string api)
        {
            await system.ActorOf<Api>(api).Tell(new MonitorAvailabilityChanges(observable));
        }

        static void LogToConsole(Notification notification)
        {
            var e = (AvailabilityChanged) notification.Message;

            Log.Message(
                !e.Available ? ConsoleColor.Red : ConsoleColor.Green,
                !e.Available ? "*{0}* gone wild. Unavailable!" : "*{0}* is back available again!", 
                notification.Source.Id);
        }
    }
}