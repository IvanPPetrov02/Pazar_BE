using BLL.DTOs.ItemDTOs;
using BLL.Item_related;
using BLL.ManagerInterfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Pazar.Controllers
{
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
            try
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), HttpContext.RequestAborted);
                while (!result.CloseStatus.HasValue)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessage(socketId, message);
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), HttpContext.RequestAborted);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
            finally
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", HttpContext.RequestAborted);
            }
        }

        private async Task ProcessMessage(string socketId, string message)
        {
            try
            {
                // Parse the message to extract itemId
                var messageData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(message);
                if (messageData != null && messageData.TryGetValue("itemId", out var itemIdString) && int.TryParse(itemIdString, out var itemId))
                {
                    var updatedBids = await _itemManager.GetBidsByItemIdAsync(itemId);
                    var updatedBidsWithNames = updatedBids.Select(b => new
                    {
                        b.Bid,
                        b.BidTime,
                        BidderName = $"{b.Bidder.Name} {b.Bidder.Surname}",
                        BidderID = b.Bidder.UUID.ToString()
                    });

                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(updatedBidsWithNames);
                    var serverMsg = Encoding.UTF8.GetBytes(jsonResponse);

                    foreach (var ws in _webSockets.Values)
                    {
                        if (ws.State == WebSocketState.Open)
                        {
                            await ws.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, HttpContext.RequestAborted);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("newbid")]
        public async Task<IActionResult> NewBid([FromBody] CreateItemBidDTO bid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User information is missing in the token.");
            }

            try
            {
                await _itemManager.NewBidAsync(bid, userId);

                // Fetch the updated list of bids
                var updatedBids = await _itemManager.GetBidsByItemIdAsync(bid.ItemID);

                // Include bidder name in the updated bids
                var updatedBidsWithNames = updatedBids.Select(b => new
                {
                    b.Bid,
                    b.BidTime,
                    BidderName = $"{b.Bidder.Name} {b.Bidder.Surname}",
                    BidderID = b.Bidder.UUID.ToString()
                });

                var updatedBidsJson = System.Text.Json.JsonSerializer.Serialize(updatedBidsWithNames);
                var bidMessage = Encoding.UTF8.GetBytes(updatedBidsJson);

                // Notify all WebSocket clients about the new bid
                foreach (var ws in _webSockets.Values)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.SendAsync(new ArraySegment<byte>(bidMessage, 0, bidMessage.Length), WebSocketMessageType.Text, true, HttpContext.RequestAborted);
                    }
                }

                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        [HttpGet("bids/{itemId}")]
        public async Task<IActionResult> GetBidsByItemId(int itemId)
        {
            var bids = await _itemManager.GetBidsByItemIdAsync(itemId);
            
            var bidsWithNames = bids.Select(b => new
            {
                b.Bid,
                b.BidTime,
                BidderName = $"{b.Bidder.Name} {b.Bidder.Surname}",
                BidderID = b.Bidder.UUID.ToString()
            });

            return Ok(bidsWithNames);
        }
    }
}
