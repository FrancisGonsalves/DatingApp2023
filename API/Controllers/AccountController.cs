using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using AutoMapper;

namespace API.Controllers
{
    public class AccountController: BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto register)
        {
            if(await UserExists(register.UserName.ToLower()))
                return BadRequest("Username is taken");
            
            AppUser user = _mapper.Map<AppUser>(register);

            user.UserName = user.UserName.ToLower();
            
            var result = await _userManager.CreateAsync(user, register.Password);
            if(!result.Succeeded)
                return BadRequest(result.Errors);
            
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded)
                return BadRequest(result.Errors);

            return new UserDto {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> UserExists(string userName) 
        {
            return await _userManager.Users.AnyAsync(user => user.UserName == userName);
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {
            AppUser user = await _userManager.Users.Include(x => x.Photos).SingleOrDefaultAsync(user => user.UserName == login.UserName);
            if(user == null)
                return Unauthorized("Invalid Username");

            var result = await _userManager.CheckPasswordAsync(user, login.Password);
            if(!result)
                return Unauthorized("Invalid Password");
           
            return new UserDto {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
    }
}