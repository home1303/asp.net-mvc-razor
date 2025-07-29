using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProductsRazor.Models;

public partial class ProductsContext : DbContext
{
    public ProductsContext()
    {
    }

    public ProductsContext(DbContextOptions<ProductsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemSerial> ItemSerials { get; set; }

    public virtual DbSet<Serial> Serials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-7BO34V39\\SQLEXPRESS;Initial Catalog=Products;Integrated Security=True;Pooling=False;Encrypt=False;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Items)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_Category");
        });

        modelBuilder.Entity<ItemSerial>(entity =>
        {
            entity.HasKey(e => new { e.ItemId, e.SerialId }).HasName("PK_join_table");

            entity.HasOne(d => d.Item).WithMany(p => p.ItemSerials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_join_table_Item");

            entity.HasOne(d => d.Serial).WithMany(p => p.ItemSerials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_join_table_Serial");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
