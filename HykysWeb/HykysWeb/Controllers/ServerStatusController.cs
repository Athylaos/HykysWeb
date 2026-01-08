using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MineStatLib;
using System.Net;
using System.Net.Sockets;

namespace HykysWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerStatusController : ControllerBase
    {
        [HttpGet("check-port")]
        public async Task<IActionResult> CheckPort(string ip, int port)
        {

            try
            {
                using var client = new TcpClient(AddressFamily.InterNetwork);
                var timeoutTask = Task.Delay(5000);

                var addresses = await Dns.GetHostAddressesAsync(ip);
                var targetIp = addresses.FirstOrDefault();

                var connectTask = client.ConnectAsync(targetIp, port);
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask && client.Connected)
                {
                    return Ok(new { IsOnline = true, Message = "Port is open" });
                }
                else
                {
                    return Ok(new { IsOnline = false, Message = "Port is closed" });
                }
            }
            catch
            {
                return Ok(new { IsOnline = false, Message = "Can't connect"});
            }
        }


        [HttpGet("mc-status")]
        public IActionResult GetMinecraftStatus(string ip, int port = 25565)
        {
            try
            {
                var ms = new MineStat(ip, (ushort)port);

                if (ms.ServerUp)
                {
                    return Ok(new
                    {
                        IsOnline = true,
                        Players = ms.CurrentPlayers,
                        MaxPlayers = ms.MaximumPlayers,
                        Motd = ms.Motd,
                        Version = ms.Version
                    });
                }
                else
                {
                    return Ok(new { IsOnline = false });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
