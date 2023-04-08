using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using API.Data;
using API.Entities;

namespace API.Controllers
{
    public class BuggyController: BaseApiController
    {
        private readonly DataContext _context;
        public BuggyController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetAuth()
        {
            return "Super Secret";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound() {
            AppUser user = _context.Users.Find(-1);
            if(user == null)
                return NotFound("User Not Found");
            else
                return user;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest() {
            return BadRequest("This is bad request");
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError() {
            AppUser user = _context.Users.Find(-1);
            return user.ToString();
        }
    }
}