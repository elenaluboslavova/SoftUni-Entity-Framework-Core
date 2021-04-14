using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cinema.Data.Models
{
    public class Customer
    {
        public Customer()
        {
            this.Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int Age { get; set; }

        public decimal Balance { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
}
