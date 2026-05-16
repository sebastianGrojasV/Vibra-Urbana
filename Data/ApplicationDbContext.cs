using Microsoft.EntityFrameworkCore;
using VibraUrbana.Models;

namespace VibraUrbana.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Permiso> Permisos => Set<Permiso>();

    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(usuario => usuario.Id);

            entity.Property(usuario => usuario.Cedula)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(usuario => usuario.NombreCompleto)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(usuario => usuario.Correo)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(usuario => usuario.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(usuario => usuario.Activo)
                .HasDefaultValue(true);

            entity.Property(usuario => usuario.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(usuario => usuario.Cedula)
                .IsUnique();

            entity.HasIndex(usuario => usuario.Correo)
                .IsUnique();

            entity.HasOne(usuario => usuario.Rol)
                .WithMany(rol => rol.Usuarios)
                .HasForeignKey(usuario => usuario.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(rol => rol.Id);

            entity.Property(rol => rol.Nombre)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(rol => rol.Descripcion)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(rol => rol.Activo)
                .HasDefaultValue(true);

            entity.Property(rol => rol.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(rol => rol.Nombre)
                .IsUnique();
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.ToTable("Permisos");
            entity.HasKey(permiso => permiso.Id);

            entity.Property(permiso => permiso.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(permiso => permiso.Descripcion)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(permiso => permiso.Activo)
                .HasDefaultValue(true);

            entity.HasIndex(permiso => permiso.Nombre)
                .IsUnique();
        });

        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.ToTable("RolPermisos");
            entity.HasKey(rolPermiso => new { rolPermiso.RolId, rolPermiso.PermisoId });

            entity.HasOne(rolPermiso => rolPermiso.Rol)
                .WithMany(rol => rol.RolPermisos)
                .HasForeignKey(rolPermiso => rolPermiso.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rolPermiso => rolPermiso.Permiso)
                .WithMany(permiso => permiso.RolPermisos)
                .HasForeignKey(rolPermiso => rolPermiso.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
