using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace ChurchHospitalityApp.Controllers
{
    [ApiController]
    [Route("api/qrcode")]
    [Authorize(Policy = "AdminOnly")]
    public class QrCodeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public QrCodeController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>Returns a PNG QR code for the public guest-form URL; the URL is in X-Guest-Form-Url.</summary>
        [HttpGet]
        public IActionResult Get()
        {
            var url = _config["App:PublicGuestFormUrl"] ?? string.Empty;

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var bytes = new PngByteQRCode(qrData).GetGraphic(20);

            Response.Headers["X-Guest-Form-Url"] = url;
            Response.Headers["Access-Control-Expose-Headers"] = "X-Guest-Form-Url";
            return File(bytes, "image/png");
        }
    }
}
