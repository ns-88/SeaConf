using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Common.Models;

/// <summary>
/// Configuration save event data.
/// </summary>
public class ChangedModelsEventArgs : EventArgs
{
	/// <summary>
	/// Modified data models.
	/// </summary>
	public readonly IChangedModels ChangedModels;

	public ChangedModelsEventArgs(IChangedModels changedModels)
	{
		ChangedModels = changedModels;
	}
}