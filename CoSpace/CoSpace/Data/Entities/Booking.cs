using CoSpace.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace CoSpace.Data.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        public User? User { get; set; }

        public Space? Space { get; set; } 

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        [Display(Name = "Fecha Inicio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        [Display(Name = "Fecha Final")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Display(Name = "Precio Total")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal TotalPrice { get; set; }

        public BookingState BookingState { get; set; }

        //public ICollection<Pay> Pays { get; set; } = null!;
    }
}
