using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesParser.Tests
{
    /// <summary>
    /// Use this class to get a clean copy of data and params
    /// Help keep the test body smaller 
    /// </summary>
    public static class TestSupport
    {
        private static NameValueCollection Params =  new NameValueCollection();

        private static List<MockPerson> people = new List<MockPerson>
        {
            new MockPerson
            {
                FirstName = "James",
                LastName = "Jamie",
                BirthDate = DateTime.Parse("5/3/1960"),
                Children = 5,
                height = 5.4M,
                Weight = 250M
            },
            new MockPerson
            {
                FirstName = "Tony",
                LastName = "Tonia",
                BirthDate = DateTime.Parse("7/3/1961"),
                Children = 3,
                height = 4.4M,
                Weight = 150M
            },
            new MockPerson
            {
                FirstName = "Bandy",
                LastName = "Momin",
                BirthDate = DateTime.Parse("8/3/1970"),
                Children = 1,
                height = 5.4M,
                Weight = 250M
            },
            new MockPerson
            {
                FirstName = "Tannie",
                LastName = "Tanner",
                BirthDate = DateTime.Parse("2/3/1950"),
                Children = 0,
                height = 6.4M,
                Weight = 350M
            },
            new MockPerson
            {
                FirstName = "Cromie",
                LastName = "Crammer",
                BirthDate = DateTime.Parse("9/3/1953"),
                Children = 15,
                height = 6.2M,
                Weight = 120M
            },
            new MockPerson
            {
                FirstName = "Xorie",
                LastName = "Zera",
                BirthDate = DateTime.Parse("10/3/1974"),
                Children = 2,
                height = 5.9M,
                Weight = 175M
            }
        };

          
        static TestSupport()
        {
            Params.Add(Constants.DRAW, "1");
            Params.Add(Constants.DISPLAY_START, "0");
            Params.Add(Constants.DISPLAY_LENGTH, "10");
            Params.Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "0"), "FirstName");
            Params.Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "0"), "true");
            Params.Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "0"), "");
            Params.Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "0"), "false");

            Params.Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "1"), "LastName");
            Params.Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "1"), "true");
            Params.Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "1"), "");
            Params.Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "1"), "false");

            Params.Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "2"), "BirthDate");
            Params.Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "2"), "true");
            Params.Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "2"), "");
            Params.Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "2"), "false");

            Params.Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "3"), "Weight");
            Params.Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "3"), "true");
            Params.Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "3"), "");
            Params.Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "3"), "false");

            Params.Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "4"), "Height");
            Params.Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "4"), "true");
            Params.Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "4"), "");
            Params.Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "4"), "false");

            Params.Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "5"), "Children");
            Params.Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "5"), "true");
            Params.Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "5"), "");
            Params.Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "5"), "false");

            Params.Add(Constants.GetKey(Constants.DATA_PROPERTY_FORMAT, "6"), "TotalRedBloodCells");
            Params.Add(Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT, "6"), "true");
            Params.Add(Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT, "6"), "");
            Params.Add(Constants.GetKey(Constants.SEARCH_REGEX_PROPERTY_FORMAT, "6"), "false");

            Params.Add(Constants.SEARCH_KEY, "");
            Params.Add(Constants.SEARCH_REGEX_KEY, "false");

            Params.Add(Constants.GetKey(Constants.ORDER_COLUMN_FORMAT, "0"), "0");
            Params.Add(Constants.GetKey(Constants.ORDER_DIRECTION_FORMAT, "0"), "0");

        }

        public static NameValueCollection CreateParams()
        {
            return  new NameValueCollection(Params);
        }

        public static List<MockPerson> CreateData()
        {
            return people.ConvertAll(p => new MockPerson
            {
                FirstName = p.FirstName,
                LastName = p.LastName,
                BirthDate = p.BirthDate,
                Children = p.Children,
                height = p.height,
                Weight = p.Weight,
                TotalRedBloodCells = p.TotalRedBloodCells
            });

        }

    }
}
