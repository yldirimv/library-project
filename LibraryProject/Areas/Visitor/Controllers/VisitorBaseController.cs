using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Areas.Visitor.Controllers
{

    [Area("Visitor")]
    [Authorize(Roles = "Visitor")]
    public abstract class VisitorBaseController : Controller { }
}
