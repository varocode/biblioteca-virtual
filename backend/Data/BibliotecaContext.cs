using BibliotecaAPI.Models.Entities;
using BibliotecaAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Data;

public class BibliotecaContext(DbContextOptions<BibliotecaContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Ejemplar> Ejemplares => Set<Ejemplar>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Multa> Multas => Set<Multa>();
    public DbSet<PagoMulta> PagosMulta => Set<PagoMulta>();
    public DbSet<IntentoPago> IntentosPago => Set<IntentoPago>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    public override int SaveChanges()
    {
        ProtegerAuditoriaAppendOnly();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ProtegerAuditoriaAppendOnly();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigurarUsuarios(modelBuilder);
        ConfigurarCatalogo(modelBuilder);
        ConfigurarCirculacion(modelBuilder);
    }

    private static void ConfigurarUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasIndex(usuario => usuario.Email).IsUnique();
            entity.Property(usuario => usuario.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(usuario => usuario.Email).HasMaxLength(150).IsRequired();
            entity.Property(usuario => usuario.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(usuario => usuario.Rol).HasConversion<string>().HasMaxLength(20).HasDefaultValue(RolUsuario.Lector);
            entity.Property(usuario => usuario.Telefono).HasMaxLength(20);
            entity.Property(usuario => usuario.Direccion).HasMaxLength(255);
        });
    }

    private static void ConfigurarCatalogo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasIndex(categoria => categoria.Nombre).IsUnique();
            entity.Property(categoria => categoria.Nombre).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Autor>(entity =>
        {
            entity.ToTable("autores");
            entity.Property(autor => autor.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(autor => autor.Nacionalidad).HasMaxLength(50);
        });

        modelBuilder.Entity<Libro>(entity =>
        {
            entity.ToTable("libros");
            entity.HasIndex(libro => libro.Isbn).IsUnique();
            entity.Property(libro => libro.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(libro => libro.Isbn).HasMaxLength(20).IsRequired();
            entity.Property(libro => libro.Editorial).HasMaxLength(100);
            entity.Property(libro => libro.PortadaUrl).HasMaxLength(500);
            entity.ToTable(tabla =>
            {
                tabla.HasCheckConstraint("ck_libros_stock_no_negativo", "\"Stock\" >= 0");
                tabla.HasCheckConstraint("ck_libros_disponibles_validos", "\"Disponibles\" >= 0 AND \"Disponibles\" <= \"Stock\"");
            });
        });

        modelBuilder.Entity<Ejemplar>(entity =>
        {
            entity.ToTable("ejemplares");
            entity.HasIndex(ejemplar => ejemplar.Codigo).IsUnique();
            entity.HasIndex(ejemplar => new { ejemplar.LibroId, ejemplar.Estado });
            entity.Property(ejemplar => ejemplar.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(ejemplar => ejemplar.Estado).HasConversion<string>().HasMaxLength(20).HasDefaultValue(EstadoEjemplar.Disponible);
            entity.Property(ejemplar => ejemplar.Tipo).HasConversion<string>().HasMaxLength(20).HasDefaultValue(TipoEjemplar.Fisico);
            entity.Property(ejemplar => ejemplar.Ubicacion).HasMaxLength(100);
        });
    }

    private static void ConfigurarCirculacion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.ToTable("prestamos");
            entity.Property(prestamo => prestamo.Estado).HasConversion<string>().HasMaxLength(20).HasDefaultValue(EstadoPrestamo.Activo);
            entity.HasOne(prestamo => prestamo.Ejemplar).WithMany(ejemplar => ejemplar.Prestamos).HasForeignKey(prestamo => prestamo.EjemplarId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("reservas");
            entity.HasIndex(reserva => new { reserva.LibroId, reserva.Estado, reserva.PosicionCola });
            entity.Property(reserva => reserva.Estado).HasConversion<string>().HasMaxLength(20).HasDefaultValue(EstadoReserva.Activa);
            entity.HasOne(reserva => reserva.Ejemplar).WithMany(ejemplar => ejemplar.Reservas).HasForeignKey(reserva => reserva.EjemplarId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Multa>(entity =>
        {
            entity.ToTable("multas");
            entity.HasIndex(multa => multa.PrestamoId).IsUnique();
            entity.Property(multa => multa.Monto).HasPrecision(10, 2);
            entity.Property(multa => multa.Estado).HasConversion<string>().HasMaxLength(20).HasDefaultValue(EstadoMulta.Pendiente);
            entity.ToTable(tabla =>
            {
                tabla.HasCheckConstraint("ck_multas_monto_no_negativo", "\"Monto\" >= 0");
                tabla.HasCheckConstraint("ck_multas_dias_no_negativo", "\"DiasRetraso\" >= 0");
            });
        });

        modelBuilder.Entity<IntentoPago>(entity =>
        {
            entity.ToTable("intentos_pago");
            entity.HasIndex(intento => intento.Referencia).IsUnique();
            entity.HasIndex(intento => new { intento.MultaId, intento.Estado });
            entity.Property(intento => intento.Monto).HasPrecision(10, 2);
            entity.Property(intento => intento.Estado).HasConversion<string>().HasMaxLength(20).HasDefaultValue(EstadoIntentoPago.Creado);
            entity.Property(intento => intento.Referencia).HasMaxLength(80).IsRequired();
            entity.Property(intento => intento.MotivoRechazo).HasMaxLength(200);
        });

        modelBuilder.Entity<PagoMulta>(entity =>
        {
            entity.ToTable("pagos_multa");
            entity.HasIndex(pago => pago.MultaId).IsUnique();
            entity.HasIndex(pago => pago.Referencia).IsUnique();
            entity.Property(pago => pago.Monto).HasPrecision(10, 2);
            entity.Property(pago => pago.Referencia).HasMaxLength(80).IsRequired();
            entity.Property(pago => pago.Recibo).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("audit_events");
            entity.HasIndex(audit => new { audit.Entidad, audit.EntidadId });
            entity.HasIndex(audit => audit.Fecha);
            entity.Property(audit => audit.Accion).HasMaxLength(80).IsRequired();
            entity.Property(audit => audit.Entidad).HasMaxLength(80).IsRequired();
            entity.Property(audit => audit.EntidadId).HasMaxLength(80).IsRequired();
            entity.Property(audit => audit.Resultado).HasMaxLength(40).IsRequired();
            entity.Property(audit => audit.Detalle).HasMaxLength(500);
            entity.HasOne(audit => audit.ActorUsuario).WithMany().HasForeignKey(audit => audit.ActorUsuarioId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("notificaciones");
            entity.HasIndex(notificacion => new { notificacion.UsuarioId, notificacion.Fecha });
            entity.Property(notificacion => notificacion.Tipo).HasConversion<string>().HasMaxLength(20).HasDefaultValue(TipoNotificacion.Prestamo);
            entity.Property(notificacion => notificacion.Titulo).HasMaxLength(120).IsRequired();
            entity.Property(notificacion => notificacion.Mensaje).HasMaxLength(500).IsRequired();
            entity.Property(notificacion => notificacion.Referencia).HasMaxLength(80);
        });
    }

    private void ProtegerAuditoriaAppendOnly()
    {
        if (ChangeTracker.Entries<AuditEvent>().Any(entry => entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException("Los eventos de auditoría son append-only y no se pueden modificar ni eliminar.");
        }
    }
}
