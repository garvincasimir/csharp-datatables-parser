    using Microsoft.EntityFrameworkCore;
    
    namespace TestLib
    {
        public class PersonContext : DbContext
        {
            public PersonContext(){ }

            public PersonContext(DbContextOptions<PersonContext> options)
            : base(options){ }

            public DbSet<Person> People { get; set; }
        }
    }
   