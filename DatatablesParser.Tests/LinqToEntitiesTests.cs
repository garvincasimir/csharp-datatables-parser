using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
using System.IO;

namespace DataTablesParser.Tests
{
    [TestClass]
    public class LinqToEntitiesTests
    {
        string file;
        PersonContext context;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            
            Database.SetInitializer<PersonContext>(new PersonContextInitializer()); 
        }

        [TestInitialize]
        public void Setup()
        {
             file = Path.GetTempFileName();
             context = new PersonContext(file);

        }

        [TestCleanup]
        public void TearDown()
        {
            File.Delete(file);
        }

        [TestMethod]
        public void TotalRecordsTest()
        {
            var r = new MockHttpRequest();
            var total = 6;
            //Not all params below are used by the parser. We may support them later
            var nvparams = TestSupport.CreateParams();
            r.Add(nvparams);
        
            var result = new DataTablesParser<Person>(r, context.People).Parse();
        
            Console.WriteLine(result.GetQuery());
          
            Assert.AreEqual(total, result.recordsTotal);
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

            var result = new DataTablesParser<Person>(r, context.People).Parse();

            Console.WriteLine(result.GetQuery());

            Assert.AreEqual(resultLength, result.data.Count);

        }

        //This will fail because ce does not support SqlFunctions. May switch to localdb.
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

            var parser = new DataTablesParser<Person>(r, context.People);

            Assert.AreEqual(displayLength, parser.Parse().recordsFiltered);

        }

    }
}
