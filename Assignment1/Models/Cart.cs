using Assignment1.Areas.Identity.Data;

namespace Assignment1.Models
{
    public class Cart
    {
        public string UId { get; set; }
        public string BookIsbn { get; set; }

        public int Quantity { get; set; }
        
        public Assignment1User? User { get; set; }
        public Book? Book { get; set; }
    }
}