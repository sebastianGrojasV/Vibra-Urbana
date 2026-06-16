using Microsoft.EntityFrameworkCore;
using VibraUrbana.Models;

namespace VibraUrbana.Data;

public class ApplicationDbContext : DbContext
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Permiso> Permisos => Set<Permiso>();

    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    public DbSet<Bitacora> Bitacora => Set<Bitacora>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Inventario> Inventario => Set<Inventario>();

    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    public DbSet<Venta> Ventas => Set<Venta>();

    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();

    public DbSet<Factura> Facturas => Set<Factura>();

    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();

    public DbSet<CierreCaja> CierresCaja => Set<CierreCaja>();

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<DetallePedido> DetallePedidos => Set<DetallePedido>();

    public DbSet<ComprobantePago> ComprobantesPago => Set<ComprobantePago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureSeguridad(modelBuilder);
        ConfigureClientes(modelBuilder);
        ConfigureProductosInventario(modelBuilder);
        ConfigureVentasFacturacion(modelBuilder);
        ConfigurePedidos(modelBuilder);
        ConfigureBitacora(modelBuilder);
        ConfigureSeed(modelBuilder);
    }

    private static void ConfigureSeguridad(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(usuario => usuario.Id);

            entity.Property(usuario => usuario.Cedula).IsRequired().HasMaxLength(20);
            entity.Property(usuario => usuario.NombreCompleto).IsRequired().HasMaxLength(150);
            entity.Property(usuario => usuario.Correo).IsRequired().HasMaxLength(150);
            entity.Property(usuario => usuario.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(usuario => usuario.Activo).HasDefaultValue(true);
            entity.Property(usuario => usuario.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(usuario => usuario.Cedula).IsUnique();
            entity.HasIndex(usuario => usuario.Correo).IsUnique();

            entity.HasOne(usuario => usuario.Rol)
                .WithMany(rol => rol.Usuarios)
                .HasForeignKey(usuario => usuario.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(rol => rol.Id);

            entity.Property(rol => rol.Nombre).IsRequired().HasMaxLength(80);
            entity.Property(rol => rol.Descripcion).IsRequired().HasMaxLength(250);
            entity.Property(rol => rol.Activo).HasDefaultValue(true);
            entity.Property(rol => rol.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(rol => rol.Nombre).IsUnique();
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.ToTable("Permisos");
            entity.HasKey(permiso => permiso.Id);

            entity.Property(permiso => permiso.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(permiso => permiso.Descripcion).IsRequired().HasMaxLength(250);
            entity.Property(permiso => permiso.Activo).HasDefaultValue(true);

            entity.HasIndex(permiso => permiso.Nombre).IsUnique();
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

    private static void ConfigureClientes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(cliente => cliente.Id);

            entity.Property(cliente => cliente.Identificacion).IsRequired().HasMaxLength(30);
            entity.Property(cliente => cliente.NombreCompleto).IsRequired().HasMaxLength(150);
            entity.Property(cliente => cliente.Telefono).HasMaxLength(30);
            entity.Property(cliente => cliente.Correo).HasMaxLength(150);
            entity.Property(cliente => cliente.Direccion).HasMaxLength(300);
            entity.Property(cliente => cliente.Activo).HasDefaultValue(true);
            entity.Property(cliente => cliente.FechaRegistro).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(cliente => cliente.Identificacion).IsUnique();
        });
    }

    private static void ConfigureProductosInventario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(categoria => categoria.Id);

            entity.Property(categoria => categoria.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(categoria => categoria.Descripcion).HasMaxLength(250);
            entity.Property(categoria => categoria.Activo).HasDefaultValue(true);

            entity.HasIndex(categoria => categoria.Nombre).IsUnique();
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos");
            entity.HasKey(producto => producto.Id);

            entity.Property(producto => producto.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(producto => producto.Descripcion).HasMaxLength(500);
            entity.Property(producto => producto.Precio).HasPrecision(18, 2);
            entity.Property(producto => producto.Talla).HasMaxLength(20);
            entity.Property(producto => producto.Color).HasMaxLength(50);
            entity.Property(producto => producto.ImagenUrl).HasMaxLength(500);
            entity.Property(producto => producto.Activo).HasDefaultValue(true);
            entity.Property(producto => producto.FechaRegistro).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(producto => producto.Categoria)
                .WithMany(categoria => categoria.Productos)
                .HasForeignKey(producto => producto.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.ToTable("Inventario");
            entity.HasKey(inventario => inventario.Id);

            entity.Property(inventario => inventario.FechaActualizacion).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(inventario => inventario.Version).IsRowVersion();

            entity.HasIndex(inventario => inventario.ProductoId).IsUnique();

            entity.HasOne(inventario => inventario.Producto)
                .WithOne(producto => producto.Inventario)
                .HasForeignKey<Inventario>(inventario => inventario.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.ToTable("MovimientosInventario");
            entity.HasKey(movimiento => movimiento.Id);

            entity.Property(movimiento => movimiento.TipoMovimiento).IsRequired().HasMaxLength(30);
            entity.Property(movimiento => movimiento.Motivo).HasMaxLength(300);
            entity.Property(movimiento => movimiento.FechaMovimiento).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(movimiento => movimiento.Producto)
                .WithMany(producto => producto.MovimientosInventario)
                .HasForeignKey(movimiento => movimiento.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(movimiento => movimiento.Usuario)
                .WithMany(usuario => usuario.MovimientosInventario)
                .HasForeignKey(movimiento => movimiento.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureVentasFacturacion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.ToTable("MetodosPago");
            entity.HasKey(metodoPago => metodoPago.Id);

            entity.Property(metodoPago => metodoPago.Nombre).IsRequired().HasMaxLength(80);
            entity.Property(metodoPago => metodoPago.Activo).HasDefaultValue(true);

            entity.HasIndex(metodoPago => metodoPago.Nombre).IsUnique();
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("Ventas");
            entity.HasKey(venta => venta.Id);

            entity.Property(venta => venta.FechaVenta).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(venta => venta.Subtotal).HasPrecision(18, 2);
            entity.Property(venta => venta.Descuento).HasPrecision(18, 2);
            entity.Property(venta => venta.Impuesto).HasPrecision(18, 2);
            entity.Property(venta => venta.Total).HasPrecision(18, 2);
            entity.Property(venta => venta.Estado).IsRequired().HasMaxLength(40);
            entity.Property(venta => venta.Observacion).HasMaxLength(500);

            entity.HasOne(venta => venta.Cliente)
                .WithMany(cliente => cliente.Ventas)
                .HasForeignKey(venta => venta.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(venta => venta.Usuario)
                .WithMany(usuario => usuario.Ventas)
                .HasForeignKey(venta => venta.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(venta => venta.MetodoPago)
                .WithMany(metodoPago => metodoPago.Ventas)
                .HasForeignKey(venta => venta.MetodoPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(venta => venta.CierreCaja)
                .WithMany(cierreCaja => cierreCaja.Ventas)
                .HasForeignKey(venta => venta.CierreCajaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.ToTable("DetalleVentas");
            entity.HasKey(detalle => detalle.Id);

            entity.Property(detalle => detalle.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(detalle => detalle.Subtotal).HasPrecision(18, 2);

            entity.HasOne(detalle => detalle.Venta)
                .WithMany(venta => venta.DetalleVentas)
                .HasForeignKey(detalle => detalle.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(detalle => detalle.Producto)
                .WithMany(producto => producto.DetalleVentas)
                .HasForeignKey(detalle => detalle.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.ToTable("Facturas");
            entity.HasKey(factura => factura.Id);

            entity.Property(factura => factura.NumeroFactura).IsRequired().HasMaxLength(50);
            entity.Property(factura => factura.FechaEmision).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(factura => factura.Subtotal).HasPrecision(18, 2);
            entity.Property(factura => factura.Descuento).HasPrecision(18, 2);
            entity.Property(factura => factura.Impuesto).HasPrecision(18, 2);
            entity.Property(factura => factura.Total).HasPrecision(18, 2);
            entity.Property(factura => factura.Estado).IsRequired().HasMaxLength(40);

            entity.HasIndex(factura => factura.NumeroFactura).IsUnique();
            entity.HasIndex(factura => factura.VentaId).IsUnique();

            entity.HasOne(factura => factura.Venta)
                .WithOne(venta => venta.Factura)
                .HasForeignKey<Factura>(factura => factura.VentaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CierreCaja>(entity =>
        {
            entity.ToTable("CierresCaja");
            entity.HasKey(cierre => cierre.Id);

            entity.Property(cierre => cierre.FechaCierre).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(cierre => cierre.TotalVentas).HasPrecision(18, 2);
            entity.Property(cierre => cierre.TotalEfectivo).HasPrecision(18, 2);
            entity.Property(cierre => cierre.TotalSinpe).HasPrecision(18, 2);
            entity.Property(cierre => cierre.TotalTarjeta).HasPrecision(18, 2);
            entity.Property(cierre => cierre.Observacion).HasMaxLength(500);

            entity.HasOne(cierre => cierre.Usuario)
                .WithMany(usuario => usuario.CierresCaja)
                .HasForeignKey(cierre => cierre.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePedidos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedidos");
            entity.HasKey(pedido => pedido.Id);

            entity.Property(pedido => pedido.NombreCliente).IsRequired().HasMaxLength(150);
            entity.Property(pedido => pedido.Telefono).IsRequired().HasMaxLength(30);
            entity.Property(pedido => pedido.Correo).IsRequired().HasMaxLength(150);
            entity.Property(pedido => pedido.DireccionEntrega).IsRequired().HasMaxLength(300);
            entity.Property(pedido => pedido.FechaPedido).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(pedido => pedido.Subtotal).HasPrecision(18, 2);
            entity.Property(pedido => pedido.Descuento).HasPrecision(18, 2);
            entity.Property(pedido => pedido.Impuesto).HasPrecision(18, 2);
            entity.Property(pedido => pedido.Total).HasPrecision(18, 2);
            entity.Property(pedido => pedido.Estado).IsRequired().HasMaxLength(40);
            entity.Property(pedido => pedido.Observacion).HasMaxLength(500);

            entity.HasOne(pedido => pedido.Cliente)
                .WithMany(cliente => cliente.Pedidos)
                .HasForeignKey(pedido => pedido.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(pedido => pedido.MetodoPago)
                .WithMany(metodoPago => metodoPago.Pedidos)
                .HasForeignKey(pedido => pedido.MetodoPagoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.ToTable("DetallePedidos");
            entity.HasKey(detalle => detalle.Id);

            entity.Property(detalle => detalle.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(detalle => detalle.Subtotal).HasPrecision(18, 2);

            entity.HasOne(detalle => detalle.Pedido)
                .WithMany(pedido => pedido.DetallePedidos)
                .HasForeignKey(detalle => detalle.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(detalle => detalle.Producto)
                .WithMany(producto => producto.DetallePedidos)
                .HasForeignKey(detalle => detalle.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComprobantePago>(entity =>
        {
            entity.ToTable("ComprobantesPago");
            entity.HasKey(comprobante => comprobante.Id);

            entity.Property(comprobante => comprobante.ReferenciaPago).IsRequired().HasMaxLength(100);
            entity.Property(comprobante => comprobante.ImagenComprobanteUrl).HasMaxLength(500);
            entity.Property(comprobante => comprobante.FechaRegistro).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(comprobante => comprobante.EstadoVerificacion).IsRequired().HasMaxLength(40);

            entity.HasIndex(comprobante => comprobante.PedidoId).IsUnique();

            entity.HasOne(comprobante => comprobante.Pedido)
                .WithOne(pedido => pedido.ComprobantePago)
                .HasForeignKey<ComprobantePago>(comprobante => comprobante.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(comprobante => comprobante.MetodoPago)
                .WithMany(metodoPago => metodoPago.ComprobantesPago)
                .HasForeignKey(comprobante => comprobante.MetodoPagoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBitacora(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bitacora>(entity =>
        {
            entity.ToTable("Bitacora");
            entity.HasKey(bitacora => bitacora.Id);

            entity.Property(bitacora => bitacora.Modulo).IsRequired().HasMaxLength(80);
            entity.Property(bitacora => bitacora.Accion).IsRequired().HasMaxLength(80);
            entity.Property(bitacora => bitacora.Descripcion).HasMaxLength(1000);
            entity.Property(bitacora => bitacora.FechaRegistro).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(bitacora => bitacora.Usuario)
                .WithMany(usuario => usuario.Bitacoras)
                .HasForeignKey(bitacora => bitacora.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureSeed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Administrador", Descripcion = "Acceso administrativo general", Activo = true, FechaCreacion = SeedDate },
            new Rol { Id = 2, Nombre = "Cajero", Descripcion = "Gestion de ventas y facturacion", Activo = true, FechaCreacion = SeedDate },
            new Rol { Id = 3, Nombre = "Inventario", Descripcion = "Gestion de productos e inventario", Activo = true, FechaCreacion = SeedDate },
            new Rol { Id = 4, Nombre = "Consulta", Descripcion = "Acceso de solo consulta", Activo = true, FechaCreacion = SeedDate });

        modelBuilder.Entity<Permiso>().HasData(
            new Permiso { Id = 1, Nombre = "Usuarios.Gestionar", Descripcion = "Gestionar usuarios", Activo = true },
            new Permiso { Id = 2, Nombre = "Roles.Gestionar", Descripcion = "Gestionar roles y permisos", Activo = true },
            new Permiso { Id = 3, Nombre = "Clientes.Gestionar", Descripcion = "Gestionar clientes", Activo = true },
            new Permiso { Id = 4, Nombre = "Productos.Gestionar", Descripcion = "Gestionar productos", Activo = true },
            new Permiso { Id = 5, Nombre = "Inventario.Gestionar", Descripcion = "Gestionar inventario", Activo = true },
            new Permiso { Id = 6, Nombre = "Ventas.Gestionar", Descripcion = "Gestionar ventas", Activo = true },
            new Permiso { Id = 7, Nombre = "Facturacion.Gestionar", Descripcion = "Gestionar facturacion", Activo = true },
            new Permiso { Id = 8, Nombre = "Reportes.Ver", Descripcion = "Ver reportes", Activo = true });

        modelBuilder.Entity<RolPermiso>().HasData(
            new RolPermiso { RolId = 1, PermisoId = 1 },
            new RolPermiso { RolId = 1, PermisoId = 2 },
            new RolPermiso { RolId = 1, PermisoId = 3 },
            new RolPermiso { RolId = 1, PermisoId = 4 },
            new RolPermiso { RolId = 1, PermisoId = 5 },
            new RolPermiso { RolId = 1, PermisoId = 6 },
            new RolPermiso { RolId = 1, PermisoId = 7 },
            new RolPermiso { RolId = 1, PermisoId = 8 },
            new RolPermiso { RolId = 2, PermisoId = 6 },
            new RolPermiso { RolId = 2, PermisoId = 7 },
            new RolPermiso { RolId = 3, PermisoId = 4 },
            new RolPermiso { RolId = 3, PermisoId = 5 },
            new RolPermiso { RolId = 4, PermisoId = 8 });

        modelBuilder.Entity<MetodoPago>().HasData(
            new MetodoPago { Id = 1, Nombre = "Efectivo", Activo = true },
            new MetodoPago { Id = 2, Nombre = "SINPE", Activo = true },
            new MetodoPago { Id = 3, Nombre = "Tarjeta", Activo = true });
    }
}
