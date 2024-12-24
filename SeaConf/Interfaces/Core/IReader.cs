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
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        ValueTask<string> ReadStringAsync(string propertyName);

        /// <summary>
        /// Reading a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        ValueTask<int> ReadIntAsync(string propertyName);

        /// <summary>
        /// Reading a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        ValueTask<long> ReadLongAsync(string propertyName);

        /// <summary>
        /// Reading a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        ValueTask<ulong> ReadUlongAsync(string propertyName);

        /// <summary>
        /// Reading a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        ValueTask<double> ReadDoubleAsync(string propertyName);

        /// <summary>
        /// Reading a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        ValueTask<decimal> ReadDecimalAsync(string propertyName);

        /// <summary>
        /// Reading a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        ValueTask<bool> ReadBooleanAsync(string propertyName);

        /// <summary>
        /// Reading a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(string propertyName);

        /// <summary>
        /// Checking for property existence.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Sign of the presence.</returns>
        ValueTask<bool> PropertyExistsAsync(string propertyName);
    }
}