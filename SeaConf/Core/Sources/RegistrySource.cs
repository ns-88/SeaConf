using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Win32;
using SeaConf.Infrastructure;
using SeaConf.Interfaces.Core;

namespace SeaConf.Core.Sources
{
    /// <summary>
	/// Configuration data source in registry Windows.
	/// </summary>
	[SupportedOSPlatform("windows")]
	internal class RegistrySource : SourceBase<IStorageModel>, IStorageSource
    {
		private readonly string _path;
		private readonly string _rootKeyName;
		private RegistryKey? _rootKey;

		public RegistrySource(string companyName, string appName, string rootKeyName)
		{
			Guard.ThrowIfEmptyString(companyName);
			Guard.ThrowIfEmptyString(appName);

			_rootKeyName = Guard.ThrowIfEmptyString(rootKeyName);
			_path = $"Software\\{companyName}\\{appName}\\{rootKeyName}";
		}

        /// <summary>
        /// Loading.
        /// </summary>
        public ValueTask LoadAsync()
		{
			DisposableHelper.ThrowIfDisposed();

			try
			{
				var rootKey = Registry.CurrentUser.OpenSubKey(_path, true) ?? Registry.CurrentUser.CreateSubKey(_path, true);

				if (rootKey == null!)
				{
					throw new InvalidOperationException(string.Format(Strings.RegistryRootKeyNotExist, _rootKeyName));
				}

				_rootKey = rootKey;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Strings.GetRegistryRootKeyFailed, _rootKeyName), ex);
			}

			SetIsLoaded();

