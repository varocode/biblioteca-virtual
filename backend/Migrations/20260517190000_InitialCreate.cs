using System;
using BibliotecaAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BibliotecaAPI.Migrations;

[DbContextAttribute(typeof(BibliotecaContext))]
[Migration("20260517190000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "autores",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Nacionalidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Biografia = table.Column<string>(type: "text", nullable: true),
                FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_autores", x => x.Id));

        migrationBuilder.CreateTable(
            name: "categorias",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Descripcion = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_categorias", x => x.Id));

        migrationBuilder.CreateTable(
            name: "usuarios",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Lector"),
                FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Activo = table.Column<bool>(type: "boolean", nullable: false),
                Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Direccion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_usuarios", x => x.Id));

        migrationBuilder.CreateTable(
            name: "libros",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Isbn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Anio = table.Column<int>(type: "integer", nullable: false),
                Editorial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Sinopsis = table.Column<string>(type: "text", nullable: true),
                PortadaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                Stock = table.Column<int>(type: "integer", nullable: false),
                Disponibles = table.Column<int>(type: "integer", nullable: false),
                FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CategoriaId = table.Column<int>(type: "integer", nullable: false),
                AutorId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_libros", x => x.Id);
                table.CheckConstraint("ck_libros_disponibles_validos", "\"Disponibles\" >= 0 AND \"Disponibles\" <= \"Stock\"");
                table.CheckConstraint("ck_libros_stock_no_negativo", "\"Stock\" >= 0");
                table.ForeignKey("FK_libros_autores_AutorId", x => x.AutorId, "autores", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_libros_categorias_CategoriaId", x => x.CategoriaId, "categorias", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "prestamos",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UsuarioId = table.Column<int>(type: "integer", nullable: false),
                LibroId = table.Column<int>(type: "integer", nullable: false),
                FechaPrestamo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                FechaDevolucionEsperada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                FechaDevolucionReal = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                Observaciones = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_prestamos", x => x.Id);
                table.ForeignKey("FK_prestamos_libros_LibroId", x => x.LibroId, "libros", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_prestamos_usuarios_UsuarioId", x => x.UsuarioId, "usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "reservas",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UsuarioId = table.Column<int>(type: "integer", nullable: false),
                LibroId = table.Column<int>(type: "integer", nullable: false),
                FechaReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                PosicionCola = table.Column<int>(type: "integer", nullable: false),
                Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Activa")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_reservas", x => x.Id);
                table.ForeignKey("FK_reservas_libros_LibroId", x => x.LibroId, "libros", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_reservas_usuarios_UsuarioId", x => x.UsuarioId, "usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "multas",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UsuarioId = table.Column<int>(type: "integer", nullable: false),
                PrestamoId = table.Column<int>(type: "integer", nullable: false),
                Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                DiasRetraso = table.Column<int>(type: "integer", nullable: false),
                Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                FechaGeneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_multas", x => x.Id);
                table.CheckConstraint("ck_multas_dias_no_negativo", "\"DiasRetraso\" >= 0");
                table.CheckConstraint("ck_multas_monto_no_negativo", "\"Monto\" >= 0");
                table.ForeignKey("FK_multas_prestamos_PrestamoId", x => x.PrestamoId, "prestamos", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_multas_usuarios_UsuarioId", x => x.UsuarioId, "usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_categorias_Nombre", "categorias", "Nombre", unique: true);
        migrationBuilder.CreateIndex("IX_libros_AutorId", "libros", "AutorId");
        migrationBuilder.CreateIndex("IX_libros_CategoriaId", "libros", "CategoriaId");
        migrationBuilder.CreateIndex("IX_libros_Isbn", "libros", "Isbn", unique: true);
        migrationBuilder.CreateIndex("IX_multas_PrestamoId", "multas", "PrestamoId", unique: true);
        migrationBuilder.CreateIndex("IX_multas_UsuarioId", "multas", "UsuarioId");
        migrationBuilder.CreateIndex("IX_prestamos_LibroId", "prestamos", "LibroId");
        migrationBuilder.CreateIndex("IX_prestamos_UsuarioId", "prestamos", "UsuarioId");
        migrationBuilder.CreateIndex("IX_reservas_LibroId_Estado_PosicionCola", "reservas", new[] { "LibroId", "Estado", "PosicionCola" });
        migrationBuilder.CreateIndex("IX_reservas_UsuarioId", "reservas", "UsuarioId");
        migrationBuilder.CreateIndex("IX_usuarios_Email", "usuarios", "Email", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("multas");
        migrationBuilder.DropTable("reservas");
        migrationBuilder.DropTable("prestamos");
        migrationBuilder.DropTable("libros");
        migrationBuilder.DropTable("usuarios");
        migrationBuilder.DropTable("autores");
        migrationBuilder.DropTable("categorias");
    }
}
