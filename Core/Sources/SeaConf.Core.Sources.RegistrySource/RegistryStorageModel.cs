using Microsoft.Win32;
using SeaConf.Common;
using SeaConf.Common.Infrastructure;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using SeaConf.Core.Common.Properties;
using System.Runtime.Versioning;

namespace SeaConf.Core.Sources.RegistrySource;

/// <summary>
/// Configuration data model in registry Windows.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class RegistryStorageModel : StorageModelBase
{
	private readonly RegistryKey _rootKey;

	private RegistryStorageModel(string name, ModelPath path, RegistryKey rootKey) : base(name, path)
	{
		_rootKey = rootKey;
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

	/// <inheritdoc />
	public override ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
	{
		var nodes = new List<INode>();
		var keyNames = _rootKey.GetSubKeyNames();

		foreach (var keyName in keyNames)
		{
			var subKey = _rootKey.OpenSubKey(keyName, true);

			if (subKey == null)
			{
				throw new InvalidOperationException(string.Format(Resources.FailedGetRegistryKey, keyName));
			}

			var modelName = GetName(subKey);

			nodes.Add(new RegistryStorageModel(modelName, new ModelPath(modelName, Path), subKey));
		}

		return new ValueTask<IReadOnlyList<INode>>(nodes);
	}

	/// <inheritdoc />
	public override ValueTask AddPropertyAsync(IProperty propertyInfo)
	{
		Guard.ThrowIfNull(propertyInfo);

		_rootKey.SetValue(propertyInfo.Name, string.Empty, RegistryValueKind.String);

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public override ValueTask DeletePropertyAsync(IProperty propertyInfo)
	{
		Guard.ThrowIfNull(propertyInfo);

		_rootKey.DeleteValue(propertyInfo.Name, true);

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public override IEnumerable<IProperty> GetProperties()
	{
		var valueNames = _rootKey.GetValueNames();

		foreach (var valueName in valueNames)
		{
			yield return new Property(valueName);
		}
	}

	/// <inheritdoc />
	public override IWriter CreateWriter()
	{
		return new RegistryReaderWriter(_rootKey);
	}

	/// <inheritdoc />
	public override IReader CreateReader()
	{
		return new RegistryReaderWriter(_rootKey);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"Name = {Name}, Key = {_rootKey}";
	}

	/// <inheritdoc />
	public override ValueTask DisposeAsync()
	{
		if (_rootKey != null!)
		{
			_rootKey.Dispose();
		}

		return ValueTask.CompletedTask;
	}
}