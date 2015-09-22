using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using System.Collections.Generic;
using DataTablesParser;
using System.Linq;

namespace DataTablesParser.Tests
{
    [TestClass]
    public class LinqToObjectTests
    {

        [TestMethod]
        public void TotalRecordsTest()
        {
            var r = new MockHttpRequest();
            
            //Not all params below are used by the parser. We may support them later
            var nvparams = TestSupport.CreateParams();
            r.Add(nvparams);

            var people = TestSupport.CreateData();

            var parser = new DataTablesParser<MockPerson>(r, people.AsQueryable());

            Console.WriteLine(people.Count);

            Assert.AreEqual(people.Count,parser.Parse().recordsTotal);

        }

        [TestMethod]
        public void TotalResultsTest()
        {
            var r = new MockHttpRequest();
            var resultLength = 3;

            //Not all params below are used by the parser. We may support them later
            var nvparams = TestSupport.CreateParams();

            //override display length
            nvparams[Constants.DISPLAY_LENGTH] = Convert.ToString(resultLength); 

            r.Add(nvparams);

            var people = TestSupport.CreateData();

            var parser = new DataTablesParser<MockPerson>(r, people.AsQueryable());

            Console.WriteLine(people.Count);

            Assert.AreEqual(resultLength, parser.Parse().data.Count);

        }

        [TestMethod]
        public void TotalDisplayTest()
        {
            var r = new MockHttpRequest();
            var displayLength = 1;

            //Not all params below are used by the parser. We may support them later
            var nvparams = TestSupport.CreateParams();

            //Set filter parameter
            nvparams[Constants.SEARCH_KEY] = "Cromie";

            r.Add(nvparams);

            var people = TestSupport.CreateData();

            var parser = new DataTablesParser<MockPerson>(r, people.AsQueryable());

            Console.WriteLine(people.Count);

            Assert.AreEqual(displayLength, parser.Parse().recordsFiltered);

        }


    }
}
