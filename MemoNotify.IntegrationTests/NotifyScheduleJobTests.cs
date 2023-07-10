using NSubstitute;
using Tgs.MessageQueue;

namespace MemoNotify.IntegrationTests;

public class NotifyScheduleJobTests
{
	[Fact]
	public async Task 排程觸發後會發送訊息給服務()
	{
		// Arrange
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new NotifyScheduleJob(fakeMessageSender);

		var subject = "tgs.test";
		var payload = new TstMemoNotify(
			"memo title",
			"memo something",
			DateTime.Now,
			new[] { "GP1", "GP2" });

		// Act
		await sut.ExecuteAsync("test", subject, payload.ScheduleTime.ToUniversalTime(), payload);

		// Assert
		_ = fakeMessageSender.PublishAsync(
			Arg.Is(subject),
			Arg.Any<ReadOnlyMemory<byte>>());
	}
}
