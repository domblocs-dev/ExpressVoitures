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
            var type = await _context.TypeReparations.FindAsync(form.TypeReparationId);
            if (type == null)
            {
                return NotFound();
            }

            var reparation = new Reparation
            {
                TypeReparationId = form.TypeReparationId,
                Cout = type.CoutParDefaut   // coût pré-rempli depuis le type
            };

            voiture.Vente.Reparations.Add(reparation);
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

    // POST: /Voitures/UpdateReparation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReparation(int reparationId, int voitureId, decimal cout)
    {
        var voiture = await ChargerVoitureAvecReparationsAsync(voitureId);
        if (voiture == null)
        {
            return NotFound();
        }

        var reparation = voiture.Vente.Reparations.FirstOrDefault(r => r.Id == reparationId);
        if (reparation != null && cout >= 0)
        {
            reparation.Cout = cout;
            RecalculerPrixVente(voiture.Vente);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = voitureId });
    }



    // GET: /Voitures/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var voiture = await _context.Voitures
            .Include(v => v.Vente)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (voiture == null)
        {
            return NotFound();
        }

        var form = new VoitureFormViewModel
        {
            Id = voiture.Id,
            Annee = voiture.Annee,
            CodeVIN = voiture.CodeVIN,
            Finition = voiture.Finition,
            ModeleId = voiture.ModeleId,
            PrixAchat = voiture.Vente.PrixAchat,
            DateAchat = voiture.Vente.DateAchat,
            DateDisponibilite = voiture.Vente.DateDisponibilite,
            DateVente = voiture.Vente.DateVente,
            Description = voiture.Vente.Description,
            PhotoUrl = voiture.Vente.PhotoUrl
        };

        ViewData["ModeleId"] = await GetModelesSelectListAsync(voiture.ModeleId);
        return View(form);
    }

    // POST: /Voitures/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VoitureFormViewModel form)
    {
        if (id != form.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewData["ModeleId"] = await GetModelesSelectListAsync(form.ModeleId);
            return View(form);
        }

        var voiture = await _context.Voitures
            .Include(v => v.Vente)
                .ThenInclude(vt => vt.Reparations)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (voiture == null)
        {
            return NotFound();
        }

        bool etaitVendue = voiture.Vente.DateVente is not null;

        // Mise à jour de la Voiture
        voiture.Annee = form.Annee;
        voiture.CodeVIN = form.CodeVIN;
        voiture.Finition = form.Finition;
        voiture.ModeleId = form.ModeleId;

        // Mise à jour de la Vente
        var vente = voiture.Vente;
        vente.PrixAchat = form.PrixAchat;
        vente.DateAchat = form.DateAchat;
        vente.DateDisponibilite = form.DateDisponibilite;
        vente.DateVente = form.DateVente;
        vente.Description = form.Description;
        vente.PhotoUrl = form.PhotoUrl;

        // Recalcul du prix, sauf si la voiture était DÉJÀ vendue avant cette modif (prix figé)
        if (!etaitVendue)
        {
            vente.PrixVente = vente.PrixAchat + vente.Reparations.Sum(r => r.Cout) + 500m;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Voitures/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var voiture = await _context.Voitures
            .Include(v => v.Modele)
                .ThenInclude(m => m.Marque)
            .Include(v => v.Vente)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (voiture == null)
        {
            return NotFound();
        }

        return View(voiture);
    }

    // POST: /Voitures/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var voiture = await _context.Voitures
            .Include(v => v.Vente)
                .ThenInclude(vt => vt.Reparations)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (voiture != null)
        {
            _context.Reparations.RemoveRange(voiture.Vente.Reparations);
            _context.Ventes.Remove(voiture.Vente);
            _context.Voitures.Remove(voiture);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
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
