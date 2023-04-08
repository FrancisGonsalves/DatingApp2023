using API.Interfaces;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;
using API.Entities;
using API.DTOs;
using API.Helpers;

namespace API.Controllers;
public class LikesController: BaseApiController
{
    private readonly IUnitOfWork _uow;
    public LikesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpPost("{userName}")]
    public async Task<ActionResult> AddLike(string userName)
    {
        int sourceUserId = User.GetUserId();
        AppUser sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);
        AppUser targetUser = await _uow.UserRepository.GetUserByUserNameAsync(userName);

        if(targetUser == null)
            return NotFound();
        
        if(sourceUser.LikedUsers.Any(x => x.SourceUserId == sourceUserId && x.TargetUserId == targetUser.Id))
            return BadRequest("You already liked this user");
        
        if(sourceUser.Id == targetUser.Id)
            return BadRequest("You cannot like yourself");
        

        sourceUser.LikedUsers.Add(new UserLike {
            SourceUserId = sourceUserId,
            TargetUserId = targetUser.Id
        });

        if(await _uow.Complete())
            return Ok();
        
        return BadRequest("Failed to like user");
    } 
    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        PagedList<LikeDto> pagedList = await _uow.LikesRepository.GetUserLikes(likesParams);
        Response.AddPaginationHeader(new PaginationHeader(
            pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount, pagedList.TotalPages
        ));

        return Ok(pagedList);
    }
}