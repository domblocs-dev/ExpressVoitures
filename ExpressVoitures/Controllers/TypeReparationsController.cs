using ExpressVoitures.Data;
using ExpressVoitures.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpressVoitures.Controllers;

public class TypeReparationsController : Controller
{
    private readonly ApplicationDbContext _context;
    public TypeReparationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /TypeReparations
    public async Task<IActionResult> Index()
    {
        var typeReparations = await _context.TypeReparations.ToListAsync();
        return View(typeReparations);
    }

    // GET: /TypeReparations/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /TypeReparations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TypeReparation typeReparation)
    {
        if (ModelState.IsValid)
        {
            _context.Add(typeReparation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(typeReparation);
    }

    // GET: /TypeReparations/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var typeReparation = await _context.TypeReparations.FindAsync(id);
        if (typeReparation == null)
        {
            return NotFound();
        }
        return View(typeReparation);
    }

    // POST: /TypeReparations/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TypeReparation typeReparation)
    {
        if (id != typeReparation.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(typeReparation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(typeReparation);
    }

    // GET: /TypeReparations/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var typeReparation = await _context.TypeReparations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (typeReparation == null)
        {
            return NotFound();
        }
        return View(typeReparation);
    }

    // POST: /TypeReparations/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var typeReparation = await _context.TypeReparations.FindAsync(id);
        if (typeReparation != null)
        {
            _context.TypeReparations.Remove(typeReparation);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
