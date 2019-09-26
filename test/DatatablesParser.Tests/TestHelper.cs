using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace DataTablesParser.Tests
{
    public class TestHelper
    {
        private static Dictionary<string,StringValues> Params = new Dictionary<string,StringValues>();
        private static List<Person> people = new List<Person>
        {
            new Person
            {
                FirstName = "James",
                LastName = "Jamie",
                BirthDate = DateTime.Parse("5/3/1960"),
                Children = 5,
                height = 5.4M,
                Weight = 250M
            },
            new Person
            {
                FirstName = "Tony",
                LastName = "Tonia",
                BirthDate = DateTime.Parse("7/3/1961"),
                Children = 3,
                height = 4.4M,
                Weight = 150M
            },
            new Person
            {
                FirstName = "Bandy",
                LastName = "Momin",
                BirthDate = DateTime.Parse("8/3/1970"),
                Children = 1,
                height = 5.4M,
                Weight = 250M
            },
            new Person
            {
                FirstName = "Tannie",
                LastName = "Tanner",
                BirthDate = DateTime.Parse("2/3/1950"),
                Children = 0,
                height = 6.4M,
                Weight = 350M
            },
            new Person
            {
                FirstName = "Cromie",
                LastName = "Crammer",
                BirthDate = DateTime.Parse("9/3/1953"),
                Children = 15,
                height = 6.2M,
                Weight = 120M
            },
            new Person
            {
                FirstName = "Xorie",
                LastName = "Zera",
                BirthDate = DateTime.Parse("10/3/1974"),
                Children = 2,
                height = 5.9M,
                Weight = 175M
            }
        };

          
        static TestHelper()
        {
            Add(Constants.DRAW, "1");
            Add(Constants.DISPLAY_START, "0");
            Add(Constants.DISPLAY_LENGTH, "10");
            Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "0"), "FirstName");
            Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "0"), "true");
            Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "0"), "");
            Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "0"), "false");

            Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "1"), "LastName");
            Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "1"), "true");
            Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "1"), "");
            Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "1"), "false");

            Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "2"), "BirthDate");
            Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "2"), "true");
            Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "2"), "");
            Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "2"), "false");

            Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "3"), "Weight");
            Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "3"), "true");
            Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "3"), "");
            Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "3"), "false");

            Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "4"), "Height");
            Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "4"), "true");
            Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "4"), "");
            Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "4"), "false");

            Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "5"), "Children");
            Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "5"), "true");
            Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "5"), "");
            Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "5"), "false");

            Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "6"), "TotalRedBloodCells");
            Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "6"), "true");
            Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "6"), "");
            Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "6"), "false");

            Add(Constants.SEARCH_KEY, "");
            Add(Constants.SEARCH_REGEX_KEY, "false");

            Add(Constants.GetKey(Constants.ORDER_COLUMN_FORMAT, "0"), "0");
            Add(Constants.GetKey(Constants.ORDER_DIRECTION_FORMAT, "0"), "0");

        }

        public static Dictionary<string,StringValues>  CreateParams()
        {
            return  Params.ToDictionary(k => k.Key,v => v.Value);
        }

        public static IEnumerable<Person> CreateData()
        {
            return from p in people select new Person
            {
                FirstName = p.FirstName,
                LastName = p.LastName,
                BirthDate = p.BirthDate,
                Children = p.Children,
                height = p.height,
                Weight = p.Weight,
                TotalRedBloodCells = p.TotalRedBloodCells
            };

        }

        private static void Add(string key,string value)
        {

            Params.Add(key,new StringValues(value));
        }
        public static readonly ILoggerFactory consoleLogger  
            = new LoggerFactory(new[] {
                    new EFlogger()
                  });
        public static PersonContext GetInMemoryContext()
        {

           var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<PersonContext>();
            builder.UseInMemoryDatabase("testdb")
                   .UseInternalServiceProvider(serviceProvider)
                   .UseLoggerFactory(consoleLogger);

            var context = new PersonContext(builder.Options);
            
            context.People.AddRange(CreateData());
            context.SaveChanges();
            return context;

        }

        public static PersonContext GetMysqlContext()
        {
           
           var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkMySql()
                .BuildServiceProvider();
                
            var builder = new DbContextOptionsBuilder<PersonContext>();
                builder.UseMySql(@"server=mysql;database=dotnettest;user=tester;password=Rea11ytrong_3")
                 .UseInternalServiceProvider(serviceProvider)
                 .UseLoggerFactory(consoleLogger);
                  
            var context = new PersonContext(builder.Options);

            context.Database.EnsureCreated();
            context.Database.ExecuteSqlRaw("truncate table People;");
            
            context.People.AddRange(CreateData());
            context.SaveChanges();
            
            return context;

        }

        public static PersonContext GetPgsqlContext()
        {

           var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<PersonContext>();
                builder.UseNpgsql(@"Host=pgsql;Database=dotnettest;User ID=tester;Password=Rea11ytrong_3")
                    .UseInternalServiceProvider(serviceProvider)
                    .UseLoggerFactory(consoleLogger);
                    
            var context = new PersonContext(builder.Options);
            
            context.Database.EnsureCreated();
            context.Database.ExecuteSqlRaw("truncate table public.\"People\";");
            
            context.People.AddRange(CreateData());
            context.SaveChanges();
            
            return context;

        }
    

        public static PersonContext GetMssqlContext()
        {

           var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<PersonContext>();
            builder.UseSqlServer(@"Data Source=mssql;Initial Catalog=TestNetCoreEF;user id=sa;password=Rea11ytrong_3")
                    .UseInternalServiceProvider(serviceProvider)
                    .UseLoggerFactory(consoleLogger);
                    

            var context = new PersonContext(builder.Options);
            context.Database.EnsureCreated();
            context.Database.ExecuteSqlRaw("truncate table People;");
            
            context.People.AddRange(CreateData());
            context.SaveChanges();
            return context;

        }
    }
}
