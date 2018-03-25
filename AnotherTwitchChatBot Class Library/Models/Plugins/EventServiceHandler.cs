using ATCB.Library.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Services;

namespace ATCB.Library.Models.Plugins
{
    public class EventServiceHandler
    {
        private List<EventsService> Services;

        public EventServiceHandler(FollowerService followerService, TwitchClient userClient, TwitchClient botClient)
        {
            Services = new List<EventsService>();

            // Check to see if plugins folder exists, then try loading
            if (Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}plugins"))
            {
                var directoryInfo = new DirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}plugins");
                foreach (var plugin in directoryInfo.GetFiles("*.dll"))
                {
                    var DLL = Assembly.LoadFile(plugin.FullName);

                    foreach (Type type in DLL.GetExportedTypes().Where(x => typeof(EventsService).IsAssignableFrom(x)))
                    {
                        var c = Activator.CreateInstance(type) as EventsService;
                        Services.Add(c);
                    }
                }
            }

            // Then attach all events and plugins together
            foreach (var service in Services)
            {
                followerService.OnNewFollowersDetected += (sender, e) => service.OnFollower(new OnFollowerEventArgs(e));
                botClient.OnNewSubscriber += (sender, e) => service.OnSubscription(new OnSubscriptionEventArgs(e));
                botClient.OnReSubscriber += (sender, e) => service.OnSubscription(new OnSubscriptionEventArgs(e));
                botClient.OnGiftedSubscription += (sender, e) => service.OnSubscription(new OnSubscriptionEventArgs(e));
                botClient.OnMessageReceived += (sender, e) => service.OnChatMessage(new OnChatMessageEventArgs(e));
            }
        }

        public List<EventsService> GetServices() => Services;
    }
}
