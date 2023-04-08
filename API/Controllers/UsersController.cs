using Microsoft.AspNetCore.Mvc;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using API.Helpers;
using AutoMapper;
using CloudinaryDotNet.Actions;
//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
namespace API.Controllers
{
    [Authorize]
    public class UsersController: BaseApiController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            _uow = uow;
            _mapper = mapper;
            _photoService = photoService;
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            string gender = await _uow.UserRepository.GetUserGender(User.GetUserName());

            userParams.CurrentUserName = User.GetUserName();
            if(String.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender == "male" ? "female": "male";

            PagedList<MemberDto> users = await _uow.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages
            ));
            return Ok(users);
        }
        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            return await _uow.UserRepository.GetMemberAsync(userName);
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            AppUser user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            if(user == null) 
                return NotFound();
            _mapper.Map(memberUpdateDto, user);
            if(await _uow.Complete()) 
                return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            
            if(user == null)
                return NotFound();
            
            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null)
                return BadRequest(result.Error.Message);

            Photo photo = new Photo {
                Url = result.Url.AbsoluteUri,
                PublicId = result.PublicId,
                IsMain = user.Photos.Count == 0
            };

            user.Photos.Add(photo);

            if(await _uow.Complete())
                return CreatedAtAction(nameof(GetUser), new { userName = user.UserName }, _mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem Adding Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            AppUser user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            if(user == null)
                return NotFound("User Not Found");
            
            Photo photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if(photo == null)
                return NotFound("Photo Not Found");
            
            if(photo.IsMain)
                return BadRequest("Photo is already main photo");
            
            Photo mainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
            if(mainPhoto != null)
                mainPhoto.IsMain = false;

             photo.IsMain = true;

            if(await _uow.Complete())
                return NoContent();
            
            return BadRequest("Problem in setting main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            AppUser user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());

            Photo photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null)
                return NotFound("Photo not found");
            
            if(photo.IsMain)
                return BadRequest("you cannot delete main photo");
            
            if(photo.PublicId != null)
            {

            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null)
                return BadRequest(result.Error.Message);

            }
            user.Photos.Remove(photo);

            if(await _uow.Complete())
                return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}