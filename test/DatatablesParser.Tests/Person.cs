using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTablesParser.Tests
{
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal Weight { get; set; }
        public decimal height { get; set; }
        public int Children { get; set; }
        public long TotalRedBloodCells { get; set; }
    }
}