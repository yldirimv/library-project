using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Interfaces;

namespace LibraryProject.Business.Services
{
    public class QrService : IQrService
    {
        private readonly IUnitOfWork _uow;
        private const int TokenLifetimeSeconds = 10;

        public QrService(IUnitOfWork uow) => _uow = uow;

        public async Task<string> GenerateTokenAsync()
        {
            // Kriptografik rastgele, tahmin edilemez token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "").Replace("/", "").Replace("=", "");

            await _uow.QrTokens.AddAsync(new QrToken
            {
                Token = token,
                ExpiresAt = DateTime.Now.AddSeconds(TokenLifetimeSeconds),
                IsUsed = false
            });

            await _uow.SaveChangesAsync();
            return token;
        }

        public async Task<(bool valid, string message)> ValidateAndConsumeAsync(string token)
        {
            var qrToken = await _uow.QrTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (qrToken == null)
                return (false, "Geçersiz QR kod.");

            if (qrToken.IsUsed)
                return (false, "Bu QR kod zaten kullanılmış.");

            if (qrToken.ExpiresAt < DateTime.Now)
                return (false, "QR kodun süresi dolmuş. Lütfen ekrandaki güncel kodu okutun.");

            qrToken.IsUsed = true;   // tek kullanımlık: consume
            _uow.QrTokens.Update(qrToken);
            await _uow.SaveChangesAsync();

            return (true, "");
        }
    }
}