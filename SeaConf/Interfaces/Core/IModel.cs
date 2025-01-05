using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SeaConf.Core;
using SeaConf.Models;

namespace SeaConf.Interfaces.Core
{
    /// <summary>
    /// Initialized configuration data model in memory.
    /// </summary>
    internal interface IMemoryInitializedModel
    {
        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="memoryModel">Data model in memory.</param>
        /// <param name="propertiesData">Properties data.</param>
        void Initialize(IMemoryModel memoryModel, IReadOnlyDictionary<string, IPropertyData> propertiesData);
    }

    /// <summary>
    /// Configuration data model.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Path.
        /// </summary>
        ModelPath Path { get; }
    }

    /// <summary>
    /// Configuration data model in storage.
    /// </summary>
    public interface IStorageModel : IModel, IAsyncDisposable
    {
        /// <summary>
        /// Loading.
        /// </summary>
        ValueTask LoadAsync();

        /// <summary>
        /// Saving.
        /// </summary>
        ValueTask SaveAsync();

        /// <summary>
        /// Adding property.
        /// </summary>
        /// <param name="property">Information about the stored property.</param>
        ValueTask AddPropertyAsync(IProperty property);

        /// <summary>
        /// Deleting property.
        /// </summary>
        /// <param name="property">Information about the stored property.</param>
        ValueTask DeletePropertyAsync(IProperty property);

        /// <summary>
        /// Getting all properties.
        /// </summary>
        /// <returns>All properties.</returns>
        IEnumerable<IProperty> GetProperties();

        /// <summary>
        /// Creating a writer.
        /// </summary>
        IWriter CreateWriter();

        /// <summary>
        /// Creating a reader.
        /// </summary>
        IReader CreateReader();
    }

    /// <summary>
    /// Configuration data model in memory.
    /// </summary>
    public interface IMemoryModel : IModel
    {
        /// <summary>
        /// Type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Initialization sign.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Number of elements.
        /// </summary>
        ElementsCount ElementsCount { get; }

        /// <summary>
        /// Getting all properties.
        /// </summary>
        /// <returns>All properties.</returns>
        IEnumerable<IPropertyData> GetProperties();

        /// <summary>
        /// Getting modified properties.
        /// </summary>
        /// <returns>Modified properties.</returns>
        IEnumerable<IPropertyData> GetModifiedProperties();

        internal static string GetName(MemberInfo memberInfo)
        {
            var typeName = memberInfo.Name;

            if (typeName.Length == 1)
            {
                return typeName;
            }

            if (typeName.StartsWith('I'))
            {
                typeName = typeName.Substring(1, typeName.Length - 1);
            }

            return typeName;
        }

        internal static string GetName(MemberInfo memberInfo, ModelAttribute attribute)
        {
            return string.IsNullOrWhiteSpace(attribute.Name)
                ? GetName(memberInfo)
                : attribute.Name;
        }
    }
}