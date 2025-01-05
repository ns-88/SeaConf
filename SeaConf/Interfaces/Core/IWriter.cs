using System;
using System.Threading.Tasks;

namespace SeaConf.Interfaces.Core
{
    /// <summary>
    /// Configuration writer.
    /// </summary>
    public interface IWriter 
    {
        /// <summary>
        /// Writing a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        ValueTask WriteStringAsync(string propertyValue, IPropertyInfo propertyInfo);

        /// <summary>
        /// Writing a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        ValueTask WriteIntAsync(int propertyValue, IPropertyInfo propertyInfo);

        /// <summary>
        /// Writing a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        ValueTask WriteLongAsync(long propertyValue, IPropertyInfo propertyInfo);

        /// <summary>
        /// Writing a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        ValueTask WriteUlongAsync(ulong propertyValue, IPropertyInfo propertyInfo);

        /// <summary>
        /// Writing a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        ValueTask WriteDoubleAsync(double propertyValue, IPropertyInfo propertyInfo);

        /// <summary>
        /// Writing a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        ValueTask WriteDecimalAsync(decimal propertyValue, IPropertyInfo propertyInfo);

        /// <summary>
        /// Writing a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        ValueTask WriteBooleanAsync(bool propertyValue, IPropertyInfo propertyInfo);

        /// <summary>
        /// Writing a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        ValueTask WriteBytesAsync(ReadOnlyMemory<byte> propertyValue, IPropertyInfo propertyInfo);
    }
}