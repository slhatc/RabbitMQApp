using IdentityRabbitMQWeb.Data;
using IdentityRabbitMQWeb.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IdentityRabbitMQWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHubContext<MyHub> _hubContext;
        public FilesController(AppDbContext appDbContext, IHubContext<MyHub> hubContext = null)
        {
            _appDbContext = appDbContext;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if (file.Length <= 0 )
            {
                return BadRequest();
            }
            var userFile =await _appDbContext.UserFiles.FirstAsync(x => x.Id == fileId);
            var filePath = userFile.FileName + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);
            using FileStream stream = new(path, FileMode.Create);
            await file.CopyToAsync(stream);

            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = Models.FileStatus.Competed;
            await _appDbContext.SaveChangesAsync();
            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");
            return Ok();
        }
    }
}
