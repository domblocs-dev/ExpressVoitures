using System.ComponentModel.DataAnnotations;

namespace ExpressVoitures.ViewModels;

public class VoitureFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    // --- Voiture ---
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir un modèle.")]
    public int ModeleId { get; set; }

    [Required(ErrorMessage = "L'année est obligatoire.")]
    public int Annee { get; set; }

    [Display(Name = "Code VIN")]
    public string? CodeVIN { get; set; }

    public string? Finition { get; set; }

    // --- Vente ---
    [Required(ErrorMessage = "Le prix d'achat est obligatoire.")]
    [Range(0, double.MaxValue, ErrorMessage = "Le prix d'achat doit être positif.")]
    public decimal PrixAchat { get; set; }

    [Required(ErrorMessage = "La date d'achat est obligatoire.")]
    [Display(Name = "Date d'achat")]
    public DateOnly DateAchat { get; set; }

    [Display(Name = "Date de disponibilité")]
    public DateOnly? DateDisponibilite { get; set; }

    [Display(Name = "Date de vente")]
    public DateOnly? DateVente { get; set; }

    public string? Description { get; set; }

    [Display(Name = "URL de la photo")]
    public string? PhotoUrl { get; set; }


    // Règle métier : année entre 1990 et l'année courante (jamais dans le futur)

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        int anneeCourante = DateTime.Today.Year;
        if (Annee < 1990 || Annee > anneeCourante)
        {
            yield return new ValidationResult(
                $"L'année doit être comprise entre 1990 et {anneeCourante}.",
                new[] { nameof(Annee) });
        }

        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        if (DateAchat > aujourdHui)
        {
            yield return new ValidationResult(
                "La date d'achat ne peut pas être dans le futur.",
                new[] { nameof(DateAchat) });
        }
    }

}