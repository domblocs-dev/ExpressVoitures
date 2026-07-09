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

    // GET: /Voitures/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var voiture = await ChargerVoitureAvecReparationsAsync(id.Value);

        if (voiture == null)
        {
            return NotFound();
        }

        ViewData["TypeReparationId"] = new SelectList(_context.TypeReparations, "Id", "Libelle");
        return View(new VoitureDetailsViewModel { Voiture = voiture });
    }

    // POST: /Voitures/AddReparation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReparation(int voitureId, VoitureDetailsViewModel form)
    {
        // La voiture (affichage) n'est pas postée par le formulaire : on l'exclut de la validation
        ModelState.Remove(nameof(VoitureDetailsViewModel.Voiture));

        var voiture = await ChargerVoitureAvecReparationsAsync(voitureId);
        if (voiture == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var reparation = new Reparation
            {
                TypeReparationId = form.TypeReparationId,
                Cout = form.Cout
            };

            voiture.Vente.Reparations.Add(reparation); // EF insérera la réparation
            RecalculerPrixVente(voiture.Vente);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = voitureId });
        }

        // saisie invalide : on ré-affiche la page Détails avec les messages d'erreur
        form.Voiture = voiture;
        ViewData["TypeReparationId"] = new SelectList(_context.TypeReparations, "Id", "Libelle");
        return View(nameof(Details), form);
    }

    // POST: /Voitures/DeleteReparation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReparation(int reparationId, int voitureId)
    {
        var voiture = await ChargerVoitureAvecReparationsAsync(voitureId);
        if (voiture == null)
        {
            return NotFound();
        }

        var reparation = voiture.Vente.Reparations.FirstOrDefault(r => r.Id == reparationId);
        if (reparation != null)
        {
            voiture.Vente.Reparations.Remove(reparation);
            _context.Reparations.Remove(reparation);
            RecalculerPrixVente(voiture.Vente);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = voitureId });
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

    // Charge une voiture avec son modèle, sa marque, sa vente et ses réparations
    private async Task<Voiture?> ChargerVoitureAvecReparationsAsync(int id)
    {
        return await _context.Voitures
            .Include(v => v.Modele)
                .ThenInclude(m => m.Marque)
            .Include(v => v.Vente)
                .ThenInclude(vt => vt.Reparations)
                    .ThenInclude(r => r.TypeReparation)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    // Prix de vente = prix d'achat + somme des réparations + 500, tant que la voiture n'est pas vendue
    private static void RecalculerPrixVente(Vente vente)
    {
        if (vente.DateVente is null)
        {
            vente.PrixVente = vente.PrixAchat + vente.Reparations.Sum(r => r.Cout) + 500m;
        }
    }

}
