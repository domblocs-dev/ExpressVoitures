using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ExpressVoitures;

// Accepte le point ET la virgule comme séparateur décimal
public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var resultat = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (resultat == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        // conserve la valeur saisie (pour la ré-afficher en cas d'erreur)
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, resultat);

        var brut = resultat.FirstValue;
        var estNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;

        if (string.IsNullOrWhiteSpace(brut))
        {
            // vide accepté uniquement si le type est decimal?
            if (estNullable)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            return Task.CompletedTask;
        }

        var normalise = brut.Replace(',', '.');
        if (decimal.TryParse(normalise, NumberStyles.Number, CultureInfo.InvariantCulture, out var valeur))
        {
            bindingContext.Result = ModelBindingResult.Success(valeur);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                "Veuillez saisir un nombre valide.");
        }

        return Task.CompletedTask;
    }
}

// Indique à ASP.NET d'utiliser ce binder pour tout decimal / decimal?
public class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = context.Metadata.ModelType;
        if (type == typeof(decimal) || type == typeof(decimal?))
        {
            return new DecimalModelBinder();
        }
        return null;
    }
}