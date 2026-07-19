using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace ExpressVoitures.Infrastructure;

public class FrenchValidationMetadataProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var metadata in context.ValidationMetadata.ValidatorMetadata)
        {
            // On ne traite que les attributs de validation sans message déjà défini
            if (metadata is not ValidationAttribute attribute) continue;
            if (attribute.ErrorMessage is not null || attribute.ErrorMessageResourceName is not null) continue;

            attribute.ErrorMessage = attribute switch
            {
                RequiredAttribute => "Le champ {0} est obligatoire.",
                RangeAttribute => "Le champ {0} doit être compris entre {1} et {2}.",
                StringLengthAttribute => "Le champ {0} doit avoir au maximum {1} caractères.",
                MaxLengthAttribute => "Le champ {0} dépasse la longueur maximale.",
                MinLengthAttribute => "Le champ {0} est trop court.",
                EmailAddressAttribute => "Le champ {0} n'est pas une adresse e-mail valide.",
                CompareAttribute => "Le champ {0} et sa confirmation ne correspondent pas.",
                _ => attribute.ErrorMessage
            };
        }
    }
}