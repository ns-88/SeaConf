using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Models;
using Moq;

namespace AppSettingsMini.Test.Infrastructure
{
	public record struct Mocks<T>(Mock<T> Program, Mock<T> User, Mock<T> Address) where T : class;

	internal class MockSourceFactory : ISourceFactory<IStorageModel>
	{
		private readonly ISource<IStorageModel> _source;

		private MockSourceFactory(ISource<IStorageModel> source)
		{
			_source = source;
		}

		public static MockSourceFactory CreateFactory(out Mocks<IWriter> writerMocks, out Mocks<IReader> readerMocks)
		{
			var mockSource = new Mock<ISource<IStorageModel>>();
			var mockRootNodes = new Mock<INode>[] { new() };
			var programSettingsMock = new Mock<IStorageModel>();
			var userSettingsMock = new Mock<IStorageModel>();
			var addressSettingsMock = new Mock<IStorageModel>();
			var mockModels = new[] { programSettingsMock, userSettingsMock, addressSettingsMock };

			writerMocks = new Mocks<IWriter>(new Mock<IWriter>(), new Mock<IWriter>(), new Mock<IWriter>());
			readerMocks = new Mocks<IReader>(new Mock<IReader>(), new Mock<IReader>(), new Mock<IReader>());

			#region ProgramSettings

			programSettingsMock
				.As<IPathModel>()
				.SetupGet(x => x.Path)
				.Returns(new ModelPath("ProgramSettings"));

			mockRootNodes[0]
				.SetupGet(x => x.Name)
				.Returns("ProgramSettings");

			programSettingsMock
				.Setup(x => x.CreateReader())
				.Returns(readerMocks.Program.Object);

			programSettingsMock
				.Setup(x => x.CreateWriter())
				.Returns(writerMocks.Program.Object);

			#endregion

			#region ProgramSettings\UserSettings

			userSettingsMock
				.As<IPathModel>()
				.SetupGet(x => x.Path)
				.Returns(new ModelPath("UserSettings", new ModelPath("ProgramSettings")));

			userSettingsMock
				.SetupGet(x => x.Name)
				.Returns("UserSettings");

			userSettingsMock
				.Setup(x => x.CreateReader())
				.Returns(readerMocks.User.Object);

			userSettingsMock
				.Setup(x => x.CreateWriter())
				.Returns(writerMocks.User.Object);

			#endregion

			#region ProgramSettings\UserSettings\AddressSettings

			addressSettingsMock
				.As<IPathModel>()
				.SetupGet(x => x.Path)
				.Returns(new ModelPath("AddressSettings", new ModelPath("UserSettings", new ModelPath("ProgramSettings"))));

			addressSettingsMock
				.SetupGet(x => x.Name)
				.Returns("AddressSettings");

			addressSettingsMock
				.Setup(x => x.CreateReader())
				.Returns(readerMocks.Address.Object);

			addressSettingsMock
				.Setup(x => x.CreateWriter())
				.Returns(writerMocks.Address.Object);

			#endregion

			mockSource
				.Setup(x => x.GetRootNodes())
				.ReturnsAsync(mockRootNodes.Select(x => x.Object).ToArray);

			mockSource
				.Setup(x => x.GetModelsAsync(It.IsAny<IEnumerable<INode>>()))
				.Returns(mockModels.Select(x => x.Object).ToAsyncEnumerable());

			return new MockSourceFactory(mockSource.Object);
		}

		public ISource<IStorageModel> Create()
		{
			return _source;
		}
	}
}