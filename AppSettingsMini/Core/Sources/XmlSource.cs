using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Models;

namespace AppSettingsMini.Core.Sources
{
	internal class XmlSource : SourceBase<IStorageModel>, IWritableSource<IStorageModel>
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool TryGetElement(XContainer parentElement, string name, [MaybeNullWhen(false)] out XElement childElement)
		{
			childElement = parentElement.Elements().FirstOrDefault(x => x.Name.LocalName.Equals(name, StringComparison.Ordinal));
			return childElement != null;
		}

		public override async ValueTask LoadAsync()
		{
			DisposableHelper.ThrowIfDisposed();

			try
			{
				_document = await XDocument.LoadAsync(_fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
				_rootElement = _document.Element(_rootElementName);

				if (_rootElement == null)
				{
					throw new InvalidOperationException(string.Format(Strings.XmlRootElementNotExist, _rootElementName));
				}
			}
			catch (XmlException ex) when (ex.Message == "Root element is missing.")
			{
				_rootElement = new XElement(_rootElementName);
				_document = new XDocument(_rootElement);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(Strings.FailedLoadSettingsFromSource, ex);
			}

			SetIsLoaded();
		}

		public override async ValueTask SaveAsync()
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
				throw new InvalidOperationException(Strings.FailedSaveSettingsToSource, ex);
			}
		}

		public override ValueTask<IReadOnlyList<INode>> GetRootNodes()
		{
			DisposableHelper.ThrowIfDisposed();
			ThrowIfNotLoaded();

			var rootNodes = new List<INode>();

			foreach (var element in _rootElement!.Elements())
			{
				rootNodes.Add(XmlSettingsNode.FromElement(element));
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

			var found = true;
			var rootElement = _rootElement!;
			var curName = path[0];

			if (!TryGetElement(rootElement, curName, out var newElement))
			{
				found = false;

				newElement = new XElement(curName);
				rootElement.Add(newElement);
			}

			var element = newElement;

			for (var i = 1; i < path.Count; i++)
			{
				curName = path[i];

				if (!TryGetElement(element, curName, out newElement))
				{
					found = false;

					newElement = new XElement(curName);
					element.Add(newElement);
				}

				element = newElement;
			}

			if (found)
			{
				throw new InvalidOperationException(string.Format(Strings.XmlDocumentElementAlreadyExists, path.ToString()));
			}

			return XmlSettingsNode.FromElement(newElement, path);
		}

		public void DeleteModel(ModelPath path)
		{
			DisposableHelper.ThrowIfDisposed();
			ThrowIfNotLoaded();

			if (path.Count == 0)
			{
				throw new ArgumentException(Strings.NotFoundModelPathElements, nameof(path));
			}

			var rootElement = _rootElement!;

			if (!TryGetElement(rootElement, path[0], out var deleteElement))
			{
				throw new InvalidOperationException(string.Format(Strings.XmlDocumentElementNotExist, path[0]));
			}

			for (var i = 1; i < path.Count; i++)
			{
				if (!TryGetElement(deleteElement, path[i], out deleteElement))
				{
					throw new InvalidOperationException(string.Format(Strings.XmlDocumentElementNotExist, path[i]));
				}
			}

			deleteElement.Remove();
		}

		public override async ValueTask DisposeAsync()
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

	#region Infrastructure

	file class XmlSettingsNode : INode, IStorageModel, IPathModel
	{
		private readonly XElement _element;

		public string Name { get; }
		public ModelPath Path { get; }

		private XmlSettingsNode(string name, ModelPath path, XElement element)
		{
			_element = element;

			Name = name;
			Path = path;
		}

		private static string GetName(XElement element)
		{
			return element.Name.LocalName;
		}

		public static XmlSettingsNode FromElement(XElement element)
		{
			var name = GetName(element);
			return new XmlSettingsNode(name, new ModelPath(name), element);
		}

		public static XmlSettingsNode FromElement(XElement element, ModelPath path)
		{
			var name = GetName(element);
			return new XmlSettingsNode(name, path, element);
		}

		public ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
		{
			var nodes = new List<INode>();

			foreach (var element in _element.Elements())
			{
				if (element.HasAttributes)
				{
					continue;
				}

				var name = GetName(element);

				nodes.Add(new XmlSettingsNode(name, new ModelPath(name, Path), element));
			}

			return new ValueTask<IReadOnlyList<INode>>(nodes);
		}

		public IWriter CreateWriter()
		{
			return new XmlReaderWriter(_element);
		}

		public IReader CreateReader()
		{
			return new XmlReaderWriter(_element);
		}

		public override string ToString()
		{
			return $"Name = {Name}, Element = {_element.Name}";
		}

		public ValueTask DisposeAsync()
		{
			return ValueTask.CompletedTask;
		}
	}

	file class XmlReaderWriter : IReader, IWriter
	{
		private const string ValueAttributeName = "value";
		private readonly XElement _collectionElement;
		private DisposableHelper _disposableHelper;

		public XmlReaderWriter(XElement collectionElement)
		{
			_collectionElement = collectionElement;
			_disposableHelper = new DisposableHelper(GetType().Name);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static XElement GetPropertyElement(XContainer collectionElement, string propertyElementName, string valueAttributeName, bool throwIfNull = true)
		{
			var propertyElement = collectionElement.Element(propertyElementName);

			if (throwIfNull && propertyElement == null)
			{
				throw new InvalidOperationException(string.Format(Strings.XmlDocumentElementNotExist, propertyElement));
			}

			if (propertyElement == null)
			{
				propertyElement = new XElement(propertyElementName);
				collectionElement.Add(propertyElement);
			}

			var valueAttribute = propertyElement.FirstAttribute;

			if (valueAttribute == null)
			{
				if (throwIfNull)
				{
					throw new InvalidOperationException(string.Format(Strings.XmlValueAttributeNotFound, propertyElementName));
				}

				valueAttribute = new XAttribute(valueAttributeName, string.Empty);

				propertyElement.Add(valueAttribute);
			}
			else
			{
				if (!valueAttribute.Name.LocalName.Equals(valueAttributeName, StringComparison.Ordinal))
				{
					throw new InvalidOperationException(string.Format(Strings.XmlValueAttributeNotFound, propertyElementName));
				}
			}

			return propertyElement;
		}

		#region Implementation of IReader

		private string ReadInternal(string propertyName)
		{
			Guard.ThrowIfEmptyString(propertyName);

			_disposableHelper.ThrowIfDisposed();

			var propertyElement = GetPropertyElement(_collectionElement, propertyName, ValueAttributeName);

			return propertyElement.FirstAttribute!.Value;
		}

		public ValueTask<string> ReadStringAsync(string propertyName)
		{
			return new ValueTask<string>(ReadInternal(propertyName));
		}

		public ValueTask<int> ReadIntAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);

			if (!int.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<int>(rawValue);
			}

			return new ValueTask<int>(value);
		}

		public ValueTask<long> ReadLongAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);

			if (!long.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<long>(rawValue);
			}

			return new ValueTask<long>(value);
		}

		public ValueTask<double> ReadDoubleAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);

			if (!double.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<double>(rawValue);
			}

			return new ValueTask<double>(value);
		}

		public ValueTask<decimal> ReadDecimalAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);

			if (!decimal.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<decimal>(rawValue);
			}

			return new ValueTask<decimal>(value);
		}

		public ValueTask<bool> ReadBooleanAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);

			if (!bool.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<bool>(rawValue);
			}

			return new ValueTask<bool>(value);
		}

		public ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);
			byte[] byteArray = null!;

			try
			{
				byteArray = Convert.FromBase64String(rawValue);
			}
			catch
			{
				SourceHelper.ThrowCannotConverted<ReadOnlyMemory<byte>>(rawValue);
			}

			var value = new ReadOnlyMemory<byte>(byteArray);

			return new ValueTask<ReadOnlyMemory<byte>>(value);
		}

		#endregion

		#region Implementation of IWriter

		private void WriteInternal(string value, string propertyName)
		{
			Guard.ThrowIfEmptyString(propertyName);

			_disposableHelper.ThrowIfDisposed();

			var propertyElement = GetPropertyElement(_collectionElement, propertyName, ValueAttributeName, false);

			propertyElement.FirstAttribute!.Value = value;
		}

		public ValueTask WriteStringAsync(string value, string propertyName)
		{
			Guard.ThrowIfNull(value);

			WriteInternal(value, propertyName);

			return ValueTask.CompletedTask;
		}

		public ValueTask WriteIntAsync(int value, string propertyName)
		{
			WriteInternal(value.ToString(), propertyName);

			return ValueTask.CompletedTask;
		}

		public ValueTask WriteLongAsync(long value, string propertyName)
		{
			WriteInternal(value.ToString(), propertyName);

			return ValueTask.CompletedTask;
		}

		public ValueTask WriteDoubleAsync(double value, string propertyName)
		{
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName);

			return ValueTask.CompletedTask;
		}

		public ValueTask WriteDecimalAsync(decimal value, string propertyName)
		{
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName);

			return ValueTask.CompletedTask;
		}

		public ValueTask WriteBooleanAsync(bool value, string propertyName)
		{
			WriteInternal(value.ToString(), propertyName);

			return ValueTask.CompletedTask;
		}

		public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> value, string propertyName)
		{
			WriteInternal(Convert.ToBase64String(value.Span), propertyName);

			return ValueTask.CompletedTask;
		}

		public ValueTask<bool> PropertyExistsAsync(string propertyName)
		{
			Guard.ThrowIfEmptyString(propertyName);

			var result = _collectionElement.Element(propertyName) != null;

			return ValueTask.FromResult(result);
		}

		#endregion

		#region Implementation of IDisposable

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

	#endregion
}