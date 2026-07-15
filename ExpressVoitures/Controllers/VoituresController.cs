using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpressVoitures.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpressVoitures.Controllers;

public class VoituresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public VoituresController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
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
        if (form.Photo is not null)
        {
            PhotoEstValide(form.Photo);   // ajoute l'erreur au ModelState si besoin
        }

        if (ModelState.IsValid)
        {
            string? photoUrl = null;
            if (form.Photo is not null)
            {
                photoUrl = await EnregistrerPhotoAsync(form.Photo);
            }
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
                    PhotoUrl = photoUrl,
                    // Prix de vente = prix d'achat + 500 (aucune réparation pour l'instant)
                    PrixVente = form.PrixAchat + 500m
                }
            };

            _context.Add(voiture);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Publiee));   // au lieu de nameof(Index)
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
        // Vente finalisée : les réparations ne sont plus modifiables
        if (voiture.Vente.DateVente is not null)
        {
            return RedirectToAction(nameof(Details), new { id = voitureId });
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

        // Vente finalisée : les réparations ne sont plus modifiables
        if (voiture.Vente.DateVente is not null)
        {
            return RedirectToAction(nameof(Details), new { id = voitureId });
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

        // Vente finalisée : les réparations ne sont plus modifiables
        if (voiture.Vente.DateVente is not null)
        {
            return RedirectToAction(nameof(Details), new { id = voitureId });
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

        if (form.Photo is not null)
        {
            PhotoEstValide(form.Photo);
        }

        if (!ModelState.IsValid)
        {
            // on recharge la photo actuelle, pour que l'aperçu ne disparaisse pas
            form.PhotoUrl = await _context.Ventes
                .Where(v => v.VoitureId == id)
                .Select(v => v.PhotoUrl)
                .FirstOrDefaultAsync();

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
        // Photo : on ne remplace que si une nouvelle image a été envoyée
        if (form.Photo is not null)
        {
            vente.PhotoUrl = await EnregistrerPhotoAsync(form.Photo);
        }

        // Le prix n'est figé que si la voiture était vendue ET le reste après cette modif
        bool resteVendue = etaitVendue && vente.DateVente is not null;
        if (!resteVendue)
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
        var voiture = await ChargerVoitureAvecReparationsAsync(id);

        if (voiture != null)
        {
            // on capture le libellé AVANT de supprimer
            TempData["VoitureSupprimee"] = $"{voiture.Annee} {voiture.Modele.Marque.Nom} {voiture.Modele.Nom}";

            _context.Reparations.RemoveRange(voiture.Vente.Reparations);
            _context.Ventes.Remove(voiture.Vente);
            _context.Voitures.Remove(voiture);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Supprimee));
    }

    // GET: /Voitures/Publiee
    public IActionResult Publiee()
    {
        return View();
    }

    // GET: /Voitures/Supprimee
    public IActionResult Supprimee()
    {
        return View();
    }


    // -------------------------------------------------------------------

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

    // --- Téléversement des photos ---
    private static readonly string[] ExtensionsAutorisees = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long TailleMaxOctets = 5 * 1024 * 1024;   // 5 Mo

    // Vérifie le fichier reçu ; ajoute une erreur au ModelState s'il est invalide
    private bool PhotoEstValide(IFormFile photo)
    {
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!ExtensionsAutorisees.Contains(extension))
        {
            ModelState.AddModelError(nameof(VoitureFormViewModel.Photo),
                "Format non autorisé. Formats acceptés : JPG, PNG, WEBP.");
            return false;
        }
        if (!photo.ContentType.StartsWith("image/"))
        {
            ModelState.AddModelError(nameof(VoitureFormViewModel.Photo),
                "Le fichier envoyé n'est pas une image.");
            return false;
        }
        if (photo.Length > TailleMaxOctets)
        {
            ModelState.AddModelError(nameof(VoitureFormViewModel.Photo),
                "L'image ne doit pas dépasser 5 Mo.");
            return false;
        }
        return true;
    }

    // Enregistre la photo sous un nom généré, et renvoie son chemin web
    private async Task<string> EnregistrerPhotoAsync(IFormFile photo)
    {
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var nomFichier = $"{Guid.NewGuid()}{extension}";      // on n'utilise JAMAIS le nom fourni

        var dossier = Path.Combine(_environment.WebRootPath, "images", "voitures");
        Directory.CreateDirectory(dossier);

        var cheminComplet = Path.Combine(dossier, nomFichier);
        using var flux = new FileStream(cheminComplet, FileMode.Create);
        await photo.CopyToAsync(flux);

        return "/images/voitures/" + nomFichier;   // utilisable directement dans <img src="...">
    }

}
