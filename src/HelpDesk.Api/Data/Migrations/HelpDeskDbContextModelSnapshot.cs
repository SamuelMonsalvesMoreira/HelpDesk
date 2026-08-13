using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Api.Data.Migrations;

[DbContext(typeof(HelpDeskDbContext))]
public partial class HelpDeskDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.9")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("HelpDesk.Api.Models.Ticket", entity =>
        {
            entity.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property<int>("Id"));

            entity.Property<int?>("AssignedTechnicianUserId").HasColumnType("int");
            entity.Property<string>("AssignedTechnicianName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            entity.Property<string>("Description").IsRequired().HasMaxLength(2000).HasColumnType("nvarchar(2000)");
            entity.Property<TicketPriority>("Priority").HasConversion<string>().HasMaxLength(20).HasColumnType("nvarchar(20)");
            entity.Property<string>("RequesterEmail").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            entity.Property<string>("RequesterName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property<int?>("RequesterUserId").HasColumnType("int");
            entity.Property<TicketStatus>("Status").HasConversion<string>().HasMaxLength(20).HasColumnType("nvarchar(20)");
            entity.Property<string>("Title").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            entity.Property<DateTime?>("UpdatedAtUtc").HasColumnType("datetime2");
            entity.HasKey("Id");
            entity.ToTable("Tickets");
        });

        modelBuilder.Entity("HelpDesk.Api.Models.TicketComment", entity =>
        {
            entity.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property<int>("Id"));

            entity.Property<string>("AuthorName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property<int>("AuthorUserId").HasColumnType("int");
            entity.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            entity.Property<string>("Message").IsRequired().HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            entity.Property<int>("TicketId").HasColumnType("int");
            entity.HasKey("Id");
            entity.HasIndex("TicketId");
            entity.ToTable("TicketComments");
        });

        modelBuilder.Entity("HelpDesk.Api.Models.TicketHistoryEntry", entity =>
        {
            entity.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property<int>("Id"));

            entity.Property<string>("Action").IsRequired().HasMaxLength(60).HasColumnType("nvarchar(60)");
            entity.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            entity.Property<string>("Details").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property<int>("TicketId").HasColumnType("int");
            entity.Property<int>("UserId").HasColumnType("int");
            entity.Property<string>("UserName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.HasKey("Id");
            entity.HasIndex("TicketId");
            entity.ToTable("TicketHistoryEntries");
        });

        modelBuilder.Entity("HelpDesk.Api.Models.User", entity =>
        {
            entity.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(entity.Property<int>("Id"));

            entity.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            entity.Property<string>("Email").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            entity.Property<bool>("IsActive").HasColumnType("bit");
            entity.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property<string>("PasswordHash").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property<UserRole>("Role").HasConversion<string>().HasMaxLength(20).HasColumnType("nvarchar(20)");
            entity.HasKey("Id");
            entity.HasIndex("Email").IsUnique();
            entity.ToTable("Users");
        });

        modelBuilder.Entity("HelpDesk.Api.Models.TicketComment", entity =>
        {
            entity.HasOne("HelpDesk.Api.Models.Ticket", null)
                .WithMany("Comments")
                .HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("HelpDesk.Api.Models.TicketHistoryEntry", entity =>
        {
            entity.HasOne("HelpDesk.Api.Models.Ticket", null)
                .WithMany("History")
                .HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("HelpDesk.Api.Models.Ticket", entity =>
        {
            entity.Navigation("Comments");
            entity.Navigation("History");
        });
    }
}
