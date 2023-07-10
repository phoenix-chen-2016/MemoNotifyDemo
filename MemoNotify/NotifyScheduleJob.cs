using System.ComponentModel;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MemoNotify.Protos;
using Tgs.MessageQueue;

namespace MemoNotify;

public class NotifyScheduleJob
{
	private readonly IMessageSender _messageSender;

	public NotifyScheduleJob(IMessageSender messageSender)
	{
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
	}

	[DisplayName("{0}")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:移除未使用的參數", Justification = "<暫止>")]
	public async Task ExecuteAsync<TPayload>(
		string jobTitle,
		string subject,
		DateTime scheduleTime,
		TPayload payload,
		CancellationToken cancellationToken = default)
	{
		var request = new ScheduleRequest
		{
			Subject = subject,
			ScheduleTime = Timestamp.FromDateTime(scheduleTime),
			Payload = JsonSerializer.Serialize(payload)
		};

		await _messageSender.PublishAsync(
			subject,
			request.ToByteArray(),
			cancellationToken).ConfigureAwait(false);
	}
}
