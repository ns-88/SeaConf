using SeaConf.Common;
using SeaConf.Core.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using System.Xml;
using System.Xml.Linq;

namespace SeaConf.Core.Sources.XmlSource;

/// <summary>
/// Configuration data source in xml-file.
/// </summary>
internal sealed class XmlSource : SourceBase<IStorageModel>, IStorageSource
{
	private readonly string _rootElementName;
	private readonly FileStream _fs;
	private XDocument? _document;
	private XElement? _rootElement;

	public XmlSource(string path, string rootElementName)
	{
		_fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
		_rootElementName = rootElementName;
	}

	/// <inheritdoc />
	public async ValueTask LoadAsync()
	{
		DisposableHelper.ThrowIfDisposed();

		try
		{
			_document = await XDocument.LoadAsync(_fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
			_rootElement = _document.Element(_rootElementName);

			if (_rootElement == null)
			{
				throw new InvalidOperationException(string.Format(Resources.XmlRootElementNotExist, _rootElementName));
			}
		}
		catch (XmlException ex) when (ex.Message == "Root element is missing.")
		{
			_rootElement = new XElement(_rootElementName);
			_document = new XDocument(_rootElement);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(Resources.FailedLoadSettingsFromSource, ex);
		}

		SetIsLoaded();
	}

	/// <inheritdoc />
	public async ValueTask SaveAsync()
	{
		DisposableHelper.ThrowIfDisposed();
		ThrowIfNotLoaded();

		try
		{
			_fs.Seek(0, SeekOrigin.Begin);
			_fs.SetLength(0);
                
			await _document!.SaveAsync(_fs, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(Resources.FailedSaveSettingsToSource, ex);
		}
	}

	/// <inheritdoc />
	public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
	{
		DisposableHelper.ThrowIfDisposed();
		ThrowIfNotLoaded();

		var rootNodes = new List<INode>();

		foreach (var element in _rootElement!.Elements())
		{
			rootNodes.Add(XmlStorageModel.FromElement(element));
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

		var found = true;
		var rootElement = _rootElement!;
		var curName = path[0];

		if (!Helper.TryGetElement(rootElement, curName, out var newElement))
		{
			found = false;

			newElement = new XElement(curName);
			rootElement.Add(newElement);
		}

		var element = newElement;

		for (var i = 1; i < path.Count; i++)
		{
			curName = path[i];

			if (!Helper.TryGetElement(element, curName, out newElement))
			{
				found = false;

				newElement = new XElement(curName);
				element.Add(newElement);
			}

			element = newElement;
		}

		if (found)
		{
			throw new InvalidOperationException(string.Format(Resources.XmlDocumentElementAlreadyExists, path.ToString()));
		}

		return ValueTask.FromResult((IStorageModel)XmlStorageModel.FromElement(newElement, path));
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

		var rootElement = _rootElement!;

		if (!Helper.TryGetElement(rootElement, path[0], out var deleteElement))
		{
			throw new InvalidOperationException(string.Format(Resources.XmlDocumentElementNotExist, path[0]));
		}

		for (var i = 1; i < path.Count; i++)
		{
			if (!Helper.TryGetElement(deleteElement, path[i], out deleteElement))
			{
				throw new InvalidOperationException(string.Format(Resources.XmlDocumentElementNotExist, path[i]));
			}
		}

		deleteElement.Remove();

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (DisposableHelper.IsDisposed)
		{
			return;
		}

		if (_fs != null!)
		{
			await _fs.DisposeAsync().ConfigureAwait(false);
		}

		DisposableHelper.SetIsDisposed();
	}
}