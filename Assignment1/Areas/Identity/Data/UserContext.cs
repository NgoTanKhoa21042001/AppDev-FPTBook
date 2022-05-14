using Assignment1.Areas.Identity.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.Data;

public class UserContext : IdentityDbContext<Assignment1User>
{
    public UserContext(DbContextOptions<UserContext> options)
        : base(options)
    {
    }
    public DbSet<Store> Store { get; set; }
    public DbSet<Book> Book { get; set; }
    public DbSet<OrderDetail> OrderDetail { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<Cart> Cart { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        builder.Entity<Assignment1User>()
            .HasOne<Store>(au => au.Store)
            .WithOne(st => st.User)
            .HasForeignKey<Store>(st => st.UId);
        builder.Entity<Book>()
            .HasOne<Store>(b => b.Store)
            .WithMany(st => st.Books)
            .HasForeignKey(st => st.StoreId);
        builder.Entity<Order>()
            .HasOne<Assignment1User>(o => o.User)
            .WithMany(ap => ap.Orders)
            .HasForeignKey(o => o.UId);
           

        builder.Entity<OrderDetail>()
            .HasKey(od => new { od.OrderId, od.BookIsbn });
        builder.Entity<OrderDetail>()
            .HasOne<Order>(od => od.Order)
            .WithMany(or => or.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<OrderDetail>()
            .HasOne<Book>(od => od.Book)
            .WithMany(b => b.OrderDetails)
            .HasForeignKey(od => od.BookIsbn);
        builder.Entity<Cart>()
           .HasKey(c => new { c.UId, c.BookIsbn });
        builder.Entity<Cart>()
            .HasOne<Assignment1User>(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UId);
        builder.Entity<Cart>()
            .HasOne<Book>(od => od.Book)
            .WithMany(b => b.Carts)
            .HasForeignKey(od => od.BookIsbn)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
