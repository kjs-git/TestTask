using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IFileQueryService _fileQueryService;

        public FilesController(
            IFileProcessingService fileProcessingService,
            IFileQueryService fileQueryService)
        {
            _fileProcessingService = fileProcessingService;
            _fileQueryService = fileQueryService;
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

        [HttpGet("results")]
        public async Task<IActionResult> GetResults([FromQuery] ResultFilterDto filter)
        {
            var results = await _fileQueryService.GetResultsAsync(filter);
            return Ok(results);
        }

        [HttpGet("{fileName}/values")]
        public async Task<IActionResult> GetLastValues(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Имя файла не может быть пустым.");

            var values = await _fileQueryService.GetLastValuesAsync(fileName);

            if (!values.Any())
                return NotFound($"Данные для файла '{fileName}' не найдены.");

            return Ok(values);
        }
    }
}
