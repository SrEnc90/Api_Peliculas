using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.Validaciones
{
    public class PesoArchivoValidacion : ValidationAttribute
    {
        private readonly int pesoMaximoEnMegaByte;

        //Como vamos a utilizar esta validacion en varios dataForm, vamos a pasar a cada uno el peso máximo que debe aceptar
        public PesoArchivoValidacion(int pesoMaximoEnMegaByte)
        {
            this.pesoMaximoEnMegaByte = pesoMaximoEnMegaByte;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) { return ValidationResult.Success; } //Solo queremos validar el peso, ya hay otro atributo que valdia si es nulo(Required)

            IFormFile formFile = value as IFormFile;

            if (formFile == null) { return ValidationResult.Success; } //Si la transformación no es exitosa que retorne success

            /*
             * Como pesoMaximoEnMegaByte está en megabytes multiplicamos por 1024 para convertirlos a kilobyte
             * y finalmente nuevamente multiplicamos por 1024 para convertirlo a bytes y poder comparar bytes entre ambos
             */
            if (formFile.Length > pesoMaximoEnMegaByte * 1024 * 1024)
            {
                return new ValidationResult($"El peso del archivo no debe ser mayor a {pesoMaximoEnMegaByte}mb");
            }

            return ValidationResult.Success;
        }
    }
}
