using BLL.DTOs.ItemDTOs;
using BLL.Item_related;
using BLL.ManagerInterfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;

namespace Pazar.Controllers;

[ApiController]
[Route("ws/[controller]")]
public class ItemBiddingController : ControllerBase
{
    private readonly IItemManager _itemManager;
    private static ConcurrentDictionary<string, WebSocket> _webSockets = new ConcurrentDictionary<string, WebSocket>();

    public ItemBiddingController(IItemManager itemManager)
    {
        _itemManager = itemManager;
    }

    [HttpGet]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            string socketId = Guid.NewGuid().ToString();
            _webSockets.TryAdd(socketId, webSocket);

            await HandleWebSocketConnection(socketId, webSocket);

            _webSockets.TryRemove(socketId, out _);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }

    private async Task HandleWebSocketConnection(string socketId, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), HttpContext.RequestAborted);
        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            // Handle the received message here
            await ProcessMessage(socketId, message);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), HttpContext.RequestAborted);
        }
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, HttpContext.RequestAborted);
    }

    private async Task ProcessMessage(string socketId, string message)
    {
        // Example: Broadcast the message to all connected clients
        foreach (var ws in _webSockets.Values)
        {
            if (ws.State == WebSocketState.Open)
            {
                var serverMsg = Encoding.UTF8.GetBytes($"Client {socketId}: {message}");
                await ws.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, HttpContext.RequestAborted);
            }
        }
    }

    [Authorize]
    [HttpPost("newbid")]
    public async Task<IActionResult> NewBid([FromBody] CreateItemBid bid)
    {
        await _itemManager.NewBidAsync(bid);
        
        // Notify all WebSocket clients about the new bid
        var bidMessage = Encoding.UTF8.GetBytes($"New bid placed on item {bid.ItemID}: ${bid.Bid}");
        foreach (var ws in _webSockets.Values)
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(new ArraySegment<byte>(bidMessage, 0, bidMessage.Length), WebSocketMessageType.Text, true, HttpContext.RequestAborted);
            }
        }

        return Ok();
    }

    [Authorize]
    [HttpGet("bids/{itemId}")]
    public async Task<IActionResult> GetBidsByItemId(int itemId)
    {
        var bids = await _itemManager.GetBidsByItemIdAsync(itemId);
        return Ok(bids);
    }
}
