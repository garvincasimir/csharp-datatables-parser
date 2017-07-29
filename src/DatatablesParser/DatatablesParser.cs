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

        //Global configs
        private int _take;
        private int _skip;
        private bool _sortDisabled = false;

    
        private Type[] _translatable = 
        { 
            typeof(string)
            
        };

        public Parser(IEnumerable<KeyValuePair<string, StringValues>> configParams, IQueryable<T> queriable)
        {
            _queriable = queriable;
            _config = configParams.ToDictionary(k => k.Key,v=> v.Value.First().Trim());
            _type = typeof(T);
            
            //This associates class properties with corresponding datatable configuration options
             _propertyMap = (from param in _config
                             join prop in _type.GetProperties() on param.Value equals prop.Name
                             where Regex.IsMatch(param.Key,Constants.COLUMN_PROPERTY_PATTERN)
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

                                        
            if(_config.ContainsKey(Constants.DISPLAY_START))
            {
                int.TryParse(_config[Constants.DISPLAY_START], out _skip);
            }

            
            if(_config.ContainsKey(Constants.DISPLAY_LENGTH))
            {
                int.TryParse(_config[Constants.DISPLAY_LENGTH], out _take);
            }
            else
            {
                _take = 10;
            }

            _sortDisabled = _config.ContainsKey(Constants.ORDERING_ENABLED) && _config[Constants.ORDERING_ENABLED] == "false";
        }

        public FormatedList<T> Parse()
        {
            var list = new FormatedList<T>();

            // parse the echo property (must be returned as int to prevent XSS-attack)
            list.draw = int.Parse(_config[Constants.DRAW]);

            // count the record BEFORE filtering
            list.recordsTotal =  _queriable.Count();

            //sort results if sorting isn't disabled or skip needs to be called
            if(!_sortDisabled || _skip > 0)
            {
                ApplySort();
            }

            IEnumerable<T> resultQuery;
            var hasFilterText = !string.IsNullOrWhiteSpace(_config[Constants.SEARCH_KEY]);
            //Use query expression to return filtered paged list
            //This is a best effort to avoid client evaluation whenever possible
            //No good api to determine support for .ToString() on a type
            if(_queriable.Provider is System.Linq.EnumerableQuery && hasFilterText)
            {     
                 resultQuery = _queriable.Where(EnumerablFilter)
                            .Skip(_skip)
                            .Take(_take);

                list.recordsFiltered =  _queriable.Count(EnumerablFilter);
            }
            else if(hasFilterText)
            {
                var entityFilter = GenerateEntityFilter();
                resultQuery = _queriable.Where(entityFilter)
                            .Skip(_skip)
                            .Take(_take);
                            
                list.recordsFiltered =  _queriable.Count(entityFilter);           
            }
            else
            {
                resultQuery = _queriable
                            .Skip(_skip)
                            .Take(_take);
                            
                list.recordsFiltered =  list.recordsTotal;

            }
            

            list.data = resultQuery
                            .ToList();

            list.SetQuery(resultQuery.ToString());

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

                // ignore disabled columns 
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
            if (!sorted )
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


        private bool EnumerablFilter(T item)
        {
                bool found = false;

                var sSearch = _config[Constants.SEARCH_KEY]; 
               
                foreach (var map in _propertyMap)
                {

                    if (map.Value.Searchable && Convert.ToString(map.Value.Property.GetValue(item, null)).ToLower().Contains((sSearch).ToLower()))
                    {
                        found = true;
                    }
                }
                return found;
        }

        /// <summary>
        /// Expression for an all column search, which will filter the result based on this criterion
        /// </summary>
        private Expression<Func<T, bool>> GenerateEntityFilter()
        {

                string search = _config[Constants.SEARCH_KEY];

                // invariant expressions
                var searchExpression = Expression.Constant(search.ToLower());
                var paramExpression = Expression.Parameter(typeof(T), "val");
                List<MethodCallExpression> searchProps = new List<MethodCallExpression>();

                foreach (var propMap in _propertyMap)
                {
                    var property = propMap.Value.Property;

                    if (!property.CanWrite || !propMap.Value.Searchable || !_translatable.Any(t => t == property.PropertyType) ) 
                    {
                        continue; 
                    }

                    var propExp = Expression.Property(paramExpression, property);
  
                    searchProps.Add(Expression.Call(propExp, typeof(string).GetMethod("Contains"), searchExpression));

                }
  
                var propertyQuery = searchProps.ToArray();
                // we now need to compound the expression by starting with the first
                // expression and build through the iterator
                Expression compoundExpression = propertyQuery[0];
               
                // add the other expressions
                for (int i = 1; i < propertyQuery.Length; i++)
                {
                    compoundExpression = Expression.Or(compoundExpression, propertyQuery[i]);
                }

                // compile the expression into a lambda 
                return Expression.Lambda<Func<T, bool>>(compoundExpression, paramExpression);
            
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
        public const string ORDERING_ENABLED = "ordering";

        public static string GetKey(string format,string index)
        {
            return String.Format(format, index);
        }
    }
}
