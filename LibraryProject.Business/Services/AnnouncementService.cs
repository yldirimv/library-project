using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Interfaces;

namespace LibraryProject.Business.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _uow;

        public AnnouncementService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<Announcement>> GetAllAsync()
            => (await _uow.Announcements.GetAllAsync())
                .OrderByDescending(a => a.CreatedDate).ToList();

        public async Task<List<Announcement>> GetPublishedAsync()
            => (await _uow.Announcements.FindAsync(a => a.IsActive && a.IsPublished))
                .OrderByDescending(a => a.CreatedDate).ToList();

        public async Task<Announcement?> GetByIdAsync(int id)
            => await _uow.Announcements.GetByIdAsync(id);

        public async Task CreateAsync(Announcement announcement)
        {
            await _uow.Announcements.AddAsync(announcement);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateAsync(Announcement announcement)
        {
            _uow.Announcements.Update(announcement);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var announcement = await _uow.Announcements.GetByIdAsync(id);
            if (announcement == null) return;

            announcement.IsActive = false;      // soft delete: silmiyoruz sadeece pasife çekiyoruz
            _uow.Announcements.Update(announcement);
            await _uow.SaveChangesAsync();
        }
    }
}