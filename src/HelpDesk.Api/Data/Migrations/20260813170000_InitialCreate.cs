using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Api.Data.Migrations;

[DbContext(typeof(HelpDeskDbContext))]
[Migration("20260813170000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tickets",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                RequesterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                RequesterEmail = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                RequesterUserId = table.Column<int>(type: "int", nullable: true),
                AssignedTechnicianUserId = table.Column<int>(type: "int", nullable: true),
                AssignedTechnicianName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Tickets", item => item.Id));

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", item => item.Id));

        migrationBuilder.CreateTable(
            name: "TicketComments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TicketId = table.Column<int>(type: "int", nullable: false),
                AuthorUserId = table.Column<int>(type: "int", nullable: false),
                AuthorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TicketComments", item => item.Id);
                table.ForeignKey(
                    name: "FK_TicketComments_Tickets_TicketId",
                    column: item => item.TicketId,
                    principalTable: "Tickets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TicketHistoryEntries",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TicketId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
                UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Action = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TicketHistoryEntries", item => item.Id);
                table.ForeignKey(
                    name: "FK_TicketHistoryEntries_Tickets_TicketId",
                    column: item => item.TicketId,
                    principalTable: "Tickets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TicketComments_TicketId",
            table: "TicketComments",
            column: "TicketId");

        migrationBuilder.CreateIndex(
            name: "IX_TicketHistoryEntries_TicketId",
            table: "TicketHistoryEntries",
            column: "TicketId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TicketComments");
        migrationBuilder.DropTable(name: "TicketHistoryEntries");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.DropTable(name: "Tickets");
    }
}
