namespace UnityEngine.Analytics.Experimental
{
    /// <summary>
    /// The type of currency (premium or soft) used to acquire the item.
    /// </summary>
    public enum AcquisitionType
    {
        /// <summary>Not directly purchased with real-world money.</summary>
        Soft = 0,
        /// <summary>Purchased with real-world money.</summary>
        Premium,
    }
}
