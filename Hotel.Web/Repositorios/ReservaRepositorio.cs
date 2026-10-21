using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

/// <summary>
/// Repositorio para operaciones sobre Reserva.
/// Todas las consultas son asíncronas y usan AsNoTracking().
/// </summary>
public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    // ── Consultas ────────────────────────────────────────────────────────────

    /// <summary>Listado completo de reservas con sus entidades relacionadas.</summary>
    public async Task<List<Reserva>> ObtenerTodasAsync()
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();
    }

    /// <summary>Obtiene una reserva por Id.</summary>
    public async Task<Reserva?> ObtenerPorIdAsync(int id)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(r => r.IdReserva == id);
    }

    /// <summary>
    /// Habitaciones activas disponibles en el rango [ingreso, salida).
    /// Una habitación está ocupada si tiene reservas cuyo estado NO es "Cancelada"
    /// y el rango se cruza: FechaIngreso &lt; salida AND FechaSalida &gt; ingreso.
    /// Se resuelve en una sola consulta LINQ mediante !Any.
    /// </summary>
    public async Task<List<Habitacion>> ObtenerDisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo &&
                        !h.Reservas.Any(r =>
                            r.Estado != "Cancelada" &&
                            r.FechaIngreso < salida &&
                            r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    /// <summary>Lista de huéspedes activos para dropdowns.</summary>
    public async Task<List<Huesped>> ObtenerHuespedesActivosAsync()
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos).ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    /// <summary>Lista de habitaciones activas para dropdowns.</summary>
    public async Task<List<Habitacion>> ObtenerHabitacionesActivasAsync()
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si la habitación está disponible para el rango indicado,
    /// excluyendo opcionalmente una reserva existente (útil en edición).
    /// </summary>
    public async Task<bool> HabitacionDisponibleAsync(int idHabitacion, DateOnly ingreso, DateOnly salida, int? excluirIdReserva = null)
    {
        var query = _context.Reservas
            .AsNoTracking()
            .Where(r => r.IdHabitacion == idHabitacion &&
                        r.Estado != "Cancelada" &&
                        r.FechaIngreso < salida &&
                        r.FechaSalida > ingreso);

        if (excluirIdReserva.HasValue)
            query = query.Where(r => r.IdReserva != excluirIdReserva.Value);

        return !await query.AnyAsync();
    }

    // ── Persistencia ─────────────────────────────────────────────────────────

    public async Task CrearAsync(Reserva reserva)
    {
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
    }

    /// <summary>Cancela una reserva estableciendo Estado = "Cancelada".</summary>
    public async Task CancelarAsync(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is not null && (reserva.Estado == "Pendiente" || reserva.Estado == "Confirmada"))
        {
            reserva.Estado = "Cancelada";
            await _context.SaveChangesAsync();
        }
    }
}
