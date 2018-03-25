using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Events.Client;
using TwitchLib.Events.Services.FollowerService;

namespace ATCB.Library.Models.Plugins
{
    public abstract class EventsService
    {
        public abstract void OnSubscription(OnSubscriptionEventArgs e);

        public abstract void OnFollower(OnFollowerEventArgs e);

        public abstract void OnChatMessage(OnChatMessageEventArgs e);
    }

    public enum SubscriptionType
    {
        Subscription,
        Resubscription,
        Twitch_Prime,
        Twitch_Prime_Resubscription,
        Gift
    }

    public class OnSubscriptionEventArgs : EventArgs
    {
        public OnSubscriptionEventArgs(OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == TwitchLib.Enums.SubscriptionPlan.Prime)
                Subscription = SubscriptionType.Twitch_Prime;
            else
                Subscription = SubscriptionType.Subscription;
            Message = null;
            DisplayName = e.Subscriber.DisplayName;
        }
        public OnSubscriptionEventArgs(OnReSubscriberArgs e)
        {
            if (e.ReSubscriber.SubscriptionPlan == TwitchLib.Enums.SubscriptionPlan.Prime)
                Subscription = SubscriptionType.Twitch_Prime_Resubscription;
            else
                Subscription = SubscriptionType.Resubscription;
            Message = e.ReSubscriber.ResubMessage;
            DisplayName = e.ReSubscriber.DisplayName;
        }
        public OnSubscriptionEventArgs(OnGiftedSubscriptionArgs e)
        {
            Message = null;
            DisplayName = e.GiftedSubscription.DisplayName;
        }

        public SubscriptionType Subscription { get; set; }

        public string Message { get; set; }

        public string DisplayName { get; set; }
    }

    public class OnFollowerEventArgs : EventArgs
    {
        public OnFollowerEventArgs(OnNewFollowersDetectedArgs e)
        {
            foreach (var follow in e.NewFollowers)
            {
                DisplayName = follow.User.DisplayName;
            }
        }

        public string DisplayName { get; set; }
    }

    public class OnChatMessageEventArgs : EventArgs
    {
        public OnChatMessageEventArgs(OnMessageReceivedArgs e)
        {
            Message = e.ChatMessage.Message;
            DisplayName = e.ChatMessage.DisplayName;
        }

        public string Message { get; set; }

        public string DisplayName { get; set; }
    }
}
