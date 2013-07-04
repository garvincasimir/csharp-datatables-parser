using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DataTablesParser.WebSample.Models
{
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public int Children { get; set; }

        [NotMapped]
        public string BirthDateFormatted
        {
            get
            {
                return string.Format("{0:M/d/yyyy}", BirthDate);
            }
        }
    }
}