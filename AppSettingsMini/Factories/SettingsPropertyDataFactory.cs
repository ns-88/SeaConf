using System;
using System.Collections.Generic;
using System.Reflection;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Models;

namespace AppSettingsMini.Factories
{
	public static class SettingsPropertyDataFactory
	{
		public static ISettingsPropertyData Create(ArgsInfo args)
		{
			var openGenericType = typeof(SettingsPropertyData<>);
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

			return (ISettingsPropertyData)ctor.Invoke((object[])args.Values);
		}
	}

	public readonly struct ArgsInfo
	{
		public readonly IReadOnlyList<object> Values;
		public readonly IReadOnlyList<Type> Types;
		public readonly Type PropertyType;

		public ArgsInfo(string name, Type propertyType, ISettingsModel model, SettingsServiceBase service)
		{
			PropertyType = propertyType;
			Values = new object[] { name, propertyType, model, service };
			Types = new[] { typeof(string), typeof(Type), typeof(ISettingsModel), typeof(SettingsServiceBase) };
		}

		internal ArgsInfo(object value, string name)
		{
			PropertyType = value.GetType();
			Values = new[] { value, name };
			Types = new[] { PropertyType, typeof(string) };
		}
	}
}