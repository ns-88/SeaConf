using System;
using System.Collections.Generic;
using System.Reflection;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Models;

namespace AppSettingsMini.Core.Factories
{
    internal static class PropertyDataFactory
    {
        public static IPropertyData Create(ArgsInfo args)
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
    }

    internal readonly struct ArgsInfo
    {
        public readonly IReadOnlyList<object> Values;
        public readonly IReadOnlyList<Type> Types;
        public readonly Type PropertyType;

        public ArgsInfo(string name, Type propertyType, IMemoryModel model, SettingsServiceBase service)
        {
            PropertyType = propertyType;
            Values = new object[] { name, propertyType, model, service };
            Types = new[] { typeof(string), typeof(Type), typeof(IMemoryModel), typeof(SettingsServiceBase) };
        }

        internal ArgsInfo(object value, string name)
        {
            PropertyType = value.GetType();
            Values = new[] { value, name };
            Types = new[] { PropertyType, typeof(string) };
        }
    }
}