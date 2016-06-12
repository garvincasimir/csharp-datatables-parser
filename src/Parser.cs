using System;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DataTablesParser
{
    public class Parser<T> where T : class
    {
        private IQueryable<T> _queriable;
        private readonly Dictionary<string,string> _config;
        private readonly Type _type;
        private IDictionary<int, PropertyMapping> _propertyMap ;
    
        private Type[] _translatable = 
        { 
            typeof(string), 
            typeof(int), 
            typeof(Nullable<int>), 
            typeof(decimal), 
            typeof(Nullable<decimal>),
            typeof(float),
            typeof(Nullable<float>),
            typeof(DateTime), 
            typeof(Nullable<DateTime>),
            typeof(long),
            typeof(Nullable<long>)
            
        };

        public Parser(IEnumerable<KeyValuePair<string, StringValues>> configParams, IQueryable<T> queriable)
        {
            _queriable = queriable;
            _config = configParams.Where(k => Regex.IsMatch(k.Key,Constants.COLUMN_PROPERTY_PATTERN)).ToDictionary(k => k.Key,v=> v.Value.First().Trim());
            _type = typeof(T);
            
            //This associates class properties with relevant datatable configuration options
            //Single pass for key then hash lookups for corresponding properties
             _propertyMap = (from param in _config
                             join prop in _type.GetProperties() on param.Value equals prop.Name
                             let index = Regex.Match(param.Key, Constants.COLUMN_PROPERTY_PATTERN).Groups[1].Value
                             let searchableKey = Constants.GetKey(Constants.SEARCHABLE_PROPERTY_FORMAT,index)
                             let searchable = _config.ContainsKey(searchableKey) && _config[searchableKey] == "true"
                             let orderableKey = Constants.GetKey(Constants.ORDERABLE_PROPERTY_FORMAT, index)
                             let orderable = _config.ContainsKey(orderableKey) && _config[orderableKey]== "true"
                             // Set regex and individual search when implemented

                             select new
                             {
                                 index = int.Parse(index),
                                 map = new PropertyMapping
                                 {
                                     Property = prop,
                                     Searchable = searchable,
                                     Orderable = orderable
                                 }
                             }).Distinct().ToDictionary(k => k.index, v => v.map);
        }

        public FormatedList<T> Parse()
        {
            var list = new FormatedList<T>();

            // parse the echo property (must be returned as int to prevent XSS-attack)
            list.draw = int.Parse(_config[Constants.DRAW]);

            // count the record BEFORE filtering
            list.recordsTotal =  _queriable.Count();

            ApplySort();

                        int skip = 0, take = 10;
            int.TryParse(_config[Constants.DISPLAY_START], out skip);
            int.TryParse(_config[Constants.DISPLAY_LENGTH], out take);

            //This needs to be an expression or else it won't limit results
            Func<T, bool> GenericFind = delegate(T item)
            {
                bool found = false;
                var sSearch = _config[Constants.SEARCH_KEY]; 

                if(string.IsNullOrWhiteSpace(sSearch))
                {
                    return true;
                }
    
                foreach (var map in _propertyMap)
                {

                    if (map.Value.Searchable && Convert.ToString(map.Value.Property.GetValue(item, null)).ToLower().Contains((sSearch).ToLower()))
                    {
                        found = true;
                    }
                }
                return found;

            };


                // setup the data with individual property search, all fields search,
                // paging, and property list selection
                var resultQuery = _queriable.Where(GenericFind)
                            .Skip(skip)
                            .Take(take);

                list.data = resultQuery
                            .ToList();

                list.SetQuery(resultQuery.ToString());


                // total records that are displayed after filter
                list.recordsFiltered = string.IsNullOrWhiteSpace(_config[Constants.SEARCH_KEY])? list.recordsTotal : _queriable.Count(GenericFind);
        

            return list;
        }

        private void ApplySort()
        {
            var sorted = false;
            var paramExpr = Expression.Parameter(typeof(T), "val");

            // Enumerate the keys sort keys in the order we received them
            foreach (var param in _config.Where(k => Regex.IsMatch(k.Key, Constants.ORDER_PATTERN)))
            {
                // column number to sort (same as the array)
                int sortcolumn = int.Parse(param.Value);

                // ignore invalid for disabled columns 
                if (!_propertyMap.ContainsKey(sortcolumn) || !_propertyMap[sortcolumn].Orderable)
                {
                    continue;
                }

                var index = Regex.Match(param.Key, Constants.ORDER_PATTERN).Groups[1].Value;
                var orderDirectionKey = Constants.GetKey(Constants.ORDER_DIRECTION_FORMAT, index);

                // get the direction of the sort
                string sortdir = _config[orderDirectionKey];


                var sortProperty = _propertyMap[sortcolumn].Property;
                var expression1 = Expression.Property(paramExpr, sortProperty);
                var propType = sortProperty.PropertyType;
                var delegateType = Expression.GetFuncType(typeof(T), propType);
                var propertyExpr = Expression.Lambda(delegateType, expression1, paramExpr);
               
                // apply the sort (default is ascending if not specified)
                 string methodName;
                 if (string.IsNullOrEmpty(sortdir) || sortdir.Equals(Constants.ASCENDING_SORT, StringComparison.OrdinalIgnoreCase))
                 {
                     methodName = sorted ? "ThenBy" : "OrderBy";
                 }
                 else
                 {
                     methodName = sorted ? "ThenByDescending" : "OrderByDescending";
                 }

                _queriable = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                                && method.IsGenericMethodDefinition
                                && method.GetGenericArguments().Length == 2
                                && method.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), propType)
                        .Invoke(null, new object[] { _queriable, propertyExpr }) as IOrderedQueryable<T>;

                     sorted = true;
            }

			//Linq to entities needs a sort to implement skip
            //Not sure if we care about the queriables that come in sorted? IOrderedQueryable does not seem to be a reliable test
            if (!sorted)
            {
                var firstProp = Expression.Property(paramExpr, _propertyMap.First().Value.Property);
                var propType = _propertyMap.First().Value.Property.PropertyType;
                var delegateType = Expression.GetFuncType(typeof(T), propType);
                var propertyExpr = Expression.Lambda(delegateType, firstProp, paramExpr);
         
                _queriable = typeof(Queryable).GetMethods().Single(
             method => method.Name == "OrderBy"
                         && method.IsGenericMethodDefinition
                         && method.GetGenericArguments().Length == 2
                         && method.GetParameters().Length == 2)
                 .MakeGenericMethod(typeof(T), propType)
                 .Invoke(null, new object[] { _queriable, propertyExpr }) as IOrderedQueryable<T>;

            }

        }


        private class PropertyMapping
        {
            public PropertyInfo Property { get; set; }
            public bool Orderable { get; set; }
            public bool Searchable { get; set; }
            public string Regex { get; set; } //Not yet implemented
            public string SearchInput { get; set; } //Not yet implemented
        }

    }
    public class FormatedList<T>
    {
        private string _query;

        internal void SetQuery(string query)
        {
            _query = query;
        }

        public string GetQuery()
        {
            return _query;
        }

        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }

    }

    public class Constants
    {
        public const string COLUMN_PROPERTY_PATTERN = @"columns\[(\d+)\]\[data\]";
        public const string ORDER_PATTERN = @"order\[(\d+)\]\[column\]";

        public const string DISPLAY_START = "start";
        public const string DISPLAY_LENGTH = "length";
        public const string DRAW = "draw";
        public const string ASCENDING_SORT = "asc";
        public const string SEARCH_KEY = "search[value]";
        public const string SEARCH_REGEX_KEY = "search[regex]";

        public const string DATA_PROPERTY_FORMAT = "columns[{0}][data]";
        public const string SEARCHABLE_PROPERTY_FORMAT = "columns[{0}][searchable]";
        public const string ORDERABLE_PROPERTY_FORMAT = "columns[{0}][orderable]";
        public const string SEARCH_VALUE_PROPERTY_FORMAT = "columns[{0}][search][value]";
        public const string SEARCH_REGEX_PROPERTY_FORMAT = "columns[{0}][search][regex]";
        public const string ORDER_COLUMN_FORMAT = "order[{0}][column]";
        public const string ORDER_DIRECTION_FORMAT = "order[{0}][dir]";

        public static string GetKey(string format,string index)
        {
            return String.Format(format, index);
        }
    }
}
