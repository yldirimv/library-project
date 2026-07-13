using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryProject.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Visitor")]
    public abstract class ApiBaseController : ControllerBase
    {
        protected string CurrentUserId
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}