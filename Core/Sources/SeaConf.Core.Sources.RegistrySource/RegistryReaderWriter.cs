using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.Win32;
using SeaConf.Core.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Sources.RegistrySource;

/// <summary>
/// Configuration reader/writer.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class RegistryReaderWriter : IReader, IWriter
{
	private readonly RegistryKey _collectionKey;
	private readonly IReadOnlySet<string> _valueNames;

	public RegistryReaderWriter(RegistryKey collectionKey)
	{
		_collectionKey = collectionKey;
		_valueNames = collectionKey.GetValueNames().ToHashSet();
	}

	#region Implementation of IReader

	private ValueTask<T> ReadInternal<T>(IPropertyInfo propertyInfo, T defaultValue)
	{
		if (!TryReadInternal(propertyInfo, out var rawValue))
		{
			return ValueTask.FromResult(defaultValue);
		}

		return ValueTask.FromResult(SourceHelper.ThrowIfFailedCastType<T>(rawValue));
	}

	private bool TryReadInternal(IPropertyInfo propertyInfo, [MaybeNullWhen(false)] out object value)
	{
		value = _collectionKey.GetValue(propertyInfo.Name);

		return value != null && (value is not string rawValueText || !string.IsNullOrWhiteSpace(rawValueText));
	}

	private ValueTask<T> ReadValueType<T>(IPropertyInfo propertyInfo, T defaultValue) where T : struct, IParsable<T>
	{
		if (!TryReadInternal(propertyInfo, out var rawValue))
		{
			return ValueTask.FromResult(defaultValue);
		}

		var rawValueText = SourceHelper.ThrowIfFailedCastType<string>(rawValue);

		if (!T.TryParse(rawValueText, null, out var value))
		{
			SourceHelper.ThrowCannotConverted<T>(rawValueText);
		}

		return ValueTask.FromResult(value);
	}

	/// <inheritdoc />
	public ValueTask<string> ReadStringAsync(IPropertyInfo propertyInfo, string defaultValue)
	{
		return ReadInternal(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
	public ValueTask<int> ReadIntAsync(IPropertyInfo propertyInfo, int defaultValue)
	{
		return ReadInternal(propertyInfo, defaultValue);
	}

	/// <inheritdoc />
	public ValueTask<long> ReadLongAsync(IPropertyInfo propertyInfo, long defaultValue)
	{
		return ReadInternal(propertyInfo, defaultValue);
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
	public async ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(IPropertyInfo propertyInfo, ReadOnlyMemory<byte> defaultValue)
	{
		return new ReadOnlyMemory<byte>(await ReadInternal<byte[]>(propertyInfo, []).ConfigureAwait(false));
	}

	/// <inheritdoc />
	public ValueTask<bool> PropertyExistsAsync(IPropertyInfo propertyInfo)
	{
		return ValueTask.FromResult(_valueNames.Contains(propertyInfo.Name));
	}

	#endregion

	#region Implementation of IWriter

	private ValueTask WriteInternal<T>(T propertyValue, string propertyName, RegistryValueKind valueKind) where T : notnull
	{
		_collectionKey.SetValue(propertyName, propertyValue, valueKind);

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public ValueTask WriteStringAsync(string propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
	}

	/// <inheritdoc />
	public ValueTask WriteIntAsync(int propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.DWord);
	}

	/// <inheritdoc />
	public ValueTask WriteLongAsync(long propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.QWord);
	}

	/// <inheritdoc />
	public ValueTask WriteUlongAsync(ulong propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
	}

	/// <inheritdoc />
	public ValueTask WriteDoubleAsync(double propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
	}

	/// <inheritdoc />
	public ValueTask WriteDecimalAsync(decimal propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
	}

	/// <inheritdoc />
	public ValueTask WriteBooleanAsync(bool propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
	}

	/// <inheritdoc />
	public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> propertyValue, IPropertyInfo propertyInfo)
	{
		return WriteInternal(propertyValue.ToArray(), propertyInfo.Name, RegistryValueKind.Binary);
	}

	#endregion
}