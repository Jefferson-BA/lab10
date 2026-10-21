using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Models;

public partial class HotelContext : DbContext
{
    public HotelContext(DbContextOptions<HotelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Habitacion> Habitacions { get; set; }

    public virtual DbSet<Huesped> Huespeds { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<TipoHabitacion> TipoHabitacions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habitacion>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Habitacion_Activo");

            entity.HasOne(d => d.IdTipoHabitacionNavigation).WithMany(p => p.Habitacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Habitacion_TipoHabitacion");
        });

        modelBuilder.Entity<Huesped>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Huesped_Activo");
            entity.Property(e => e.Dni).IsFixedLength();
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue("Pendiente", "DF_Reserva_Estado");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())", "DF_Reserva_FechaRegistro");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Reservas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Habitacion");

            entity.HasOne(d => d.IdHuespedNavigation).WithMany(p => p.Reservas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Huesped");
        });

        modelBuilder.Entity<TipoHabitacion>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_TipoHabitacion_Activo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
