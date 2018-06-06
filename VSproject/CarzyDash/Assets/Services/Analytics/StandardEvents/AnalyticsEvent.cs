using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace UnityEngine.Analytics.Experimental
{
    /// <summary>
    /// The main class of the Unity Analytics Standard Event SDK.
    /// <remarks>
    /// The event methods in this class provide "fire and forget" ease of use,
    /// allowing you to send a standard event with a single line of code.
    /// </remarks>
    /// </summary>
    /// <remarks>
    /// If you expect to be sending the same standard event with some amount of frequency,
    /// you may want to create an event payload and invoke the <c>Send</c> method when the event occurs.
    /// </remarks>
    public static class AnalyticsEvent
    {
        static readonly string k_SdkVersion = "0.3.0";

        static readonly string k_ErrorFormat_RequiredParamNotSet = "Required param not set ({0}).";

        static readonly Dictionary<string, object> m_EventData = new Dictionary<string, object>();

        /// <summary>
        /// Gets the Unity Analytics Standard Event SDK version.
        /// </summary>
        /// <value>The SDK version in semantic versioning format.</value>
        public static string sdkVersion { get { return k_SdkVersion; } }

        /// <summary>
        /// Gets or sets a value indicating whether debug mode is enabled.
        /// <remarks>
        /// When debug mode is enabled, a debug log is generated for each event sent. If an error occurs when
        /// sending an event, an error log is generated regardless of whether debug mode is enabled.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// To view the event name and a listing of event data parameters within the debug log messages for each event,
        /// add DEBUG_ANALYTICS_STANDARD_EVENTS to Scripting Define Symbols for the target platforms in Player Settings.
        /// </remarks>
        /// <value><c>true</c> if debug mode is enabled; otherwise, <c>false</c>.</value>
        public static bool debugMode
        {
#if DEBUG_ANALYTICS_STANDARD_EVENTS
            get { return true; } 
            set { }
#else
            get;
            set;
#endif
        }

        static void OnValidationFailed (string message)
        {
            throw new ArgumentException(message);
        }

        static void AddCustomEventData (IDictionary<string, object> eventData)
        {
            if (eventData == null) return;

            for (int i = 0; i < eventData.Count; i++)
            {
                var param = eventData.ElementAt(i);

                if (!m_EventData.ContainsKey(param.Key))
                {
                    m_EventData.Add(param.Key, param.Value);
                }
            }
        }

        static string SplitCamelCase (string input)
        {
            input = Regex.Replace(input, "([a-z](?=[A-Z]))", "$0_");
            return Regex.Replace(input, "(?<!_|^)[A-Z][a-z]", "_$0");
        }

        /// <summary>
        /// Converts a Standard Event enum value to its standardized string value.
        /// <remarks>
        /// Any Non-Standard Event enum values provided are simply converted to string.
        /// </remarks>
        /// </summary>
        /// <returns>The standard string value for the provided enum value.</returns>
        /// <param name="enumValue">The num value.</param>
        public static string EnumToString (object enumValue)
        {
            var result = enumValue.ToString();

            if (enumValue is AdvertisingNetwork ||
                enumValue is AuthorizationNetwork ||
                enumValue is SocialNetwork)
            {
                return result.ToLower();
            }

            if (enumValue is AcquisitionSource ||
                enumValue is AcquisitionType ||
                enumValue is ScreenName ||
                enumValue is ShareType ||
                enumValue is StoreType)
            {
                return SplitCamelCase(result).ToLower();
            }

            return result;
        }

        /// <summary>
        /// Sends a custom event (eventName) with data (eventData)
        /// </summary>
        /// <returns>The result of the analytics event sent</returns>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="eventData">Custom event data.</param>
        public static AnalyticsResult Custom (string eventName, IDictionary<string, object> eventData = null)
        {
            var result = AnalyticsResult.UnsupportedPlatform;
            var verboseLog = string.Empty;

            if (string.IsNullOrEmpty(eventName))
            {
                OnValidationFailed("Custom event name cannot be set to null or an empty string.");
            }

#if UNITY_ANALYTICS
            // The following solution is meant to address backwards compatability issues:
            //  - Unity 5.4: CustomEvent does not support passing null as a dictionary value.
            //  - Unity 5.3: CustomEvent does not support passing eventName only.
            if (eventData == null)
            {
#if UNITY_5_4_OR_NEWER
                result = Analytics.CustomEvent(eventName);
#else
                m_EventData.Clear();
                result = Analytics.CustomEvent(eventName, m_EventData);
#endif
            }
            else
            {
                result = Analytics.CustomEvent(eventName, eventData);
            }
#endif

            // Enable verbose logging by adding the following to Scripting Define Symbols in Player Settings.
#if DEBUG_ANALYTICS_STANDARD_EVENTS
            if (eventData == null)
            {
                verboseLog += "\n  Event Data (null)";
            }
            else
            {
                verboseLog += string.Format("\n  Event Data ({0} params):", eventData.Count);

                for (int i = 0; i < eventData.Count; i++)
                {
                    var element = eventData.ElementAt(i);
                    verboseLog += string.Format("\n    Key: '{0}';  Value: '{1}'", element.Key, element.Value);
                }
            }
#endif

            switch (result)
            {
            case AnalyticsResult.Ok:
                if (debugMode)
                {
                    Debug.LogFormat("Successfully sent '{0}' event (Result: '{1}').{2}", eventName, result, verboseLog);
                }
                break;
            case AnalyticsResult.InvalidData:
            case AnalyticsResult.TooManyItems:
                Debug.LogErrorFormat("Failed to send '{0}' event (Result: '{1}').{2}", eventName, result, verboseLog);
                break;
            default:
                Debug.LogWarningFormat("Unable to send '{0}' event (Result: '{1}').{2}", eventName, result, verboseLog);
                break;
            }

            return result;
        }

        /// <summary>
        /// Sends an <c>achievement_step</c> event.
        /// <remarks>
        /// Send this event when a requirement or step toward completing a multi-part achievement is complete.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into achievement completion rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="stepIndex">Index of the step completed in a multi-part achievement.</param>
        /// <param name="achievementId">The achievement ID.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AchievementStep (int stepIndex, string achievementId, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("step_index", stepIndex);

            if (string.IsNullOrEmpty(achievementId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "achievement_id"));
            }
            else
            {
                m_EventData.Add("achievement_id", achievementId);
            }

            AddCustomEventData(eventData);

            return Custom("achievement_step", m_EventData);
        }

        /// <summary>
        /// Sends an <c>achievement_unlocked</c> event.
        /// <remarks>
        /// Send this event when all requirements to unlock an achievement have been met.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into achievement completion rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="achievementId">The achievement ID.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AchievementUnlocked (string achievementId, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(achievementId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "achievement_id"));
            }
            else
            {
                m_EventData.Add("achievement_id", achievementId);
            }

            AddCustomEventData(eventData);

            return Custom("achievement_unlocked", m_EventData);
        }

        /// <summary>
        /// Sends an <c>ad_complete</c> event.
        /// <remarks>
        /// Send this event when an ad is successfully viewed and not skipped.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into ad completion rates among players, and may indicate the effectiveness of ads by placement in game.
        /// The <c>ad_complete</c> event should be preceded by an <c>ad_start</c> event, sent using AnalyticsEvent.AdStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider (optional).</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdComplete (bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("rewarded", rewarded);

            if (!string.IsNullOrEmpty(advertisingNetwork))
            {
                m_EventData.Add("network", advertisingNetwork);
            }

            if (!string.IsNullOrEmpty(placementId))
            {
                m_EventData.Add("placement_id", placementId);
            }

            AddCustomEventData(eventData);

            return Custom("ad_complete", m_EventData);
        }

        /// <summary>
        /// Sends an <c>ad_complete</c> event.
        /// <remarks>
        /// Send this event when an ad is successfully viewed and not skipped.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into ad completion rates among players, and may indicate the effectiveness of ads by placement in game.
        /// The <c>ad_complete</c> event should be preceded by an <c>ad_start</c> event, sent using AnalyticsEvent.AdStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider.</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdComplete (bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
        {
            return AdComplete(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
        }

        /// <summary>
        /// Sends an <c>ad_offer</c> event.
        /// <remarks>
        /// Send this event when the player is offered the opportunity to view an ad.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into how players respond to ad offers with regards to the placement in game.
        /// Offers are typically provided prior to showing a rewarded ad, where players are granted a reward for viewing an ad without skipping it.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider (optional).</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdOffer (bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("rewarded", rewarded);

            if (!string.IsNullOrEmpty(advertisingNetwork))
            {
                m_EventData.Add("network", advertisingNetwork);
            }

            if (!string.IsNullOrEmpty(placementId))
            {
                m_EventData.Add("placement_id", placementId);
            }

            AddCustomEventData(eventData);

            return Custom("ad_offer", m_EventData);
        }

        /// <summary>
        /// Sends an <c>ad_offer</c> event.
        /// <remarks>
        /// Send this event when the player is offered the opportunity to view an ad.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into how players respond to ad offers with regards to the placement in game.
        /// Offers are typically provided prior to showing a rewarded ad, where players are granted a reward for viewing an ad without skipping it.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider.</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdOffer (bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
        {
            return AdOffer(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
        }

        /// <summary>
        /// Sends an <c>ad_skip</c> event.
        /// <remarks>
        /// Send this event when the player opts to skip a video ad during video playback.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into video ad completion rates with regards to the placement in game.
        /// The <c>ad_skip</c> event should be preceded by an <c>ad_start</c> event, sent using AnalyticsEvent.AdStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider (optional).</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdSkip (bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("rewarded", rewarded);

            if (!string.IsNullOrEmpty(advertisingNetwork))
            {
                m_EventData.Add("network", advertisingNetwork);
            }

            if (!string.IsNullOrEmpty(placementId))
            {
                m_EventData.Add("placement_id", placementId);
            }

            AddCustomEventData(eventData);

            return Custom("ad_skip", m_EventData);
        }

        /// <summary>
        /// Sends an <c>ad_skip</c> event.
        /// <remarks>
        /// Send this event when the player opts to skip a video ad during video playback.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into video ad completion rates with regards to the placement in game.
        /// The <c>ad_skip</c> event should be preceded by an <c>ad_start</c> event, sent using AnalyticsEvent.AdStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider.</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdSkip (bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
        {
            return AdSkip(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
        }

        /// <summary>
        /// Sends an <c>ad_start</c> event.
        /// <remarks>
        /// Send this event when an ad is shown.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into how players are actually choosing to start watching an ad.
        /// The <c>ad_start</c> event should precede <c>ad_complete</c>,
        /// <c>ad_skip</c>, and <c>post_ad_action</c> events.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider (optional).</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdStart (bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("rewarded", rewarded);

            if (!string.IsNullOrEmpty(advertisingNetwork))
            {
                m_EventData.Add("network", advertisingNetwork);
            }

            if (!string.IsNullOrEmpty(placementId))
            {
                m_EventData.Add("placement_id", placementId);
            }

            AddCustomEventData(eventData);

            return Custom("ad_start", m_EventData);
        }

        /// <summary>
        /// Sends an <c>ad_start</c> event.
        /// <remarks>
        /// Send this event when an ad is shown.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into how players are actually choosing to start watching an ad.
        /// The <c>ad_start</c> event should precede <c>ad_complete</c>,
        /// <c>ad_skip</c>, and <c>post_ad_action</c> events.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider.</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult AdStart (bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
        {
            return AdStart(rewarded, EnumToString(advertisingNetwork), placementId, eventData);
        }

        /// <summary>
        /// Sends a <c>chat_msg_sent</c> event.
        /// <remarks>
        /// Send this event when the player sends a chat message in game.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into how often users are sending chat messages in your game.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult ChatMessageSent (IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            AddCustomEventData(eventData);

            return Custom("chat_msg_sent", m_EventData);
        }

        /// <summary>
        /// Sends a <c>cutscene_skip</c> event.
        /// <remarks>
        /// Send this event when the player opts to skip a cutscene or cinematic screen.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into how cutscenes may affect player engagement.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="cutsceneName">The cutscene name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult CutsceneSkip (string cutsceneName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(cutsceneName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "scene_name"));
            }
            else
            {
                m_EventData.Add("scene_name", cutsceneName);
            }

            AddCustomEventData(eventData);

            return Custom("cutscene_skip", m_EventData);
        }

        /// <summary>
        /// Sends a <c>cutscene_start</c> event.
        /// <remarks>
        /// Send this event when the player opts to skip a cutscene or cinematic screen.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into how cutscenes may affect player engagement.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="cutsceneName">The cutscene name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult CutsceneStart (string cutsceneName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(cutsceneName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "scene_name"));
            }
            else
            {
                m_EventData.Add("scene_name", cutsceneName);
            }

            AddCustomEventData(eventData);

            return Custom("cutscene_start", m_EventData);
        }

        /// <summary>
        /// Sends a <c>first_interaction</c> event.
        /// <remarks>
        /// Send this event with the first action the user takes after install.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into the amount of time it takes for players to engage with your game after install,
        /// or if there is any interaction at all after install.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="actionId">The action ID or name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult FirstInteraction (string actionId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (!string.IsNullOrEmpty(actionId))
            {
                m_EventData.Add("action_id", actionId);
            }

            AddCustomEventData(eventData);

            return Custom("first_interaction", m_EventData);
        }

        /// <summary>
        /// Sends a <c>game_over</c> event.
        /// <remarks>
        /// Send this event when gameplay ends.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight to duration of gameplay and progression rates among players.
        /// The <c>game_over</c> event should be preceded by a <c>game_start</c>event,
        /// sent using AnalyticsEvent.GameStart.
        /// This event is intended for use with games that do not utilize a traditional level structure, or for games that advance through
        /// multiple levels over the course of a single gameplay while starting from a common entry level, such as 0 or 1.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelName">The level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult GameOver (string levelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (!string.IsNullOrEmpty(levelName))
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("game_over", m_EventData);
        }

        /// <summary>
        /// Sends a <c>game_over</c> event.
        /// <remarks>
        /// Send this event when gameplay ends.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight to duration of gameplay and progression rates among players.
        /// The <c>game_start</c> event should precede the <c>game_over</c> event,
        /// sent using AnalyticsEvent.GameOver.
        /// This event is intended for use with games that do not utilize a traditional level structure, or for games that advance through
        /// multiple levels over the course of a single gameplay while starting from a common entry level, such as 0 or 1.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelIndex">The level index or number.</param>
        /// <param name="levelName">The level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult GameOver (int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("level_index", levelIndex);

            if (!string.IsNullOrEmpty(levelName))
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("game_over", m_EventData);
        }

        /// <summary>
        /// Sends a <c>game_start</c> event.
        /// <remarks>
        /// Send this event when gameplay starts.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight to duration of gameplay and progression rates among players.
        /// The <c>game_start</c> event should precede the ><c>game_over</c> event,
        /// sent using AnalyticsEvent.GameOver.
        /// This event is intended for use with games that do not utilize a traditional level structure, or for games that advance through
        /// multiple levels over the course of a single gameplay while starting from a common entry level, such as 0 or 1.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult GameStart (IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            AddCustomEventData(eventData);

            return Custom("game_start", m_EventData);
        }

        /// <summary>
        /// Sends an <c>iap_transaction</c> event.
        /// <remarks>
        /// Send this event when the player spends real-world money to make an In-App Purchase.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// Provides information regarding the acquisition of items via IAP. This can provide insight into both real-world income, as well as game economy balance.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="transactionContext">In what context (store, gift, reward) was the item acquired?</param>
        /// <param name="price">The price of the purchased item.</param>
        /// <param name="itemId">A name or unique identifier for the acquired item.</param>
        /// <param name="itemType">The category of the item that was acquired.</param>
        /// <param name="level">The name or id of the level where the item was acquired (optional).</param>
        /// <param name="transactionId">A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction. (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult IAPTransaction (string transactionContext, float price, string itemId, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(transactionContext))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
            }
            else
            {
                m_EventData.Add("transaction_context", transactionContext);
            }

            if (string.IsNullOrEmpty(itemId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
            }
            else
            {
                m_EventData.Add("item_id", itemId);
            }

            if (!string.IsNullOrEmpty(itemType))
            {
                m_EventData.Add("item_type", itemType);
            }

            if (!string.IsNullOrEmpty(level))
            {
                m_EventData.Add("level", level);
            }

            if (!string.IsNullOrEmpty(transactionId))
            {
                m_EventData.Add("transaction_id", transactionId);
            }

            m_EventData.Add("price", price);

            AddCustomEventData(eventData);

            return Custom("iap_transaction", m_EventData);
        }

        /// <summary>
        /// Sends an <c>item_acquired</c> event.
        /// <remarks>
        /// Send this event when the player acquires an item within the game. Note that in some games acquisitions can occur quite frequently. 
        /// To avoid sending events too frequently, it might be sensible to batch item_acquired events.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into item accumulation rates between players,
        /// and the effect that might have on in-game economies.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="currencyType">Set to AcquisitionType.Premium if the item was purchased with real money; otherwise, AcqusitionType.Soft.</param>
        /// <param name="transactionContext">In what context (store, gift, reward, crafting) was the item acquired?</param>
        /// <param name="amount">The unit quantity of the item that was acquired.</param>
        /// <param name="itemId">A name or unique identifier for the acquired item.</param>
        /// <param name="balance">The balance of the acquired item.</param>
        /// <param name="itemType">The category of the item that was acquired.</param>
        /// <param name="level">The name or id of the level where the item was acquired (optional).</param>
        /// <param name="transactionId">A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction. (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult ItemAcquired (AcquisitionType currencyType, string transactionContext, float amount, string itemId, float balance, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(transactionContext))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
            }
            else
            {
                m_EventData.Add("transaction_context", transactionContext);
            }

            if (string.IsNullOrEmpty(itemId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
            }
            else
            {
                m_EventData.Add("item_id", itemId);
            }

            if (!string.IsNullOrEmpty(itemType))
            {
                m_EventData.Add("item_type", itemType);
            }

            if (!string.IsNullOrEmpty(level))
            {
                m_EventData.Add("level", level);
            }

            if (!string.IsNullOrEmpty(transactionId))
            {
                m_EventData.Add("transaction_id", transactionId);
            }

            m_EventData.Add("currency_type", EnumToString(currencyType));
            m_EventData.Add("amount", amount);
            m_EventData.Add("balance", balance);

            AddCustomEventData(eventData);

            return Custom("item_acquired", m_EventData);
        }

        /// <summary>
        /// Sends an <c>item_acquired</c> event.
        /// <remarks>
        /// Send this event when the player acquires an item within the game. Note that in some games acquisitions can occur quite frequently. 
        /// To avoid sending events too frequently, it might be sensible to batch item_acquired events.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into item accumulation rates between players,
        /// and the effect that might have on in-game economies.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="currencyType">Set to AcquisitionType.Premium if the item was purchased with real money; otherwise, AcqusitionType.Soft.</param>
        /// <param name="transactionContext">In what context (store, gift, reward, crafting) was the item acquired?</param>
        /// <param name="amount">The unit quantity of the item that was acquired.</param>
        /// <param name="itemId">A name or unique identifier for the acquired item.</param>
        /// <param name="itemType">The category of the item that was acquired.</param>
        /// <param name="level">The name or id of the level where the item was acquired (optional).</param>
        /// <param name="transactionId">A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction. (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult ItemAcquired (AcquisitionType currencyType, string transactionContext, float amount, string itemId, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(transactionContext))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
            }
            else
            {
                m_EventData.Add("transaction_context", transactionContext);
            }

            if (string.IsNullOrEmpty(itemId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
            }
            else
            {
                m_EventData.Add("item_id", itemId);
            }

            if (!string.IsNullOrEmpty(itemType))
            {
                m_EventData.Add("item_type", itemType);
            }

            if (!string.IsNullOrEmpty(level))
            {
                m_EventData.Add("level", level);
            }

            if (!string.IsNullOrEmpty(transactionId))
            {
                m_EventData.Add("transaction_id", transactionId);
            }

            m_EventData.Add("currency_type", EnumToString(currencyType));
            m_EventData.Add("amount", amount);

            AddCustomEventData(eventData);

            return Custom("item_acquired", m_EventData);
        }

        /// <summary>
        /// Sends an <c>item_spent</c> event.
        /// </summary>
        /// <remarks>
        /// Send this event when the user expends any resource within the game. Note that in some games expenditures can occur quite frequently. To avoid sending events too frequently, it might be sensible to batch item_spent events.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="currencyType">Set to AcquisitionType.Premium if the item was purchased with real money; otherwise, AcqusitionType.Soft.</param>
        /// <param name="transactionContext">In what context (store, gift, reward, crafting) was the item spent?</param>
        /// <param name="amount">The unit quantity of the item that was spent.</param>
        /// <param name="itemId">A name or unique identifier for the spent item.</param>
        /// <param name="balance">The balance of the acquired item.</param>
        /// <param name="itemType">The category of the item that was spent.</param>
        /// <param name="level">The name or id of the level where the item was acquired (optional).</param>
        /// <param name="transactionId">A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction. (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult ItemSpent (AcquisitionType currencyType, string transactionContext, float amount, string itemId, float balance, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(transactionContext))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
            }
            else
            {
                m_EventData.Add("transaction_context", transactionContext);
            }

            if (string.IsNullOrEmpty(itemId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
            }
            else
            {
                m_EventData.Add("item_id", itemId);
            }

            if (!string.IsNullOrEmpty(itemType))
            {
                m_EventData.Add("item_type", itemType);
            }

            if (!string.IsNullOrEmpty(level))
            {
                m_EventData.Add("level", level);
            }

            if (!string.IsNullOrEmpty(transactionId))
            {
                m_EventData.Add("transaction_id", transactionId);
            }

            m_EventData.Add("currency_type", EnumToString(currencyType));
            m_EventData.Add("amount", amount);
            m_EventData.Add("balance", balance);

            AddCustomEventData(eventData);

            return Custom("item_spent", m_EventData);
        }

        /// <summary>
        /// Sends an <c>item_spent</c> event.
        /// </summary>
        /// <remarks>
        /// Send this event when the user expends any resource within the game. Note that in some games expenditures can occur quite frequently. To avoid sending events too frequently, it might be sensible to batch item_spent events.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="currencyType">Set to AcquisitionType.Premium if the item was purchased with real money; otherwise, AcqusitionType.Soft.</param>
        /// <param name="transactionContext">In what context (store, gift, reward, crafting) was the item spent?</param>
        /// <param name="amount">The unit quantity of the item that was spent.</param>
        /// <param name="itemId">A name or unique identifier for the spent item.</param>
        /// <param name="itemType">The category of the item that was spent.</param>
        /// <param name="level">The name or id of the level where the item was spent (optional).</param>
        /// <param name="transactionId">A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction. (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult ItemSpent (AcquisitionType currencyType, string transactionContext, float amount, string itemId, string itemType = null, string level = null, string transactionId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(transactionContext))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "transaction_context"));
            }
            else
            {
                m_EventData.Add("transaction_context", transactionContext);
            }

            if (string.IsNullOrEmpty(itemId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id"));
            }
            else
            {
                m_EventData.Add("item_id", itemId);
            }

            if (!string.IsNullOrEmpty(itemType))
            {
                m_EventData.Add("item_type", itemType);
            }

            if (!string.IsNullOrEmpty(level))
            {
                m_EventData.Add("level", level);
            }

            if (!string.IsNullOrEmpty(transactionId))
            {
                m_EventData.Add("transaction_id", transactionId);
            }

            m_EventData.Add("currency_type", EnumToString(currencyType));
            m_EventData.Add("amount", amount);

            AddCustomEventData(eventData);

            return Custom("item_spent", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_complete</c> event.
        /// <remarks>
        /// Send this event when the player successfully completes a level.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players.
        /// The <c>level_complete</c> event should be preceded by a <c>level_start</c> event,
        /// sent using AnalyticsEvent.LevelStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelName">The level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelComplete (string levelName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(levelName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
            }
            else
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_complete", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_complete</c> event.
        /// <remarks>
        /// Send this event when the player successfully completes a level.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players.
        /// The <c>level_complete</c> event should be preceded by a <c>level_start</c> event,
        /// sent using AnalyticsEvent.LevelStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelIndex">The level index or number.</param>
        /// <param name="levelName">The level name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelComplete (int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("level_index", levelIndex);

            if (!string.IsNullOrEmpty(levelName))
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_complete", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_fail</c> event.
        /// <remarks>
        /// Send this event when the player fails to successfully complete a level.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players, and potentially help predict when players may churn.
        /// The <c>level_fail</c> event should be preceded by a <c>level_start</c> event,
        /// sent using AnalyticsEvent.LevelStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelName">The level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelFail (string levelName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(levelName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
            }
            else
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_fail", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_fail</c> event.
        /// <remarks>
        /// Send this event when the player fails to successfully complete a level.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players, and potentially help predict when players may churn.
        /// The <c>level_fail</c> event should be preceded by a <c>level_start</c> event,
        /// sent using AnalyticsEvent.LevelStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelIndex">The level index or number.</param>
        /// <param name="levelName">The level name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelFail (int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("level_index", levelIndex);

            if (!string.IsNullOrEmpty(levelName))
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_fail", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_quit</c> event.
        /// <remarks>
        /// Send this event when the player opts to quit from a level before completing it.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into gameplay attrition rates by level, and potentially help predict when players may churn.
        /// The <c>level_quit</c> event should be preceded by a <c>level_start</c> event,
        /// sent using AnalyticsEvent.LevelStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelName">The level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelQuit (string levelName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(levelName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
            }
            else
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_quit", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_quit</c> event.
        /// <remarks>
        /// Send this event when the player opts to quit from a level before completing it.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into gameplay attrition rates by level, and potentially help predict when players may churn.
        /// The <c>level_quit</c> event should be preceded by a <c>level_start</c> event,
        /// sent using AnalyticsEvent.LevelStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelIndex">The level index or number.</param>
        /// <param name="levelName">The level name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelQuit (int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("level_index", levelIndex);

            if (!string.IsNullOrEmpty(levelName))
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_quit", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_skip</c> event.
        /// <remarks>
        /// Send this event when the player opts to skip a level in order to continue onto the next without having to completing it first.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players, and potentially help predict when players may churn.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelName">The level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelSkip (string levelName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(levelName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
            }
            else
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_skip", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_skip</c> event.
        /// <remarks>
        /// Send this event when the player opts to skip a level in order to contiue onto the next without having to completing it.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players, and potentially help predict when players may churn.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelIndex">The level index or number.</param>
        /// <param name="levelName">The level name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelSkip (int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("level_index", levelIndex);

            if (!string.IsNullOrEmpty(levelName))
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_skip", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_start</c> event.
        /// <remarks>
        /// Send this event when the player enters into a level.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players.
        /// The <c>level_start</c> event should precede most other level specific events, including 
        /// <c>level_complete</c>, <c>level_fail</c>, and <c>level_quit</c>.
        /// The <c>level_skip</c> event does not need to be preceded by <c>level_start</c>
        /// if the level is skipped without having to enter into the level.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelName">The level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelStart (string levelName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(levelName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "level_name"));
            }
            else
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_start", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_start</c> event.
        /// <remarks>
        /// Send this event when the player enters into a level.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into level progression rates among players.
        /// The <c>level_start</c> event should precede most other level specific events, including 
        /// <c>level_complete</c>, <c>level_fail</c>, and <c>level_quit</c>.
        /// The <c>level_skip</c> event does not need to be preceded by <c>level_start</c>
        /// if the level is skipped without having to enter into the level.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="levelIndex">The level index or number.</param>
        /// <param name="levelName">The level name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelStart (int levelIndex, string levelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("level_index", levelIndex);

            if (!string.IsNullOrEmpty(levelName))
            {
                m_EventData.Add("level_name", levelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_start", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_up</c> event.
        /// <remarks>
        /// Send this event when the player rank increases, or when the accumulated experience reaches the next level tier.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into gameplay progression rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="newLevelName">The new rank or level name.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelUp (string newLevelName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(newLevelName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "new_level_name"));
            }
            else
            {
                m_EventData.Add("new_level_name", newLevelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_up", m_EventData);
        }

        /// <summary>
        /// Sends a <c>level_up</c> event.
        /// <remarks>
        /// Send this event when the player rank increases, or when the accumulated experience reaches the next level tier.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into gameplay progression rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="newLevelIndex">The level index or number.</param>
        /// <param name="newLevelName">The new rank or level name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult LevelUp (int newLevelIndex, string newLevelName = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("new_level_index", newLevelIndex);

            if (!string.IsNullOrEmpty(newLevelName))
            {
                m_EventData.Add("new_level_name", newLevelName);
            }

            AddCustomEventData(eventData);

            return Custom("level_up", m_EventData);
        }

        /// <summary>
        /// Sends a <c>post_ad_action</c> event.
        /// <remarks>
        /// Send this event when the player takes action in response to an ad, such as clicking a download link.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into how players might be responding to ads depending on placement.
        /// A lack of <c>post_ad_action</c> events may help in identifying player churn.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider.</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult PostAdAction (bool rewarded, string advertisingNetwork = null, string placementId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("rewarded", rewarded);

            if (!string.IsNullOrEmpty(advertisingNetwork))
            {
                m_EventData.Add("network", advertisingNetwork);
            }

            if (!string.IsNullOrEmpty(placementId))
            {
                m_EventData.Add("placement_id", placementId);
            }

            AddCustomEventData(eventData);

            return Custom("post_ad_action", m_EventData);
        }

        /// <summary>
        /// Sends a <c>post_ad_action</c> event.
        /// <remarks>
        /// Send this event when the player takes action in response to an ad, such as clicking a download link.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can help provide insight into how players might be responding to ads depending on placement.
        /// A lack of <c>post_ad_action</c> events may help in identifying player churn.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="rewarded">Set to <c>true</c> if a reward was offered for viewing the ad; otherwise, <c>false</c>.</param>
        /// <param name="advertisingNetwork">The ad or mediation network provider.</param>
        /// <param name="placementId">The ad placement or configuration ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult PostAdAction (bool rewarded, AdvertisingNetwork advertisingNetwork, string placementId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("rewarded", rewarded);
            m_EventData.Add("network", EnumToString(advertisingNetwork));

            if (!string.IsNullOrEmpty(placementId))
            {
                m_EventData.Add("placement_id", placementId);
            }

            AddCustomEventData(eventData);

            return Custom("post_ad_action", m_EventData);
        }

        /// <summary>
        /// Sends a <c>push_notification_click</c> event.
        /// <remarks>
        /// Send this event when the player clicks on a push notification.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into the level of player engagement with push notifications.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="messageId">The message name or ID.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult PushNotificationClick (string messageId, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(messageId))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "message_id"));
            }
            else
            {
                m_EventData.Add("message_id", messageId);
            }

            AddCustomEventData(eventData);

            return Custom("push_notification_click", m_EventData);
        }

        /// <summary>
        /// Sends a <c>push_notification_enable</c> event.
        /// <remarks>
        /// Send this event when the player enables or grants permission for the game to use push notifications.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into player acceptance rates for push notifications.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult PushNotificationEnable (IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            AddCustomEventData(eventData);

            return Custom("push_notification_enable", m_EventData);
        }

        /// <summary>
        /// Sends a <c>screen_visit</c> event.
        /// <remarks>
        /// Send this event when the player opens a menu or visits a screen in the game.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into navigational flows, and may help predict when players may churn.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="screenName">The name of the screen or type of screen visited.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult ScreenVisit (ScreenName screenName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("screen_name", EnumToString(screenName));

            AddCustomEventData(eventData);

            return Custom("screen_visit", m_EventData);
        }

        /// <summary>
        /// Sends a <c>screen_visit</c> event.
        /// <remarks>
        /// Send this event when the player opens a menu or visits a screen in the game.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into navigational flows, and may help predict when players may churn.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="screenName">The name of the screen or type of screen visited.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult ScreenVisit (string screenName, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(screenName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "screen_name"));
            }
            else
            {
                m_EventData.Add("screen_name", screenName);
            }

            AddCustomEventData(eventData);

            return Custom("screen_visit", m_EventData);
        }

        /// <summary>
        /// Sends a <c>social_share</c> event.
        /// <remarks>
        /// Send this event when the player posts a message about the game through social media.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShare (ShareType shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("share_type", EnumToString(shareType));
            m_EventData.Add("social_network", EnumToString(socialNetwork));

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }

            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share", m_EventData);
        }

        /// <summary>
        /// Sends a <c>social_share</c> event.
        /// <remarks>
        /// Send this event when the player posts a message about the game through social media.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShare (ShareType shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("share_type", EnumToString(shareType));
            if (string.IsNullOrEmpty(socialNetwork))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
            }
            else
            {
                m_EventData.Add("social_network", socialNetwork);
            }

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }

            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share", m_EventData);
        }

        /// <summary>
        /// Sends a <c>social_share</c> event.
        /// <remarks>
        /// Send this event when the player posts a message about the game through social media.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShare (string shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(shareType))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
            }
            else
            {
                m_EventData.Add("share_type", shareType);
            }

            m_EventData.Add("social_network", EnumToString(socialNetwork));

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }
            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share", m_EventData);
        }

        /// <summary>
        /// Sends a <c>social_share</c> event.
        /// <remarks>
        /// Send this event when the player posts a message about the game through social media.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShare (string shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(shareType))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
            }
            else
            {
                m_EventData.Add("share_type", shareType);
            }

            if (string.IsNullOrEmpty(socialNetwork))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
            }
            else
            {
                m_EventData.Add("social_network", socialNetwork);
            }

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }

            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share", m_EventData);
        }


        /// <summary>
        /// Sends a <c>social_share_accept</c> event.
        /// <remarks>
        /// Send this event when a player reacts to a social share event sent by another user.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShareAccept (ShareType shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("share_type", EnumToString(shareType));
            m_EventData.Add("social_network", EnumToString(socialNetwork));

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }

            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share_accept", m_EventData);
        }

        /// <summary>
        /// Sends a <c>social_share_accept</c> event.
        /// <remarks>
        /// Send this event when a player reacts to a social share event sent by another user.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShareAccept (ShareType shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("share_type", EnumToString(shareType));

            if (string.IsNullOrEmpty(socialNetwork))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
            }
            else
            {
                m_EventData.Add("social_network", socialNetwork);
            }

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }

            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share_accept", m_EventData);
        }

        /// <summary>
        /// Sends a <c>social_share_accept</c> event.
        /// <remarks>
        /// Send this event when a player reacts to a social share event sent by another user.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShareAccept (string shareType, SocialNetwork socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(shareType))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
            }
            else
            {
                m_EventData.Add("share_type", shareType);
            }

            m_EventData.Add("social_network", EnumToString(socialNetwork));

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }

            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share_accept", m_EventData);
        }

        /// <summary>
        /// Sends a <c>social_share_accept</c> event.
        /// <remarks>
        /// Send this event when a player reacts to a social share event sent by another user.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into social engagement trends.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="shareType">The mode of sharing, or media type used in the social engagement.</param>
        /// <param name="socialNetwork">The network used in the social engagement.</param>
        /// <param name="senderId">The id of the sender (optional)</param>
        /// <param name="recipientId">The id of the recipient (optional)</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult SocialShareAccept (string shareType, string socialNetwork, string senderId = null, string recipientId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(shareType))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "share_type"));
            }
            else
            {
                m_EventData.Add("share_type", shareType);
            }

            if (string.IsNullOrEmpty(socialNetwork))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "social_network"));
            }
            else
            {
                m_EventData.Add("social_network", socialNetwork);
            }

            if (!string.IsNullOrEmpty(senderId))
            {
                m_EventData.Add("sender_id", senderId);
            }

            if (!string.IsNullOrEmpty(recipientId))
            {
                m_EventData.Add("recipient_id", recipientId);
            }

            AddCustomEventData(eventData);

            return Custom("social_share_accept", m_EventData);
        }

        /// <summary>
        /// Sends a <c>store_item_click</c> event.
        /// <remarks>
        /// Send this event when the player clicks on an item in the store.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into player engagement with store inventory.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="storeType">Set to StoreType.Premium if purchases use real-world money; otherwise, StoreType.Soft</param>
        /// <param name="itemId">The item ID.</param>
        /// <param name="itemName">The item name (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult StoreItemClick (StoreType storeType, string itemId, string itemName = null, Dictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("type", EnumToString(storeType));

            if (string.IsNullOrEmpty(itemId) && string.IsNullOrEmpty(itemName))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "item_id or item_name"));
            }
            else
            {
                if (!string.IsNullOrEmpty(itemId))
                {
                    m_EventData.Add("item_id", itemId);
                }

                if (!string.IsNullOrEmpty(itemName))
                {
                    m_EventData.Add("item_name", itemName);
                }
            }

            AddCustomEventData(eventData);

            return Custom("store_item_click", m_EventData);
        }

        /// <summary>
        /// Sends a <c>store_opened</c> event.
        /// <remarks>
        /// Send this event when the player opens a store in game.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into potential player engagement with store inventory.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="storeType">Set to StoreType.Premium if purchases use real-world money; otherwise, StoreType.Soft</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult StoreOpened (StoreType storeType, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("type", EnumToString(storeType));

            AddCustomEventData(eventData);

            return Custom("store_opened", m_EventData);
        }

        /// <summary>
        /// Sends a <c>tutorial_complete</c> event.
        /// <remarks>
        /// Send this event when the player completes a tutorial.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into tutorial completion rates among players.
        /// The <c>tutorial_complete</c> event should be preceded by an <c>tutorial_start</c> event,
        /// sent using AnalyticsEvent.TutorialStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="tutorialId">The tutorial name or ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult TutorialComplete (string tutorialId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (!string.IsNullOrEmpty(tutorialId))
            {
                m_EventData.Add("tutorial_id", tutorialId);
            }

            AddCustomEventData(eventData);

            return Custom("tutorial_complete", m_EventData);
        }

        /// <summary>
        /// Sends a <c>tutorial_skip</c> event.
        /// <remarks>
        /// Send this event when the player opts to skip a tutorial.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into tutorial progression rates among players.
        /// The <c>tutorial_complete</c> event should be preceded by an <c>tutorial_start</c> event,
        /// sent using AnalyticsEvent.TutorialStart.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="tutorialId">The tutorial name or ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult TutorialSkip (string tutorialId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (!string.IsNullOrEmpty(tutorialId))
            {
                m_EventData.Add("tutorial_id", tutorialId);
            }

            AddCustomEventData(eventData);

            return Custom("tutorial_skip", m_EventData);
        }

        /// <summary>
        /// Sends a <c>tutorial_start</c> event.
        /// <remarks>
        /// Send this event when the player starts a tutorial.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into tutorial progression rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="tutorialId">The tutorial name or ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult TutorialStart (string tutorialId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (!string.IsNullOrEmpty(tutorialId))
            {
                m_EventData.Add("tutorial_id", tutorialId);
            }

            AddCustomEventData(eventData);

            return Custom("tutorial_start", m_EventData);
        }

        /// <summary>
        /// Sends a <c>tutorial_step</c> event.
        /// <remarks>
        /// Send this event when the player completes a step or stage in a multi-part tutorial.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into tutorial progression rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="stepIndex">The step or stage completed in a multi-part tutorial.</param>
        /// <param name="tutorialId">The tutorial name or ID (optional).</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult TutorialStep (int stepIndex, string tutorialId = null, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("step_index", stepIndex);

            if (!string.IsNullOrEmpty(tutorialId))
            {
                m_EventData.Add("tutorial_id", tutorialId);
            }

            AddCustomEventData(eventData);

            return Custom("tutorial_step", m_EventData);
        }

        /// <summary>
        /// Sends a <c>user_signup</c> event.
        /// <remarks>
        /// Send this event when the player registers for an account or logs in for the first time in game.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into login acceptance rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="authorizationNetwork">The authorization network or login service provider.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult UserSignup (string authorizationNetwork, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            if (string.IsNullOrEmpty(authorizationNetwork))
            {
                OnValidationFailed(string.Format(k_ErrorFormat_RequiredParamNotSet, "authorization_network"));
            }
            else
            {
                m_EventData.Add("authorization_network", authorizationNetwork);
            }

            AddCustomEventData(eventData);

            return Custom("user_signup", m_EventData);
        }

        /// <summary>
        /// Sends a <c>user_signup</c> event.
        /// <remarks>
        /// Send this event when the player registers for an account or logs in for the first time in game.
        /// </remarks>
        /// </summary>
        /// <remarks>
        /// This standard event can provide insight into login acceptance rates among players.
        /// </remarks>
        /// <returns>The result of the analytics event sent.</returns>
        /// <param name="authorizationNetwork">The authorization or login service provider.</param>
        /// <param name="eventData">Custom event data (optional).</param>
        public static AnalyticsResult UserSignup (AuthorizationNetwork authorizationNetwork, IDictionary<string, object> eventData = null)
        {
            m_EventData.Clear();

            m_EventData.Add("authorization_network", EnumToString(authorizationNetwork));

            AddCustomEventData(eventData);

            return Custom("user_signup", m_EventData);
        }
    }
}
