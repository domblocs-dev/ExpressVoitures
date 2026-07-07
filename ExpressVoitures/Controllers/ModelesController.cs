using ExpressVoitures.Data;
using ExpressVoitures.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; // Add this using directive for SelectList

namespace ExpressVoitures.Controllers;

public class ModelesController : Controller
{
    private readonly ApplicationDbContext _context;
    public ModelesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Modeles
    public async Task<IActionResult> Index()
    {
        var modeles = await _context.Modeles
            .Include(m => m.Marque)
            .ToListAsync();
        return View(modeles);
    }

    // GET: /Modeles/Create
    public IActionResult Create()
    {
        ViewData["MarqueId"] = new SelectList(_context.Marques, "Id", "Nom");
        
        return View();
    }

    // POST: /Modeles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Modele modele)
    {
        // La navigation Marque n'est pas envoyée par le formulaire (seul MarqueId l'est) :
        // on l'exclut de la validation, sinon ModelState serait invalide.

        ModelState.Remove(nameof(Modele.Marque));
        if (ModelState.IsValid)
        {
            _context.Add(modele);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["MarqueId"] = new SelectList(_context.Marques, "Id", "Nom", modele.MarqueId);
        return View(modele);
    }

    // GET: /Modeles/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var modele = await _context.Modeles.FindAsync(id);
        if (modele == null)
        {
            return NotFound();
        }

        ViewData["MarqueId"] = new SelectList(_context.Marques, "Id", "Nom", modele.MarqueId);
        return View(modele);
    }

    // POST: /Modeles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Modele modele)
    {
        if (id != modele.Id)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Modele.Marque));

        if (ModelState.IsValid)
        {
            _context.Update(modele);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["MarqueId"] = new SelectList(_context.Marques, "Id", "Nom", modele.MarqueId);
        return View(modele);
    }

    // GET: /Modeles/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var modele = await _context.Modeles
            .Include(m => m.Marque)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (modele == null)
        {
            return NotFound();
        }

        return View(modele);
    }

    // POST: /Modeles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var modele = await _context.Modeles.FindAsync(id);
        if (modele != null)
        {
            _context.Modeles.Remove(modele);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }


}
