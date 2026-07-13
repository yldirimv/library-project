namespace LibraryProject.Business.Interfaces
{
    public interface IQrService
    {
        Task<string> GenerateTokenAsync();
        Task<(bool valid, string message)> ValidateAndConsumeAsync(string token);
    }
}