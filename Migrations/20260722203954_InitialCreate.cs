using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FitwomanAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id_categoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "contacto",
                columns: table => new
                {
                    id_contacto = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teléfono = table.Column<string>(type: "text", nullable: true),
                    correo = table.Column<string>(type: "text", nullable: true),
                    dirección = table.Column<string>(type: "text", nullable: true),
                    ciudad = table.Column<string>(type: "text", nullable: true),
                    pais = table.Column<string>(type: "text", nullable: true),
                    url_google_maps = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacto", x => x.id_contacto);
                });

            migrationBuilder.CreateTable(
                name: "horarios",
                columns: table => new
                {
                    id_horarios = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dia_semana = table.Column<string>(type: "text", nullable: false),
                    hora_inicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_fin = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarios", x => x.id_horarios);
                });

            migrationBuilder.CreateTable(
                name: "miembro",
                columns: table => new
                {
                    id_miembro = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellido = table.Column<string>(type: "text", nullable: false),
                    correo = table.Column<string>(type: "text", nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    plan = table.Column<long>(type: "bigint", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_miembro", x => x.id_miembro);
                });

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    id_notificaciones = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<bool>(type: "boolean", nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: false),
                    leida = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    enlace_referencia = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones", x => x.id_notificaciones);
                });

            migrationBuilder.CreateTable(
                name: "planes",
                columns: table => new
                {
                    id_planes = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    precio = table.Column<double>(type: "double precision", nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false),
                    destacado = table.Column<bool>(type: "boolean", nullable: false),
                    mensaje_whatsapp = table.Column<string>(type: "text", nullable: true),
                    items_incluidos = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planes", x => x.id_planes);
                });

            migrationBuilder.CreateTable(
                name: "profesores",
                columns: table => new
                {
                    id_profesores = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellido = table.Column<string>(type: "text", nullable: false),
                    fecha_de_nacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profesores", x => x.id_profesores);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuarios = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellido = table.Column<string>(type: "text", nullable: false),
                    correo = table.Column<string>(type: "text", nullable: false),
                    contraseña = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuarios);
                });

            migrationBuilder.CreateTable(
                name: "producto",
                columns: table => new
                {
                    id_producto = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    precio = table.Column<decimal>(type: "numeric", nullable: false),
                    tallas = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    visibilidad = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    imagen = table.Column<string>(type: "text", nullable: true),
                    id_categoria = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_producto_categoria_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categoria",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    id_pagos = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mes_facturado = table.Column<string>(type: "text", nullable: false),
                    monto = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: false),
                    id_miembro = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.id_pagos);
                    table.ForeignKey(
                        name: "FK_pagos_miembro_id_miembro",
                        column: x => x.id_miembro,
                        principalTable: "miembro",
                        principalColumn: "id_miembro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registro_pesos",
                columns: table => new
                {
                    id_registro_pesos = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    peso = table.Column<decimal>(type: "numeric", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_miembro = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_pesos", x => x.id_registro_pesos);
                    table.ForeignKey(
                        name: "FK_registro_pesos_miembro_id_miembro",
                        column: x => x.id_miembro,
                        principalTable: "miembro",
                        principalColumn: "id_miembro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clases",
                columns: table => new
                {
                    id_clases = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    modalidad = table.Column<string>(type: "text", nullable: false),
                    duración = table.Column<int>(type: "integer", nullable: false),
                    nivel = table.Column<string>(type: "text", nullable: false),
                    cupos = table.Column<int>(type: "integer", nullable: false),
                    descripción = table.Column<string>(type: "text", nullable: true),
                    id_profesor = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clases", x => x.id_clases);
                    table.ForeignKey(
                        name: "FK_clases_profesores_id_profesor",
                        column: x => x.id_profesor,
                        principalTable: "profesores",
                        principalColumn: "id_profesores",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clases_horarios",
                columns: table => new
                {
                    id_clases = table.Column<long>(type: "bigint", nullable: false),
                    id_horarios = table.Column<long>(type: "bigint", nullable: false),
                    aula = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clases_horarios", x => new { x.id_clases, x.id_horarios });
                    table.ForeignKey(
                        name: "FK_clases_horarios_clases_id_clases",
                        column: x => x.id_clases,
                        principalTable: "clases",
                        principalColumn: "id_clases",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clases_horarios_horarios_id_horarios",
                        column: x => x.id_horarios,
                        principalTable: "horarios",
                        principalColumn: "id_horarios",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clases_id_profesor",
                table: "clases",
                column: "id_profesor");

            migrationBuilder.CreateIndex(
                name: "IX_clases_horarios_id_horarios",
                table: "clases_horarios",
                column: "id_horarios");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_id_miembro",
                table: "pagos",
                column: "id_miembro");

            migrationBuilder.CreateIndex(
                name: "IX_producto_id_categoria",
                table: "producto",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_registro_pesos_id_miembro",
                table: "registro_pesos",
                column: "id_miembro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clases_horarios");

            migrationBuilder.DropTable(
                name: "contacto");

            migrationBuilder.DropTable(
                name: "notificaciones");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "planes");

            migrationBuilder.DropTable(
                name: "producto");

            migrationBuilder.DropTable(
                name: "registro_pesos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "clases");

            migrationBuilder.DropTable(
                name: "horarios");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "miembro");

            migrationBuilder.DropTable(
                name: "profesores");
        }
    }
}
