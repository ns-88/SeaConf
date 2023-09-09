using AppSettingsMini.Interfaces;
using AppSettingsMini.Models;
using AppSettingsMini.Test.Infrastructure;
using AppSettingsMini.Test.Infrastructure.Models;
using Moq;
using NUnit.Framework;

namespace AppSettingsMini.Test
{
	[TestFixture]
	public class SettingsServiceTest
	{
		[Test]
		public async Task LoadAsync_ValidData_Success()
		{
			// #### Arrange ####
			var factory = MockSettingsSourceProviderFactory.CreateForReadableSource(out var readableSourceMock);
			var service = new SettingsService(factory);
			var eventLoadedRaised = false;
			const string enumValueText = "Auto";

			IProgramSettings expectedResult = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test",
				BoolValue = true,
				EnumValue = Regime.Auto
			};

			service.Loaded += (_, _) => eventLoadedRaised = true;

			readableSourceMock
				.Setup(x => x.PropertyExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(true);

			readableSourceMock
				.Setup(x => x.GetBytesValueAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResult.BytesValue);

			readableSourceMock
				.Setup(x => x.GetIntValueAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResult.IntValue);

			readableSourceMock
				.Setup(x => x.GetLongValueAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResult.LongValue);

			readableSourceMock
				.Setup(x => x.GetDoubleValueAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResult.DoubleValue);

			readableSourceMock
				.SetupSequence(x => x.GetStringValueAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResult.StringValue!)
				.ReturnsAsync(enumValueText);

			readableSourceMock
				.Setup(x => x.GetBooleanValueAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResult.BoolValue);

			// #### Act ####
			await service.LoadAsync();

			// #### Assert ####
			Assert.IsTrue(eventLoadedRaised);

			Assert.That(service.ProgramSettings, Is.EqualTo(expectedResult)
				  .Using<IProgramSettings>((lhs, rhs) =>
					  // IntValue
					  lhs.IntValue == rhs.IntValue &&
					  // LongValue
					  lhs.LongValue == rhs.LongValue &&
					  // DoubleValue
					  Math.Abs(lhs.DoubleValue - rhs.DoubleValue) < double.Epsilon &&
					  // StringValue
					  lhs.StringValue == rhs.StringValue &&
					  // BoolValue
					  lhs.BoolValue == rhs.BoolValue &&
					  // EnumValue
					  lhs.EnumValue == rhs.EnumValue &&
					  // BytesValue
					  lhs.BytesValue.ToArray().SequenceEqual(rhs.BytesValue.ToArray())));

			readableSourceMock
				.Verify(x => x.PropertyExistsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(7));

			readableSourceMock
				.Verify(x => x.GetBytesValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "BytesValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetIntValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "IntValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetLongValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "LongValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetDoubleValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "DoubleValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetStringValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "StringValue")), Times.Exactly(1));

