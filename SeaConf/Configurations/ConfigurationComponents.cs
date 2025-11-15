using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;

namespace SeaConf.Configurations;

internal partial class Configuration : IComponents
{
	/// <inheritdoc />
	public void RaiseLoadedEvent()
	{
		Volatile.Read(ref Loaded)?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc />
	public void RaiseSavedEvent(IChangedModels changedModels)
	{
		Volatile.Read(ref Saved)?.Invoke(this, new ChangedModelsEventArgs(changedModels));
	}

	/// <inheritdoc />
	public void RaisePropertyChangedEvent(IPropertyData propertyData)
	{
		Volatile.Read(ref PropertyChanged)?.Invoke(this, new PropertyChangedEventArgs(propertyData));
	}

	/// <inheritdoc />
	public IEqualityComparer<T> GetComparer<T>()
	{
		return _valueProvidersManager.GetComparer<T>();
	}

	/// <inheritdoc />
	public void ThrowIfNotSupportedType(Type type)
	{
		_valueProvidersManager.ThrowIfNotSupportedType(type);
	}
}