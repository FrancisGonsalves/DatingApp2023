using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;
public class AdminController: BaseApiController
{
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
                    .Select(u => new {
                        u.Id,
                        u.UserName,
                        Roles = u.UserRoles.Select(role => role.Role.Name)
                    }).ToListAsync();
        
        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{userName}")]
    public async Task<ActionResult> EditRoles(string userName, [FromQuery]string roles)
    {
        if(String.IsNullOrEmpty(roles))
            return BadRequest("Please select atleast one role");
        
        AppUser user = await _userManager.FindByNameAsync(userName);
        if(user == null)
            return NotFound();

        var selectedRoles = roles.Split(","); 

        var userRoles = await _userManager.GetRolesAsync(user);

        
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if(!result.Succeeded)
            return BadRequest("Failed to add roles to user");
        
        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if(!result.Succeeded)
            return BadRequest("Failed to remove roles from user");
        
        return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-for-moderate")]
    public ActionResult GetPhotosForModeration()
    {
        return Ok("Admins or Moderators can see this");
    }
}