using System.Text.Json;
using MemoNotify.Protos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tgs.GameServer.Clients;
using Tgs.MessageQueue;

namespace MemoNotify.MessageQueue;

public class TstMemoNotifyHandler : IMessageHandler
{
	private readonly ITelegramBotClient _telegramBotClient;
	private readonly IGameServerClient _gameServerClient;
	private readonly ILogger<TstMemoNotifyHandler> _logger;
	private readonly ChatId _chatId;

	public TstMemoNotifyHandler(
		ITelegramBotClient telegramBotClient,
		IGameServerClient gameServerClient,
		IConfiguration configuration,
		ILogger<TstMemoNotifyHandler> logger)
	{
		_telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
		_gameServerClient = gameServerClient ?? throw new ArgumentNullException(nameof(gameServerClient));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_chatId = new ChatId(configuration.GetValue<long>("MemoSettings:TstChatId"));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = ScheduleRequest.Parser.ParseFrom(data.Span);

		var memo = JsonSerializer.Deserialize<TstMemoNotify>(request.Payload)!;

		var allGameProviders = await _gameServerClient.ListGameProvidersAsync(cancellationToken)
			.ToDictionaryAsync(
				gp => gp.Code,
				gp => gp.Id,
				StringComparer.OrdinalIgnoreCase,
				cancellationToken)
			.ConfigureAwait(false);

		var executedGameProviders = new List<string>();

		foreach (var gpCode in memo.GameProviderCodes)
			if (allGameProviders.TryGetValue(gpCode, out var id))
				try
				{
					executedGameProviders.Add(gpCode);
					await _gameServerClient.SetGameProviderStatusAsync(
						id,
						Tgs.GameServer.GameStatus.Maintenance,
						cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Set GameProvider Maintain occur error.");
				}

		_ = await _telegramBotClient.SendTextMessageAsync(
			_chatId,
			$"""
				【{memo.Title}】

				{memo.Description}

				GP: {string.Join(", ", memo.GameProviderCodes)}
				Executed: {string.Join(", ", executedGameProviders)}
				  - {memo.ScheduleTime:O} -
				""",
			cancellationToken: cancellationToken).ConfigureAwait(false);
	}
}
