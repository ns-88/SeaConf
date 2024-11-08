using System;
using System.Runtime.CompilerServices;
using SeaConf.Infrastructure;

#nullable disable

namespace SeaConf.Models
{
    /// <summary>
    /// Configuration data model base class.
    /// </summary>
    public abstract class ModelBase
    {
        private bool _isInit;

        /// <summary>
        /// Setting the "initialized" flag.
        /// </summary>
        protected void SetInit()
        {
            _isInit = true;
        }

        /// <summary>
        /// Throwing an exception if not initialized.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfNoInit()
        {
            if (!_isInit)
            {
                throw new InvalidOperationException(string.Format(Strings.ModelNotInitialized, GetType().Name));
            }
        }
    }
}