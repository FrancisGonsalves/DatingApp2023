using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;
public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;
    public LikesRepository(DataContext context)
    {
        _context = context;
    }
    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        var users = _context.Users.OrderBy(x => x.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();
        if(likesParams.Predicate == "Liked")
        {
            users = from like in likes 
                    join user in users 
                    on like.TargetUserId equals user.Id 
                    where like.SourceUserId == likesParams.UserId 
                    select user;
        }
        if(likesParams.Predicate == "LikedBy")
        {
            users = from like in likes
                    join user in users
                    on like.SourceUserId equals user.Id
                    where like.TargetUserId == likesParams.UserId
                    select user;
        }
        var query = users.Select(user => new LikeDto {
            Id = user.Id,
            UserName = user.UserName,
            Age = user.DateOfBirth.CalculateAge(),
            KnownAs = user.KnownAs,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
            City = user.City
        }).AsNoTracking();

        return await PagedList<LikeDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLikes(int userId)
    {
        return await _context.Users.Include(x => x.LikedUsers).Where(u => u.Id == userId).FirstOrDefaultAsync();
    }
}