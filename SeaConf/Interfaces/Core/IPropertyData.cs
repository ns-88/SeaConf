using System;

namespace SeaConf.Interfaces.Core
{
    /// <summary>
	/// Information about the stored property.
	/// </summary>
	public interface IPropertyInfo
    {
        /// <summary>
        /// Parent model.
        /// </summary>
        IMemoryModel Model { get; }

        /// <summary>
        /// Name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Type.
        /// </summary>
        Type Type { get; }
    }

    /// <summary>
    /// Stored property.
    /// </summary>
    public interface IPropertyData : IPropertyInfo
    {
        /// <summary>
        /// Property has been changed.
        /// </summary>
        bool IsModified { get; }

        /// <summary>
        /// Getting a typed property.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>Typed property.</returns>
        IPropertyData<T> ToTyped<T>();

        /// <summary>
        /// Getting a typed property.
        /// </summary>
        /// <param name="type">Value type.</param>
        /// <returns>Typed property.</returns>
        IPropertyData ToTyped(Type type);

        /// <summary>
        /// Getting a typed value.
        /// </summary>
        /// <typeparam name="TData">Value type.</typeparam>
        /// <returns>Typed value.</returns>
        TData Get<TData>();

        /// <summary>
        /// Setting a property value using another stored property.
        /// </summary>
        /// <param name="propertyData">Stored property.</param>
        void Set(IPropertyData propertyData);
    }

    /// <summary>
    /// Typed stored property.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IPropertyData<T> : IPropertyData
    {
        /// <summary>
        /// Getting a typed value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>Typed value.</returns>
        T Get();

        /// <summary>
        /// Setting a property value.
        /// </summary>
        /// <param name="value">Value.</param>
        void Set(T value);
    }
}