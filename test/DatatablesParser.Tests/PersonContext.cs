    using Microsoft.EntityFrameworkCore;
    using System;

    namespace DataTablesParser.Tests
    {
        public class PersonContext : DbContext
        {
            public PersonContext(){ }

            public PersonContext(DbContextOptions<PersonContext> options)
            : base(options){ }

            //Sql Server >= 2012
            [DbFunction(Schema="")]
            public static string Format(DateTime data,string format)
            {
                throw new Exception();
            }

            //MySql
            [DbFunction]
            public static string Date_Format(DateTime data,string format)
            {
                throw new Exception();
            }

            //Postgres
            [DbFunction]
            public static string To_Char(DateTime data,string format)
            {
                throw new Exception();
            }


            public DbSet<Person> People { get; set; }
        }
    }
   