using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

/// <summary>
/// Repositorio para operaciones CRUD sobre Habitacion.
/// Todas las consultas son asíncronas y usan AsNoTracking().
/// El borrado es lógico: se establece Activo = false.
/// </summary>
public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    // ── Consultas ────────────────────────────────────────────────────────────

    /// <summary>Lista habitaciones activas ordenadas por número, opcionalmente filtradas por tipo.</summary>
    public async Task<List<Habitacion>> ObtenerActivasAsync(int? idTipo = null)
    {
        var query = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo);

        if (idTipo.HasValue)
            query = query.Where(h => h.IdTipoHabitacion == idTipo.Value);

        return await query.OrderBy(h => h.Numero).ToListAsync();
    }

    /// <summary>Obtiene una habitación por su Id (activa o no, sin tracking).</summary>
    public async Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
    }

    /// <summary>Lista de todos los TipoHabitacion activos para dropdowns.</summary>
    public async Task<List<TipoHabitacion>> ObtenerTiposAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    /// <summary>Verifica si ya existe otra habitación con el mismo número.</summary>
    public async Task<bool> NumeroExisteAsync(string numero, int? excluirId = null)
    {
        var query = _context.Habitacions.AsNoTracking()
            .Where(h => h.Numero == numero);

        if (excluirId.HasValue)
            query = query.Where(h => h.IdHabitacion != excluirId.Value);

        return await query.AnyAsync();
    }

    // ── Persistencia ─────────────────────────────────────────────────────────

    public async Task CrearAsync(Habitacion habitacion)
    {
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Habitacion habitacion)
    {
        _context.Habitacions.Update(habitacion);
        await _context.SaveChangesAsync();
    }

    /// <summary>Borrado lógico: establece Activo = false.</summary>
    public async Task EliminarLogicoAsync(int id)
    {
        var habitacion = await _context.Habitacions.FindAsync(id);
        if (habitacion is not null)
        {
            habitacion.Activo = false;
            await _context.SaveChangesAsync();
        }
    }
}
