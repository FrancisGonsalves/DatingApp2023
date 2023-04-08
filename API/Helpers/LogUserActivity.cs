using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using API.Entities;
using API.Interfaces;
using System.Net.Mime;

namespace API.Helpers;
public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();
        if(!resultContext.HttpContext.User.Identity.IsAuthenticated)
            return;

        int userId = resultContext.HttpContext.User.GetUserId();
        IUnitOfWork uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        AppUser user = await uow.UserRepository.GetUserByIdAsync(userId);
        user.LastActive = DateTime.UtcNow;

        await uow.Complete();
    }
}