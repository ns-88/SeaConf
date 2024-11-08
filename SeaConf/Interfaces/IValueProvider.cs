using System;
using System.Threading.Tasks;
using SeaConf.Interfaces.Core;

namespace SeaConf.Interfaces
{
    /// <summary>
    /// Provider for setting and getting data of a specific type.
    /// </summary>
    public interface IValueProvider
    {
        /// <summary>
        /// Supported data type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo);

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
        ValueTask SetAsync(IWriter writer, IPropertyData propertyData);
    }
}