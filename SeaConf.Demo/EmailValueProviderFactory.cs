using System.Collections;
using SeaConf.Core;
using SeaConf.Demo.Models;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;
using SeaConf.Interfaces.Factories;

namespace SeaConf.Demo
{
    internal class EmailValueProviderFactory : IValueProviderFactory
    {
        /// <summary>
        /// Supported data type.
        /// </summary>
        public Type Type { get; } = typeof(Email);

        /// <summary>
        /// Comparer.
        /// </summary>
        public IEqualityComparer Comparer { get; } = EqualityComparer<Email>.Default;

        /// <summary>
        /// Creating.
        /// </summary>
        /// <returns>Value provider</returns>
        public IValueProvider Create()
        {
            return new EmailValueProvider();
        }

        #region Nested types

        private class EmailValueProvider : IValueProvider
        {
            /// <summary>
            /// Supported data type.
            /// </summary>
            public Type Type { get; } = typeof(Email);

            /// <summary>
            /// Getting value.
            /// </summary>
            /// <param name="reader">Configuration reader.</param>
            /// <param name="propertyInfo">Property info.</param>
            /// <returns>Value.</returns>
            public async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
            {
                var email = new Email(string.Empty);
                
                if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
                {
                    var rawValue = await reader.ReadStringAsync(propertyInfo.Name);
                    email = new Email(rawValue);
                }

                return PropertyData<Email>.Create(email, propertyInfo.Name);
            }

            /// <summary>
            /// Setting value.
            /// </summary>
            /// <param name="writer">Configuration writer.</param>
            /// <param name="propertyData">Stored property.</param>
            public ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
            {
                writer.WriteStringAsync(propertyData.ToTyped<Email>().Get().Value, propertyData.Name);
                return ValueTask.CompletedTask;
            }
        }

        #endregion
    }
}