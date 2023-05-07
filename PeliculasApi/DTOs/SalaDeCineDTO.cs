namespace PeliculasApi.DTOs
{
    public class SalaDeCineDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        /*
         * no vamos a mandar a nuestros usuarios la data de tipo Point, lo que se quiere es devolver la latitud y la longitud
         */
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}
