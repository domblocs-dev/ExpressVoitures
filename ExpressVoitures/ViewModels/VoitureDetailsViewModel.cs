using System.ComponentModel.DataAnnotations;

namespace ExpressVoitures.ViewModels;

public class VoitureDetailsViewModel
{
    // La voiture à afficher (avec sa vente et ses réparations)
    public Voiture Voiture { get; set; } = null!;

    // --- Champs du formulaire d'ajout d'une réparation ---
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir un type de réparation.")]
    public int TypeReparationId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Le coût doit être positif.")]
    public decimal Cout { get; set; }
}
