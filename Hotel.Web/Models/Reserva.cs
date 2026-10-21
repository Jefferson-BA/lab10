using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Models;

[Table("Reserva")]
[Index("IdHabitacion", "FechaIngreso", "FechaSalida", Name = "IX_Reserva_Habitacion_Fechas")]
public partial class Reserva
{
    [Key]
    public int IdReserva { get; set; }

    public int IdHuesped { get; set; }

    public int IdHabitacion { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public DateOnly FechaSalida { get; set; }

    [StringLength(15)]
    public string Estado { get; set; } = null!;

    [Precision(0)]
    public DateTime FechaRegistro { get; set; }

    [StringLength(200)]
    public string? Observaciones { get; set; }

    [ForeignKey("IdHabitacion")]
    [InverseProperty("Reservas")]
    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    [ForeignKey("IdHuesped")]
    [InverseProperty("Reservas")]
    public virtual Huesped IdHuespedNavigation { get; set; } = null!;
}
