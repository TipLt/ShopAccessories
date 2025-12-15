using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BL5_PRN212_MustPass_Project.Models;

public partial class ShopAccessoriesContext : DbContext
{
    public ShopAccessoriesContext()
    {
    }

    public ShopAccessoriesContext(DbContextOptions<ShopAccessoriesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(30);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_Orders_CreatedAt");

            entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerId");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users_CreatedBy");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Orders_Customers");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_OrderItems_ProductId");

            entity.Property(e => e.LineTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderItems_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_Products_Name");

            entity.HasIndex(e => e.Code, "UQ_Products_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Password).HasMaxLength(200);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.Users)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Users_Customers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
