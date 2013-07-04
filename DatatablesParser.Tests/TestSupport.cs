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
            Params.Add("sEcho", "1");
            Params.Add("iColumns", "6");
            Params.Add("sColumns", "");
            Params.Add("iDisplayStart", "0");
            Params.Add("iDisplayLength", "10");
            Params.Add("mDataProp_0", "FirstName");
            Params.Add("mDataProp_1", "LastName");
            Params.Add("mDataProp_2", "BirthDate");
            Params.Add("mDataProp_3", "Weight");
            Params.Add("mDataProp_4", "Height");
            Params.Add("mDataProp_5", "Children");
            Params.Add("sSearch", "");
            Params.Add("bRegex", "false");
            Params.Add("sSearch_0", "");
            Params.Add("bRegex_0", "false");
            Params.Add("bSearchable_0", "true");
            Params.Add("sSearch_1", "");
            Params.Add("bRegex_1", "false");
            Params.Add("bSearchable_1", "true");
            Params.Add("sSearch_2", "");
            Params.Add("bRegex_2", "false");
            Params.Add("bSearchable_2", "true");
            Params.Add("sSearch_3", "");
            Params.Add("bRegex_3", "false");
            Params.Add("bSearchable_3", "true");
            Params.Add("sSearch_4", "");
            Params.Add("bRegex_4", "false");
            Params.Add("bSearchable_4", "true");
            Params.Add("sSearch_5", "");
            Params.Add("bRegex_5", "false");
            Params.Add("bSearchable_5", "true");
            Params.Add("iSortCol_0", "0");
            Params.Add("sSortDir_0", "asc");
            Params.Add("iSortingCols", "1");
            Params.Add("bSortable_0", "true");
            Params.Add("bSortable_1", "true");
            Params.Add("bSortable_2", "true");
            Params.Add("bSortable_3", "true");
            Params.Add("bSortable_4", "true");
            Params.Add("bSortable_5", "true");
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
                Weight = p.Weight
            });

        }

    }
}
