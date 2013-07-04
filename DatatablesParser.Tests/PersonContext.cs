using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DataTablesParser.Tests
{
    public class PersonContext : DbContext
    {
        public PersonContext(string connectionString)
            : base(connectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Person> People { get; set; }
    }
}