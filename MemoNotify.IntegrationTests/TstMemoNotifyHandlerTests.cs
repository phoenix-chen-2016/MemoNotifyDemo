using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MemoNotify.MessageQueue;
using MemoNotify.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tgs.GameServer;
using Tgs.GameServer.Clients;

namespace MemoNotify.IntegrationTests;

public class TstMemoNotifyHandlerTests
{
	[Fact]
	public async Task 處理排程訊息()
	{
		// Arrange
		var chatId = 12345L;
		var fakeTelegramClient = Substitute.For<ITelegramBotClient>();
		var fakeGameServerClient = Substitute.For<IGameServerClient>();
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Bot:TstChatId"] = chatId.ToString()
			})
			.Build();

		var sut = new TstMemoNotifyHandler(
			fakeTelegramClient,
			fakeGameServerClient,
			configuration,
			NullLogger<TstMemoNotifyHandler>.Instance);

		var scheduleTime = DateTime.Now;
		var payload = new TstMemoNotify(
			"memo title",
			"memo something",
			scheduleTime,
			new[] { "GP1", "GP2" });

		var request = new ScheduleRequest
		{
			Subject = "tgs.test",
			ScheduleTime = Timestamp.FromDateTime(scheduleTime.ToUniversalTime()),
			Payload = JsonSerializer.Serialize(payload)
		};

		var gp1Id = Guid.NewGuid();
		var gp2Id = Guid.NewGuid();
		_ = fakeGameServerClient.ListGameProvidersAsync()
			.Returns(new[]
			{
				new GameProvider(gp1Id, "GP1", null),
				new GameProvider(gp2Id, "GP2", null)
			}.ToAsyncEnumerable());

		// Act
		await sut.HandleAsync(request.ToByteArray());

		// Assert
		_ = fakeGameServerClient.Received(1)
			.SetGameProviderStatusAsync(Arg.Is(gp1Id), Arg.Is(GameStatus.Maintenance));

		_ = fakeGameServerClient.Received(1)
			.SetGameProviderStatusAsync(Arg.Is(gp2Id), Arg.Is(GameStatus.Maintenance));

		_ = fakeTelegramClient.SendTextMessageAsync(
			Arg.Is((ChatId)chatId),
			Arg.Any<string>());
	}
}
