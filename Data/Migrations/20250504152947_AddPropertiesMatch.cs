using Microsoft.EntityFrameworkCore.Migrations;

namespace Tournament.Data.Migrations
{
    public partial class AddPropertiesMatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bracket",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Round",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SourceMatchAId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceMatchBId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SourceMatchAId",
                table: "Matches",
                column: "SourceMatchAId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SourceMatchBId",
                table: "Matches",
                column: "SourceMatchBId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Matches_SourceMatchAId",
                table: "Matches",
                column: "SourceMatchAId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Matches_SourceMatchBId",
                table: "Matches",
                column: "SourceMatchBId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Matches_SourceMatchAId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Matches_SourceMatchBId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_SourceMatchAId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_SourceMatchBId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Bracket",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Round",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SourceMatchAId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SourceMatchBId",
                table: "Matches");
        }
    }
}
