using Microsoft.AspNetCore.SignalR;

namespace Media.Control.App.Api.Model
{

    public class LogHub : Hub
    {
        // 클라이언트로 메시지를 전송하는 메서드
        public async Task SendLogToClient(string message)
        {
            await Clients.All.SendAsync("ReceiveLog", message);
        }
    }

}
