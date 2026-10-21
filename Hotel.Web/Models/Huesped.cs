using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Models;

[Table("Huesped")]
[Index("Dni", Name = "UQ_Huesped_Dni", IsUnique = true)]
public partial class Huesped
{
    [Key]
    public int IdHuesped { get; set; }

    [StringLength(8)]
    [Unicode(false)]
    public string Dni { get; set; } = null!;

    [StringLength(100)]
    public string Nombres { get; set; } = null!;

    [StringLength(100)]
    public string Apellidos { get; set; } = null!;

    [StringLength(120)]
    public string? Correo { get; set; }

    [StringLength(15)]
    public string? Telefono { get; set; }

    public bool Activo { get; set; }

    [InverseProperty("IdHuespedNavigation")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
