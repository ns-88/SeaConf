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
			var provider = MockSettingsSourceProvider.CreateForReadableSource(out var readableSourceMock);
			var service = new SettingsService(provider);

			IProgramSettings expectedResult = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test"
			};

			readableSourceMock
				.Setup(x => x.PropertyExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(true);

			readableSourceMock
				.Setup(x => x.CollectionExistsAsync(It.IsAny<string>()))
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

			Assert.That(service.ProgramSettings, Is.EqualTo(expectedResult)
				  .Using<IProgramSettings>((lhs, rhs) => lhs.IntValue == rhs.IntValue &&
														 lhs.LongValue == rhs.LongValue &&
														 Math.Abs(lhs.DoubleValue - rhs.DoubleValue) < double.Epsilon &&
														 lhs.StringValue == rhs.StringValue &&
														 lhs.BytesValue.ToArray().SequenceEqual(rhs.BytesValue.ToArray())));

			// #### Assert ####
			readableSourceMock
				.Verify(x => x.GetBytesValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "ProgramSettings.BytesValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetIntValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "ProgramSettings.IntValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetLongValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "ProgramSettings.LongValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetDoubleValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "ProgramSettings.DoubleValue")), Times.Once);

			readableSourceMock
				.Verify(x => x.GetStringValueAsync(It.IsAny<string>(), It.Is<string>(p => p == "ProgramSettings.StringValue")), Times.Once);
		}

		[Test]
		public async Task SaveAsync_ValidData_Success()
		{
			// #### Arrange ####
			var provider = MockSettingsSourceProvider.CreateForWriteableSource(out var writeableSourceMock);
			var service = new SettingsService(provider);

			var expectedResult = new MockProgramSettings
			{
				BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				IntValue = 25,
				LongValue = 701089,
				DoubleValue = 5.357,
				StringValue = "Test test test"
			};

			var values = new List<object>();

			writeableSourceMock
				.Setup(x => x.SetBytesValueAsync((ReadOnlyMemory<byte>)Capture.In(values), It.IsAny<string>(), It.IsAny<string>()))
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
			Assert.That(values, Is.EqualTo(expectedResult).Using<object>((x, y) =>
			{
				var lhs = ((List<object>)x!).ToDictionary(k => k.GetType());
				var rhs = (MockProgramSettings)y!;

				return Get<ReadOnlyMemory<byte>>(lhs).ToArray().SequenceEqual(rhs.BytesValue.ToArray()) &&
				       Get<int>(lhs) == rhs.IntValue &&
				       Get<long>(lhs) == rhs.LongValue &&
				       Math.Abs(Get<double>(lhs) - rhs.DoubleValue) < double.Epsilon &&
				       Get<string>(lhs) == rhs.StringValue;

				static T Get<T>(IReadOnlyDictionary<Type, object> map)
				{
					return (T)map[typeof(T)];
				}
			}));
		}
	}
}