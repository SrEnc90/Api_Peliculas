using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class SalaDeCineCercanoFiltroDTO
    {
        [Range(-90, 90)]
        public double Latitud { get; set; }
        [Range(-180, 180)]
        public double Longitud { get; set; }
        private int _distanciaMaximaKms = 50;
        private int _distanciaEnKms = 10;
        public int DistanciaEnKms 
        { 
            get
            {
                return _distanciaEnKms;
            }
            set
            {
                _distanciaEnKms = (value > _distanciaMaximaKms) ? _distanciaMaximaKms : value;
            }
        }
    }
}
