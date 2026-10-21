using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Models;

[Table("Habitacion")]
[Index("Numero", Name = "UQ_Habitacion_Numero", IsUnique = true)]
public partial class Habitacion
{
    [Key]
    public int IdHabitacion { get; set; }

    [StringLength(10)]
    public string Numero { get; set; } = null!;

    public int Piso { get; set; }

    public int IdTipoHabitacion { get; set; }

    public bool Activo { get; set; }

    [ForeignKey("IdTipoHabitacion")]
    [InverseProperty("Habitacions")]
    public virtual TipoHabitacion IdTipoHabitacionNavigation { get; set; } = null!;

    [InverseProperty("IdHabitacionNavigation")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
