using System;
using System.Collections.Generic;
using SeaConf.Core;
using SeaConf.Interfaces.Core;

namespace SeaConf.Interfaces
{
    /// <summary>
    /// Configuration components.
    /// </summary>
    public interface IComponents
    {
        /// <summary>
        /// Registered data models.
        /// </summary>
        IReadOnlyDictionary<ModelData, IModel> RegisteredModels { get; }

        /// <summary>
        /// Known types.
        /// </summary>
        IReadOnlyDictionary<Type, Type> KnownTypes { get; }

        /// <summary>
        /// Raise configuration load event.
        /// </summary>
        void RaiseLoadedEvent();

        /// <summary>
        /// Raise configuration saving event.
        /// </summary>
        /// <param name="changedModels">Modified data models.</param>
        void RaiseSavedEvent(IChangedModels changedModels);

        /// <summary>
        /// Raise roperty change event in data model.
        /// </summary>
        void RaisePropertyChangedEvent(IPropertyData propertyData);

        /// <summary>
        /// Get comparer for comparing supported data types.
        /// </summary>
        /// <typeparam name="T">Supported data type.</typeparam>
        /// <returns>Comparer.</returns>
        IEqualityComparer<T> GetComparer<T>();

        /// <summary>
        /// Throw exception if type is not supported.
        /// </summary>
        /// <param name="type">Type.</param>
        void ThrowIfNotSupportedType(Type type);
    }
}