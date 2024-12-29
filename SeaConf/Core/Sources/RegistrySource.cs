using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Win32;
using SeaConf.Infrastructure;
using SeaConf.Interfaces.Core;

namespace SeaConf.Core.Sources
{
    /// <summary>
	/// Configuration data source in registry Windows.
	/// </summary>
	[SupportedOSPlatform("windows")]
    internal class RegistrySource : SourceBase<IStorageModel>, IStorageSource
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

        /// <summary>
        /// Loading.
        /// </summary>
        public ValueTask LoadAsync()
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

        /// <summary>
        /// Saving.
        /// </summary>
        public ValueTask SaveAsync()
        {
            return ValueTask.CompletedTask;
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
            var rootKey = _rootKey!;
            var keyNames = rootKey.GetSubKeyNames();

            foreach (var keyName in keyNames)
            {
                var subKey = rootKey.OpenSubKey(keyName, true);

                if (subKey == null)
                {
                    throw new InvalidOperationException(string.Format(Strings.FailedGetRegistryKey, keyName));
                }

                rootNodes.Add(RegistryStorageModel.FromKey(subKey));
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

            var pathText = path.ToString();
            using var existingKey = _rootKey!.OpenSubKey(pathText);

            if (existingKey != null)
            {
                throw new InvalidOperationException(string.Format(Strings.RegistryKeyAlreadyExists, pathText));
            }

            var key = _rootKey!.CreateSubKey(pathText, true);

            return ValueTask.FromResult((IStorageModel)RegistryStorageModel.FromKey(key, path));
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

            _rootKey!.DeleteSubKey(path.ToString(), true);

            return ValueTask.CompletedTask;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
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

    /// <summary>
    /// Configuration data model in registry Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
    file class RegistryStorageModel : StorageModelBase
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

        /// <summary>
        /// Getting child elements.
        /// </summary>
        /// <returns>Child elements.</returns>
        public override ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
        {
            var nodes = new List<INode>();
            var keyNames = _rootKey.GetSubKeyNames();

            foreach (var keyName in keyNames)
            {
                var subKey = _rootKey.OpenSubKey(keyName, true);

                if (subKey == null)
                {
                    throw new InvalidOperationException(string.Format(Strings.FailedGetRegistryKey, keyName));
                }

                var modelName = GetName(subKey);

                nodes.Add(new RegistryStorageModel(modelName, new ModelPath(modelName, Path), subKey));
            }

            return new ValueTask<IReadOnlyList<INode>>(nodes);
        }

        /// <summary>
        /// Adding property.
        /// </summary>
        /// <param name="propertyInfo">Information about the stored property.</param>
        public override ValueTask AddPropertyAsync(IProperty propertyInfo)
        {
            Guard.ThrowIfNull(propertyInfo);

            _rootKey.SetValue(propertyInfo.Name, string.Empty, RegistryValueKind.String);

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Deleting property.
        /// </summary>
        /// <param name="propertyInfo">Information about the stored property.</param>
        public override ValueTask DeletePropertyAsync(IProperty propertyInfo)
        {
            Guard.ThrowIfNull(propertyInfo);

            _rootKey.DeleteValue(propertyInfo.Name, true);

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Getting all properties.
        /// </summary>
        /// <returns>All properties.</returns>
        public override IEnumerable<IProperty> GetProperties()
        {
            var valueNames = _rootKey.GetValueNames();

            foreach (var valueName in valueNames)
            {
                yield return new Property(valueName);
            }
        }

        /// <summary>
        /// Creating a writer.
        /// </summary>
        public override IWriter CreateWriter()
        {
            return new RegistryReaderWriter(_rootKey);
        }

        /// <summary>
        /// Creating a reader.
        /// </summary>
        public override IReader CreateReader()
        {
            return new RegistryReaderWriter(_rootKey);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Name = {Name}, Key = {_rootKey}";
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public override ValueTask DisposeAsync()
        {
            if (_rootKey != null!)
            {
                _rootKey.Dispose();
            }

            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Configuration reader/writer.
    /// </summary>
    [SupportedOSPlatform("windows")]
    file class RegistryReaderWriter : IReader, IWriter
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

        /// <summary>
        /// Reading a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask<string> ReadStringAsync(IPropertyInfo propertyInfo, string defaultValue)
        {
            return ReadInternal(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        public ValueTask<int> ReadIntAsync(IPropertyInfo propertyInfo, int defaultValue)
        {
            return ReadInternal(propertyInfo, defaultValue);
        }

        /// <summary>
        /// Reading a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        public ValueTask<long> ReadLongAsync(IPropertyInfo propertyInfo, long defaultValue)
        {
            return ReadInternal(propertyInfo, defaultValue);
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
        public async ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(IPropertyInfo propertyInfo, ReadOnlyMemory<byte> defaultValue)
        {
            return new ReadOnlyMemory<byte>(await ReadInternal<byte[]>(propertyInfo, []).ConfigureAwait(false));
        }

        /// <summary>
        /// Checking for property existence.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns>Sign of the presence.</returns>
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

        /// <summary>
        /// Writing a value of type <see cref="string"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="string"/>.</returns>
        public ValueTask WriteStringAsync(string propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
        }

        /// <summary>
        /// Writing a value of type <see cref="int"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="int"/>.</returns>
        public ValueTask WriteIntAsync(int propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.DWord);
        }

        /// <summary>
        /// Writing a value of type <see cref="long"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="long"/>.</returns>
        public ValueTask WriteLongAsync(long propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.QWord);
        }

        /// <summary>
        /// Writing a value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="ulong"/>.</returns>
        public ValueTask WriteUlongAsync(ulong propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
        }

        /// <summary>
        /// Writing a value of type <see cref="double"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="double"/>.</returns>
        public ValueTask WriteDoubleAsync(double propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
        }

        /// <summary>
        /// Writing a value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="decimal"/>.</returns>
        public ValueTask WriteDecimalAsync(decimal propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
        }

        /// <summary>
        /// Writing a value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="bool"/>.</returns>
        public ValueTask WriteBooleanAsync(bool propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue, propertyInfo.Name, RegistryValueKind.String);
        }

        /// <summary>
        /// Writing a value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value of type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.</returns>
        public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> propertyValue, IPropertyInfo propertyInfo)
        {
            return WriteInternal(propertyValue.ToArray(), propertyInfo.Name, RegistryValueKind.Binary);
        }

        #endregion
    }
}