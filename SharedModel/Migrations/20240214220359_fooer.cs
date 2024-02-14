using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedModel.Migrations
{
    /// <inheritdoc />
    public partial class fooer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScryfallId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CardLayout = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    When = table.Column<string>(type: "TEXT", nullable: false),
                    UnrecognizedCards = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UpdateStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<string>(type: "TEXT", nullable: false),
                    StatusState = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Face",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    OracleText = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    TypeLine = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ManaCost = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Power = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    Toughness = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    Loyalty = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Face", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Face_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanitizedCardNames",
                columns: table => new
                {
                    SanitizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CardId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanitizedCardNames", x => x.SanitizedName);
                    table.ForeignKey(
                        name: "FK_SanitizedCardNames_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardSearchRecord",
                columns: table => new
                {
                    CardsId = table.Column<int>(type: "INTEGER", nullable: false),
                    SearchRecordsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardSearchRecord", x => new { x.CardsId, x.SearchRecordsId });
                    table.ForeignKey(
                        name: "FK_CardSearchRecord_Cards_CardsId",
                        column: x => x.CardsId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardSearchRecord_SearchRecords_SearchRecordsId",
                        column: x => x.SearchRecordsId,
                        principalTable: "SearchRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name",
                table: "Cards",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardSearchRecord_SearchRecordsId",
                table: "CardSearchRecord",
                column: "SearchRecordsId");

            migrationBuilder.CreateIndex(
                name: "IX_Face_CardId",
                table: "Face",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_SanitizedCardNames_CardId",
                table: "SanitizedCardNames",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardSearchRecord");

            migrationBuilder.DropTable(
                name: "Face");

            migrationBuilder.DropTable(
                name: "SanitizedCardNames");

            migrationBuilder.DropTable(
                name: "UpdateStatuses");

            migrationBuilder.DropTable(
                name: "SearchRecords");

            migrationBuilder.DropTable(
                name: "Cards");
        }
    }
}
