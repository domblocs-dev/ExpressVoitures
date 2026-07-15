using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExpressVoitures.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: /  (la vitrine : toutes les voitures)
    public async Task<IActionResult> Index()
    {
        var voitures = await _context.Voitures
            .Include(v => v.Modele)
                .ThenInclude(m => m.Marque)
            .Include(v => v.Vente)
            .ToListAsync();
        return View(voitures);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
