using System;
using Xunit;
using DataTablesParser;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace DataTablesParser.Tests
{
  
    public class LinqToObjectTests
    {
        
        [Fact]
        public void TotalRecordsTest()
        {
            var context = TestHelper.GetInMemoryContext();

            var p = TestHelper.CreateParams();

            var parser = new Parser<Person>(p, context.People.AsQueryable());

            Console.WriteLine("InMemory - Total People TotalRecordsTest: {0}",context.People.Count());

            Assert.Equal(context.People.Count(),parser.Parse().recordsTotal);

        }

        [Fact]
        public void TotalResultsTest()
        {
            var context = TestHelper.GetInMemoryContext();

            var p = TestHelper.CreateParams();

            var resultLength = 3;

            //override display length
            p[Constants.DISPLAY_LENGTH] = new StringValues(Convert.ToString(resultLength)); 

            var parser = new Parser<Person>(p, context.People.AsQueryable());

            Console.WriteLine("InMemory - Total People TotalResultsTest: {0}",context.People.Count());

            Assert.Equal(resultLength, parser.Parse().data.Count);

        }

        [Fact]
        public void TotalDisplayTest()
        {
            var context = TestHelper.GetInMemoryContext();
            var p = TestHelper.CreateParams();
            var displayLength = 1;

           
            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues("Cromie");

            var parser = new Parser<Person>(p, context.People.ToList().AsQueryable());

            Console.WriteLine("InMemory - Total People TotalDisplayTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }

        [Fact]
        public void TotalDisplayIndividualTest()
        {
            var context = TestHelper.GetInMemoryContext();
            var p = TestHelper.CreateParams();
            var displayLength = 1;

           
            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues("a");
            p[Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "1")] = "mmer";

            var parser = new Parser<Person>(p, context.People);

            Console.WriteLine("InMemory - Total People TotalDisplayIndividualTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }

        [Fact]
        public void TotalDisplayIndividualMutiTest()
        {
            var context = TestHelper.GetInMemoryContext();
            var p = TestHelper.CreateParams();
            var displayLength = 1;

           
            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues("a");
            p[Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "0")] = "omie";
            p[Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "1")] = "mmer";
            
            var parser = new Parser<Person>(p, context.People);

            Console.WriteLine("InMemory - Total People TotalDisplayIndividualMutiTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }

        [Fact]
        public void StartsWithTest()
        {
            var context = TestHelper.GetInMemoryContext();
            var p = TestHelper.CreateParams();
            var displayLength = 1;

            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues(Constants.DEFAULT_STARTS_WITH_TOKEN + "Cr");

            var parser = new Parser<Person>(p, context.People.ToList().AsQueryable());

            Console.WriteLine("InMemory - StartsWithTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }

        [Fact]
        public void EndsWithTest()
        {
            var context = TestHelper.GetInMemoryContext();
            var p = TestHelper.CreateParams();
            var displayLength = 4;

            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues("ie" + Constants.DEFAULT_ENDS_WITH_TOKEN );

            var parser = new Parser<Person>(p, context.People.ToList().AsQueryable());

            Console.WriteLine("InMemory - EndsWithTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }


    }
}