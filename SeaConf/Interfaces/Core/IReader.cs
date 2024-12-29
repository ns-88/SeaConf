using System;
using System.Threading.Tasks;

namespace SeaConf.Interfaces.Core
{
    /// <summary>
    /// Configuration reader.
    /// </summary>
    public interface IReader
    {
        /// <summary>
        /// Reading a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        ValueTask<string> ReadStringAsync(IPropertyInfo propertyInfo, string defaultValue);

        /// <summary>
        /// Reading a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        ValueTask<int> ReadIntAsync(IPropertyInfo propertyInfo, int defaultValue);

        /// <summary>
        /// Reading a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        ValueTask<long> ReadLongAsync(IPropertyInfo propertyInfo, long defaultValue);

        /// <summary>
        /// Reading a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        ValueTask<ulong> ReadUlongAsync(IPropertyInfo propertyInfo, ulong defaultValue);

        /// <summary>
        /// Reading a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        ValueTask<double> ReadDoubleAsync(IPropertyInfo propertyInfo, double defaultValue);

        /// <summary>
        /// Reading a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        ValueTask<decimal> ReadDecimalAsync(IPropertyInfo propertyInfo, decimal defaultValue);

        /// <summary>
        /// Reading a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        ValueTask<bool> ReadBooleanAsync(IPropertyInfo propertyInfo, bool defaultValue);

        /// <summary>
        /// Reading a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(IPropertyInfo propertyInfo, ReadOnlyMemory<byte> defaultValue);

        /// <summary>
        /// Checking for property existence.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Sign of the presence.</returns>
        ValueTask<bool> PropertyExistsAsync(IPropertyInfo propertyInfo);
    }
}