			readableSourceMock
				.Verify(x => x.GetStringValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "EnumValue")), Times.Exactly(1));

			readableSourceMock
				.Verify(x => x.GetBooleanValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "BoolValue")), Times.Once);

			readableSourceMock.VerifyNoOtherCalls();
		}

		[Test]
		public async Task SaveAsync_ValidData_Success()
		{
			// #### Arrange ####
			var factory = MockSettingsSourceProviderFactory.CreateForWriteableSource(out var writeableSourceMock);
			var service = new SettingsService(factory);
			var eventSavedRaised = false;
			var expectedChangedProperties = new List<string> { "StringValue", "IntValue", "LongValue", "DoubleValue", "BoolValue", "EnumValue", "BytesValue" };
			IChangedModels? changedModels = null;

			var expectedResult = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test",
				BoolValue = true,
				EnumValue = Regime.Auto
			};

			var args = new Dictionary<string, object>();

			service.Saved += (_, e) =>
			{
				eventSavedRaised = true;
				changedModels = e;
			};

			writeableSourceMock
				.Setup(x => x.SetBytesValueAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(AddArgs));

			writeableSourceMock
				.Setup(x => x.SetIntValueAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(AddArgs));

			writeableSourceMock
				.Setup(x => x.SetLongValueAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(AddArgs));

			writeableSourceMock
				.Setup(x => x.SetDoubleValueAsync(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(AddArgs));

			writeableSourceMock
				.Setup(x => x.SetStringValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(AddArgs));

			writeableSourceMock
				.Setup(x => x.SetBooleanValueAsync(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(AddArgs));

			// #### Act ####
			service.ProgramSettings.BytesValue = expectedResult.BytesValue;
			service.ProgramSettings.IntValue = expectedResult.IntValue;
			service.ProgramSettings.LongValue = expectedResult.LongValue;
			service.ProgramSettings.DoubleValue = expectedResult.DoubleValue;
			service.ProgramSettings.StringValue = expectedResult.StringValue;
			service.ProgramSettings.BoolValue = expectedResult.BoolValue;
			service.ProgramSettings.EnumValue = expectedResult.EnumValue;

			await service.SaveAsync();

			// #### Assert ####
			Assert.IsTrue(eventSavedRaised);

			Assert.IsTrue(changedModels!.HasChanged);

			var result = changedModels.TryGetProperties<IProgramSettings>(out var actualChangedProperties);

			Assert.IsTrue(result);

			CollectionAssert.AreEqual(expectedChangedProperties, actualChangedProperties);

			Assert.That(args, Is.EqualTo(expectedResult).Using<object>((x, y) =>
			{
				var lhs = ((IReadOnlyDictionary<string, object>)x!);
				var rhs = (MockProgramSettings)y!;

				return
					// BytesValue
					Get<ReadOnlyMemory<byte>>(nameof(rhs.BytesValue)).Span.SequenceEqual(rhs.BytesValue.Span) &&
					// IntValue
					Get<int>(nameof(rhs.IntValue)) == rhs.IntValue &&
					// LongValue
					Get<long>(nameof(rhs.LongValue)) == rhs.LongValue &&
					// DoubleValue
					Math.Abs(Get<double>(nameof(rhs.DoubleValue)) - rhs.DoubleValue) < double.Epsilon &&
					// StringValue
					Get<string>(nameof(rhs.StringValue)) == rhs.StringValue &&
					// BoolValue
					Get<bool>(nameof(rhs.BoolValue)) == rhs.BoolValue &&
					// EnumValue
					Enum.Parse<Regime>(Get<string>(nameof(rhs.EnumValue))) == rhs.EnumValue;

				T Get<T>(string key)
				{
					return (T)lhs[key];
				}
			}));

			writeableSourceMock
				.Verify(x => x.SetBytesValueAsync(
					It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.Is<string>(p => p == nameof(IProgramSettings.BytesValue))), Times.Once);

			writeableSourceMock
				.Verify(x => x.SetIntValueAsync(
					It.IsAny<int>(), It.IsAny<string>(), It.Is<string>(p => p == nameof(IProgramSettings.IntValue))), Times.Once);

			writeableSourceMock
				.Verify(x => x.SetLongValueAsync(
					It.IsAny<long>(), It.IsAny<string>(), It.Is<string>(p => p == nameof(IProgramSettings.LongValue))), Times.Once);

			writeableSourceMock
				.Verify(x => x.SetDoubleValueAsync(
					It.IsAny<double>(), It.IsAny<string>(), It.Is<string>(p => p == nameof(IProgramSettings.DoubleValue))), Times.Once);

			writeableSourceMock
				.Verify(x => x.SetStringValueAsync(
					It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(p => p == nameof(IProgramSettings.StringValue))), Times.Exactly(1));

			writeableSourceMock
				.Verify(x => x.SetStringValueAsync(
					It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(p => p == nameof(IProgramSettings.EnumValue))), Times.Exactly(1));

			writeableSourceMock
				.Verify(x => x.SetBooleanValueAsync(
					It.IsAny<bool>(), It.IsAny<string>(), It.Is<string>(p => p == nameof(IProgramSettings.BoolValue))), Times.Once);

			writeableSourceMock.VerifyNoOtherCalls();

			void AddArgs(IInvocation invocation)
			{
				args.Add((string)invocation.Arguments[2], invocation.Arguments[0]);
			}
		}

		[Test]
		public void ChangeProperties_RaisePropertyChangedEvent_Success()
		{
			// #### Arrange ####
			var factory = MockSettingsSourceProviderFactory.CreateForWriteableSource(out _);
			var service = new SettingsService(factory);
			var changedProperties = new List<PropertyChangedEventArgs>();

			var settings = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test",
				BoolValue = true,
				EnumValue = Regime.Auto
			};

			service.PropertyChanged += (_, e) => changedProperties.Add(e);

			// #### Act ####
			service.ProgramSettings.BytesValue = settings.BytesValue;
			service.ProgramSettings.LongValue = settings.LongValue;
			service.ProgramSettings.DoubleValue = settings.DoubleValue;
			service.ProgramSettings.StringValue = settings.StringValue;
			service.ProgramSettings.BoolValue = settings.BoolValue;
			service.ProgramSettings.EnumValue = settings.EnumValue;

			// #### Assert ####
			Assert.IsTrue(changedProperties.Count == 6);

			Assert.IsTrue(changedProperties[0].TryGetValue<ReadOnlyMemory<byte>, IProgramSettings>(out var bytesValue, nameof(IProgramSettings.BytesValue)));
			Assert.AreEqual(settings.BytesValue, bytesValue);

			Assert.IsTrue(changedProperties[1].TryGetValue<long, IProgramSettings>(out var longValue, nameof(IProgramSettings.LongValue)));
			Assert.AreEqual(settings.LongValue, longValue);

			Assert.IsTrue(changedProperties[2].TryGetValue<double, IProgramSettings>(out var doubleValue, nameof(IProgramSettings.DoubleValue)));
			Assert.AreEqual(settings.DoubleValue, doubleValue);

			Assert.IsTrue(changedProperties[3].TryGetValue<string, IProgramSettings>(out var stringValue, nameof(IProgramSettings.StringValue)));
			Assert.AreEqual(settings.StringValue, stringValue);

			Assert.IsTrue(changedProperties[4].TryGetValue<bool, IProgramSettings>(out var boolValue, nameof(IProgramSettings.BoolValue)));
			Assert.AreEqual(settings.BoolValue, boolValue);

			Assert.IsTrue(changedProperties[5].TryGetValue<Regime, IProgramSettings>(out var enumValue, nameof(IProgramSettings.EnumValue)));
			Assert.AreEqual(settings.EnumValue, enumValue);
		}
	}
}