using System;
using System.Collections.Generic;
using System.Reflection;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;

namespace SeaConf.Core
{
    /// <summary>
    /// Stored property.
    /// </summary>
    public abstract class PropertyData : IPropertyData
    {
        protected PropertyData(string name, Type type, IMemoryModel memoryModel)
        {
            Name = name;
            Type = type;
            Model = memoryModel;
        }

        #region Factory methods

        private static IPropertyData Create(ArgsInfo args)
        {
            var openGenericType = typeof(PropertyData<>);
            var typeArg = new[] { args.PropertyType };
            var genericType = openGenericType.MakeGenericType(typeArg);

            var ctor = genericType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                CallingConventions.Standard | CallingConventions.HasThis,
                (Type[])args.Types,
                null);

            if (ctor == null)
            {
                throw new InvalidOperationException(string.Format(Strings.CtorWithRequiredArgsNotFound, genericType.Name));
            }

            return (IPropertyData)ctor.Invoke((object[])args.Values);
        }

        internal static IPropertyData Create(string name, Type propertyType, IMemoryModel model, IComponents configurationComponents)
        {
            return Create(new ArgsInfo(name, propertyType, model, configurationComponents));
        }

        internal static IPropertyData Create(object value, string name)
        {
            return Create(new ArgsInfo(value, name));
        }

        #endregion

        #region Abstract methods

        protected abstract void OnSet(IPropertyData propertyData);

        protected abstract T OnGet<T>();

        #endregion

        #region Implementation of IPropertyInfo

        /// <summary>
        /// Parent model.
        /// </summary>
        public IMemoryModel Model { get; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type.
        /// </summary>
        public Type Type { get; }

        #endregion

        #region Implementation of IPropertyData

        /// <summary>
        /// Property has been changed.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Getting a typed property.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>Typed property.</returns>
        public IPropertyData<T> ToTyped<T>()
        {
            if (this is not IPropertyData<T> typedValue)
            {
                throw new InvalidCastException(string.Format(Strings.PropertyTypeNotCorrect, Type, typeof(T)));
            }

            return typedValue;
        }

        /// <summary>
        /// Getting a typed property.
        /// </summary>
        /// <param name="type">Value type.</param>
        /// <returns>Typed property.</returns>
        public IPropertyData ToTyped(Type type)
        {
            const string methodName = "ToTyped";

            var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            if (method == null)
            {
                throw new InvalidOperationException(string.Format(Strings.MethodNotFound, methodName));
            }

            return (IPropertyData)method.MakeGenericMethod(type).Invoke(this, null)!;
        }

        /// <summary>
        /// Getting a typed value.
        /// </summary>
        /// <typeparam name="TData">Value type.</typeparam>
        /// <returns>Typed value.</returns>
        public TData Get<TData>()
        {
            return OnGet<TData>();
        }

        /// <summary>
        /// Setting a property value using another stored property.
        /// </summary>
        /// <param name="propertyData">Stored property.</param>
        public void Set(IPropertyData propertyData)
        {
            OnSet(propertyData);
        }

        #endregion

        #region Nested types

        private readonly struct ArgsInfo
        {
            public readonly IReadOnlyList<object> Values;
            public readonly IReadOnlyList<Type> Types;
            public readonly Type PropertyType;

            public ArgsInfo(string name, Type propertyType, IMemoryModel model, IComponents configurationComponents)
            {
                PropertyType = propertyType;
                Values = new object[] { name, propertyType, model, configurationComponents };
                Types = new[] { typeof(string), typeof(Type), typeof(IMemoryModel), typeof(IComponents) };
            }

            internal ArgsInfo(object value, string name)
            {
                PropertyType = value.GetType();
                Values = new object[] { value, name };
                Types = new[] { PropertyType, typeof(string) };
            }
        }

        #endregion
    }

    /// <summary>
    /// Typed stored property.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class PropertyData<T> : PropertyData, IPropertyData<T>
    {
        private readonly IComponents? _components;
        private readonly IEqualityComparer<T>? _comparer;
        private T? _value;

        private PropertyData(T value, string name) : base(name, typeof(T), null!)
        {
            _value = Guard.ThrowIfNull(value);
        }

        // ReSharper disable once UnusedMember.Local
#pragma warning disable IDE0051
        private PropertyData(string name, Type type, IMemoryModel model, IComponents components) : base(name, type, model)
        {
            components.ThrowIfNotSupportedType(type);

            _components = components;
            _comparer = _components.GetComparer<T>();
        }

        #region Factory methods

        public static IPropertyData<T> Create(T value, string name)
        {
            return new PropertyData<T>(value, name);
        }

        #endregion

        #region Overrides of PropertyData

        protected override void OnSet(IPropertyData propertyData)
        {
            _value = propertyData.ToTyped<T>().Get();
        }

        protected override TData OnGet<TData>()
        {
            if (_value is not TData data)
            {
                throw new InvalidCastException(string.Format(Strings.PropertyTypeNotCorrect, Type, typeof(TData)));
            }

            return data;
        }

        #endregion

        #region Implementation of IPropertyData<T>

        /// <summary>
        /// Getting a typed value.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>Typed value.</returns>
        public T Get()
        {
            return _value!;
        }

        /// <summary>
        /// Setting a property value.
        /// </summary>
        /// <param name="value">Value.</param>
        public void Set(T value)
        {
            Guard.ThrowIfNull(value);
            Guard.ThrowIfNull(_comparer);
            Guard.ThrowIfNull(Model);

            if (_comparer!.Equals(_value!, value))
            {
                return;
            }

            IsModified = true;

            _value = value;
            _components!.RaisePropertyChangedEvent(this);
        }

        #endregion

        #region Overrides of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var localValue = _value ?? default;

            return $"Name = {Name}, Type = {Type}, Value = {(localValue != null ? localValue : "null")}";
        }

        #endregion
    }
}