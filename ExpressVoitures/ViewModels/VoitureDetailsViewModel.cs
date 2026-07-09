using System.ComponentModel.DataAnnotations;

namespace ExpressVoitures.ViewModels;

public class VoitureDetailsViewModel
{
    public Voiture Voiture { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir un type de réparation.")]
    public int TypeReparationId { get; set; }
}