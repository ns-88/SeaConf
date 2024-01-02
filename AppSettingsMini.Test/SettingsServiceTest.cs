using System.Net;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Models;
using AppSettingsMini.Test.Infrastructure;
using AppSettingsMini.Test.Infrastructure.Models;
using AutoFixture;
using AutoFixture.Kernel;
using Moq;
using NUnit.Framework;

namespace AppSettingsMini.Test
{
	[TestFixture]
	public class SettingsServiceTest
	{
		private readonly Fixture _fixture;

		public SettingsServiceTest()
		{
			_fixture = new Fixture();

			_fixture.Customizations.Add(new TypeRelay(typeof(IUserSettings), typeof(MockUserSettings)));
			_fixture.Customizations.Add(new TypeRelay(typeof(IAddressSettings), typeof(MockAddressSettings)));
		}

		[Test]
		public void LoadAsync_ValidData_Success()
		{
			// #### Arrange ####
			var factory = MockSourceFactory.CreateFactory(out _, out var readerMocks);
			var service = new SettingsService(factory);
			var eventLoadedRaised = false;
			const string enumValueText = "Auto";

			var expectedResult = _fixture
				.Build<MockProgramSettings>()
				.With(x => x.DateOnlyValue, new DateOnly(2024, 01, 01))
				.With(x => x.TimeOnlyValue, new TimeOnly(09, 10, 30))
				.With(x => x.EnumValue, Regime.Auto)
				.Create();

			service.Loaded += (_, _) => eventLoadedRaised = true;

			#region ProgramSettings

			readerMocks.Program
				.Setup(x => x.PropertyExistsAsync(It.IsAny<string>()))
				.ReturnsAsync(true);

			readerMocks.Program
				.Setup(x => x.ReadBytesAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.BytesValue);

			readerMocks.Program
				.Setup(x => x.ReadIntAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.IntValue);

			readerMocks.Program
				.Setup(x => x.ReadLongAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.LongValue);

			readerMocks.Program
				.Setup(x => x.ReadDecimalAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.DecimalValue);

			readerMocks.Program
				.Setup(x => x.ReadDoubleAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.DoubleValue);

			readerMocks.Program
				.Setup(x => x.ReadStringAsync(nameof(IProgramSettings.StringValue)))
				.ReturnsAsync(expectedResult.StringValue!);

			readerMocks.Program
				.Setup(x => x.ReadStringAsync(nameof(IProgramSettings.EnumValue)))
				.ReturnsAsync(enumValueText);

			readerMocks.Program
				.Setup(x => x.ReadStringAsync(nameof(IProgramSettings.DateTimeValue)))
				.ReturnsAsync(expectedResult.DateTimeValue.ToString);

			readerMocks.Program
				.Setup(x => x.ReadStringAsync(nameof(IProgramSettings.DateOnlyValue)))
				.ReturnsAsync(expectedResult.DateOnlyValue.ToString);

			readerMocks.Program
				.Setup(x => x.ReadStringAsync(nameof(IProgramSettings.TimeOnlyValue)))
				.ReturnsAsync(expectedResult.TimeOnlyValue.ToString);

			readerMocks.Program
				.Setup(x => x.ReadStringAsync(nameof(IProgramSettings.IpEndPointValue)))
				.ReturnsAsync(expectedResult.IpEndPointValue!.ToString);

			readerMocks.Program
				.Setup(x => x.ReadBooleanAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.BoolValue);

			#endregion

			#region ProgramSettings\UserSettings

			readerMocks.User
				.Setup(x => x.PropertyExistsAsync(It.IsAny<string>()))
				.ReturnsAsync(true);

			readerMocks.User
				.Setup(x => x.ReadIntAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.UserSettings.IntValue);

			readerMocks.User
				.Setup(x => x.ReadLongAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.UserSettings.LongValue);

			readerMocks.User
				.Setup(x => x.ReadStringAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.UserSettings.StringValue!);

			#endregion

			#region ProgramSettings\UserSettings\AddressSettings

			readerMocks.Address
				.Setup(x => x.PropertyExistsAsync(It.IsAny<string>()))
				.ReturnsAsync(true);

			readerMocks.Address
				.Setup(x => x.ReadIntAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.UserSettings.AddressSettings.IntValue);

			readerMocks.Address
				.Setup(x => x.ReadLongAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.UserSettings.AddressSettings.LongValue);

			readerMocks.Address
				.Setup(x => x.ReadStringAsync(It.IsAny<string>()))
				.ReturnsAsync(expectedResult.UserSettings.AddressSettings.StringValue!);

			#endregion

			// #### Act/Assert ####
			Assert.DoesNotThrowAsync(async () => await service.LoadAsync());

			Assert.That(eventLoadedRaised);

			Assert.That(service.ProgramSettings, Is.EqualTo(expectedResult).Using(new LoadEqualityComparer()));

			#region ProgramSettings

			readerMocks.Program
				.Verify(x => x.PropertyExistsAsync(It.IsAny<string>()), Times.Exactly(12));

			readerMocks.Program
				.Verify(x => x.ReadBytesAsync(nameof(IProgramSettings.BytesValue)), Times.Once);

			readerMocks.Program
				.Verify(x => x.ReadIntAsync(nameof(IProgramSettings.IntValue)), Times.Once);

			readerMocks.Program
				.Verify(x => x.ReadLongAsync(nameof(IProgramSettings.LongValue)), Times.Once);

			readerMocks.Program
				.Verify(x => x.ReadDoubleAsync(nameof(IProgramSettings.DoubleValue)), Times.Once);

			readerMocks.Program
				.Verify(x => x.ReadDecimalAsync(nameof(IProgramSettings.DecimalValue)), Times.Exactly(1));

			readerMocks.Program
				.Verify(x => x.ReadStringAsync(nameof(IProgramSettings.StringValue)), Times.Exactly(1));

			readerMocks.Program
				.Verify(x => x.ReadStringAsync(nameof(IProgramSettings.DateTimeValue)), Times.Exactly(1));

			readerMocks.Program
				.Verify(x => x.ReadStringAsync(nameof(IProgramSettings.DateOnlyValue)), Times.Exactly(1));

			readerMocks.Program
				.Verify(x => x.ReadStringAsync(nameof(IProgramSettings.TimeOnlyValue)), Times.Exactly(1));

			readerMocks.Program
				.Verify(x => x.ReadStringAsync(nameof(IProgramSettings.IpEndPointValue)), Times.Exactly(1));

			readerMocks.Program
				.Verify(x => x.ReadStringAsync(nameof(IProgramSettings.EnumValue)), Times.Exactly(1));

			readerMocks.Program
				.Verify(x => x.ReadBooleanAsync(nameof(IProgramSettings.BoolValue)), Times.Once);

			readerMocks.Program
				.Verify(x => x.DisposeAsync(), Times.Once);

			readerMocks.Program.VerifyNoOtherCalls();

			#endregion

			#region ProgramSettings\UserSettings

			readerMocks.User
				.Verify(x => x.PropertyExistsAsync(It.IsAny<string>()), Times.Exactly(3));

			readerMocks.User
				.Verify(x => x.ReadIntAsync(It.IsAny<string>()), Times.Once);

			readerMocks.User
				.Verify(x => x.ReadLongAsync(It.IsAny<string>()), Times.Once);

			readerMocks.User
				.Verify(x => x.ReadStringAsync(It.IsAny<string>()), Times.Once);

			readerMocks.User
				.Verify(x => x.DisposeAsync(), Times.Once);

			readerMocks.User.VerifyNoOtherCalls();

			#endregion

			#region ProgramSettings\UserSettings\AddressSettings

			readerMocks.Address
				.Verify(x => x.PropertyExistsAsync(It.IsAny<string>()), Times.Exactly(3));

			readerMocks.Address
				.Verify(x => x.ReadIntAsync(It.IsAny<string>()), Times.Once);

			readerMocks.Address
				.Verify(x => x.ReadLongAsync(It.IsAny<string>()), Times.Once);

			readerMocks.Address
				.Verify(x => x.ReadStringAsync(It.IsAny<string>()), Times.Once);

			readerMocks.Address
				.Verify(x => x.DisposeAsync(), Times.Once);

			readerMocks.Address.VerifyNoOtherCalls();

			#endregion
		}

