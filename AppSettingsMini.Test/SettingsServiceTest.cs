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

			IProgramSettings expectedResult = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test"
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
				.Setup(x => x.GetStringValueAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(expectedResult.StringValue!);

			// #### Act ####
			await service.LoadAsync();

			// #### Assert ####
			Assert.IsTrue(eventLoadedRaised);

			Assert.That(service.ProgramSettings, Is.EqualTo(expectedResult)
				.Using<IProgramSettings>((lhs, rhs) => lhs.IntValue == rhs.IntValue &&
													   lhs.LongValue == rhs.LongValue &&
													   Math.Abs(lhs.DoubleValue - rhs.DoubleValue) < double.Epsilon &&
													   lhs.StringValue == rhs.StringValue &&
													   lhs.BytesValue.ToArray()
														   .SequenceEqual(rhs.BytesValue.ToArray())));

			readableSourceMock
				.Verify(x => x.PropertyExistsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(5));

			readableSourceMock
				.Verify(x => x.GetBytesValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "BytesValue")),
					Times.Once);

			readableSourceMock
				.Verify(x => x.GetIntValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "IntValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetLongValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "LongValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetDoubleValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "DoubleValue")),
					Times.Once);

			readableSourceMock
				.Verify(x => x.GetStringValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "StringValue")),
					Times.Once);

			readableSourceMock.VerifyNoOtherCalls();
		}

		[Test]
		public async Task SaveAsync_ValidData_Success()
		{
			// #### Arrange ####
			var factory = MockSettingsSourceProviderFactory.CreateForWriteableSource(out var writeableSourceMock);
			var service = new SettingsService(factory);
			var eventSavedRaised = false;
			var expectedChangedProperties = new List<string>
				{ "StringValue", "IntValue", "LongValue", "DoubleValue", "BytesValue" };
			IChangedModels? changedModels = null;

			var expectedResult = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test"
			};

			var values = new List<object>();

			service.Saved += (_, e) =>
			{
				eventSavedRaised = true;
				changedModels = e;
			};

			writeableSourceMock
				.Setup(x => x.SetBytesValueAsync((ReadOnlyMemory<byte>)Capture.In(values), It.IsAny<string>(),
					It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask);

			writeableSourceMock
				.Setup(x => x.SetIntValueAsync((int)Capture.In(values), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask);

			writeableSourceMock
				.Setup(x => x.SetLongValueAsync((long)Capture.In(values), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask);

			writeableSourceMock
				.Setup(x => x.SetDoubleValueAsync((double)Capture.In(values), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask);

			writeableSourceMock
				.Setup(x => x.SetStringValueAsync((string)Capture.In(values), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask);

			// #### Act ####
			service.ProgramSettings.BytesValue = expectedResult.BytesValue;
			service.ProgramSettings.IntValue = expectedResult.IntValue;
			service.ProgramSettings.LongValue = expectedResult.LongValue;
			service.ProgramSettings.DoubleValue = expectedResult.DoubleValue;
			service.ProgramSettings.StringValue = expectedResult.StringValue;

			await service.SaveAsync();

			// #### Assert ####
			Assert.IsTrue(eventSavedRaised);

			Assert.IsTrue(changedModels!.HasChanged);

			var result = changedModels.TryGetProperties<IProgramSettings>(out var actualChangedProperties);

			Assert.IsTrue(result);

			CollectionAssert.AreEqual(expectedChangedProperties, actualChangedProperties);

			Assert.That(values, Is.EqualTo(expectedResult).Using<object>((x, y) =>
			{
				var lhs = ((List<object>)x!).ToDictionary(k => k.GetType());
				var rhs = (MockProgramSettings)y!;

				return Get<ReadOnlyMemory<byte>>(lhs).Span.SequenceEqual(rhs.BytesValue.Span) &&
					   Get<int>(lhs) == rhs.IntValue &&
					   Get<long>(lhs) == rhs.LongValue &&
					   Math.Abs(Get<double>(lhs) - rhs.DoubleValue) < double.Epsilon &&
					   Get<string>(lhs) == rhs.StringValue;

				static T Get<T>(IReadOnlyDictionary<Type, object> map)
				{
					return (T)map[typeof(T)];
				}
			}));

			writeableSourceMock
				.Verify(x =>
					x.SetBytesValueAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()));

			writeableSourceMock
				.Verify(x => x.SetIntValueAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

			writeableSourceMock
				.Verify(x => x.SetLongValueAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()));

			writeableSourceMock
				.Verify(x => x.SetDoubleValueAsync(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()));

			writeableSourceMock
				.Verify(x => x.SetStringValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

			writeableSourceMock.VerifyNoOtherCalls();
		}

		[Test]
		public async Task ChangeProperties_RaisePropertyChangedEvent_Success()
		{
			// #### Arrange ####
			var factory = MockSettingsSourceProviderFactory.CreateForWriteableSource(out _);
			var service = new SettingsService(factory);
			var changeProperties = new List<PropertyChangedEventArgs>();

			var settings = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test"
			};

			service.PropertyChanged += (_, e) => changeProperties.Add(e);

			// #### Act ####
			service.ProgramSettings.BytesValue = settings.BytesValue;
			service.ProgramSettings.LongValue = settings.LongValue;
			service.ProgramSettings.DoubleValue = settings.DoubleValue;
			service.ProgramSettings.StringValue = settings.StringValue;

			// #### Assert ####
			Assert.IsTrue(changeProperties.Count == 4);

			Assert.IsTrue(changeProperties[0].TryGetValue<ReadOnlyMemory<byte>, IProgramSettings>(out var bytesValue, nameof(IProgramSettings.BytesValue)));
			Assert.AreEqual(settings.BytesValue, bytesValue);

			Assert.IsTrue(changeProperties[1].TryGetValue<long, IProgramSettings>(out var longValue, nameof(IProgramSettings.LongValue)));
			Assert.AreEqual(settings.LongValue, longValue);

			Assert.IsTrue(changeProperties[2].TryGetValue<double, IProgramSettings>(out var doubleValue, nameof(IProgramSettings.DoubleValue)));
			Assert.AreEqual(settings.DoubleValue, doubleValue);

			Assert.IsTrue(changeProperties[3].TryGetValue<string, IProgramSettings>(out var stringValue, nameof(IProgramSettings.StringValue)));
			Assert.AreEqual(settings.StringValue, stringValue);
		}
	}
}