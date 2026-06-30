namespace ExpressVoitures.Models;

public class Modele
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;

    // Clé étrangère vers la Marque
    public int MarqueId { get; set; }

    // Propriété de navigation vers la Marque
    public Marque Marque { get; set; } = null!;

}
