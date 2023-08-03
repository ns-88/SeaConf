using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AppSettingsMini.Interfaces
{
	public interface IChangedModels
	{
		bool HasChanged { get; }
		bool TryGetProperties<T>([MaybeNullWhen(false)] out IReadOnlyCollection<string> properties);
		bool CheckProperty<T>(string propertyName);
	}
}