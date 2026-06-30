namespace ExpressVoitures.Models;

public class Reparation
{
    public int Id { get; set; }

    // Coût de cette réparation
    public decimal Cout { get; set; }

    // Clé étrangère + navigation vers la vente
    public int VenteId { get; set; }
    public Vente Vente { get; set; } = null!;

    // Clé étrangère + navigation vers le type de réparation
    public int TypeReparationId { get; set; }
    public TypeReparation TypeReparation { get; set; } = null!;
}
