using System.Net.Http.Headers;
using Tgs.GameServer.Clients;

namespace MemoNotify;

public class GameServerModifierHandler : IRequestContentHandler
{
	public void PrepareHeaders(HttpContentHeaders headers)
		=> headers.Add("Modifier", "system");
}
