using System;
using SeaConf.Interfaces;

namespace SeaConf.Models
{
    /// <summary>
    /// Configuration save event data.
    /// </summary>
    public class SavedEventArgs : EventArgs
    {
        /// <summary>
        /// Modified data models.
        /// </summary>
        public readonly IChangedModels ChangedModels;

        public SavedEventArgs(IChangedModels changedModels)
        {
            ChangedModels = changedModels;
        }
    }
}