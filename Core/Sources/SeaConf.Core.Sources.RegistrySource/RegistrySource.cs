using Microsoft.Win32;
using SeaConf.Common;
using SeaConf.Core.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using System.Runtime.Versioning;
using SeaConf.Common.Infrastructure;

namespace SeaConf.Core.Sources.RegistrySource;

/// <summary>
/// Configuration data source in registry Windows.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class RegistrySource : SourceBase<IStorageModel>, IStorageSource
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

	/// <inheritdoc />
	public ValueTask LoadAsync()
	{
		DisposableHelper.ThrowIfDisposed();

		try
		{
			var rootKey = Registry.CurrentUser.OpenSubKey(_path, true) ?? Registry.CurrentUser.CreateSubKey(_path, true);

			if (rootKey == null!)
			{
				throw new InvalidOperationException(string.Format(Resources.RegistryRootKeyNotExist, _rootKeyName));
			}

			_rootKey = rootKey;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(string.Format(Resources.GetRegistryRootKeyFailed, _rootKeyName), ex);
		}

		SetIsLoaded();

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public ValueTask SaveAsync()
	{
		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
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
				throw new InvalidOperationException(string.Format(Resources.FailedGetRegistryKey, keyName));
			}

			rootNodes.Add(RegistryStorageModel.FromKey(subKey));
		}

		return new ValueTask<IReadOnlyList<INode>>(rootNodes);
	}

	/// <inheritdoc />
	public ValueTask<IStorageModel> AddModelAsync(ModelPath path)
	{
		DisposableHelper.ThrowIfDisposed();
		ThrowIfNotLoaded();

		if (path.Count == 0)
		{
			throw new ArgumentException(Resources.NotFoundModelPathElements, nameof(path));
		}

		var pathText = path.ToString();
		using var existingKey = _rootKey!.OpenSubKey(pathText);

		if (existingKey != null)
		{
			throw new InvalidOperationException(string.Format(Resources.RegistryKeyAlreadyExists, pathText));
		}

		var key = _rootKey!.CreateSubKey(pathText, true);

		return ValueTask.FromResult((IStorageModel)RegistryStorageModel.FromKey(key, path));
	}

	/// <inheritdoc />
	public ValueTask DeleteModelAsync(ModelPath path)
	{
		DisposableHelper.ThrowIfDisposed();
		ThrowIfNotLoaded();

		if (path.Count == 0)
		{
			throw new ArgumentException(Resources.NotFoundModelPathElements, nameof(path));
		}

		_rootKey!.DeleteSubKey(path.ToString(), true);

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
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