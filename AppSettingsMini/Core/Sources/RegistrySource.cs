using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Models;
using Microsoft.Win32;

namespace AppSettingsMini.Core.Sources
{
	[SupportedOSPlatform("windows")]
	internal class RegistrySource : SourceBase<IStorageModel>, IWritableSource<IStorageModel>
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

		public override ValueTask LoadAsync()
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

		public override ValueTask<IReadOnlyList<INode>> GetRootNodes()
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

				rootNodes.Add(RegistryNode.FromKey(subKey));
			}

			return new ValueTask<IReadOnlyList<INode>>(rootNodes);
		}

		public IStorageModel AddModel(ModelPath path)
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

			return RegistryNode.FromKey(key, path);
		}

		public void DeleteModel(ModelPath path)
		{
			DisposableHelper.ThrowIfDisposed();
			ThrowIfNotLoaded();

			if (path.Count == 0)
			{
				throw new ArgumentException(Strings.NotFoundModelPathElements, nameof(path));
			}

			_rootKey!.DeleteSubKey(path.ToString(), true);
		}

		public override ValueTask DisposeAsync()
		{
			if (_rootKey != null && !DisposableHelper.IsDisposed)
			{
				_rootKey.Dispose();
			}

			DisposableHelper.SetIsDisposed();

			return ValueTask.CompletedTask;
		}
	}

	[SupportedOSPlatform("windows")]
	file class RegistryNode : INode, IStorageModel, IPathModel
	{
		private readonly RegistryKey _key;

		public string Name { get; }
		public ModelPath Path { get; }

		private RegistryNode(string name, ModelPath path, RegistryKey key)
		{
			_key = key;

			Name = name;
			Path = path;
		}

		private static string GetName(RegistryKey key)
		{
			var idx = key.Name.LastIndexOf('\\') + 1;
			var name = key.Name.Substring(idx, key.Name.Length - idx);

			return name;
		}

		public static RegistryNode FromKey(RegistryKey key)
		{
			var name = GetName(key);
			return new RegistryNode(name, new ModelPath(name), key);
		}

		public static RegistryNode FromKey(RegistryKey key, ModelPath path)
		{
			var name = GetName(key);
			return new RegistryNode(name, path, key);
		}

		public ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
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

				nodes.Add(new RegistryNode(modelName, new ModelPath(modelName, Path), subKey));
			}

			return new ValueTask<IReadOnlyList<INode>>(nodes);
		}

		public IWriter CreateWriter()
		{
			return new RegistryReaderWriter(_key);
		}

		public IReader CreateReader()
		{
			return new RegistryReaderWriter(_key);
		}

		public override string ToString()
		{
			return $"Name = {Name}, Key = {_key}";
		}

		public ValueTask DisposeAsync()
		{
			if (_key != null!)
			{
				_key.Dispose();
			}

			return ValueTask.CompletedTask;
		}
	}

	[SupportedOSPlatform("windows")]
	file class RegistryReaderWriter : IReader, IWriter
	{
		private readonly RegistryKey _collectionKey;
		private readonly IReadOnlySet<string> _valueNames;
		private DisposableHelper _disposableHelper;

		public RegistryReaderWriter(RegistryKey collectionKey)
		{
			_collectionKey = collectionKey;
			_valueNames = collectionKey.GetValueNames().ToHashSet();
			_disposableHelper = new DisposableHelper(GetType().Name);
		}

		public ValueTask<bool> PropertyExistsAsync(string propertyName)
		{
			return ValueTask.FromResult(_valueNames.Contains(propertyName));
		}

		#region Implementation of IReader

		private T ReadInternal<T>(string propertyName)
		{
			Guard.ThrowIfEmptyString(propertyName);

			_disposableHelper.ThrowIfDisposed();

			var retValue = _collectionKey.GetValue(propertyName);

			if (retValue == null)
			{
				throw new InvalidOperationException(string.Format(Strings.RegistryKeyValueNotSpecified, $"{propertyName}"));
			}

			return SourceHelper.ThrowIfFailedCastType<T>(retValue);
		}

		public ValueTask<string> ReadStringAsync(string propertyName)
		{
			return ValueTask.FromResult(ReadInternal<string>(propertyName));
		}

		public ValueTask<int> ReadIntAsync(string propertyName)
		{
			return ValueTask.FromResult(ReadInternal<int>(propertyName));
		}

		public ValueTask<long> ReadLongAsync(string propertyName)
		{
			return ValueTask.FromResult(ReadInternal<long>(propertyName));
		}

		public ValueTask<double> ReadDoubleAsync(string propertyName)
		{
			var rawValue = ReadInternal<string>(propertyName);

			if (!double.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<double>(rawValue);
			}

			return ValueTask.FromResult(value);
		}

		public ValueTask<decimal> ReadDecimalAsync(string propertyName)
		{
			var rawValue = ReadInternal<string>(propertyName);

			if (!decimal.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<decimal>(rawValue);
			}

			return ValueTask.FromResult(value);
		}

		public ValueTask<bool> ReadBooleanAsync(string propertyName)
		{
			var rawValue = ReadInternal<string>(propertyName);

			if (!bool.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<bool>(rawValue);
			}

			return ValueTask.FromResult(value);
		}

		public ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(string propertyName)
		{
			var value = (ReadOnlyMemory<byte>)ReadInternal<byte[]>(propertyName);
			return ValueTask.FromResult(value);
		}

		#endregion

		#region Implementation of IWriter

		private void WriteInternal<T>(T value, string propertyName, RegistryValueKind valueKind) where T : notnull
		{
			Guard.ThrowIfEmptyString(propertyName);

			_disposableHelper.ThrowIfDisposed();

			_collectionKey.SetValue(propertyName, value, valueKind);
		}

		public ValueTask WriteStringAsync(string value, string propertyName)
		{
			WriteInternal(value, propertyName, RegistryValueKind.String);
			return ValueTask.CompletedTask;
		}

		public ValueTask WriteIntAsync(int value, string propertyName)
		{
			WriteInternal(value, propertyName, RegistryValueKind.DWord);
			return ValueTask.CompletedTask;
		}

		public ValueTask WriteLongAsync(long value, string propertyName)
		{
			WriteInternal(value, propertyName, RegistryValueKind.QWord);
			return ValueTask.CompletedTask;
		}

		public ValueTask WriteDoubleAsync(double value, string propertyName)
		{
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName, RegistryValueKind.String);
			return ValueTask.CompletedTask;
		}

		public ValueTask WriteDecimalAsync(decimal value, string propertyName)
		{
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName, RegistryValueKind.String);
			return ValueTask.CompletedTask;
		}

		public ValueTask WriteBooleanAsync(bool value, string propertyName)
		{
			WriteInternal(value.ToString(), propertyName, RegistryValueKind.String);
			return ValueTask.CompletedTask;
		}

		public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> value, string propertyName)
		{
			WriteInternal(value.ToArray(), propertyName, RegistryValueKind.Binary);
			return ValueTask.CompletedTask;
		}

		#endregion

		#region Implementation of IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			if (!_disposableHelper.IsDisposed)
			{
				_disposableHelper.SetIsDisposed();
			}

			return ValueTask.CompletedTask;
		}

		#endregion
	}
}