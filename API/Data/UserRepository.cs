using API.Interfaces;
using API.Entities;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using API.Helpers;

namespace API.Data
{
    public class UserRepository: IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(x => x.Photos).ToListAsync();
        }
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            userName = userName.ToLower();
            return await _context.Users.Include(x => x.Photos).SingleOrDefaultAsync(x => x.UserName == userName);
        }
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(x => x.UserName != userParams.CurrentUserName);
            query = query.Where(x => x.Gender == userParams.Gender);

            DateOnly minDob = DateOnly.FromDateTime(DateTime.Now.AddYears(-userParams.MaxAge - 1));
            DateOnly maxDob = DateOnly.FromDateTime(DateTime.Now.AddYears(-userParams.MinAge));

            query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch {
                "created" => query.OrderByDescending(x => x.Created),
                _ => query.OrderByDescending(x => x.LastActive)
            };
            
            return await PagedList<MemberDto>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
              , userParams.PageNumber
              , userParams.PageSize);
        }
        public async Task<MemberDto> GetMemberAsync(string userName)
        {
            return await _context.Users.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(x => x.UserName == userName);
        }
        public async Task<string> GetUserGender(string userName)
        {
            return await _context.Users.Where(x => x.UserName == userName).Select(x => x.Gender).FirstOrDefaultAsync();
        }
    }
}