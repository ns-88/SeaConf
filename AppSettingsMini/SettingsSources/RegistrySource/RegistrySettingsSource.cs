using System;
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
		private RegistryRootKeyFactory _rootKeyFactory;

		public RegistrySettingsSource(string companyName, string appName, string rootKeyName)
		{
			Guard.ThrowIfEmptyString(companyName);
			Guard.ThrowIfEmptyString(appName);

			_rootKeyFactory = new RegistryRootKeyFactory($"Software\\{companyName}\\{appName}\\{rootKeyName}", false);
		}

		private static RegistryKey OpenOrCreateKey(RegistryKey parentKey, string keyName, bool create = true)
		{
			var key = parentKey.OpenSubKey(keyName, true);

			if (key == null && create)
			{
				key = parentKey.CreateSubKey(keyName);
			}

			return key ?? throw new InvalidOperationException(string.Format(Strings.FailedGetRegistryKey, keyName));
		}

		public ValueTask<bool> PropertyExistsAsync(string collectionName, string propertyName)
		{
			using var rootKey = _rootKeyFactory.CreateKey();
			using var collectionKey = rootKey.OpenSubKey(collectionName);

			return new ValueTask<bool>(collectionKey?.GetValue(propertyName) != null);
		}

		#region Implementation of IReadableSettingsSource

		private static T GetValueInternal<T>(string collectionName, string propertyName, ref RegistryRootKeyFactory rootKeyFactory)
		{
			Guard.ThrowIfEmptyString(collectionName);
			Guard.ThrowIfEmptyString(propertyName);

			using var rootKey = rootKeyFactory.CreateKey();
			using var collectionKey = OpenOrCreateKey(rootKey, collectionName);
			var retValue = collectionKey.GetValue(propertyName);

			if (retValue == null)
			{
				throw new InvalidOperationException(string.Format(Strings.RegistryKeyValueNotSpecified, $"{collectionName}\\{propertyName}"));
			}

			return SettingsSourceHelper.ThrowIfFailedCastType<T>(retValue);
		}

		public ValueTask<string> GetStringValueAsync(string collectionName, string propertyName)
		{
			return new ValueTask<string>(GetValueInternal<string>(collectionName, propertyName, ref _rootKeyFactory));
		}

		public ValueTask<int> GetIntValueAsync(string collectionName, string propertyName)
		{
			return new ValueTask<int>(GetValueInternal<int>(collectionName, propertyName, ref _rootKeyFactory));
		}

		public ValueTask<long> GetLongValueAsync(string collectionName, string propertyName)
		{
			return new ValueTask<long>(GetValueInternal<long>(collectionName, propertyName, ref _rootKeyFactory));
		}

		public ValueTask<double> GetDoubleValueAsync(string collectionName, string propertyName)
		{
			var bytes = GetValueInternal<byte[]>(collectionName, propertyName, ref _rootKeyFactory);

			return new ValueTask<double>(BitConverter.ToDouble(bytes, 0));
		}

		public ValueTask<bool> GetBooleanValueAsync(string collectionName, string propertyName)
		{
			var rawValue = GetValueInternal<string>(collectionName, propertyName, ref _rootKeyFactory);

			if (!bool.TryParse(rawValue, out var value))
			{
				SettingsSourceHelper.ThrowIfCannotConverted<bool>(rawValue);
			}

			return new ValueTask<bool>(value);
		}

		public ValueTask<ReadOnlyMemory<byte>> GetBytesValueAsync(string collectionName, string propertyName)
		{
			var value = (ReadOnlyMemory<byte>)GetValueInternal<byte[]>(collectionName, propertyName, ref _rootKeyFactory);

			return new ValueTask<ReadOnlyMemory<byte>>(value);
		}

		#endregion

		#region Implementation of IWriteableSettingsSource

		private static void SetValueInternal<T>(T value,
												string collectionName,
												string propertyName,
												RegistryValueKind registryValueKind,
												ref RegistryRootKeyFactory rootKeyFactory)
			where T : notnull
		{
			Guard.ThrowIfNull(value);
			Guard.ThrowIfEmptyString(collectionName);
			Guard.ThrowIfEmptyString(propertyName);

			using var rootKey = rootKeyFactory.CreateKey();
			using var collectionKey = OpenOrCreateKey(rootKey, collectionName);

			collectionKey.SetValue(propertyName, value, registryValueKind);
		}

		public ValueTask SetStringValueAsync(string value, string collectionName, string propertyName)
		{
			SetValueInternal(value, collectionName, propertyName, RegistryValueKind.String, ref _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetIntValueAsync(int value, string collectionName, string propertyName)
		{
			SetValueInternal(value, collectionName, propertyName, RegistryValueKind.DWord, ref _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetLongValueAsync(long value, string collectionName, string propertyName)
		{
			SetValueInternal(value, collectionName, propertyName, RegistryValueKind.QWord, ref _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetDoubleValueAsync(double value, string collectionName, string propertyName)
		{
			SetValueInternal(BitConverter.GetBytes(value), collectionName, propertyName, RegistryValueKind.Binary, ref _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetBooleanValueAsync(bool value, string collectionName, string propertyName)
		{
			SetValueInternal(value.ToString(), collectionName, propertyName, RegistryValueKind.String, ref _rootKeyFactory);

			return new ValueTask();
		}

		public ValueTask SetBytesValueAsync(ReadOnlyMemory<byte> value, string collectionName, string propertyName)
		{
			SetValueInternal(value.ToArray(), collectionName, propertyName, RegistryValueKind.Binary, ref _rootKeyFactory);

			return new ValueTask();
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
				throw new InvalidOperationException(string.Format(Strings.FailedDeleteRegistryKey, $"{collectionName}\\{propertyName}"), ex);
			}

			return new ValueTask();
		}

		#endregion

		#region Nested types

		private readonly struct RegistryRootKeyFactory
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