			return ValueTask.CompletedTask;
		}

        /// <summary>
        /// Saving.
        /// </summary>
        public ValueTask SaveAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Getting root configuration elements.
        /// </summary>
        /// <returns>Root configuration elements.</returns>
        public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
		{
			DisposableHelper.ThrowIfDisposed();
			ThrowIfNotLoaded();

			var rootNodes = new List<INode>();
			var rootKey = _rootKey!;
			var keyNames = rootKey.GetSubKeyNames();

			foreach (var keyName in keyNames)
			{
				var subKey = rootKey.OpenSubKey(keyName, true);

				if (subKey == null)
				{
					throw new InvalidOperationException(string.Format(Strings.FailedGetRegistryKey, keyName));
				}

				rootNodes.Add(RegistryStorageModel.FromKey(subKey));
			}

			return new ValueTask<IReadOnlyList<INode>>(rootNodes);
		}

        /// <summary>
        /// Adding a data model.
        /// </summary>
        /// <param name="path">Unique path.</param>
        /// <returns>Created data model.</returns>
        public ValueTask<IStorageModel> AddModelAsync(ModelPath path)
		{
			DisposableHelper.ThrowIfDisposed();
			ThrowIfNotLoaded();

			if (path.Count == 0)
			{
				throw new ArgumentException(Strings.NotFoundModelPathElements, nameof(path));
			}

			var pathText = path.ToString();
			using var existingKey = _rootKey!.OpenSubKey(pathText);

			if (existingKey != null)
			{
				throw new InvalidOperationException(string.Format(Strings.RegistryKeyAlreadyExists, pathText));
			}

			var key = _rootKey!.CreateSubKey(pathText, true);

			return ValueTask.FromResult((IStorageModel)RegistryStorageModel.FromKey(key, path));
		}

        /// <summary>
        /// Deleting a data model.
        /// </summary>
        /// <param name="path">Unique path.</param>
        public ValueTask DeleteModelAsync(ModelPath path)
		{
			DisposableHelper.ThrowIfDisposed();
			ThrowIfNotLoaded();

			if (path.Count == 0)
			{
				throw new ArgumentException(Strings.NotFoundModelPathElements, nameof(path));
			}

			_rootKey!.DeleteSubKey(path.ToString(), true);

			return ValueTask.CompletedTask;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
		{
			if (_rootKey != null && !DisposableHelper.IsDisposed)
			{
				_rootKey.Dispose();
			}

			DisposableHelper.SetIsDisposed();

			return ValueTask.CompletedTask;
		}
	}

    /// <summary>
    /// Configuration data model in registry Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
	file class RegistryStorageModel : StorageModelBase
	{
		private readonly RegistryKey _key;

		private RegistryStorageModel(string name, ModelPath path, RegistryKey key) : base(name, path)
		{
			_key = key;
		}

		private static string GetName(RegistryKey key)
		{
			var idx = key.Name.LastIndexOf('\\') + 1;
			var name = key.Name.Substring(idx, key.Name.Length - idx);

			return name;
		}

		public static RegistryStorageModel FromKey(RegistryKey key)
		{
			var name = GetName(key);
			return new RegistryStorageModel(name, new ModelPath(name), key);
		}

		public static RegistryStorageModel FromKey(RegistryKey key, ModelPath path)
		{
			var name = GetName(key);
			return new RegistryStorageModel(name, path, key);
		}

        /// <summary>
        /// Getting child elements.
        /// </summary>
        /// <returns>Child elements.</returns>
        public override ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
		{
			var nodes = new List<INode>();
			var keyNames = _key.GetSubKeyNames();

			foreach (var keyName in keyNames)
			{
				var subKey = _key.OpenSubKey(keyName, true);

				if (subKey == null)
				{
					throw new InvalidOperationException(string.Format(Strings.FailedGetRegistryKey, keyName));
				}

				var modelName = GetName(subKey);

				nodes.Add(new RegistryStorageModel(modelName, new ModelPath(modelName, Path), subKey));
			}

			return new ValueTask<IReadOnlyList<INode>>(nodes);
		}

        /// <summary>
        /// Creating a writer.
        /// </summary>
        public override IWriter CreateWriter()
		{
			return new RegistryReaderWriter(_key);
		}

        /// <summary>
        /// Creating a reader.
        /// </summary>
        public override IReader CreateReader()
		{
			return new RegistryReaderWriter(_key);
		}

		public override string ToString()
		{
			return $"Name = {Name}, Key = {_key}";
		}

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public override ValueTask DisposeAsync()
		{
			if (_key != null!)
			{
				_key.Dispose();
			}

			return ValueTask.CompletedTask;
		}
	}

    /// <summary>
    /// Configuration reader/writer.
    /// </summary>
    [SupportedOSPlatform("windows")]
	file class RegistryReaderWriter : IReader, IWriter
	{
		private readonly RegistryKey _collectionKey;
		private readonly IReadOnlySet<string> _valueNames;

		public RegistryReaderWriter(RegistryKey collectionKey)
		{
			_collectionKey = collectionKey;
			_valueNames = collectionKey.GetValueNames().ToHashSet();
		}

		#region Implementation of IReader

		private T ReadInternal<T>(string propertyName)
		{
			var retValue = _collectionKey.GetValue(Guard.ThrowIfEmptyString(propertyName));

			if (retValue == null)
			{
				throw new InvalidOperationException(string.Format(Strings.RegistryKeyValueNotSpecified, $"{propertyName}"));
			}

			return SourceHelper.ThrowIfFailedCastType<T>(retValue);
		}

        /// <summary>
        /// Reading a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask<string> ReadStringAsync(string propertyName)
		{
			return ValueTask.FromResult(ReadInternal<string>(propertyName));
		}

        /// <summary>
        /// Reading a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        public ValueTask<int> ReadIntAsync(string propertyName)
		{
			return ValueTask.FromResult(ReadInternal<int>(propertyName));
		}

        /// <summary>
        /// Reading a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        public ValueTask<long> ReadLongAsync(string propertyName)
		{
			return ValueTask.FromResult(ReadInternal<long>(propertyName));
		}

        /// <summary>
        /// Reading a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        public ValueTask<ulong> ReadUlongAsync(string propertyName)
        {
            var rawValue = ReadInternal<string>(propertyName);

            if (!ulong.TryParse(rawValue, out var value))
            {
                SourceHelper.ThrowCannotConverted<ulong>(rawValue);
            }

            return ValueTask.FromResult(value);
        }

        /// <summary>
        /// Reading a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        public ValueTask<double> ReadDoubleAsync(string propertyName)
		{
			var rawValue = ReadInternal<string>(propertyName);

			if (!double.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<double>(rawValue);
			}

            return ValueTask.FromResult(value);
        }

        /// <summary>
        /// Reading a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        public ValueTask<decimal> ReadDecimalAsync(string propertyName)
		{
			var rawValue = ReadInternal<string>(propertyName);

			if (!decimal.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<decimal>(rawValue);
			}

            return ValueTask.FromResult(value);
        }

        /// <summary>
        /// Reading a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        public ValueTask<bool> ReadBooleanAsync(string propertyName)
		{
			var rawValue = ReadInternal<string>(propertyName);

			if (!bool.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<bool>(rawValue);
			}

            return ValueTask.FromResult(value);
        }

        /// <summary>
        /// Reading a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        public ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(string propertyName)
		{
			return ValueTask.FromResult((ReadOnlyMemory<byte>)ReadInternal<byte[]>(propertyName));
		}

        /// <summary>
        /// Checking for property existence.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Sign of the presence.</returns>
        public ValueTask<bool> PropertyExistsAsync(string propertyName)
        {
            return ValueTask.FromResult(_valueNames.Contains(propertyName));
        }

        #endregion

        #region Implementation of IWriter

        private void WriteInternal<T>(T value, string propertyName, RegistryValueKind valueKind) where T : notnull
		{
			_collectionKey.SetValue(Guard.ThrowIfEmptyString(propertyName), value, valueKind);
		}

        /// <summary>
        /// Writing a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask WriteStringAsync(string value, string propertyName)
		{
			WriteInternal(value, propertyName, RegistryValueKind.String);
			return ValueTask.CompletedTask;
		}

        /// <summary>
        /// Writing a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        public ValueTask WriteIntAsync(int value, string propertyName)
		{
			WriteInternal(value, propertyName, RegistryValueKind.DWord);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writing a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        public ValueTask WriteLongAsync(long value, string propertyName)
		{
			WriteInternal(value, propertyName, RegistryValueKind.QWord);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writing a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        public ValueTask WriteUlongAsync(ulong value, string propertyName)
        {
            WriteInternal(value.ToString(), propertyName, RegistryValueKind.String);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writing a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        public ValueTask WriteDoubleAsync(double value, string propertyName)
		{
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName, RegistryValueKind.String);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writing a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        public ValueTask WriteDecimalAsync(decimal value, string propertyName)
		{
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName, RegistryValueKind.String);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writing a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        public ValueTask WriteBooleanAsync(bool value, string propertyName)
		{
			WriteInternal(value.ToString(), propertyName, RegistryValueKind.String);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writing a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> value, string propertyName)
		{
			WriteInternal(value.ToArray(), propertyName, RegistryValueKind.Binary);
            return ValueTask.CompletedTask;
        }

		#endregion
	}
}