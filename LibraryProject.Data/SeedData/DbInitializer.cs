using LibraryProject.Data.Context;
using LibraryProject.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryProject.Data.SeedData
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // İhtiyacımız olan servisleri DI kabından elle çekiyoruz
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceProvider.GetRequiredService<LibraryDbContext>();
            // getrequiredservice - service locator

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedSeatsAsync(context);
            await SeedBooksAsync(context);
            await SeedAnnouncementsAndGiftsAsync(context);
        }

        // ---------- 1. ROLLER ----------
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Employee", "Visitor","Kiosk" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ---------- 2. KULLANICILAR ----------
        private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            await CreateUserAsync(userManager, "admin@library.com", "Admin Kullanıcı", "Admin123", "Admin");
            await CreateUserAsync(userManager, "personel@library.com", "Ayşe Personel", "Personel123", "Employee");
            await CreateUserAsync(userManager, "ziyaretci1@library.com", "Mehmet Ziyaretçi", "Ziyaretci123", "Visitor", "11111111111");
            await CreateUserAsync(userManager, "ziyaretci2@library.com", "Zeynep Ziyaretçi", "Ziyaretci123", "Visitor", "22222222222");
            await CreateUserAsync(userManager, "kiosk@library.com", "Giriş Kiosku", "Kiosk123", "Kiosk");
        }

        private static async Task CreateUserAsync(UserManager<AppUser> userManager,
            string email, string fullName, string password, string role, string? tc = null)
        {
            // "Yoksa ekle" kontrolü — e-postaya göre bakıyoruz
            if (await userManager.FindByEmailAsync(email) != null) return;

            var user = new AppUser
            {
                UserName = email,       // login'de kullanıcı adı olarak e-posta kullanacağız
                Email = email,
                FullName = fullName,
                IdentityNumber = tc,    // sadece ziyaretçilerde dolu
                EmailConfirmed = true   // e-posta onay akışımız yok, kapıyı baştan açıyoruz
            };

            var result = await userManager.CreateAsync(user, password); // sifreyi hashleyip kaydeden metot
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }

        // ---------- 3. KOLTUKLAR ----------
        private static async Task SeedSeatsAsync(LibraryDbContext context)
        {
            if (await context.Seats.AnyAsync()) return;

            var seats = new List<Seat>();
            int no = 1;

            // --- KAT 1: krokiye sadık yerleşim (48 koltuk) ---

            // Sol kolon: 3 adet 6'lı masa
            foreach (var y in new[] { 230, 420, 610 })
                AddSixSeatTable(seats, ref no, 1, 80, y);

            // Üst orta: 3 adet 2'li masa
            foreach (var x in new[] { 420, 610, 800 })
                AddDuoTable(seats, ref no, 1, x, 200);

            // Orta kolon: 2 adet 6'lı masa
            foreach (var y in new[] { 380, 570 })
                AddSixSeatTable(seats, ref no, 1, 430, y);

            // Sağ kolon: 2 adet 6'lı masa
            foreach (var y in new[] { 380, 570 })
                AddSixSeatTable(seats, ref no, 1, 720, y);

            // --- KAT 2: krokiye sadık yerleşim (48 koltuk) ---
            int noB = 1;

            // Üst sıra (merdivenin sağı): 2 adet 6'lı masa
            foreach (var x in new[] { 380, 640 })
                AddSixSeatTableB(seats, ref noB, 2, x, 110);

            // Sol kolon: 3 adet 6'lı masa
            foreach (var y in new[] { 280, 470, 660 })
                AddSixSeatTableB(seats, ref noB, 2, 80, y);

            // Orta kolon: 3 adet 6'lı masa
            foreach (var y in new[] { 330, 520, 710 })
                AddSixSeatTableB(seats, ref noB, 2, 350, y);

            await context.Seats.AddRangeAsync(seats);
            await context.SaveChangesAsync();
        }

        // 6'lı masa: masanın üstüne 3, altına 3 koltuk dizer
        private static void AddSixSeatTable(List<Seat> seats, ref int no, int floor, int tableX, int tableY)
        {
            for (int i = 0; i < 3; i++)
                seats.Add(new Seat
                {
                    SeatNumber = $"A-{no++}",
                    Floor = floor,
                    PosX = tableX + 8 + i * 52,
                    PosY = tableY - 44
                });
            for (int i = 0; i < 3; i++)
                seats.Add(new Seat
                {
                    SeatNumber = $"A-{no++}",
                    Floor = floor,
                    PosX = tableX + 8 + i * 52,
                    PosY = tableY + 78
                });
        }

        private static void AddSixSeatTableB(List<Seat> seats, ref int no, int floor, int tableX, int tableY)
        {
            for (int i = 0; i < 3; i++)
                seats.Add(new Seat
                {
                    SeatNumber = $"B-{no++}",
                    Floor = floor,
                    PosX = tableX + 8 + i * 52,
                    PosY = tableY - 44
                });
            for (int i = 0; i < 3; i++)
                seats.Add(new Seat
                {
                    SeatNumber = $"B-{no++}",
                    Floor = floor,
                    PosX = tableX + 8 + i * 52,
                    PosY = tableY + 78
                });
        }

        // 2'li masa: masanın soluna ve sağına birer koltuk
        private static void AddDuoTable(List<Seat> seats, ref int no, int floor, int tableX, int tableY)
        {
            seats.Add(new Seat { SeatNumber = $"A-{no++}", Floor = floor, PosX = tableX - 44, PosY = tableY + 12 });
            seats.Add(new Seat { SeatNumber = $"A-{no++}", Floor = floor, PosX = tableX + 98, PosY = tableY + 12 });
        }

        // ---------- 4. KİTAPLAR ----------
        private static async Task SeedBooksAsync(LibraryDbContext context)
        {
            if (await context.Books.AnyAsync()) return;

            var books = new List<Book>
            {
                new Book { Title = "1984", Author = "George Orwell", ISBN = "9789750718533", TotalStock = 15 },
                new Book { Title = "Suç ve Ceza", Author = "Dostoyevski", ISBN = "9789750719387", TotalStock = 8 },
                new Book { Title = "Kürk Mantolu Madonna", Author = "Sabahattin Ali", ISBN = "9789753638029", TotalStock = 10 },
                new Book { Title = "Simyacı", Author = "Paulo Coelho", ISBN = "9789750726439", TotalStock = 5 },
                new Book { Title = "Hayvan Çiftliği", Author = "George Orwell", ISBN = "9789750718526", TotalStock = 12 },
                new Book { Title = "Tutunamayanlar", Author = "Oğuz Atay", ISBN = "9789754700114", TotalStock = 6 },
                new Book { Title = "Beyaz Zambaklar Ülkesinde", Author = "G. Petrov", ISBN = "9786053324846", TotalStock = 7 },
                new Book { Title = "Nutuk", Author = "M. Kemal Atatürk", ISBN = "9789751020582", TotalStock = 9 }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();
        }

        // ---------- 5. DUYURU + HEDİYE ----------
        private static async Task SeedAnnouncementsAndGiftsAsync(LibraryDbContext context)
        {
            if (!await context.Announcements.AnyAsync())
            {
                await context.Announcements.AddRangeAsync(
                    new Announcement { Title = "Hoş Geldiniz", Content = "Kütüphane rezervasyon sistemimiz yayında!" },
                    new Announcement { Title = "Çalışma Saatleri", Content = "Kütüphanemiz hafta içi 08:00-22:00 arası açıktır." }
                );
            }

            if (!await context.Gifts.AnyAsync())
            {
                await context.Gifts.AddRangeAsync(
                    new Gift { Name = "Kahve Kuponu", RequiredPoints = 100, Stock = 50 },
                    new Gift { Name = "Kitap Ayracı Seti", RequiredPoints = 50, Stock = 100 },
                    new Gift { Name = "Termos", RequiredPoints = 500, Stock = 20 }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}