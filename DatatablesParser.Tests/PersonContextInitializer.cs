using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace DataTablesParser.Tests
{
    public class PersonContextInitializer : DropCreateDatabaseAlways<PersonContext>
    {
        protected override void Seed(PersonContext context)
        {
            var people = new List<Person>
            {
                new Person
                {
                    FirstName = "James",
                    LastName = "Jamie",
                    BirthDate = DateTime.Parse("5/3/1960"),
                    Children = 5,
                    Height = 5.4M,
                    Weight = 250M,
                    TotalRedBloodCells = long.MaxValue
                },
                new Person
                {
                    FirstName = "Tony",
                    LastName = "Tonia",
                    BirthDate = DateTime.Parse("7/3/1961"),
                    Children = 3,
                    Height = 4.4M,
                    Weight = 150M,
                    TotalRedBloodCells = long.MaxValue -1
                },
                new Person
                {
                    FirstName = "Bandy",
                    LastName = "Momin",
                    BirthDate = DateTime.Parse("8/3/1970"),
                    Children = 1,
                    Height = 5.4M,
                    Weight = 250M,
                    TotalRedBloodCells = long.MaxValue -2
                },
                new Person
                {
                    FirstName = "Tannie",
                    LastName = "Tanner",
                    BirthDate = DateTime.Parse("2/3/1950"),
                    Children = 0,
                    Height = 6.4M,
                    Weight = 350M,
                    TotalRedBloodCells = long.MaxValue -3
                },
                new Person
                {
                    FirstName = "Cromie",
                    LastName = "Crammer",
                    BirthDate = DateTime.Parse("9/3/1953"),
                    Children = 15,
                    Height = 6.2M,
                    Weight = 120M,
                    TotalRedBloodCells = long.MaxValue -4
                },
                new Person
                {
                    FirstName = "Xorie",
                    LastName = "Zera",
                    BirthDate = DateTime.Parse("10/3/1974"),
                    Children = 2,
                    Height = 5.9M,
                    Weight = 175M,
                    TotalRedBloodCells = long.MaxValue -5
                }
            };

            people.ForEach(p => context.People.Add(p));
            context.SaveChanges();
        }
    }
}