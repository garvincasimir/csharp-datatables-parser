using System;
using Xunit;
using DataTablesParser;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace DataTablesParser.Tests
{
  
    public class PgsqlEntityTests
    {

        [Fact]
        public void TotalRecordsTest()
        {
            var context = TestHelper.GetPgsqlContext();

            var p = TestHelper.CreateParams();

            var parser = new Parser<Person>(p, context.People);

            Console.WriteLine("Pgsql - Total People TotalRecordsTest: {0}",context.People.Count());

            Assert.Equal(context.People.Count(),parser.Parse().recordsTotal);

        }

        [Fact]
        public void TotalResultsTest()
        {
            var context = TestHelper.GetPgsqlContext();

            var p = TestHelper.CreateParams();

            var resultLength = 3;

            //override display length
            p[Constants.DISPLAY_LENGTH] = new StringValues(Convert.ToString(resultLength)); 

            var parser = new Parser<Person>(p, context.People);

            Console.WriteLine("Pgsql - Total People TotalResultsTest: {0}",context.People.Count());

            Assert.Equal(resultLength, parser.Parse().data.Count);

        }

        [Fact]
        public void TotalDisplayTest()
        {
            var context = TestHelper.GetPgsqlContext();
            var p = TestHelper.CreateParams();
            var displayLength = 1;

           
            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues("Cromie");

            var parser = new Parser<Person>(p, context.People);

            Console.WriteLine("Pgsql - Total People TotalDisplayTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }

        [Fact]
        public void TotalDisplayIndividualTest()
        {
            var context = TestHelper.GetPgsqlContext();
            var p = TestHelper.CreateParams();
            var displayLength = 1;

           
            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues("a");
            p[Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "1")] = "mmer";

            var parser = new Parser<Person>(p, context.People);

            Console.WriteLine("Pgsql - Total People TotalDisplayIndividualTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }



        [Fact]
        public void TotalDisplayCustomFormatTest()
        {
            var context = TestHelper.GetPgsqlContext();
            var p = TestHelper.CreateParams();
            var displayLength = 1;

           
            //Set filter parameter
            p[Constants.SEARCH_KEY] = new StringValues("09/03/1953");

            var parser = new Parser<Person>(p, context.People)
                            .SetConverter(x => x.BirthDate, x => PersonContext.To_Char(x.BirthDate,"MM/DD/YYYY"));
                

            Console.WriteLine("Pgsql - Total People TotalDisplayCustomFormatTest: {0}",context.People.Count());

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

            Console.WriteLine("Pgsql - Total People TotalDisplayIndividualMutiTest: {0}",context.People.Count());

            Assert.Equal(displayLength, parser.Parse().recordsFiltered);

        }

    }
}