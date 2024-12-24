using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SeaConf.Interfaces.Core;

namespace SeaConf.Interfaces
{
    /// <summary>
	/// Modified data models.
	/// </summary>
	public interface IChangedModels
	{
        /// <summary>
        /// Configuration has been changed.
        /// </summary>
        bool HasChanged { get; }

        /// <summary>
        /// Attempt to get modified model properties.
        /// </summary>
        /// <typeparam name="T">Model type.</typeparam>
        /// <param name="modelName">Model name.</param>
        /// <param name="properties">Modified properties.</param>
        /// <returns>Sign of success.</returns>
        bool TryGetProperties<T>(string modelName, [MaybeNullWhen(false)] out IReadOnlyCollection<IPropertyData> properties);

        /// <summary>
        /// Checking for property value change.
        /// </summary>
        /// <typeparam name="T">Model type.</typeparam>
        /// <param name="modelName">Model name.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Sign of change.</returns>
        bool CheckProperty<T>(string modelName, string propertyName);
	}
}