using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;
public class MessageHub: Hub
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    public MessageHub(IUnitOfWork uow, IMapper mapper, IHubContext<PresenceHub> presenceHub)
    {
        _uow = uow;
        _mapper = mapper;
        _presenceHub = presenceHub;
    }
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"];
        var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _uow.MessageRepository.GetMessageThread(Context.User.GetUserName(), otherUser);
        if(_uow.HasChanges())
            await _uow.Complete();

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Group group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        await base.OnDisconnectedAsync(exception);
    }
    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var userName = Context.User.GetUserName();
        if(userName == createMessageDto.RecipientUserName)
            throw new HubException("You cannot send messages to yourself");
        
        var sender = await _uow.UserRepository.GetUserByUserNameAsync(userName);
        var recipient = await _uow.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

        if(recipient == null)
            throw new HubException("Not Found Recipient User");
        
        var message = new Message {
            Sender = sender,
            Recipient = recipient,
            SenderUserName = sender.UserName,
            RecipientUserName = recipient.UserName,
            Content = createMessageDto.Content
        };
        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await _uow.MessageRepository.GetMessageGroup(groupName);
        if(group.Connections.Any(x => x.UserName == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            List<string> connectionIds = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if(connectionIds != null)
                await _presenceHub.Clients.Clients(connectionIds).SendAsync("NewMessageReceived", new { UserName = sender.UserName, KnownAs = sender.KnownAs });
        }

        _uow.MessageRepository.AddMessage(message);
        if(await _uow.Complete())
        {
            await Clients.Group(group.Name).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }
    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
    private async Task<Group> AddToGroup(string groupName)
    {
        Group group = await _uow.MessageRepository.GetMessageGroup(groupName);
        if(group == null)
        {
            group = new Group(groupName);
            _uow.MessageRepository.AddGroup(group);
        }
        Connection connection = new Connection(Context.ConnectionId, Context.User.GetUserName());
        group.Connections.Add(connection);

        if(await _uow.Complete())
            return group;
        throw new HubException("Failed to add group");
    }
    private async Task<Group> RemoveFromMessageGroup()
    {
        Group group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        Connection connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        _uow.MessageRepository.RemoveConnection(connection);
        if(await _uow.Complete())
            return group;
        throw new HubException("Failed to remove from group");
    }
}