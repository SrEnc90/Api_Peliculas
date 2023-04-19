using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Validaciones
{
    public class TipoArchivoValidacion : ValidationAttribute
    {
        private readonly string[] tipoValidosPermitidos;

        public TipoArchivoValidacion(string[] tipoValidosPermitidos)
        {
            this.tipoValidosPermitidos = tipoValidosPermitidos;
        }

        /*
         * Creamos un enum para pasar en el constructor de manera genérica que tipo de archivo vamos a permitir
         */
        public TipoArchivoValidacion(GrupoTipoArchivo grupoTipoArchivo)
        {
            if (grupoTipoArchivo == GrupoTipoArchivo.Imagen)
            {
                tipoValidosPermitidos = new string[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) { return ValidationResult.Success; }

            IFormFile formFile = value as IFormFile;

            if (formFile == null) { return ValidationResult.Success; }

            if (!tipoValidosPermitidos.Contains(formFile.ContentType))
            {
                return new ValidationResult($"El tipo del archivo debe ser uno de los siguientes: {string.Join(", ", tipoValidosPermitidos)}");
            }

            return ValidationResult.Success;
        }
    }
}
