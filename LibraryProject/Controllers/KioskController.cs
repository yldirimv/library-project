using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace LibraryProject.Controllers
{
    [Authorize(Roles = "Kiosk")]
    public class KioskController : Controller
    {
        private readonly IQrService _qrService;

        public KioskController(IQrService qrService) => _qrService = qrService;

        public IActionResult Index() => View();

        // 5 saniyede bir çağrılacak: yeni token üret, PNG olarak döndür
        public async Task<IActionResult> QrImage()
        {
            var token = await _qrService.GenerateTokenAsync();

            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(token, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(data).GetGraphic(12);

            return File(png, "image/png");
        }
    }
}