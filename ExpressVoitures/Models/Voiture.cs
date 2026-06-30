namespace ExpressVoitures.Models;

public class Voiture
{
    public int Id { get; set; }

    // Champs optionnels : type nullable (string?)
    public string? CodeVIN { get; set; }
    public string? Finition { get; set; }

    // Champs obligatoire
    public int Annee { get; set; }

    // Clé étrangère + navigation vers le modèle
    public int ModeleId { get; set; }
    public Modele Modele { get; set; } = null!;
}
