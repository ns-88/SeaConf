using System.Collections;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Factories;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Properties;
using SeaConf.Demo.Models;

namespace SeaConf.Demo;

internal class EmailValueProviderFactory : IValueProviderFactory
{
	/// <inheritdoc />
	public Type Type { get; } = typeof(Email);

	/// <inheritdoc />
	public IEqualityComparer Comparer { get; } = EqualityComparer<Email>.Default;

	/// <inheritdoc />
	public IValueProvider Create()
	{
		return new EmailValueProvider();
	}

	#region Nested types

	/// <inheritdoc />
	private class EmailValueProvider : IValueProvider
	{
		/// <inheritdoc />
		public Type Type { get; } = typeof(Email);

		/// <inheritdoc />
		public async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var email = new Email(string.Empty);

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
			{
				var rawValue = await reader.ReadStringAsync(propertyInfo, string.Empty).ConfigureAwait(false);

				if (!string.IsNullOrWhiteSpace(rawValue))
				{
					email = new Email(rawValue);
				}
			}
			
			return PropertyData<Email>.Create(email, propertyInfo.Name);
		}

		/// <inheritdoc />
		public ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteStringAsync(propertyData.ToTyped<Email>().Get().ToString(), propertyData);
		}
	}

	#endregion
}