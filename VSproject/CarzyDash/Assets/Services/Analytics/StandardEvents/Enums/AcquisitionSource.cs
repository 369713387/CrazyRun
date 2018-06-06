namespace UnityEngine.Analytics.Experimental
{
    /// <summary>
    /// The source through which an item, consumable, or currency was acquired.
    /// </summary>
    public enum AcquisitionSource
    {
        /// <summary>No available source, or source unknown.</summary>
        None = 0,
        /// <summary>Purchased using currency or consumable resources.</summary>
        Store,
        /// <summary>Awarded through an achievement or other in-game interaction.</summary>
        Earned,
        /// <summary>Granted as a promotion of some in-game feature or through cross promotion.</summary>
        Promotion,
        /// <summary>Granted without motive other than good feelings.</summary>
        Gift,
        /// <summary>Granted as a reward for watching an advertisement.</summary>
        RewardedAd,
        /// <summary>Granted periodically.</summary>
        TimedReward,
        /// <summary>Granted through social engagement.</summary>
        SocialReward,
    }
}
