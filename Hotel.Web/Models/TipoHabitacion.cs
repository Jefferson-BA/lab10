using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Models;

[Table("TipoHabitacion")]
[Index("Nombre", Name = "UQ_TipoHabitacion_Nombre", IsUnique = true)]
public partial class TipoHabitacion
{
    [Key]
    public int IdTipoHabitacion { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    public int Capacidad { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal PrecioNoche { get; set; }

    public bool Activo { get; set; }

    [InverseProperty("IdTipoHabitacionNavigation")]
    public virtual ICollection<Habitacion> Habitacions { get; set; } = new List<Habitacion>();
}
