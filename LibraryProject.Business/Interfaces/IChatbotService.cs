namespace LibraryProject.Business.Interfaces
{
    public interface IChatbotService
    {
        Task<string> AskAsync(string question);
    }
}