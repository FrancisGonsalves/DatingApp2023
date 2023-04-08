using API.Interfaces;
using API.Entities;
using API.DTOs;
using API.Helpers;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace API.Data;
public class MessageRepository: IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }
    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }
    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
    {
        var query = _context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();
        query = messageParams.Container switch {
            "Inbox" => query.Where(x => x.RecipientUserName == messageParams.UserName && !x.RecipientDeleted),
            "Outbox" => query.Where(x => x.SenderUserName == messageParams.UserName && !x.SenderDeleted),
            _ => query.Where(x => x.RecipientUserName == messageParams.UserName && !x.RecipientDeleted && x.DateRead == null)
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).AsNoTracking();
        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }
    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
    {
        IQueryable<Message> query = _context.Messages
            .Where(x => (x.RecipientUserName == currentUserName && x.SenderUserName == recipientUserName && !x.RecipientDeleted)
                || (x.RecipientUserName == recipientUserName && x.SenderUserName == currentUserName && !x.SenderDeleted))
            .OrderBy(x => x.MessageSent)
            .AsQueryable();
        
        var unreadMessages = await query.Where(x => x.DateRead == null && x.RecipientUserName == currentUserName).ToListAsync();
        if(unreadMessages.Any())
        {
            foreach(Message message in unreadMessages)
                message.DateRead = DateTime.UtcNow;
        }
        return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }
    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
    }
    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }
    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }
    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups
               .Include(x => x.Connections)
               .FirstOrDefaultAsync(x => x.Connections.Any(c => c.ConnectionId == connectionId));
    }
}