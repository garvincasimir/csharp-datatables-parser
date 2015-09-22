using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using DataTablesParser;

namespace DataTablesParser.Tests
{
    [TestClass]
    public class ParameterTests
    {
        [TestMethod]
        public void TestColumnPropertyPattern()
        {
            var key = "columns[0][data]";

            var result = Regex.IsMatch(key, Constants.COLUMN_PROPERTY_PATTERN);

            Assert.IsTrue(result);

        }

        [TestMethod]
        public void TestOrderPattern()
        {
            var key = "order[0][column]";

            var result = Regex.IsMatch(key, Constants.ORDER_PATTERN);

            Assert.IsTrue(result);

        }
    }
}
