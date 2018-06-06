using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    public struct EventPayloads
    {
        public static Type[] s_EventTypes = new Type[35]{
            // Application
            typeof(ScreenVisit),
            typeof(CutsceneStart),
            typeof(CutsceneSkip),

            // Progression
            typeof(GameStart),
            typeof(GameOver),
            typeof(LevelStart),
            typeof(LevelSkip),
            typeof(LevelFail),
            typeof(LevelQuit),
            typeof(LevelComplete),
            typeof(LevelUp),


            // Onboarding
            typeof(FirstInteraction),
            typeof(TutorialStart),
            typeof(TutorialStep),
            typeof(TutorialSkip),
            typeof(TutorialComplete),

            // Engagement
            typeof(AchievementStep),
            typeof(AchievementUnlocked),
            typeof(ChatMessageSent),
            typeof(PushNotificationEnable),
            typeof(PushNotificationClick),
            typeof(SocialShare),
            typeof(SocialShareAccept),
            typeof(UserSignup),

            // Monetization
            typeof(AdOffer),
            typeof(AdStart), 
            typeof(AdSkip),
            typeof(AdComplete),
            typeof(PostAdAction),
            typeof(IAPTransaction),
            typeof(ItemAcquired),
            typeof(ItemSpent),
            typeof(StoreOpened),
            typeof(StoreItemClick),

            // Custom
            typeof(CustomEvent)
        };
    }

    [StandardEventName("", "", "An event you define yourself.")]
    public struct CustomEvent
    {
    }

    [StandardEventName("achievement_step", "Engagement", "Send this event when a requirement or step toward completing a multi-part achievement is complete.")]
    public struct AchievementStep
    {
        [RequiredParameter("step_index", "The order of the step (required).")]
        public int stepIndex;
        [RequiredParameter("achievement_id", "A unique id for this achievement (optional).")]
        public string achievementId;
    }

    [StandardEventName("achievement_unlocked", "Engagement", "Send this event when all requirements to unlock an achievement have been met.")]
    public struct AchievementUnlocked
    {
        [RequiredParameter("achievement_id", "A unique id for this achievement (optional).")]
        public string achievementId;
    }

    [StandardEventName("ad_complete", "Monetization", "Send this event when an ad is successfully viewed and not skipped.")]
    public struct AdComplete
    {
        [RequiredParameter("rewarded", "Set to true if a reward is offered for this ad (required).")]
        public bool rewarded;
        [CustomizableEnum(true)]
        [OptionalParameter("advertising_network", "The ad or mediation network provider (optional).")]
        public AdvertisingNetwork advertisingNetwork;
        [OptionalParameter("placement_id", "An ad placement or configuration ID (optional).")]
        public string placementId;
    }

    [StandardEventName("ad_offer", "Monetization", "Send this event when the player is offered the opportunity to view an ad.")]
    public struct AdOffer
    {
        [RequiredParameter("rewarded", "Set to true if a reward is offered for this ad (required).")]
        public bool rewarded;
        [OptionalParameter("advertising_network", "The ad or mediation network provider (optional).")]
        [CustomizableEnum(true)]
        public AdvertisingNetwork advertisingNetwork;
        [OptionalParameter("placement_id", "An ad placement or configuration ID (optional).")]
        public string placementId;
    }

    [StandardEventName("ad_skip", "Monetization", "Send this event when the player opts to skip a video ad during video playback.")]
    public struct AdSkip
    {
        [RequiredParameter("rewarded", "Set to true if a reward is offered for this ad (required).")]
        public bool rewarded;
        [CustomizableEnum(true)]
        [OptionalParameter("advertising_network", "The ad or mediation network provider (optional).")]
        public AdvertisingNetwork advertisingNetwork;
        [OptionalParameter("placement_id", "An ad placement or configuration ID (optional).")]
        public string placementId;
    }

    [StandardEventName("ad_start", "Monetization", "Send this event when playback of an ad begins.")]
    public struct AdStart
    {
        [RequiredParameter("rewarded", "Set to true if a reward is offered for this ad (required).")]
        public bool rewarded;
        [CustomizableEnum(true)]
        [OptionalParameter("advertising_network", "The ad or mediation network provider (optional).")]
        public AdvertisingNetwork advertisingNetwork;
        [OptionalParameter("placement_id", "An ad placement or configuration ID (optional).")]
        public string placementId;
    }

    [StandardEventName("post_ad_action", "Monetization", "Send this event with the first action a player takes after an ad is shown, or after an ad is offered but not shown.")]
    public struct PostAdAction
    {
        [RequiredParameter("rewarded", "Set to true if a reward is offered for this ad (required).")]
        public bool rewarded;
        [CustomizableEnum(true)]
        [OptionalParameter("advertising_network", "The ad or mediation network provider (optional).")]
        public AdvertisingNetwork advertisingNetwork;
        [OptionalParameter("placement_id", "An ad placement or configuration ID (optional).")]
        public string placementId;
    }

    [StandardEventName("chat_message_sent", "Engagement", "Send this event when the player sends a chat message in game.")]
    public struct ChatMessageSent
    {
    }

    [StandardEventName("cutscene_start", "Application", "Send this event when the player begins to watch a cutscene or cinematic screen.")]
    public struct CutsceneStart
    {
        [RequiredParameter("cutscene_name", "The name of the cutscene being viewed (required).")]
        public string name;
    }

    [StandardEventName("cutscene_skip", "Application", "Send this event when the player opts to skip a cutscene or cinematic screen.")]
    public struct CutsceneSkip
    {
        [RequiredParameter("cutscene_name", "The name of the cutscene skipped (required).")]
        public string name;
    }

    [StandardEventName("game_over", "Progression", "Send this event when gameplay ends (in a game with an identifiable conclusion).")]
    public struct GameOver
    {
        [OptionalParameter("level_name", "The level name.")]
        public string name;
        [OptionalParameter("level_index", "The order of this level within the game.")]
        public int index;
    }

    [StandardEventName("game_start", "Progression", "Send this event when gameplay starts. Usually used only in games with an identifiable conclusion.")]
    public struct GameStart
    {
    }

    [StandardEventName("iap_transaction", "Monetization", "Send this event when the player spends real-world money to make an In-App Purchase.")]
    public struct IAPTransaction
    {
        [RequiredParameter("transaction_context", "In what context (store, gift, reward) was the item acquired?")]
        public string transactionContext;
        [RequiredParameter("price", "How much did the item cost?")]
        public float price;
        [RequiredParameter("item_id", "A name or unique identifier for the acquired item.")]
        public string itemId;
        [OptionalParameter("item_type", "The category of the item that was acquired.")]
        public string itemType;
        [OptionalParameter("level", "The name or id of the level where the item was acquired.")]
        public string level;
        [OptionalParameter("transaction_id", "A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction.")]
        public string transactionId;
    }

    [StandardEventName("item_acquired", "Monetization", "Send this event when the player acquires an item within the game.")]
    public struct ItemAcquired
    {
        [RequiredParameter("currency_type", "Set to AcquisitionType.Premium if the item was purchased with real money; otherwise, AcqusitionType.Soft.")]
        public AcquisitionType currencyType;
        [RequiredParameter("transaction_context", "In what context(store, gift, reward, crafting) was the item acquired?")]
        public string transactionContext;
        [RequiredParameter("amount", "The unit quantity of the item that was acquired")]
        public float amount;
        [RequiredParameter("item_id", "A name or unique identifier for the acquired item.")]
        public string itemId;
        [OptionalParameter("balance", "The balance of the acquired item.")]
        public float balance;
        [OptionalParameter("item_type", "The category of the item that was acquired.")]
        public string itemType;
        [OptionalParameter("level", "The name or id of the level where the item was acquired.")]
        public string level;
        [OptionalParameter("transaction_id", "A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction.")]
        public string transactionId;
    }

    [StandardEventName("item_spent", "Monetization", "Send this event when the player spends an item.")]
    public struct ItemSpent
    {
        [RequiredParameter("currency_type", "Set to AcquisitionType.Premium if the item was purchased with real money; otherwise, AcqusitionType.Soft.")]
        public AcquisitionType currencyType;
        [RequiredParameter("transaction_context", "In what context(store, gift, reward, crafting) was the item spent?")]
        public string transactionContext;
        [RequiredParameter("amount", "The unit quantity of the item that was spent")]
        public float amount;
        [RequiredParameter("item_id", "A name or unique identifier for the spent item.")]
        public string itemId;
        [OptionalParameter("balance", "The balance of the spent item.")]
        public float balance;
        [OptionalParameter("item_type", "The category of the item that was spent.")]
        public string itemType;
        [OptionalParameter("level", "The name or id of the level where the item was spent.")]
        public string level;
        [OptionalParameter("transaction_id", "A unique identifier for the specific transaction that occurred. You can use this to group multiple events into a single transaction.")]
        public string transactionId;
    }

    [StandardEventName("level_complete", "Progression", "Send this event when the player has successfully completed a level.")]
    public struct LevelComplete
    {
        [RequiredParameter("level_name", "The level name. Either level_name or level_index is required.", "level")]
        public string name;
        [RequiredParameter("level_index", "The order of this level within the game. Either level_name or level_index is required.", "level")]
        public int index;
    }

    [StandardEventName("level_fail", "Progression", "Send this event when the player sucessfully completes a level.")]
    public struct LevelFail
    {
        [RequiredParameter("level_name", "The level name. Either level_name or level_index is required.", "level")]
        public string name;
        [RequiredParameter("level_index", "The order of this level within the game. Either level_name or level_index is required.", "level")]
        public int index;
    }

    [StandardEventName("level_quit", "Progression", "Send this event when the player opts to quit from a level before completing it.")]
    public struct LevelQuit
    {
        [RequiredParameter("level_name", "The level name. Either level_name or level_index is required.", "level")]
        public string name;
        [RequiredParameter("level_index", "The order of this level within the game. Either level_name or level_index is required.", "level")]
        public int index;
    }

    [StandardEventName("level_skip", "Progression", "Send this event when the player opts to skip past a level.")]
    public struct LevelSkip
    {
        [RequiredParameter("level_name", "The level name. Either level_name or level_index is required.", "level")]
        public string name;
        [RequiredParameter("level_index", "The order of this level within the game. Either level_name or level_index is required.", "level")]
        public int index;
    }

    [StandardEventName("level_start", "Progression", "Send this event when the player enters into or begins a level.")]
    public struct LevelStart
    {
        [RequiredParameter("level_name", "The level name. Either level_name or level_index is required.", "level")]
        public string name;
        [RequiredParameter("level_index", "The order of this level within the game. Either level_name or level_index is required.", "level")]
        public int index;
    }

    [StandardEventName("level_up", "Progression", "Send this event when the player rank or level increases.")]
    public struct LevelUp
    {
        [RequiredParameter("new_level_name", "The new rank or level name (required).", "level")]
        public string name;
        [RequiredParameter("new_level_index", "The new rank or level index (required).", "level")]
        public int index;
    }

    [StandardEventName("first_interaction", "Onboarding", "Send this event with the first voluntary action the user takes after install.")]
    public struct FirstInteraction
    {
        [RequiredParameter("action_id", "The action ID or name. For example, a unique identifier for the button clicked (optional).\n")]
        public string actionId;
    }

    [StandardEventName("push_notification_click", "Engagement", "Send this event when the player responds to a push notification.")]
    public struct PushNotificationClick
    {
        [RequiredParameter("message_id", "The message name or ID (required).")]
        public string message_id;
    }

    [StandardEventName("push_notification_enable", "Engagement", "Send this event when the player enables or grants permission for the game to use push notifications.")]
    public struct PushNotificationEnable
    {
    }

    [StandardEventName("screen_visit", "Application", "Send this event when the player opens a menu or visits a screen in the game.")]
    public struct ScreenVisit
    {
        [CustomizableEnum(true)]
        [RequiredParameter("screen_name", "The name of the screen or type of screen visited (required).")]
        public ScreenName screenName;
    }

    [StandardEventName("social_share", "Engagement", "Send this event when the player posts a message, gift, or invitation through social media.")]
    public struct SocialShare
    {
        [CustomizableEnum(true)]
        [RequiredParameter("share_type", "The mode of sharing, or media type used in the social engagement (required).")]
        public ShareType shareType;
        [CustomizableEnum(true)]
        [RequiredParameter("social_network", "The network through which the message is shared (required).")]
        public SocialNetwork socialNetwork;
        [OptionalParameter("sender_id", "A unique identifier for the sender (optional).")]
        public string senderId;
        [OptionalParameter("recipient_id", "A unique identifier for the recipient (optional).")]
        public string recipientId;
    }

    [StandardEventName("social_share_accept", "Engagement", "Send this event when the player accepts a message, gift, or invitation through social media.")]
    public struct SocialShareAccept
    {
        [CustomizableEnum(true)]
        [RequiredParameter("share_type", "The mode of sharing, or media type used in the social engagement (required).")]
        public ShareType shareType;
        [CustomizableEnum(true)]
        [RequiredParameter("social_network", "The network through which the message is shared (required).")]
        public SocialNetwork socialNetwork;
        [OptionalParameter("sender_id", "A unique identifier for the sender (optional).")]
        public string senderId;
        [OptionalParameter("recipient_id", "A unique identifier for the recipient (optional).")]
        public string recipientId;
    }

    [StandardEventName("store_item_click", "Monetization", "Send this event when the player clicks on an item in the store.")]
    public struct StoreItemClick
    {
        [RequiredParameter("type", "Set to StoreType.Premium if purchases use real-world money; otherwise, StoreType.Soft (required).")]
        public StoreType storeType;
        [RequiredParameter("item_id", "A unique identifier for the item (required).", "item")]
        public string itemId;
        [RequiredParameter("item_name", "The item's name (optional).", "item")]
        public string itemName;
    }

    [StandardEventName("store_opened", "Monetization", "Send this event when the player opens a store in game.")]
    public struct StoreOpened
    {
        [RequiredParameter("type", "Set to StoreType.Premium if purchases use real-world money; otherwise, StoreType.Soft (required).")]
        public StoreType storeType;
    }

    [StandardEventName("tutorial_complete", "Onboarding", "Send this event when the player completes a tutorial.")]
    public struct TutorialComplete
    {
        [OptionalParameter("tutorial_id", "The tutorial name or ID (optional).")]
        public string tutorialId;
    }

    [StandardEventName("tutorial_skip", "Onboarding", "Send this event when the player opts to skip a tutorial.")]
    public struct TutorialSkip
    {
        [OptionalParameter("tutorial_id", "The tutorial name or ID (optional).")]
        public string tutorialId;
    }

    [StandardEventName("tutorial_start", "Onboarding", "Send this event when the player starts a tutorial.")]
    public struct TutorialStart
    {
        [OptionalParameter("tutorial_id", "The tutorial name or ID (optional).")]
        public string tutorialId;
    }

    [StandardEventName("tutorial_step", "Onboarding", "Send this event when the player completes a step or stage in a multi-part tutorial.")]
    public struct TutorialStep
    {
        [RequiredParameter("step_index", "The step or stage completed in a multi-part tutorial (required).")]
        public int stepIndex;
        [OptionalParameter("tutorial_id", "The tutorial name or ID (optional).")]
        public string tutorialId;
    }

    [StandardEventName("user_signup", "Engagement", "Send this event when the player registers or logs in for the first time.")]
    public struct UserSignup
    {
        [RequiredParameter("authorization_network", "The authorization network or login service provider (required).")]
        [CustomizableEnum(true)]
        public AuthorizationNetwork authorizationNetwork;
    }
}