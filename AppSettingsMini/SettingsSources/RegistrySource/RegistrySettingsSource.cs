using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using Microsoft.Win32;

namespace AppSettingsMini.SettingsSources.RegistrySource
{
#if NET6_0
	[SupportedOSPlatform("windows")]
#endif
	internal class RegistrySettingsSource : SettingsSourceBase, IReadableSettingsSource, IWriteableSettingsSource
	{
		private readonly RegistryRootKeyFactory _rootKeyFactory;

		public RegistrySettingsSource(string appPath, string rootKeyName)
		{
			Guard.ThrowIfEmptyString(appPath);

			_rootKeyFactory = new RegistryRootKeyFactory($"Software\\{appPath}\\{rootKeyName}", false);
		}

		private static RegistryKey OpenOrCreateKey(RegistryKey parentKey, string keyName, bool create = true)
		{
			var key = parentKey.OpenSubKey(keyName, true);

			if (key == null && create)
			{
				key = parentKey.CreateSubKey(keyName);
			}

			return key ?? throw new InvalidOperationException($"Не удалось получить ключ реестра \"{keyName}\".");
		}

		private static void SetValueInternal<T>(T value,
												string collectionName,
												string propertyName,
												RegistryValueKind registryValueKind,
												RegistryRootKeyFactory rootKeyFactory)
			where T : notnull
		{
			Guard.ThrowIfNull(value, nameof(value));
			Guard.ThrowIfEmptyString(collectionName, nameof(collectionName));
			Guard.ThrowIfEmptyString(propertyName, nameof(propertyName));

			using var rootKey = rootKeyFactory.CreateKey();
			using var collectionKey = OpenOrCreateKey(rootKey, collectionName);

			collectionKey.SetValue(propertyName, value, registryValueKind);
		}

		private static T GetValueInternal<T>(string collectionName, string propertyName, RegistryRootKeyFactory rootKeyFactory)
		{
			Guard.ThrowIfEmptyString(collectionName, nameof(collectionName));
			Guard.ThrowIfEmptyString(propertyName, nameof(propertyName));

			using var rootKey = rootKeyFactory.CreateKey();
			using var collectionKey = OpenOrCreateKey(rootKey, collectionName);
			var retValue = collectionKey.GetValue(propertyName);

			return retValue == null
				? throw new InvalidOperationException($"Значение не указано. Ключ реестра: \"{collectionName}\", свойство: \"{propertyName}\".")
				: (T)retValue;
		}

		public ValueTask<bool> CollectionExistsAsync(string collectionName)
		{
			using var rootKey = _rootKeyFactory.CreateKey();

			var result = rootKey.GetSubKeyNames()
								.FirstOrDefault(x => x.Equals(collectionName, StringComparison.Ordinal)) != null;

			return new ValueTask<bool>(result);
		}

		public ValueTask<bool> PropertyExistsAsync(string collectionName, string propertyName)
		{
			using var rootKey = _rootKeyFactory.CreateKey();
			using var collectionKey = rootKey.OpenSubKey(collectionName);

			return new ValueTask<bool>(collectionKey?.GetValue(propertyName) != null);
		}

		#region Get/Set/Delete methods
		public ValueTask SetStringValueAsync(string value, string collectionName, string propertyName)
		{
			SetValueInternal(value, collectionName, propertyName, RegistryValueKind.String, _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetIntValueAsync(int value, string collectionName, string propertyName)
		{
			SetValueInternal(value, collectionName, propertyName, RegistryValueKind.DWord, _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetLongValueAsync(long value, string collectionName, string propertyName)
		{
			SetValueInternal(value, collectionName, propertyName, RegistryValueKind.QWord, _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetDoubleValueAsync(double value, string collectionName, string propertyName)
		{
			SetValueInternal(BitConverter.GetBytes(value), collectionName, propertyName, RegistryValueKind.Binary, _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetBytesValueAsync(ReadOnlyMemory<byte> value, string collectionName, string propertyName)
		{
			SetValueInternal(value.ToArray(), collectionName, propertyName, RegistryValueKind.Binary, _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask<string> GetStringValueAsync(string collectionName, string propertyName)
		{
			return new ValueTask<string>(GetValueInternal<string>(collectionName, propertyName, _rootKeyFactory));
		}

		public ValueTask<int> GetIntValueAsync(string collectionName, string propertyName)
		{
			return new ValueTask<int>(GetValueInternal<int>(collectionName, propertyName, _rootKeyFactory));
		}

		public ValueTask<long> GetLongValueAsync(string collectionName, string propertyName)
		{
			return new ValueTask<long>(GetValueInternal<long>(collectionName, propertyName, _rootKeyFactory));
		}

		public ValueTask<double> GetDoubleValueAsync(string collectionName, string propertyName)
		{
			var bytes = GetValueInternal<byte[]>(collectionName, propertyName, _rootKeyFactory);

			return new ValueTask<double>(BitConverter.ToDouble(bytes, 0));
		}

		public ValueTask<ReadOnlyMemory<byte>> GetBytesValueAsync(string collectionName, string propertyName)
		{
			var value = (ReadOnlyMemory<byte>)GetValueInternal<byte[]>(collectionName, propertyName, _rootKeyFactory);

			return new ValueTask<ReadOnlyMemory<byte>>(value);
		}

		public ValueTask DeletePropertyAsync(string collectionName, string propertyName)
		{
			using var rootKey = _rootKeyFactory.CreateKey();
			using var collectionKey = OpenOrCreateKey(rootKey, collectionName, false);

			try
			{
				collectionKey.DeleteValue(propertyName);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Не удалось удалить значение. Ключ реестра: \"{collectionName}\", свойство: \"{propertyName}\".", ex);
			}

			return new ValueTask();
		}
		#endregion

		#region Nested types

		private class RegistryRootKeyFactory
		{
			private readonly bool _onlyOpen;
			private readonly string _path;

			public RegistryRootKeyFactory(string path, bool onlyOpen)
			{
				Guard.ThrowIfEmptyString(path, nameof(path));

				_path = path;
				_onlyOpen = onlyOpen;
			}

			public RegistryKey CreateKey()
			{
				var rootKey = Registry.CurrentUser.OpenSubKey(_path, true);

				if (rootKey == null && !_onlyOpen)
					rootKey = Registry.CurrentUser.CreateSubKey(_path, true);

				if (rootKey == null)
					throw new InvalidOperationException($"Не удалось получить ветку реестра \"{_path}\".");

				return rootKey;
			}
		}

		#endregion
	}
}