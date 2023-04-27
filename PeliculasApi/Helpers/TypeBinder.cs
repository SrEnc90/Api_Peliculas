using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace PeliculasApi.Helpers
{
    /*
     * el Model Binder es una característica del framework MVC (Model-View-Controller) que se encarga de mapear automáticamente 
     * los valores de una solicitud HTTP a los parámetros de un método de acción en un controlador.
     * Cuando se envía una solicitud HTTP a un controlador MVC, 
     * el Model Binder analiza los datos de la solicitud y los enlaza con los parámetros correspondientes en el método de acción. 
     * Esto significa que no es necesario realizar el análisis manualmente, lo que ahorra tiempo y simplifica el código.
     * Ojo: Como vamos utilizar con diferente tipos de valores, es mejor convertilo a genérico
     */
    public class TypeBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var nombrePropiedad = bindingContext.ModelName;
            var proveedorDeValores = bindingContext.ValueProvider.GetValue(nombrePropiedad);

            if (proveedorDeValores == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            try
            {
                var valorDeserializado = JsonConvert.DeserializeObject<T>(proveedorDeValores.FirstValue);
                bindingContext.Result = ModelBindingResult.Success(valorDeserializado);
            }
            catch
            {
                bindingContext.ModelState.TryAddModelError(nombrePropiedad, "Valor inválido para tipo de valores List<int>");
            }
            
            return Task.CompletedTask;
        }
    }
}
