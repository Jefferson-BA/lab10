using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _repo;

    public ReservasController(ReservaRepositorio repo)
    {
        _repo = repo;
    }

    // GET: Reservas
    public async Task<IActionResult> Index()
    {
        var reservas = await _repo.ObtenerTodasAsync();
        return View(reservas);
    }

    // ── DISPONIBILIDAD ────────────────────────────────────────────────────────

    // GET: Reservas/Disponibles
    // Parámetros opcionales para el filtro por fechas
    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Si se proporcionaron fechas válidas, buscar disponibles
        List<Habitacion>? habitaciones = null;
        int? noches = null;

        if (ingreso.HasValue && salida.HasValue)
        {
            if (ingreso < hoy)
                ModelState.AddModelError(nameof(ingreso), "La fecha de ingreso no puede ser anterior a hoy.");
            else if (salida <= ingreso)
                ModelState.AddModelError(nameof(salida), "La fecha de salida debe ser posterior a la de ingreso.");
            else
            {
                noches = salida.Value.DayNumber - ingreso.Value.DayNumber;
                habitaciones = await _repo.ObtenerDisponiblesAsync(ingreso.Value, salida.Value);
            }
        }

        ViewBag.Ingreso = ingreso;
        ViewBag.Salida = salida;
        ViewBag.Noches = noches;
        ViewBag.Hoy = hoy;

        return View(habitaciones);
    }

    // ── CREAR RESERVA ─────────────────────────────────────────────────────────

    // GET: Reservas/Create?idHabitacion=3&ingreso=2026-10-25&salida=2026-10-28
    public async Task<IActionResult> Create(int? idHabitacion, DateOnly? ingreso, DateOnly? salida)
    {
        await CargarDropdownsAsync(idHabitacion);

        var reserva = new Reserva
        {
            IdHabitacion = idHabitacion ?? 0,
            FechaIngreso = ingreso ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            FechaSalida = salida ?? DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Estado = "Pendiente"
        };

        return View(reserva);
    }

    // POST: Reservas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.FechaRegistro));
        ModelState.Remove(nameof(Reserva.Estado));

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Validaciones de fechas
        if (reserva.FechaIngreso < hoy)
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");
        else if (reserva.FechaSalida <= reserva.FechaIngreso)
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior a la de ingreso.");
        else
        {
            // Validar disponibilidad de la habitación
            bool disponible = await _repo.HabitacionDisponibleAsync(
                reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida);

            if (!disponible)
                ModelState.AddModelError(nameof(Reserva.IdHabitacion),
                    "La habitación seleccionada no está disponible para el rango de fechas indicado.");
        }

        if (!ModelState.IsValid)
        {
            await CargarDropdownsAsync(reserva.IdHabitacion, reserva.IdHuesped);
            return View(reserva);
        }

        reserva.Estado = "Pendiente";
        reserva.FechaRegistro = DateTime.Now;
        await _repo.CrearAsync(reserva);

        TempData["Exito"] = "Reserva creada correctamente en estado Pendiente.";
        return RedirectToAction(nameof(Index));
    }

    // ── CANCELAR RESERVA ──────────────────────────────────────────────────────

    // POST: Reservas/Cancelar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _repo.CancelarAsync(id);
        TempData["Exito"] = "Reserva cancelada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task CargarDropdownsAsync(int? idHabitacion = null, int? idHuesped = null)
    {
        var huespedes = await _repo.ObtenerHuespedesActivosAsync();
        var habitaciones = await _repo.ObtenerHabitacionesActivasAsync();

        ViewBag.IdHuesped = new SelectList(huespedes, "IdHuesped",
            // Mostrar "Apellidos, Nombres (DNI)"
            null, idHuesped);

        // Construir SelectList con texto personalizado
        ViewBag.Huespedes = huespedes.Select(h => new SelectListItem
        {
            Value = h.IdHuesped.ToString(),
            Text = $"{h.Apellidos}, {h.Nombres} (DNI: {h.Dni})",
            Selected = h.IdHuesped == idHuesped
        }).ToList();

        ViewBag.Habitaciones = habitaciones.Select(h => new SelectListItem
        {
            Value = h.IdHabitacion.ToString(),
            Text = $"Hab. {h.Numero} - Piso {h.Piso} - {h.IdTipoHabitacionNavigation.Nombre} (S/. {h.IdTipoHabitacionNavigation.PrecioNoche:N2}/noche)",
            Selected = h.IdHabitacion == idHabitacion
        }).ToList();
    }
}
