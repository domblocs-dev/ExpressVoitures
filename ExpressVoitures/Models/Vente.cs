using System.ComponentModel.DataAnnotations.Schema;

namespace ExpressVoitures.Models;

public class Vente
{
    public int Id { get; set; }

    // Clé étrangère + navigation vers la voiture (relation 1 à 1)
    public int VoitureId { get; set; }
    public Voiture Voiture { get; set; } = null!;

    // Volet financier
    public decimal PrixAchat { get; set; }
    public decimal PrixVente { get; set; } // stocké (figé à la vente)

    // Dates du cycle de vie
    public DateOnly DateAchat { get; set; }
    public DateOnly? DateDisponibilite { get; set; }
    public DateOnly? DateVente { get; set; }

    // Présentation
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }

    // Statut calculé à partir des dates (non stocké en base)
    [NotMapped]
    public StatutVente Statut
    {
        get
        {
            if (DateVente is not null)
                return StatutVente.Vendue;

            if (DateDisponibilite is not null)
                return StatutVente.Disponible;

            return StatutVente.EnPreparation;
        }
    }
}
