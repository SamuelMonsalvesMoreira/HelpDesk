using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Data;

public class HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketHistoryEntry> TicketHistoryEntries => Set<TicketHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var ticket = modelBuilder.Entity<Ticket>();

        ticket.Property(item => item.Title).HasMaxLength(120).IsRequired();
        ticket.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        ticket.Property(item => item.RequesterName).HasMaxLength(100).IsRequired();
        ticket.Property(item => item.RequesterEmail).HasMaxLength(160).IsRequired();
        ticket.Property(item => item.AssignedTechnicianName).HasMaxLength(100);
        ticket.Property(item => item.Priority).HasConversion<string>().HasMaxLength(20);
        ticket.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);

        ticket.HasMany(item => item.Comments)
            .WithOne()
            .HasForeignKey(comment => comment.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        ticket.HasMany(item => item.History)
            .WithOne()
            .HasForeignKey(entry => entry.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        var user = modelBuilder.Entity<User>();
        user.HasIndex(item => item.Email).IsUnique();
        user.Property(item => item.Name).HasMaxLength(100).IsRequired();
        user.Property(item => item.Email).HasMaxLength(160).IsRequired();
        user.Property(item => item.PasswordHash).HasMaxLength(500).IsRequired();
        user.Property(item => item.Role).HasConversion<string>().HasMaxLength(20);

        var comment = modelBuilder.Entity<TicketComment>();
        comment.Property(item => item.AuthorName).HasMaxLength(100).IsRequired();
        comment.Property(item => item.Message).HasMaxLength(1000).IsRequired();

        var history = modelBuilder.Entity<TicketHistoryEntry>();
        history.Property(item => item.UserName).HasMaxLength(100).IsRequired();
        history.Property(item => item.Action).HasMaxLength(60).IsRequired();
        history.Property(item => item.Details).HasMaxLength(500).IsRequired();
    }
}
