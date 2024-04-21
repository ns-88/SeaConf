using System;
using System.Collections.Generic;
using System.Reflection;
using AppSettingsMini.Models;

namespace AppSettingsMini.Interfaces.Core
{
	internal interface IMemoryInitializedModel
	{
		internal void Initialize(IModelInfo modelInfo, SettingsServiceBase service);
	}

	internal interface IPathModel : IModel
	{
		ModelPath Path { get; }
	}

	internal interface IModelInfo : IModel
	{
		IMemoryModel Model { get; }
		Type Type { get; }
		IEnumerable<IModelInfo> InnerModels { get; }
		bool IsRoot { get; }

		static string GetName(MemberInfo memberInfo)
		{
			var typeName = memberInfo.Name;

			if (typeName.Length == 1)
			{
				return typeName;
			}

			if (typeName.StartsWith('I'))
			{
				typeName = typeName.Substring(1, typeName.Length - 1);
			}

			return typeName;
		}

		static string GetName(MemberInfo memberInfo, ModelAttribute attribute)
		{
			return string.IsNullOrWhiteSpace(attribute.Name)
				? GetName(memberInfo)
				: attribute.Name;
		}
	}

	public interface IModel
	{
		string Name { get; }
	}

	public interface IStorageModel : IModel, IAsyncDisposable
	{
		IWriter CreateWriter();
		IReader CreateReader();
	}

	public interface IMemoryModel : IModel
	{
		Type Type { get; }
		internal IEnumerable<IPropertyData> GetModifiedProperties();
		internal IReadOnlyCollection<IPropertyData> GetPropertiesData();
	}
}