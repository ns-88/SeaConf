namespace SeaConf.Models
{
    /// <summary>
    /// Sync mode.
    /// </summary>
    public enum SyncMode
	{
        /// <summary>
        /// Is enabled.
        /// </summary>
        Enable,

        /// <summary>
        /// Is disabled.
        /// </summary>
        Disable,

        /// <summary>
        /// Enabled if debugging is enabled.
        /// </summary>
        EnableIfDebug
    }
}