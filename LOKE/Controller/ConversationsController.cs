using Core.Controller;
using Core.Extention;
using Core.Hubs;
using Core.Service.Interfaces;
using LOKE.Models.Dto;
using LOKE.Models.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.Json;

namespace LOKE.Controller
{
    [ApiController]
    [Route("api/v1/conversations")]
    [Authorize]
    public class ConversationsController(
        IBaseService<ConversationModel> conversationService, 
        IHubContext<AppHub> hubContext,
        IRealtimeService realtime
    ) : CoreController
    {
        private readonly IBaseService<ConversationModel> _conversationService = conversationService;
        private readonly IHubContext<AppHub> _hubContext = hubContext;
        private readonly IRealtimeService _realtime = realtime;

        [HttpPost]
        public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationDto dto)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null)
                return Unauthorized();

            if(string.IsNullOrEmpty(dto.OtherId))
                return BadRequest("OtherId are required.");

            var conv = new ConversationModel
            {
                UserAId = requester.UserName!,
                UserBId = dto.OtherId
            };

            var result = await _conversationService.CreateAsync(conv);
            if (!result.IsSuccess) return BadRequest(result.Message);

            return Ok(new ConversationDto
            {
                Id = result.Data!.Id,
                UserA = result.Data.UserAId,
                UserB = result.Data.UserBId,
                Messages = []
            });
        }

        [HttpGet]
        public async Task<ActionResult<List<ConversationDto>>> GetUserConversations()
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null || string.IsNullOrEmpty(requester.UserName))
                return Unauthorized();
            var allConvs = await _conversationService.GetAllAsync();
            if (!allConvs.IsSuccess || allConvs.Data == null)
                return BadRequest(allConvs.Message);
            var userConvs = allConvs.Data
                .Where(c => c.UserAId == requester.UserName || c.UserBId == requester.UserName)
                .Select(c => new ConversationDto
                {
                    Id = c.Id,
                    UserA = c.UserAId,
                    UserB = c.UserBId,
                    Messages = [.. c.Messages
                        .TakeLast(20) // Lấy tin nhắn cuối cùng
                        .Select(m => new MessageContentDto
                        {
                            Id = m.Id,
                            SenderId = m.SenderId,
                            Content = m.Content,
                            Type = m.Type.ToString(),
                            SentAt = m.SentAt
                        })]
                })
                .ToList();
            return Ok(userConvs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDto>> GetConversation(string id)
        {
            var conv = await _conversationService.GetByIdAsync(id);
            if (!conv.IsSuccess || conv.Data == null) return NotFound();

            // Lấy tối đa 50 message cuối cùng (gần nhất)
            var messages = conv.Data.Messages
                .TakeLast(50)
                .Select(m => new MessageContentDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    Type = m.Type.ToString(),
                    SentAt = m.SentAt
                })
                .ToList();

            var dto = new ConversationDto
            {
                Id = conv.Data.Id,
                UserA = conv.Data.UserAId,
                UserB = conv.Data.UserBId,  
                Messages = messages
            };

            return Ok(dto);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<MessageContentDto>> SendMessage([FromBody] CreateMessageDto dto)
        {
            var requester = HttpContext.Request.GetInfoRequester();
            if (requester == null || requester.IsExpired || string.IsNullOrEmpty(requester.UserName))
                return Unauthorized();

            var convResult = await _conversationService.GetByIdAsync(dto.ConversationId);
            if (!convResult.IsSuccess || convResult.Data == null)
                return NotFound();

            var conv = convResult.Data;

            // xác định user nhận
            string toUserId = conv.UserAId == requester.UserName ? conv.UserBId : conv.UserAId;
            if (string.IsNullOrEmpty(toUserId))
                return BadRequest("Invalid recipient.");

            var message = new MessageContent
            {
                SenderId = requester.UserName!,
                Content = dto.Content,
                Type = dto.Type,
                SentAt = DateTime.UtcNow
            };

            var messageDto = message.ToDto();


            // 🔹 đẩy realtime qua SignalR
            var connections = await _realtime.GetConnectionsAsync(toUserId);
            foreach (var connId in connections)
            {
                await _hubContext.Clients.Client(connId)
                    .SendAsync("ReceiveMessage", messageDto);
            }

            // 🔹 lưu DB
            conv.Messages.Add(message);

            if (conv.Messages.Count > 50)
                conv.Messages.RemoveAt(0);

            var result = await _conversationService.UpdateAsync(conv.Id, conv);

            return result.IsSuccess ? Ok(messageDto) : BadRequest("Loi khi luu");
        }

    }
}
