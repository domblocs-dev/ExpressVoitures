using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpressVoitures.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressVoitures.Controllers;

public class VoituresController : Controller
{
    private readonly ApplicationDbContext _context;

    public VoituresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Voitures
    public async Task<IActionResult> Index()
    {
        var voitures = await _context.Voitures
            .Include(v => v.Modele)
            .ThenInclude(m => m.Marque)
            .Include(v => v.Vente)
            .ToListAsync();
        return View(voitures);
    }

    // GET: /Voitures/Create
    public async Task<IActionResult> Create()
    {
        ViewData["ModeleId"] = await GetModelesSelectListAsync();
        return View(new VoitureFormViewModel
        {
            DateAchat = DateOnly.FromDateTime(DateTime.Today)
        });
    }

    // POST: /Voitures/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VoitureFormViewModel form)
    {
        if (ModelState.IsValid)
        {
            var voiture = new Voiture
            {
                Annee = form.Annee,
                CodeVIN = form.CodeVIN,
                Finition = form.Finition,
                ModeleId = form.ModeleId,
                Vente = new Vente
                {
                    PrixAchat = form.PrixAchat,
                    DateAchat = form.DateAchat,
                    DateDisponibilite = form.DateDisponibilite,
                    DateVente = form.DateVente,
                    Description = form.Description,
                    PhotoUrl = form.PhotoUrl,
                    // Prix de vente = prix d'achat + 500 (aucune réparation pour l'instant)
                    PrixVente = form.PrixAchat + 500m
                }
            };

            _context.Add(voiture);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ModeleId"] = await GetModelesSelectListAsync(form.ModeleId);
        return View(form);
    }








    // Prépare la liste déroulante des modèles, affichés « Marque Modèle »
    private async Task<SelectList> GetModelesSelectListAsync(int? selectedId = null)
    {
        var modeles = await _context.Modeles
            .Include(m => m.Marque)
            .OrderBy(m => m.Marque.Nom).ThenBy(m => m.Nom)
            .Select(m => new { m.Id, Libelle = m.Marque.Nom + " " + m.Nom })
            .ToListAsync();

        return new SelectList(modeles, "Id", "Libelle", selectedId);
    }

}
