using LibraryProject.Model.Entities;

namespace LibraryProject.Business.Interfaces
{
    public interface IAnnouncementService
    {
        Task<List<Announcement>> GetAllAsync();   //admin listesi (pasifler dahil)
        Task<List<Announcement>> GetPublishedAsync();    //ziyaretçi/personel görünümü
        Task<Announcement?> GetByIdAsync(int id);
        Task CreateAsync(Announcement announcement);
        Task UpdateAsync(Announcement announcement);
        Task DeleteAsync(int id);   //soft delete
    }
}