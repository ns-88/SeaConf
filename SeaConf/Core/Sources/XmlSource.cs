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

            if (!Helper.TryGetElement(rootElement, path[0], out var deleteElement))
            {
                throw new InvalidOperationException(string.Format(Strings.XmlDocumentElementNotExist, path[0]));
            }

            for (var i = 1; i < path.Count; i++)
            {
                if (!Helper.TryGetElement(deleteElement, path[i], out deleteElement))
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
        /// Adding property.
        /// </summary>
        /// <param name="property">Information about the stored property.</param>
        public override ValueTask AddPropertyAsync(IProperty property)
        {
            Guard.ThrowIfNull(property);

            Helper.CreateOrGetPropertyElement(_element, property.Name, Helper.ValueAttributeName, false);

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Deleting property.
        /// </summary>
        /// <param name="property">Information about the stored property.</param>
        public override ValueTask DeletePropertyAsync(IProperty property)
        {
            Guard.ThrowIfNull(property);

            var element = Helper.CreateOrGetPropertyElement(_element, property.Name, Helper.ValueAttributeName);

            element.Remove();

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Getting all properties.
        /// </summary>
        /// <returns>All properties.</returns>
        public override IEnumerable<IProperty> GetProperties()
        {
            foreach (var element in _element.Elements())
            {
                var attributeName = element.FirstAttribute?.Name.LocalName;

                if (attributeName?.Equals(Helper.ValueAttributeName, StringComparison.Ordinal) == true)
                {
                    yield return new Property(element.Name.LocalName);
                }
            }
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

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
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
        private readonly XElement _collectionElement;

        public XmlReaderWriter(XElement collectionElement)
        {
            _collectionElement = collectionElement;
        }

        #region Implementation of IReader

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadInternal(IPropertyInfo propertyInfo)
        {
            Guard.ThrowIfNull(propertyInfo);

            var propertyElement = Helper.CreateOrGetPropertyElement(_collectionElement, propertyInfo.Name, Helper.ValueAttributeName);

            return propertyElement.FirstAttribute!.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryReadInternal(IPropertyInfo propertyInfo, out string value)
        {
            value = ReadInternal(propertyInfo);

            return !string.IsNullOrWhiteSpace(value);
        }

        private ValueTask<T> ReadValueType<T>(IPropertyInfo propertyInfo, T defaultValue) where T : struct, IParsable<T>
        {
            if (!TryReadInternal(propertyInfo, out var rawValue))
            {
                return ValueTask.FromResult(defaultValue);
            }

            if (!T.TryParse(rawValue, null, out var value))
            {
                SourceHelper.ThrowCannotConverted<T>(rawValue);
            }

            return ValueTask.FromResult(value);
        }

        /// <summary>
        /// Reading a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask<string> ReadStringAsync(IPropertyInfo propertyInfo, string defaultValue)
        {
            if (!TryReadInternal(propertyInfo, out var value))
            {
                return ValueTask.FromResult(defaultValue);
            }

            return ValueTask.FromResult(value);
        }

        /// <summary>
        /// Reading a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        public ValueTask<int> ReadIntAsync(IPropertyInfo propertyInfo, int defaultValue)
        {
            return ReadValueType(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        public ValueTask<long> ReadLongAsync(IPropertyInfo propertyInfo, long defaultValue)
        {
            return ReadValueType(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        public ValueTask<ulong> ReadUlongAsync(IPropertyInfo propertyInfo, ulong defaultValue)
        {
            return ReadValueType(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        public ValueTask<double> ReadDoubleAsync(IPropertyInfo propertyInfo, double defaultValue)
        {
            return ReadValueType(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        public ValueTask<decimal> ReadDecimalAsync(IPropertyInfo propertyInfo, decimal defaultValue)
        {
            return ReadValueType(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        public ValueTask<bool> ReadBooleanAsync(IPropertyInfo propertyInfo, bool defaultValue)
        {
            return ReadValueType(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        public ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(IPropertyInfo propertyInfo, ReadOnlyMemory<byte> defaultValue)
        {
            byte[] byteArray = null!;

            if (!TryReadInternal(propertyInfo, out var rawValue))
            {
                return ValueTask.FromResult(defaultValue);
            }

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

        /// <summary>
        /// Checking for property existence.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Sign of the presence.</returns>
        public ValueTask<bool> PropertyExistsAsync(IPropertyInfo propertyInfo)
        {
            return ValueTask.FromResult(_collectionElement.Element(Guard.ThrowIfEmptyString(propertyInfo.Name)) != null);
        }

        #endregion

        #region Implementation of IWriter

        private ValueTask WriteInternal(string value, string propertyName)
        {
            Guard.ThrowIfNull(value);
            Guard.ThrowIfEmptyString(propertyName);

            var propertyElement = Helper.CreateOrGetPropertyElement(_collectionElement, propertyName, Helper.ValueAttributeName, false);

            propertyElement.FirstAttribute!.Value = value;

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Writing a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask WriteStringAsync(string propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name);
        }

        /// <summary>
        /// Writing a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        public ValueTask WriteIntAsync(int propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
        }

        /// <summary>
        /// Writing a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        public ValueTask WriteLongAsync(long propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
        }

        /// <summary>
        /// Writing a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        public ValueTask WriteUlongAsync(ulong propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
        }

        /// <summary>
        /// Writing a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        public ValueTask WriteDoubleAsync(double propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue.ToString(CultureInfo.CurrentCulture), propertyInfo.Name);
        }

        /// <summary>
        /// Writing a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        public ValueTask WriteDecimalAsync(decimal propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue.ToString(CultureInfo.CurrentCulture), propertyInfo.Name);
        }

        /// <summary>
        /// Writing a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        public ValueTask WriteBooleanAsync(bool propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
        }

        /// <summary>
        /// Writing a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(Convert.ToBase64String(propertyValue.Span), propertyInfo.Name);
        }

        #endregion
    }

    file static class Helper
    {
        public const string ValueAttributeName = "value";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetElement(XContainer parentElement, string name, [MaybeNullWhen(false)] out XElement childElement)
        {
            childElement = parentElement.Elements().FirstOrDefault(x => x.Name.LocalName.Equals(name, StringComparison.Ordinal));
            return childElement != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XElement CreateOrGetPropertyElement(XContainer collectionElement, string propertyElementName, string valueAttributeName, bool throwIfNull = true)
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
    }
}