		[Test]
		public void SaveAsync_ValidData_Success()
		{
			// #### Arrange ####
			var factory = MockSourceFactory.CreateFactory(out var mockWriter, out _);
			var service = new SettingsService(factory);
			var eventSavedRaised = false;
			var methodArgs = new Dictionary<string, Dictionary<string, object>>();

			var expectedChangedProperties = new List<string>
			{
				nameof(IProgramSettings.StringValue),
				nameof(IProgramSettings.IntValue),
				nameof(IProgramSettings.LongValue),
				nameof(IProgramSettings.DoubleValue),
				nameof(IProgramSettings.BoolValue),
				nameof(IProgramSettings.EnumValue),
				nameof(IProgramSettings.BytesValue),
				nameof(IProgramSettings.DecimalValue),
				nameof(IProgramSettings.DateTimeValue),
				nameof(IProgramSettings.DateOnlyValue),
				nameof(IProgramSettings.TimeOnlyValue),
				nameof(IProgramSettings.IpEndPointValue),

				nameof(IUserSettings.IntValue),
				nameof(IUserSettings.LongValue),
				nameof(IUserSettings.StringValue),

				nameof(IAddressSettings.IntValue),
				nameof(IAddressSettings.LongValue),
				nameof(IAddressSettings.StringValue)
			};

			IChangedModels? changedModels = null;

			var expectedResult = _fixture
				.Build<MockProgramSettings>()
				.With(x => x.DateOnlyValue, new DateOnly(2024, 01, 01))
				.With(x => x.TimeOnlyValue, new TimeOnly(09, 10, 30))
				.With(x => x.BoolValue, true)
				.With(x => x.EnumValue, Regime.Auto)
				.Create();

			service.Saved += (_, e) =>
			{
				eventSavedRaised = true;
				changedModels = e;
			};

			#region ProgramSettings

			mockWriter.Program
				.Setup(x => x.WriteBytesAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteDecimalAsync(It.IsAny<decimal>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteDoubleAsync(It.IsAny<double>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.StringValue)))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.EnumValue)))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateTimeValue)))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateOnlyValue)))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.TimeOnlyValue)))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.IpEndPointValue)))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			mockWriter.Program
				.Setup(x => x.WriteBooleanAsync(It.IsAny<bool>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

			#endregion

			#region ProgramSettings\UserSettings

			mockWriter.User
				.Setup(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IUserSettings))));

			mockWriter.User
				.Setup(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IUserSettings))));

			mockWriter.User
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IUserSettings))));

			#endregion

			#region ProgramSettings\UserSettings\AddressSettings

			mockWriter.Address
				.Setup(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IAddressSettings))));

			mockWriter.Address
				.Setup(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IAddressSettings))));

			mockWriter.Address
				.Setup(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(ValueTask.CompletedTask)
				.Callback(new InvocationAction(x => AddArgs(x, nameof(IAddressSettings))));

			#endregion

			// #### Act ####
			service.ProgramSettings.BytesValue = expectedResult.BytesValue;
			service.ProgramSettings.IntValue = expectedResult.IntValue;
			service.ProgramSettings.LongValue = expectedResult.LongValue;
			service.ProgramSettings.DoubleValue = expectedResult.DoubleValue;
			service.ProgramSettings.StringValue = expectedResult.StringValue;
			service.ProgramSettings.BoolValue = expectedResult.BoolValue;
			service.ProgramSettings.EnumValue = expectedResult.EnumValue;
			service.ProgramSettings.DecimalValue = expectedResult.DecimalValue;
			service.ProgramSettings.DateTimeValue = expectedResult.DateTimeValue;
			service.ProgramSettings.DateOnlyValue = expectedResult.DateOnlyValue;
			service.ProgramSettings.TimeOnlyValue = expectedResult.TimeOnlyValue;
			service.ProgramSettings.IpEndPointValue = expectedResult.IpEndPointValue;
			service.ProgramSettings.UserSettings.IntValue = expectedResult.UserSettings.IntValue;
			service.ProgramSettings.UserSettings.LongValue = expectedResult.UserSettings.LongValue;
			service.ProgramSettings.UserSettings.StringValue = expectedResult.UserSettings.StringValue;
			service.ProgramSettings.UserSettings.AddressSettings.IntValue = expectedResult.UserSettings.AddressSettings.IntValue;
			service.ProgramSettings.UserSettings.AddressSettings.LongValue = expectedResult.UserSettings.AddressSettings.LongValue;
			service.ProgramSettings.UserSettings.AddressSettings.StringValue = expectedResult.UserSettings.AddressSettings.StringValue;

			// #### Act/Assert ####
			Assert.DoesNotThrowAsync(async () => await service.SaveAsync());

			Assert.That(eventSavedRaised);
			Assert.That(changedModels!.HasChanged);

			var programSettingsHasChanged = changedModels.TryGetProperties<IProgramSettings>(out var programSettingsActualChangedProperties);
			var userSettingsHasChanged = changedModels.TryGetProperties<IUserSettings>(out var userSettingsActualChangedProperties);
			var addressSettingsHasChanged = changedModels.TryGetProperties<IAddressSettings>(out var addressSettingsActualChangedProperties);

			Assert.That(programSettingsHasChanged);
			Assert.That(userSettingsHasChanged);
			Assert.That(addressSettingsHasChanged);

			var actualChangedProperties = programSettingsActualChangedProperties!
				.Concat(userSettingsActualChangedProperties!)
				.Concat(addressSettingsActualChangedProperties!);

			Assert.That(expectedChangedProperties, Is.EquivalentTo(actualChangedProperties));

			Assert.That(methodArgs, Is.EqualTo(expectedResult).Using(new SaveEqualityComparer()));

			#region ProgramSettings

			mockWriter.Program
				.Verify(x => x.WriteBytesAsync(It.IsAny<ReadOnlyMemory<byte>>(), nameof(IProgramSettings.BytesValue)), Times.Once);

			mockWriter.Program
				.Verify(x => x.WriteIntAsync(It.IsAny<int>(), nameof(IProgramSettings.IntValue)), Times.Once);

			mockWriter.Program
				.Verify(x => x.WriteLongAsync(It.IsAny<long>(), nameof(IProgramSettings.LongValue)), Times.Once);

			mockWriter.Program
				.Verify(x => x.WriteDoubleAsync(It.IsAny<double>(), nameof(IProgramSettings.DoubleValue)), Times.Once);

			mockWriter.Program
				.Verify(x => x.WriteDecimalAsync(It.IsAny<decimal>(), nameof(IProgramSettings.DecimalValue)), Times.Exactly(1));

			mockWriter.Program
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.StringValue)), Times.Exactly(1));

			mockWriter.Program
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateTimeValue)), Times.Exactly(1));

			mockWriter.Program
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateOnlyValue)), Times.Exactly(1));

			mockWriter.Program
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.TimeOnlyValue)), Times.Exactly(1));

			mockWriter.Program
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.IpEndPointValue)), Times.Exactly(1));

			mockWriter.Program
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.EnumValue)), Times.Exactly(1));

			mockWriter.Program
				.Verify(x => x.WriteBooleanAsync(It.IsAny<bool>(), nameof(IProgramSettings.BoolValue)), Times.Once);

			mockWriter.Program
				.Verify(x => x.DisposeAsync(), Times.Once);

			mockWriter.Program.VerifyNoOtherCalls();

			#endregion

			#region ProgramSettings\UserSettings

			mockWriter.User
				.Verify(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);

			mockWriter.User
				.Verify(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()), Times.Once);

			mockWriter.User
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

			mockWriter.User
				.Verify(x => x.DisposeAsync(), Times.Once);

			mockWriter.User.VerifyNoOtherCalls();

			#endregion

			#region ProgramSettings\UserSettings\AddressSettings

			mockWriter.Address
				.Verify(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);

			mockWriter.Address
				.Verify(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()), Times.Once);

			mockWriter.Address
				.Verify(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

			mockWriter.Address
				.Verify(x => x.DisposeAsync(), Times.Once);

			mockWriter.Address.VerifyNoOtherCalls();

			#endregion

			return;

			void AddArgs(IInvocation invocation, string modelName)
			{
				if (methodArgs.TryGetValue(modelName, out var modelArgs))
				{
					modelArgs.Add((string)invocation.Arguments[1], invocation.Arguments[0]);
				}
				else
				{
					modelArgs = new Dictionary<string, object> { { (string)invocation.Arguments[1], invocation.Arguments[0] } };
					methodArgs.Add(modelName, modelArgs);
				}
			}
		}

		[Test]
		public void ChangeProperties_RaisePropertyChangedEvent_Success()
		{
			// #### Arrange ####
			var factory = MockSourceFactory.CreateFactory(out _, out _);
			var service = new SettingsService(factory);
			var changedProperties = new List<PropertyChangedEventArgs>();

			var settings = _fixture
				.Build<MockProgramSettings>()
				.With(x => x.DateOnlyValue, new DateOnly(2024, 01, 01))
				.With(x => x.TimeOnlyValue, new TimeOnly(09, 10, 30))
				.With(x => x.EnumValue, Regime.Auto)
				.Create();

			service.PropertyChanged += (_, e) => changedProperties.Add(e);

			// #### Act ####
			service.ProgramSettings.BytesValue = settings.BytesValue;
			service.ProgramSettings.IntValue = settings.IntValue;
			service.ProgramSettings.LongValue = settings.LongValue;
			service.ProgramSettings.DoubleValue = settings.DoubleValue;
			service.ProgramSettings.StringValue = settings.StringValue;
			service.ProgramSettings.BoolValue = settings.BoolValue;
			service.ProgramSettings.EnumValue = settings.EnumValue;
			service.ProgramSettings.DecimalValue = settings.DecimalValue;
			service.ProgramSettings.DateTimeValue = settings.DateTimeValue;
			service.ProgramSettings.DateOnlyValue = settings.DateOnlyValue;
			service.ProgramSettings.TimeOnlyValue = settings.TimeOnlyValue;
			service.ProgramSettings.IpEndPointValue = settings.IpEndPointValue;
			service.ProgramSettings.UserSettings.IntValue = settings.UserSettings.IntValue;
			service.ProgramSettings.UserSettings.LongValue = settings.UserSettings.LongValue;
			service.ProgramSettings.UserSettings.StringValue = settings.UserSettings.StringValue;
			service.ProgramSettings.UserSettings.AddressSettings.IntValue = settings.UserSettings.AddressSettings.IntValue;
			service.ProgramSettings.UserSettings.AddressSettings.LongValue = settings.UserSettings.AddressSettings.LongValue;
			service.ProgramSettings.UserSettings.AddressSettings.StringValue = settings.UserSettings.AddressSettings.StringValue;

			// #### Assert ####
			Assert.That(changedProperties.Count == 18);

			#region ProgramSettings
			{
				Assert.That(changedProperties[0].TryGetValue<ReadOnlyMemory<byte>, IProgramSettings>(out var bytesValue, nameof(IProgramSettings.BytesValue)));
				Assert.That(settings.BytesValue, Is.EqualTo(bytesValue));

				Assert.That(changedProperties[1].TryGetValue<int, IProgramSettings>(out var intValue, nameof(IProgramSettings.IntValue)));
				Assert.That(settings.IntValue, Is.EqualTo(intValue));

				Assert.That(changedProperties[2].TryGetValue<long, IProgramSettings>(out var longValue, nameof(IProgramSettings.LongValue)));
				Assert.That(settings.LongValue, Is.EqualTo(longValue));

				Assert.That(changedProperties[3].TryGetValue<double, IProgramSettings>(out var doubleValue, nameof(IProgramSettings.DoubleValue)));
				Assert.That(settings.DoubleValue, Is.EqualTo(doubleValue));

				Assert.That(changedProperties[4].TryGetValue<string, IProgramSettings>(out var stringValue, nameof(IProgramSettings.StringValue)));
				Assert.That(settings.StringValue, Is.EqualTo(stringValue));

				Assert.That(changedProperties[5].TryGetValue<bool, IProgramSettings>(out var boolValue, nameof(IProgramSettings.BoolValue)));
				Assert.That(settings.BoolValue, Is.EqualTo(boolValue));

				Assert.That(changedProperties[6].TryGetValue<Regime, IProgramSettings>(out var enumValue, nameof(IProgramSettings.EnumValue)));
				Assert.That(settings.EnumValue, Is.EqualTo(enumValue));

				Assert.That(changedProperties[7].TryGetValue<decimal, IProgramSettings>(out var decimalValue, nameof(IProgramSettings.DecimalValue)));
				Assert.That(settings.DecimalValue, Is.EqualTo(decimalValue));

				Assert.That(changedProperties[8].TryGetValue<DateTime, IProgramSettings>(out var dateTimeValue, nameof(IProgramSettings.DateTimeValue)));
				Assert.That(settings.DateTimeValue, Is.EqualTo(dateTimeValue));

				Assert.That(changedProperties[9].TryGetValue<DateOnly, IProgramSettings>(out var dateOnlyValue, nameof(IProgramSettings.DateOnlyValue)));
				Assert.That(settings.DateOnlyValue, Is.EqualTo(dateOnlyValue));

				Assert.That(changedProperties[10].TryGetValue<TimeOnly, IProgramSettings>(out var timeOnlyValue, nameof(IProgramSettings.TimeOnlyValue)));
				Assert.That(settings.TimeOnlyValue, Is.EqualTo(timeOnlyValue));

				Assert.That(changedProperties[11].TryGetValue<IPEndPoint, IProgramSettings>(out var ipEndPointValue, nameof(IProgramSettings.IpEndPointValue)));
				Assert.That(settings.IpEndPointValue, Is.EqualTo(ipEndPointValue));
			}
			#endregion

			#region ProgramSettings\UserSettings
			{
				Assert.That(changedProperties[12].TryGetValue<int, IUserSettings>(out var intValue, nameof(IUserSettings.IntValue)));
				Assert.That(settings.UserSettings.IntValue, Is.EqualTo(intValue));

				Assert.That(changedProperties[13].TryGetValue<long, IUserSettings>(out var longValue, nameof(IUserSettings.LongValue)));
				Assert.That(settings.UserSettings.LongValue, Is.EqualTo(longValue));

				Assert.That(changedProperties[14].TryGetValue<string, IUserSettings>(out var stringValue, nameof(IUserSettings.StringValue)));
				Assert.That(settings.UserSettings.StringValue, Is.EqualTo(stringValue));
			}
			#endregion

			#region ProgramSettings\UserSettings\AddressSettings
			{
				Assert.That(changedProperties[15].TryGetValue<int, IAddressSettings>(out var intValue, nameof(IAddressSettings.IntValue)));
				Assert.That(settings.UserSettings.AddressSettings.IntValue, Is.EqualTo(intValue));

				Assert.That(changedProperties[16].TryGetValue<long, IAddressSettings>(out var longValue, nameof(IAddressSettings.LongValue)));
				Assert.That(settings.UserSettings.AddressSettings.LongValue, Is.EqualTo(longValue));

				Assert.That(changedProperties[17].TryGetValue<string, IAddressSettings>(out var stringValue, nameof(IAddressSettings.StringValue)));
				Assert.That(settings.UserSettings.AddressSettings.StringValue, Is.EqualTo(stringValue));
			}
			#endregion
		}
	}
}