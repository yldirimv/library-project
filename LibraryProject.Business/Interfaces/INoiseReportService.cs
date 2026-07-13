using LibraryProject.Model.Entities;

namespace LibraryProject.Business.Interfaces
{
    public interface INoiseReportService
    {
        Task<(bool ok, string message, NoiseReport? report)> CreateAsync(string visitorId);
        Task<List<NoiseReport>> GetOpenReportsAsync();
        Task MarkHandledAsync(int reportId);
    }
}