using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.SettingsSources.XmlSource
{
	internal class XmlSettingsSource : SettingsSourceBase, IReadableSettingsSource, IWriteableSettingsSource
	{
		private const string ValueAttributeName = "value";
		private readonly string _rootElementName;
		private readonly FileStream _fs;
		private XDocument? _document;
		private DisposableHelper _disposableHelper;

		public XmlSettingsSource(string path, string rootElementName)
		{
			_fs = XmlSettingsSourceHelper.CreateFileStream(path);
			_disposableHelper = new DisposableHelper(GetType().Name);
			_rootElementName = rootElementName;
		}

		#region Implementation of ISettingsSource

		public override async ValueTask LoadAsync()
		{
			XmlSettingsSourceHelper.ThrowIfNullOrDispose(_document, ref _disposableHelper, false);

			try
			{
#if NET6_0
				_document = await XDocument.LoadAsync(_fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
#else
				_document = XDocument.Load(_fs);

				await new ValueTask();
#endif

				var rootElement = _document.GetRootElement(_rootElementName, false);

				if (rootElement == null)
				{
					throw new InvalidOperationException(string.Format(Strings.RootCollectionElementNotExist, _rootElementName));
				}
			}
			catch (XmlException ex) when (ex.Message == "Root element is missing.")
			{
				_document = new XDocument(new XElement(_rootElementName));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(Strings.FailedLoadSettingsFromSource, ex);
			}
		}

		public override async ValueTask SaveAsync()
		{
			XmlSettingsSourceHelper.ThrowIfNullOrDispose(_document, ref _disposableHelper);

			try
			{
				_fs.Seek(0, SeekOrigin.Begin);
				_fs.SetLength(0);
#if NET6_0
				await _document!.SaveAsync(_fs, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
#else
				_document!.Save(_fs, SaveOptions.None);

				await new ValueTask();
#endif
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(Strings.FailedSaveSettingsToSource, ex);
			}
		}

		public ValueTask<bool> PropertyExistsAsync(string collectionName, string propertyName)
		{
			Guard.ThrowIfEmptyString(collectionName);
			Guard.ThrowIfEmptyString(propertyName);

			XmlSettingsSourceHelper.ThrowIfNullOrDispose(_document, ref _disposableHelper);

			var result = _document!.GetRootElement(_rootElementName).GetElement(collectionName, false).GetElement(propertyName, false) != null;

			return new ValueTask<bool>(result);
		}

		#endregion

		#region Implementation of IReadableSettingsSource

		private string GetValueInternal(string collectionName, string propertyName)
		{
			Guard.ThrowIfEmptyString(collectionName);
			Guard.ThrowIfEmptyString(propertyName);

			var collectionElement = _document!.GetCollectionElement(_rootElementName, collectionName);
			var propertyElement = collectionElement.GetPropertyElement(propertyName, ValueAttributeName);

			return propertyElement.FirstAttribute!.Value;
		}

		public ValueTask<string> GetStringValueAsync(string collectionName, string propertyName)
		{
			var value = GetValueInternal(collectionName, propertyName);

			return new ValueTask<string>(value);
		}

		public ValueTask<int> GetIntValueAsync(string collectionName, string propertyName)
		{
			var rawValue = GetValueInternal(collectionName, propertyName);

			if (!int.TryParse(rawValue, out var value))
			{
				SettingsSourceHelper.ThrowIfCannotConverted<int>(rawValue);
			}

			return new ValueTask<int>(value);
		}

		public ValueTask<long> GetLongValueAsync(string collectionName, string propertyName)
		{
			var rawValue = GetValueInternal(collectionName, propertyName);

			if (!long.TryParse(rawValue, out var value))
			{
				SettingsSourceHelper.ThrowIfCannotConverted<long>(rawValue);
			}

			return new ValueTask<long>(value);
		}

		public ValueTask<double> GetDoubleValueAsync(string collectionName, string propertyName)
		{
			var rawValue = GetValueInternal(collectionName, propertyName);

			if (!double.TryParse(rawValue, out var value))
			{
				SettingsSourceHelper.ThrowIfCannotConverted<double>(rawValue);
			}

			return new ValueTask<double>(value);
		}

		public ValueTask<bool> GetBooleanValueAsync(string collectionName, string propertyName)
		{
			var rawValue = GetValueInternal(collectionName, propertyName);

			if (!bool.TryParse(rawValue, out var value))
			{
				SettingsSourceHelper.ThrowIfCannotConverted<bool>(rawValue);
			}

			return new ValueTask<bool>(value);
		}

		public ValueTask<ReadOnlyMemory<byte>> GetBytesValueAsync(string collectionName, string propertyName)
		{
			var rawValue = GetValueInternal(collectionName, propertyName);
			byte[] byteArray = null!;

			try
			{
				byteArray = Convert.FromBase64String(rawValue);
			}
			catch
			{
				SettingsSourceHelper.ThrowIfCannotConverted<ReadOnlyMemory<byte>>(rawValue);
			}

			var value = new ReadOnlyMemory<byte>(byteArray);

			return new ValueTask<ReadOnlyMemory<byte>>(value);
		}

		#endregion

		#region Implementation of IWriteableSettingsSource

		private void SetValueInternal(string value, string collectionName, string propertyName)
		{
			Guard.ThrowIfEmptyString(collectionName);
			Guard.ThrowIfEmptyString(propertyName);

			var collectionElement = _document!.GetCollectionElement(_rootElementName, collectionName, false);
			var propertyElement = collectionElement.GetPropertyElement(propertyName, ValueAttributeName, false);

			propertyElement.FirstAttribute!.Value = value;
		}

		public ValueTask SetStringValueAsync(string value, string collectionName, string propertyName)
		{
			Guard.ThrowIfNull(value);

			SetValueInternal(value, collectionName, propertyName);

			return new ValueTask();
		}

		public ValueTask SetIntValueAsync(int value, string collectionName, string propertyName)
		{
			SetValueInternal(value.ToString(), collectionName, propertyName);

			return new ValueTask();
		}

		public ValueTask SetLongValueAsync(long value, string collectionName, string propertyName)
		{
			SetValueInternal(value.ToString(), collectionName, propertyName);

			return new ValueTask();
		}

		public ValueTask SetDoubleValueAsync(double value, string collectionName, string propertyName)
		{
			SetValueInternal(value.ToString(CultureInfo.CurrentCulture), collectionName, propertyName);

			return new ValueTask();
		}

		public ValueTask SetBooleanValueAsync(bool value, string collectionName, string propertyName)
		{
			SetValueInternal(value.ToString(), collectionName, propertyName);

			return new ValueTask();
		}

		public ValueTask SetBytesValueAsync(ReadOnlyMemory<byte> value, string collectionName, string propertyName)
		{
#if NET6_0
			SetValueInternal(Convert.ToBase64String(value.Span), collectionName, propertyName);
#else
			SetValueInternal(Convert.ToBase64String(value.Span.ToArray()), collectionName, propertyName);
#endif
			return new ValueTask();
		}

		public ValueTask DeletePropertyAsync(string collectionName, string propertyName)
		{
			return new ValueTask();
		}

		#endregion

		public override void Dispose()
		{
			if (_disposableHelper.IsDisposed)
			{
				return;
			}

			if (_fs != null!)
			{
				_fs.Dispose();
			}

			_disposableHelper.SetIsDisposed();
		}

		public override async ValueTask DisposeAsync()
		{
			if (_disposableHelper.IsDisposed)
			{
				return;
			}

			if (_fs != null!)
			{
#if NET6_0
				await _fs.DisposeAsync().ConfigureAwait(false);
#else
				_fs.Dispose();

				await new ValueTask();
#endif
			}

			_disposableHelper.SetIsDisposed();
		}
	}

	file static class XmlSettingsSourceHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ThrowIfNullOrDispose(XDocument? document, ref DisposableHelper helper, bool checkNull = true)
		{
			if (checkNull && document == null)
			{
				throw new InvalidOperationException(Strings.XmlDocumentRootElementIsMissing);
			}

			helper.ThrowIfDisposed();
		}

		public static FileStream CreateFileStream(string path)
		{
			return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
		}
	}

	file static class XDocumentExtension
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static XElement? GetElement(this XElement? element, string elementName, bool throwIfNull = true)
		{
			var next = element?.Element(elementName);

			if (throwIfNull && next == null)
			{
				throw new InvalidOperationException(string.Format(Strings.XmlDocumentElementNotExist, elementName));
			}

			return next;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static XElement? GetRootElement(this XDocument document, string elementName, bool throwIfNull = true)
		{
			var element = document.Element(elementName);

			if (element == null && throwIfNull)
			{
				throw new InvalidOperationException(string.Format(Strings.RootCollectionElementNotExist, elementName));
			}

			return element;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static XElement GetCollectionElement(this XDocument document, string rootElementName, string collectionElementName, bool throwIfNull = true)
		{
			var rootElement = document.GetRootElement(rootElementName)!;
			var collectionElement = rootElement.GetElement(collectionElementName, throwIfNull);

			if (collectionElement == null)
			{
				collectionElement = new XElement(collectionElementName);
				rootElement.Add(collectionElement);
			}

			return collectionElement;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static XElement GetPropertyElement(this XElement collectionElement, string propertyElementName, string valueAttributeName, bool throwIfNull = true)
		{
			var propertyElement = collectionElement.GetElement(propertyElementName, throwIfNull);

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
					throw new InvalidOperationException(string.Format(Strings.ValueAttributeNotFound, propertyElementName));
				}

				valueAttribute = new XAttribute(valueAttributeName, string.Empty);
				propertyElement.Add(valueAttribute);
			}
			else
			{
				if (!valueAttribute.Name.LocalName.Equals(valueAttributeName, StringComparison.Ordinal))
				{
					throw new InvalidOperationException(string.Format(Strings.ValueAttributeNotFound, propertyElementName));
				}
			}

			return propertyElement;
		}
	}
}