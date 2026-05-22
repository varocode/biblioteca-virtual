using System;
using BibliotecaAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BibliotecaAPI.Migrations;

[DbContextAttribute(typeof(BibliotecaContext))]
[Migration("20260517214500_AddEjemplaresInventory")]
public partial class AddEjemplaresInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ejemplares",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Disponible"),
                Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Fisico"),
                Ubicacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                LibroId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ejemplares", x => x.Id);
                table.ForeignKey("FK_ejemplares_libros_LibroId", x => x.LibroId, "libros", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.AddColumn<int>(name: "EjemplarId", table: "prestamos", type: "integer", nullable: true);
        migrationBuilder.AddColumn<int>(name: "EjemplarId", table: "reservas", type: "integer", nullable: true);

        migrationBuilder.Sql("""
            INSERT INTO ejemplares ("Codigo", "Estado", "Tipo", "Ubicacion", "FechaRegistro", "LibroId")
            SELECT l."Isbn" || '-' || LPAD(s.indice::text, 3, '0'),
                   CASE WHEN s.indice <= l."Disponibles" THEN 'Disponible' ELSE 'Prestado' END,
                   'Fisico',
                   'Estante ' || (((s.indice - 1) % 5) + 1),
                   NOW() AT TIME ZONE 'UTC',
                   l."Id"
            FROM libros l
            JOIN LATERAL generate_series(1, GREATEST(l."Stock", 0)) AS s(indice) ON TRUE;
            """);

        migrationBuilder.Sql("""
            WITH candidatos AS (
                SELECT p."Id" AS prestamo_id, e."Id" AS ejemplar_id,
                       ROW_NUMBER() OVER (PARTITION BY p."Id" ORDER BY e."Id") AS rn
                FROM prestamos p
                JOIN ejemplares e ON e."LibroId" = p."LibroId"
                WHERE p."Estado" IN ('Activo', 'Vencido') AND e."Estado" <> 'Disponible'
            )
            UPDATE prestamos p
            SET "EjemplarId" = c.ejemplar_id
            FROM candidatos c
            WHERE p."Id" = c.prestamo_id AND c.rn = 1;
            """);

        migrationBuilder.CreateIndex("IX_ejemplares_Codigo", "ejemplares", "Codigo", unique: true);
        migrationBuilder.CreateIndex("IX_ejemplares_LibroId_Estado", "ejemplares", new[] { "LibroId", "Estado" });
        migrationBuilder.CreateIndex("IX_prestamos_EjemplarId", "prestamos", "EjemplarId");
        migrationBuilder.CreateIndex("IX_reservas_EjemplarId", "reservas", "EjemplarId");
        migrationBuilder.AddForeignKey("FK_prestamos_ejemplares_EjemplarId", "prestamos", "EjemplarId", "ejemplares", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_reservas_ejemplares_EjemplarId", "reservas", "EjemplarId", "ejemplares", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_prestamos_ejemplares_EjemplarId", "prestamos");
        migrationBuilder.DropForeignKey("FK_reservas_ejemplares_EjemplarId", "reservas");
        migrationBuilder.DropIndex("IX_prestamos_EjemplarId", "prestamos");
        migrationBuilder.DropIndex("IX_reservas_EjemplarId", "reservas");
        migrationBuilder.DropColumn("EjemplarId", "prestamos");
        migrationBuilder.DropColumn("EjemplarId", "reservas");
        migrationBuilder.DropTable("ejemplares");
    }
}
