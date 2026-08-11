using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileProcessingService _fileProcessingService;

        public FilesController(IFileProcessingService fileProcessingService)
        {
            _fileProcessingService = fileProcessingService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран.");

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Неверный формат файла. Ожидается CSV.");

            try
            {
                using var stream = file.OpenReadStream();
                await _fileProcessingService.ProcessFileAsync(stream, file.FileName);
                return Ok(new { Message = "Файл успешно обработан и сохранен." });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { Error = errorMessage });
            }
        }
    }
}
