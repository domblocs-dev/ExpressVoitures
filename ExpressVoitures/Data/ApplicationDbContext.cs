using ExpressVoitures.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpressVoitures.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Marque> Marques { get; set; } = null!;
    public DbSet<Modele> Modeles { get; set; } = null!;
    public DbSet<Voiture> Voitures { get; set; } = null!;
    public DbSet<Vente> Ventes { get; set; } = null!;
    public DbSet<TypeReparation> TypeReparations { get; set; } = null!;
    public DbSet<Reparation> Reparations { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unicité des référentiels
        modelBuilder.Entity<Marque>()
            .HasIndex(m => m.Nom)
            .IsUnique();

        modelBuilder.Entity<Modele>()
            .HasIndex(m => new { m.MarqueId, m.Nom })
            .IsUnique();

        modelBuilder.Entity<TypeReparation>()
            .HasIndex(t => t.Libelle)
            .IsUnique();

        // Relation 1 à 1 : une voiture donne lieu à au plus une vente
        modelBuilder.Entity<Vente>()
            .HasIndex(v => v.VoitureId)
            .IsUnique();


        // Précision des montants : decimal(18,2)
        modelBuilder.Entity<Vente>().Property(v => v.PrixAchat).HasPrecision(18, 2);
        modelBuilder.Entity<Vente>().Property(v => v.PrixVente).HasPrecision(18, 2);
        modelBuilder.Entity<Reparation>().Property(r => r.Cout).HasPrecision(18, 2);
        modelBuilder.Entity<TypeReparation>().Property(t => t.CoutParDefaut).HasPrecision(18, 2);

    }




}
