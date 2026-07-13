using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Web.Areas.Visitor.Controllers
{
    public class ChatbotController : VisitorBaseController
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
            => _chatbotService = chatbotService;

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest();

            var answer = await _chatbotService.AskAsync(request.Question.Trim());
            return Json(new { answer });
        }

        public record AskRequest(string Question);
    }
}