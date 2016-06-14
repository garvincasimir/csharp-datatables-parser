using DataTablesParser;
using Xunit;
using System.Text.RegularExpressions;

namespace TestLib
{
  
    public class ParameterTests
    {
        [Fact]
        public void TestColumnPropertyPattern()
        {
            var key = "columns[0][data]";

            var result = Regex.IsMatch(key, Constants.COLUMN_PROPERTY_PATTERN);

            Assert.True(result);

        }

        [Fact]
        public void TestOrderPattern()
        {
            var key = "order[0][column]";

            var result = Regex.IsMatch(key, Constants.ORDER_PATTERN);

            Assert.True(result);

        }
    }
}