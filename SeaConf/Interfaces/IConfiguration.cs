using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SeaConf.Core;
using SeaConf.Interfaces.Core;
using SeaConf.Models;

namespace SeaConf.Interfaces
{
    /// <summary>
    /// Configuration access tool - reading and writing data, subscribing to events, and receive data models.
    /// </summary>
    public interface IConfiguration
	{
        /// <summary>
        /// Loading event.
        /// </summary>
        event EventHandler Loaded;

        /// <summary>
        /// Saving event.
        /// </summary>
		event EventHandler<SavedEventArgs> Saved;

        /// <summary>
        /// Property change event in data model.
        /// </summary>
		event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Registered data models.
        /// </summary>
		IReadOnlyDictionary<ModelData, IModel> RegisteredModels { get; }

        /// <summary>
        /// Known types.
        /// </summary>
		IReadOnlyDictionary<Type, Type> KnownTypes { get; }

        /// <summary>
        /// Loading.
        /// </summary>
        ValueTask LoadAsync();

        /// <summary>
        /// Saving.
        /// </summary>
		ValueTask SaveAsync();

        /// <summary>
        /// Getting data model.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="name">Name</param>
        /// <returns>Data model.</returns>
        T GetModel<T>(string? name = null) where T : class;
    }
}