using System;
using System.Collections.Generic;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => new { e.CartId, e.ProductId }, "UQ_CartItems_Cart_Product").IsUnique();

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItems_Carts");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItems_Products");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CartId, "UQ_Orders_CartId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Cart).WithOne(p => p.Order)
                .HasForeignKey<Order>(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Carts");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.Property(e => e.ProductCode).HasMaxLength(20);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.ProductCode, "UQ_Products_ProductCode").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ProductCode).HasMaxLength(20);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductId).ValueGeneratedNever();

            entity.HasOne(d => d.Product).WithOne(p => p.Stock)
                .HasForeignKey<Stock>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stocks_Products");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
