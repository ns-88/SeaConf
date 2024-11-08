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
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        ValueTask WriteStringAsync(string value, string propertyName);

        /// <summary>
        /// Writing a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        ValueTask WriteIntAsync(int value, string propertyName);

        /// <summary>
        /// Writing a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        ValueTask WriteLongAsync(long value, string propertyName);

        /// <summary>
        /// Writing a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        ValueTask WriteUlongAsync(ulong value, string propertyName);

        /// <summary>
        /// Writing a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        ValueTask WriteDoubleAsync(double value, string propertyName);

        /// <summary>
        /// Writing a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        ValueTask WriteDecimalAsync(decimal value, string propertyName);

        /// <summary>
        /// Writing a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        ValueTask WriteBooleanAsync(bool value, string propertyName);

        /// <summary>
        /// Writing a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        ValueTask WriteBytesAsync(ReadOnlyMemory<byte> value, string propertyName);
    }
}