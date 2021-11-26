using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedModel.Migrations.SqliteMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Layout = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Face",
                columns: table => new
                {
                    CardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OracleText = table.Column<string>(type: "TEXT", nullable: true),
                    TypeLine = table.Column<string>(type: "TEXT", nullable: false),
                    ManaCost = table.Column<string>(type: "TEXT", nullable: true),
                    Power = table.Column<string>(type: "TEXT", nullable: true),
                    Toughness = table.Column<string>(type: "TEXT", nullable: true),
                    Loyalty = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Face", x => new { x.CardId, x.Sequence });
                    table.ForeignKey(
                        name: "FK_Face_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Index",
                columns: table => new
                {
                    SanitizedName = table.Column<string>(type: "TEXT", nullable: false),
                    CardId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Index", x => x.SanitizedName);
                    table.ForeignKey(
                        name: "FK_Index_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Index_CardId",
                table: "Index",
                column: "CardId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Face");

            migrationBuilder.DropTable(
                name: "Index");

            migrationBuilder.DropTable(
                name: "Cards");
        }
    }
}
