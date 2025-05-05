using Microsoft.EntityFrameworkCore.Migrations;

namespace Tournament.Data.Migrations
{
    public partial class AddNumbersToMatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "MatchId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_MatchId",
                table: "Matches",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Matches_MatchId",
                table: "Matches",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Matches_MatchId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_MatchId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Matches");

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
    }
}
