using BairlyMN.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<LocationNode> LocationNodes => Set<LocationNode>();
    public DbSet<ApartmentDetails> ApartmentDetails => Set<ApartmentDetails>();
    public DbSet<HouseDetails> HouseDetails => Set<HouseDetails>();
    public DbSet<LandDetails> LandDetails => Set<LandDetails>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.Property(u => u.Bio).HasMaxLength(500);
        });

        builder.Entity<LocationNode>(e =>
        {
            e.HasOne(l => l.Parent)
             .WithMany(l => l.Children)
             .HasForeignKey(l => l.ParentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Listing>(e =>
        {
            e.Property(l => l.Price).HasPrecision(18, 2);
            e.Property(l => l.Area).HasPrecision(10, 2);
            e.Property(l => l.Title).IsRequired().HasMaxLength(300);
            e.Property(l => l.Description).IsRequired().HasMaxLength(5000);

            e.HasOne(l => l.User).WithMany(u => u.Listings)
             .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Category).WithMany(c => c.Listings)
             .HasForeignKey(l => l.CategoryId).OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.LocationNode).WithMany(loc => loc.Listings)
             .HasForeignKey(l => l.LocationNodeId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ListingImage>(e =>
        {
            e.HasOne(i => i.Listing).WithMany(l => l.Images)
             .HasForeignKey(i => i.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApartmentDetails>(e =>
        {
            e.HasOne(a => a.Listing).WithOne(l => l.ApartmentDetails)
             .HasForeignKey<ApartmentDetails>(a => a.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<HouseDetails>(e =>
        {
            e.HasOne(h => h.Listing).WithOne(l => l.HouseDetails)
             .HasForeignKey<HouseDetails>(h => h.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LandDetails>(e =>
        {
            e.HasOne(l => l.Listing).WithOne(listing => listing.LandDetails)
             .HasForeignKey<LandDetails>(l => l.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Conversation>(e =>
        {
            e.HasOne(c => c.Listing).WithMany(l => l.Conversations)
             .HasForeignKey(c => c.ListingId).OnDelete(DeleteBehavior.SetNull);

            e.HasOne(c => c.Initiator).WithMany(u => u.ConversationsAsInitiator)
             .HasForeignKey(c => c.InitiatorId).OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Participant).WithMany(u => u.ConversationsAsParticipant)
             .HasForeignKey(c => c.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ChatMessage>(e =>
        {
            e.Property(m => m.Content).IsRequired().HasMaxLength(2000);

            e.HasOne(m => m.Conversation).WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Sender).WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Favorite>()
     .HasOne(f => f.User)
     .WithMany(u => u.Favorites)
     .HasForeignKey(f => f.UserId)
     .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favorite>()
            .HasOne(f => f.Listing)
            .WithMany(l => l.Favorites)
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}