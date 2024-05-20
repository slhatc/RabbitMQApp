using IdentityRabbitMQWeb.Data;
using IdentityRabbitMQWeb.Models;
using IdentityRabbitMQWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityRabbitMQWeb.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RabbitMQPublisher _publisher;

        public ProductController(AppDbContext dbContext, UserManager<IdentityUser> userManager, RabbitMQPublisher publisher)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _publisher = publisher;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CreateProductexcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(0, 10)}";

            UserFile userFile = new UserFile()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };
            await _dbContext.UserFiles.AddAsync(userFile);
            await _dbContext.SaveChangesAsync();
            _publisher.Publish(new Shared.CreateExcelMessage { FileId = userFile.Id });
            TempData["StartCreatingExcel"] = true;
            return RedirectToAction("Files");   
        }
        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
            var model = await _dbContext.UserFiles.Where(x=>x.UserId == user!.Id).ToListAsync();
            return View(model);
        }
    }
}
