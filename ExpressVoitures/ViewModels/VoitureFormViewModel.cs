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

        if (DateDisponibilite is not null && DateDisponibilite > aujourdHui)
        {
            yield return new ValidationResult(
                "La date de disponibilité ne peut pas être dans le futur.",
                new[] { nameof(DateDisponibilite) });
        }

        if (DateVente is not null && DateVente > aujourdHui)
        {
            yield return new ValidationResult(
                "La date de vente ne peut pas être dans le futur.",
                new[] { nameof(DateVente) });
        }

        // Cohérence des dates entre elles
        if (DateDisponibilite is not null && DateDisponibilite < DateAchat)
        {
            yield return new ValidationResult(
                "La date de disponibilité ne peut pas être antérieure à la date d'achat.",
                new[] { nameof(DateDisponibilite) });
        }

        if (DateVente is not null && DateVente < DateAchat)
        {
            yield return new ValidationResult(
                "La date de vente ne peut pas être antérieure à la date d'achat.",
                new[] { nameof(DateVente) });
        }

        if (DateDisponibilite is not null && DateVente is not null && DateDisponibilite > DateVente)
        {
            yield return new ValidationResult(
                "La date de disponibilité ne peut pas être postérieure à la date de vente.",
                new[] { nameof(DateDisponibilite) });
        }

        // -------------------

    }


}