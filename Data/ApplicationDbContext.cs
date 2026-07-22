using Microsoft.EntityFrameworkCore;
using FitwomanAPI.Models;

namespace FitwomanAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Contacto> Contactos { get; set; }
    public DbSet<Plan> Planes { get; set; }
    public DbSet<Profesor> Profesores { get; set; }
    public DbSet<Clase> Clases { get; set; }
    public DbSet<Horario> Horarios { get; set; }
    public DbSet<ClaseHorario> ClasesHorarios { get; set; }
    public DbSet<Miembro> Miembros { get; set; }
    public DbSet<RegistroPeso> RegistrosPesos { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de llave primaria compuesta para la tabla intermedia clases_horarios
        modelBuilder.Entity<ClaseHorario>()
            .HasKey(ch => new { ch.IdClases, ch.IdHorarios });

        // Configuración de relación Producto -> Categoria (1:N)
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuración de relación Clase -> Profesor (1:N)
        modelBuilder.Entity<Clase>()
            .HasOne(c => c.Profesor)
            .WithMany(p => p.Clases)
            .HasForeignKey(c => c.IdProfesor)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuración de relación RegistroPeso -> Miembro (1:N)
        modelBuilder.Entity<RegistroPeso>()
            .HasOne(rp => rp.Miembro)
            .WithMany(m => m.RegistrosPesos)
            .HasForeignKey(rp => rp.IdMiembro)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuración de relación Pago -> Miembro (1:N)
        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Miembro)
            .WithMany(m => m.Pagos)
            .HasForeignKey(p => p.IdMiembro)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuración de relación N:M entre Clase y Horario mediante ClaseHorario
        modelBuilder.Entity<ClaseHorario>()
            .HasOne(ch => ch.Clase)
            .WithMany(c => c.ClasesHorarios)
            .HasForeignKey(ch => ch.IdClases);

        modelBuilder.Entity<ClaseHorario>()
            .HasOne(ch => ch.Horario)
            .WithMany(h => h.ClasesHorarios)
            .HasForeignKey(ch => ch.IdHorarios);
    }
}
