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
using SeaConf.Infrastructure;
using SeaConf.Interfaces.Core;

namespace SeaConf.Core.Sources
{
    /// <summary>
    /// Configuration data source in xml-file.
    /// </summary>
    internal class XmlSource : SourceBase<IStorageModel>, IStorageSource
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

        /// <summary>
        /// Loading.
        /// </summary>
        public async ValueTask LoadAsync()
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

        /// <summary>
        /// Saving.
        /// </summary>
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
				throw new InvalidOperationException(Strings.FailedSaveSettingsToSource, ex);
			}
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

			foreach (var element in _rootElement!.Elements())
			{
				rootNodes.Add(XmlStorageModel.FromElement(element));
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

			return ValueTask.FromResult((IStorageModel)XmlStorageModel.FromElement(newElement, path));
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

			return ValueTask.CompletedTask;
		}

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
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

    /// <summary>
    /// Configuration data model in xml-file.
    /// </summary>
    file class XmlStorageModel : StorageModelBase
	{
		private readonly XElement _element;

		private XmlStorageModel(string name, ModelPath path, XElement element) : base(name, path)
        {
			_element = element;
		}

		private static string GetName(XElement element)
		{
			return element.Name.LocalName;
		}

		public static XmlStorageModel FromElement(XElement element)
		{
			var name = GetName(element);
			return new XmlStorageModel(name, new ModelPath(name), element);
		}

		public static XmlStorageModel FromElement(XElement element, ModelPath path)
		{
			var name = GetName(element);
			return new XmlStorageModel(name, path, element);
		}

        /// <summary>
        /// Getting child elements.
        /// </summary>
        /// <returns>Child elements.</returns>
        public override ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
		{
			var nodes = new List<INode>();

			foreach (var element in _element.Elements())
			{
				if (element.HasAttributes)
				{
					continue;
				}

				var name = GetName(element);

				nodes.Add(new XmlStorageModel(name, new ModelPath(name, Path), element));
			}

			return new ValueTask<IReadOnlyList<INode>>(nodes);
		}

        /// <summary>
        /// Creating a writer.
        /// </summary>
        public override IWriter CreateWriter()
		{
			return new XmlReaderWriter(_element);
		}

        /// <summary>
        /// Creating a reader.
        /// </summary>
        public override IReader CreateReader()
		{
			return new XmlReaderWriter(_element);
		}

		public override string ToString()
		{
			return $"Name = {Name}, Element = {_element.Name}";
		}
	}

    /// <summary>
    /// Configuration reader/writer.
    /// </summary>
    file class XmlReaderWriter : IReader, IWriter
	{
		private const string ValueAttributeName = "value";
		private readonly XElement _collectionElement;

		public XmlReaderWriter(XElement collectionElement)
		{
			_collectionElement = collectionElement;
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

        /// <summary>
        /// Checking for property existence.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Sign of the presence.</returns>
        public ValueTask<bool> PropertyExistsAsync(string propertyName)
        { 
            return ValueTask.FromResult(_collectionElement.Element(Guard.ThrowIfEmptyString(propertyName)) != null);
        }

        #region Implementation of IReader

        private string ReadInternal(string propertyName)
		{
			Guard.ThrowIfEmptyString(propertyName);

			var propertyElement = GetPropertyElement(_collectionElement, propertyName, ValueAttributeName);

			return propertyElement.FirstAttribute!.Value;
		}

        /// <summary>
        /// Reading a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask<string> ReadStringAsync(string propertyName)
		{
			return ValueTask.FromResult(ReadInternal(propertyName));
		}

        /// <summary>
        /// Reading a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        public ValueTask<int> ReadIntAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);

			if (!int.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<int>(rawValue);
			}

			return ValueTask.FromResult(value);
		}

        /// <summary>
        /// Reading a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        public ValueTask<long> ReadLongAsync(string propertyName)
		{
			var rawValue = ReadInternal(propertyName);

			if (!long.TryParse(rawValue, out var value))
			{
				SourceHelper.ThrowCannotConverted<long>(rawValue);
			}

			return ValueTask.FromResult(value);
		}

        /// <summary>
        /// Reading a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        public ValueTask<ulong> ReadUlongAsync(string propertyName)
        {
            var rawValue = ReadInternal(propertyName);

            if (!ulong.TryParse(rawValue, out var value))
            {
                SourceHelper.ThrowCannotConverted<long>(rawValue);
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
			var rawValue = ReadInternal(propertyName);

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
			var rawValue = ReadInternal(propertyName);

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
			var rawValue = ReadInternal(propertyName);

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

			return ValueTask.FromResult(new ReadOnlyMemory<byte>(byteArray));
		}

		#endregion

		#region Implementation of IWriter

		private void WriteInternal(string value, string propertyName)
		{
            Guard.ThrowIfNull(value);
            Guard.ThrowIfEmptyString(propertyName);

			var propertyElement = GetPropertyElement(_collectionElement, propertyName, ValueAttributeName, false);

			propertyElement.FirstAttribute!.Value = value;
		}

        /// <summary>
        /// Writing a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="value">Property value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask WriteStringAsync(string value, string propertyName)
		{
			WriteInternal(value, propertyName);
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
			WriteInternal(value.ToString(), propertyName);
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
			WriteInternal(value.ToString(), propertyName);
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
            WriteInternal(value.ToString(), propertyName);
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
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName);
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
			WriteInternal(value.ToString(CultureInfo.CurrentCulture), propertyName);
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
			WriteInternal(value.ToString(), propertyName);
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
			WriteInternal(Convert.ToBase64String(value.Span), propertyName);
            return ValueTask.CompletedTask;
        }

		#endregion
	}
}