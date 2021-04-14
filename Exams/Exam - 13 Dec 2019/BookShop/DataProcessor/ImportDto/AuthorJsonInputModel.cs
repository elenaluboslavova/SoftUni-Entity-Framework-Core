using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.DataProcessor.ImportDto
{
    public class AuthorJsonInputModel
    {
        public AuthorJsonInputModel()
        {
            this.Books = new List<JsonBookInputModel>();
        }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("[0-9]{3}-[0-9]{3}-[0-9]{4}")]
        public string Phone { get; set; }

        public List<JsonBookInputModel> Books { get; set; }
    }

    public class JsonBookInputModel
    {
        public int? Id { get; set; }
    }
}