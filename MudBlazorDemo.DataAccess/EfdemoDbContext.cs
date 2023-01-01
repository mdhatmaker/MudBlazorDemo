using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MudBlazorDemo.DataAccess;

public partial class EfdemoDbContext : DbContext
{
    public EfdemoDbContext()
    {
    }

    public EfdemoDbContext(DbContextOptions<EfdemoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<EmailAddress> EmailAddresses { get; set; }

    public virtual DbSet<Person> People { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=GAME-PC;Database=EFDemoDb;User=sa;Password=Wikki6969;MultipleActiveResultSets=True;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasIndex(e => e.PersonId, "IX_Addresses_PersonId");

            entity.HasOne(d => d.Person).WithMany(p => p.Addresses).HasForeignKey(d => d.PersonId);
        });

        modelBuilder.Entity<EmailAddress>(entity =>
        {
            entity.HasIndex(e => e.PersonId, "IX_EmailAddresses_PersonId");

            entity.Property(e => e.EmailAddress1).HasColumnName("EmailAddress");

            entity.HasOne(d => d.Person).WithMany(p => p.EmailAddresses).HasForeignKey(d => d.PersonId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
