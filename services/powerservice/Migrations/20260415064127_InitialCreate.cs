using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace powerservice.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PowerDataHour",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    at_load_value = table.Column<double>(type: "double precision", nullable: true),
                    be_load_value = table.Column<double>(type: "double precision", nullable: true),
                    bg_load_value = table.Column<double>(type: "double precision", nullable: true),
                    ch_load_value = table.Column<double>(type: "double precision", nullable: true),
                    cy_load_value = table.Column<double>(type: "double precision", nullable: true),
                    cz_load_value = table.Column<double>(type: "double precision", nullable: true),
                    de_load_value = table.Column<double>(type: "double precision", nullable: true),
                    dk_load_value = table.Column<double>(type: "double precision", nullable: true),
                    ee_load_value = table.Column<double>(type: "double precision", nullable: true),
                    es_load_value = table.Column<double>(type: "double precision", nullable: true),
                    fi_load_value = table.Column<double>(type: "double precision", nullable: true),
                    fr_load_value = table.Column<double>(type: "double precision", nullable: true),
                    gb_load_value = table.Column<double>(type: "double precision", nullable: true),
                    gr_load_value = table.Column<double>(type: "double precision", nullable: true),
                    hr_load_value = table.Column<double>(type: "double precision", nullable: true),
                    hu_load_value = table.Column<double>(type: "double precision", nullable: true),
                    ie_load_value = table.Column<double>(type: "double precision", nullable: true),
                    it_load_value = table.Column<double>(type: "double precision", nullable: true),
                    lt_load_value = table.Column<double>(type: "double precision", nullable: true),
                    lu_load_value = table.Column<double>(type: "double precision", nullable: true),
                    lv_load_value = table.Column<double>(type: "double precision", nullable: true),
                    me_load_value = table.Column<double>(type: "double precision", nullable: true),
                    nl_load_value = table.Column<double>(type: "double precision", nullable: true),
                    no_load_value = table.Column<double>(type: "double precision", nullable: true),
                    pl_load_value = table.Column<double>(type: "double precision", nullable: true),
                    pt_load_value = table.Column<double>(type: "double precision", nullable: true),
                    ro_load_value = table.Column<double>(type: "double precision", nullable: true),
                    rs_load_value = table.Column<double>(type: "double precision", nullable: true),
                    se_load_value = table.Column<double>(type: "double precision", nullable: true),
                    si_load_value = table.Column<double>(type: "double precision", nullable: true),
                    sk_load_value = table.Column<double>(type: "double precision", nullable: true),
                    ua_load_value = table.Column<double>(type: "double precision", nullable: true),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerDataHour", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PowerDataQuarter",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nl_load_value = table.Column<double>(type: "double precision", nullable: true),
                    be_load_value = table.Column<double>(type: "double precision", nullable: true),
                    de_load_value = table.Column<double>(type: "double precision", nullable: true),
                    at_load_value = table.Column<double>(type: "double precision", nullable: true),
                    hu_load_value = table.Column<double>(type: "double precision", nullable: true),
                    lu_load_value = table.Column<double>(type: "double precision", nullable: true),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerDataQuarter", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerDataHour");

            migrationBuilder.DropTable(
                name: "PowerDataQuarter");
        }
    }
}
