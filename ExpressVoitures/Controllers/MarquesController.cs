using ExpressVoitures.Data;
using ExpressVoitures.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpressVoitures.Controllers;

public class MarquesController : Controller
{
    private readonly ApplicationDbContext _context;
    public MarquesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Marques
    public async Task<IActionResult> Index()
    {
        var marques = await _context.Marques.ToListAsync();
        return View(marques);
    }

    // GET: /Marques/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Marques/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Marque marque)
    {
        if (ModelState.IsValid)
        {
            _context.Add(marque);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(marque);
    }

    // GET: /Marques/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var marque = await _context.Marques.FindAsync(id);
        if (marque == null)
        {
            return NotFound();
        }
        return View(marque);
    }

    // POST: /Marques/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Marque marque)
    {
        if (id != marque.Id)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {

            _context.Update(marque);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(marque);
    }

    // GET: /Marques/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var marque = await _context.Marques
            .FirstOrDefaultAsync(m => m.Id == id);
        if (marque == null)
        {
            return NotFound();
        }
        return View(marque);
    }

    // POST: /Marques/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var marque = await _context.Marques.FindAsync(id);
        if (marque != null)
        {
            _context.Marques.Remove(marque);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
