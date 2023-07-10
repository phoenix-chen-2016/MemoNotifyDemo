using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace MemoNotify;

public class AnonymousHangfireAuthorization : IDashboardAsyncAuthorizationFilter
{
	public Task<bool> AuthorizeAsync([NotNull] DashboardContext context) => Task.FromResult(true);
}
