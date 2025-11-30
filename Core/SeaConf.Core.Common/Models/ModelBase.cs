using SeaConf.Common;
using System.Runtime.CompilerServices;

namespace SeaConf.Core.Common.Models;

#nullable disable

/// <summary>
/// Configuration data model base class.
/// </summary>
public abstract class ModelBase
{
	protected bool IsInitialized { get; private set; }

	/// <summary>
	/// Setting the "initialized" flag.
	/// </summary>
	protected void SetInit()
	{
		IsInitialized = true;
	}

	/// <summary>
	/// Throwing an exception if not initialized.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void ThrowIfNoInit()
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException(string.Format(Resources.ModelNotInitialized, GetType().Name));
		}
	}
}