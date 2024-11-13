using FileShare.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FileShare.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<FileModel> Files { get; set; } = default!;
    public DbSet<LinkModel> Links { get; set; } = default!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        FileModelCreating(modelBuilder);
        LinkModelCreating(modelBuilder);
        UserModelCreating(modelBuilder);
    }

    private void FileModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileModel>()
            .Property(file => file.Id)
            .ValueGeneratedOnAdd();
    }

    private void LinkModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LinkModel>()
            .Property(link => link.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<LinkModel>()
            .HasOne(link => link.File)
            .WithOne()
            .HasForeignKey<LinkModel>(link => link.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void UserModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>()
            .Property(user => user.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<UserModel>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<UserModel>()
            .HasMany(user => user.Files)
            .WithOne()
            .HasForeignKey(file => file.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
