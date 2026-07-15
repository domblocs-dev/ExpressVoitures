namespace ExpressVoitures.Models;

public static class StatutVenteExtensions
{
    public static string EnFrancais(this StatutVente statut) => statut switch
    {
        StatutVente.EnPreparation => "En préparation",
        StatutVente.Disponible => "Disponible",
        StatutVente.Vendue => "Vendue",
        _ => statut.ToString()
    };
}
