using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using SeaConf.Common.Infrastructure;
using SeaConf.Core.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Sources.XmlSource;

/// <summary>
/// Configuration reader/writer.
/// </summary>
internal sealed class XmlReaderWriter : IReader, IWriter
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

	/// <inheritdoc />
	public ValueTask<string> ReadStringAsync(IPropertyInfo propertyInfo, string defaultValue)
	{
		if (!TryReadInternal(propertyInfo, out var value))
		{
			return ValueTask.FromResult(defaultValue);
		}

		return ValueTask.FromResult(value);
	}

	/// <inheritdoc />
	public ValueTask<int> ReadIntAsync(IPropertyInfo propertyInfo, int defaultValue)
	{
		return ReadValueType(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
	public ValueTask<long> ReadLongAsync(IPropertyInfo propertyInfo, long defaultValue)
	{
		return ReadValueType(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
	public ValueTask<ulong> ReadUlongAsync(IPropertyInfo propertyInfo, ulong defaultValue)
	{
		return ReadValueType(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
	public ValueTask<double> ReadDoubleAsync(IPropertyInfo propertyInfo, double defaultValue)
	{
		return ReadValueType(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
	public ValueTask<decimal> ReadDecimalAsync(IPropertyInfo propertyInfo, decimal defaultValue)
	{
		return ReadValueType(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
	public ValueTask<bool> ReadBooleanAsync(IPropertyInfo propertyInfo, bool defaultValue)
	{
		return ReadValueType(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
	public ValueTask WriteStringAsync(string propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name);
	}

	/// <inheritdoc />
	public ValueTask WriteIntAsync(int propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
	}

	/// <inheritdoc />
	public ValueTask WriteLongAsync(long propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
	}

	/// <inheritdoc />
	public ValueTask WriteUlongAsync(ulong propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
	}

	/// <inheritdoc />
	public ValueTask WriteDoubleAsync(double propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue.ToString(CultureInfo.CurrentCulture), propertyInfo.Name);
	}

	/// <inheritdoc />
	public ValueTask WriteDecimalAsync(decimal propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue.ToString(CultureInfo.CurrentCulture), propertyInfo.Name);
	}

	/// <inheritdoc />
	public ValueTask WriteBooleanAsync(bool propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue.ToString(), propertyInfo.Name);
	}

	/// <inheritdoc />
	public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(Convert.ToBase64String(propertyValue.Span), propertyInfo.Name);
	}

	#endregion
}