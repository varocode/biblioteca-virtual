using System;
using BibliotecaAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BibliotecaAPI.Migrations;

[DbContextAttribute(typeof(BibliotecaContext))]
[Migration("20260517223000_AddPaymentsAuditNotifications")]
public partial class AddPaymentsAuditNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "audit_events",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ActorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                Accion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Entidad = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                EntidadId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Resultado = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                Detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_audit_events", x => x.Id);
                table.ForeignKey("FK_audit_events_usuarios_ActorUsuarioId", x => x.ActorUsuarioId, "usuarios", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "intentos_pago",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                MultaId = table.Column<int>(type: "integer", nullable: false),
                UsuarioId = table.Column<int>(type: "integer", nullable: false),
                Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Creado"),
                Referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                MotivoRechazo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_intentos_pago", x => x.Id);
                table.ForeignKey("FK_intentos_pago_multas_MultaId", x => x.MultaId, "multas", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_intentos_pago_usuarios_UsuarioId", x => x.UsuarioId, "usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "notificaciones",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UsuarioId = table.Column<int>(type: "integer", nullable: false),
                Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Prestamo"),
                Titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_notificaciones", x => x.Id);
                table.ForeignKey("FK_notificaciones_usuarios_UsuarioId", x => x.UsuarioId, "usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "pagos_multa",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                MultaId = table.Column<int>(type: "integer", nullable: false),
                UsuarioId = table.Column<int>(type: "integer", nullable: false),
                Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                Referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Recibo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_pagos_multa", x => x.Id);
                table.ForeignKey("FK_pagos_multa_multas_MultaId", x => x.MultaId, "multas", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_pagos_multa_usuarios_UsuarioId", x => x.UsuarioId, "usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_audit_events_ActorUsuarioId", "audit_events", "ActorUsuarioId");
        migrationBuilder.CreateIndex("IX_audit_events_Entidad_EntidadId", "audit_events", new[] { "Entidad", "EntidadId" });
        migrationBuilder.CreateIndex("IX_audit_events_Fecha", "audit_events", "Fecha");
        migrationBuilder.CreateIndex("IX_intentos_pago_MultaId_Estado", "intentos_pago", new[] { "MultaId", "Estado" });
        migrationBuilder.CreateIndex("IX_intentos_pago_Referencia", "intentos_pago", "Referencia", unique: true);
        migrationBuilder.CreateIndex("IX_intentos_pago_UsuarioId", "intentos_pago", "UsuarioId");
        migrationBuilder.CreateIndex("IX_notificaciones_UsuarioId_Fecha", "notificaciones", new[] { "UsuarioId", "Fecha" });
        migrationBuilder.CreateIndex("IX_pagos_multa_MultaId", "pagos_multa", "MultaId", unique: true);
        migrationBuilder.CreateIndex("IX_pagos_multa_Referencia", "pagos_multa", "Referencia", unique: true);
        migrationBuilder.CreateIndex("IX_pagos_multa_UsuarioId", "pagos_multa", "UsuarioId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("audit_events");
        migrationBuilder.DropTable("intentos_pago");
        migrationBuilder.DropTable("notificaciones");
        migrationBuilder.DropTable("pagos_multa");
    }
}
