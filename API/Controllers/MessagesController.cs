using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;
using API.Entities;
using API.Helpers;
using CloudinaryDotNet.Actions;

namespace API.Controllers;
public class MessagesController: BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    public MessagesController(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        string userName = User.GetUserName();
        if(userName == createMessageDto.RecipientUserName.ToLower())
            return BadRequest("You can not send messages to yourself");
        
        AppUser sender = await _uow.UserRepository.GetUserByUserNameAsync(userName);
        AppUser recipient = await _uow.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName.ToLower());

        if(recipient == null)
            return NotFound("Recipient Not Found");
        
        Message message = new Message {
            Sender = sender,
            Recipient = recipient,
            SenderUserName = sender.UserName,
            RecipientUserName = recipient.UserName,
            Content = createMessageDto.Content
        };

        _uow.MessageRepository.AddMessage(message);
        
        if(await _uow.Complete())
            return Ok(_mapper.Map<MessageDto>(message));
        
        return BadRequest("Failed to send message");
    }
    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        messageParams.UserName = User.GetUserName();
        PagedList<MessageDto> pagedList = await _uow.MessageRepository.GetMessageForUser(messageParams);
        Response.AddPaginationHeader(new PaginationHeader(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages));
        return pagedList;
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        string userName = User.GetUserName();
        Message message = await _uow.MessageRepository.GetMessage(id);

        if(message == null)
            return NotFound("Message not found");
        
        if(message.SenderUserName != userName && message.RecipientUserName != userName)
            return Unauthorized("You are not authorized to delete other members messages");
        
        if(message.SenderUserName == userName)
            message.SenderDeleted = true;
        else if(message.RecipientUserName == userName)
            message.RecipientDeleted = true;
        
        if(message.SenderDeleted && message.RecipientDeleted)
            _uow.MessageRepository.DeleteMessage(message);
        
        if(await _uow.Complete())
            return Ok();
        
        return BadRequest("Failed to delete message");
    }
}