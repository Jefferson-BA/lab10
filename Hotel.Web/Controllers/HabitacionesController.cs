using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _repo;

    public HabitacionesController(HabitacionRepositorio repo)
    {
        _repo = repo;
    }

    // GET: Habitaciones?idTipo=1
    public async Task<IActionResult> Index(int? idTipo)
    {
        var habitaciones = await _repo.ObtenerActivasAsync(idTipo);
        var tipos = await _repo.ObtenerTiposAsync();

        ViewBag.Tipos = new SelectList(tipos, "IdTipoHabitacion", "Nombre", idTipo);
        ViewBag.FiltroTipo = idTipo;

        return View(habitaciones);
    }

    // GET: Habitaciones/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var habitacion = await _repo.ObtenerPorIdAsync(id);
        if (habitacion is null) return NotFound();
        return View(habitacion);
    }

    // GET: Habitaciones/Create
    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View();
    }

    // POST: Habitaciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        // Quitar propiedades de navegación de la validación
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));

        // Validar número único
        if (await _repo.NumeroExisteAsync(habitacion.Numero))
            ModelState.AddModelError(nameof(Habitacion.Numero),
                $"Ya existe una habitación con el número '{habitacion.Numero}'.");

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        habitacion.Activo = true;
        await _repo.CrearAsync(habitacion);

        TempData["Exito"] = $"Habitación {habitacion.Numero} creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Habitaciones/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await _repo.ObtenerPorIdAsync(id);
        if (habitacion is null) return NotFound();

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    // POST: Habitaciones/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion) return BadRequest();

        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));

        // Validar número único excluyendo la habitación actual
        if (await _repo.NumeroExisteAsync(habitacion.Numero, habitacion.IdHabitacion))
            ModelState.AddModelError(nameof(Habitacion.Numero),
                $"Ya existe otra habitación con el número '{habitacion.Numero}'.");

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        await _repo.ActualizarAsync(habitacion);

        TempData["Exito"] = $"Habitación {habitacion.Numero} actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Habitaciones/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var habitacion = await _repo.ObtenerPorIdAsync(id);
        if (habitacion is null) return NotFound();
        return View(habitacion);
    }

    // POST: Habitaciones/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.EliminarLogicoAsync(id);
        TempData["Exito"] = "Habitación dada de baja correctamente (borrado lógico).";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task CargarTiposAsync(int? seleccionado = null)
    {
        var tipos = await _repo.ObtenerTiposAsync();
        ViewBag.IdTipoHabitacion = new SelectList(tipos, "IdTipoHabitacion", "Nombre", seleccionado);
    }
}
