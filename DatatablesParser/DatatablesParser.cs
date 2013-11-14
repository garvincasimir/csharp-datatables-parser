using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using System.Text.RegularExpressions;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;

namespace DataTablesParser
{
    /// <summary>
    /// Parses the request values from a query from the DataTables jQuery pluggin
    /// </summary>
    /// <typeparam name="T">List data type</typeparam>
    public class DataTablesParser<T> where T : class
    {
        /*
         * int: iDisplayStart - Display start point
        * int: iDisplayLength - Number of records to display
        * string: string: sSearch - Global search field
        * boolean: bEscapeRegex - Global search is regex or not
        * int: iColumns - Number of columns being displayed (useful for getting individual column search info)
        * string: sSortable_(int) - Indicator for if a column is flagged as sortable or not on the client-side
        * string: sSearchable_(int) - Indicator for if a column is flagged as searchable or not on the client-side
        * string: sSearch_(int) - Individual column filter
        * boolean: bEscapeRegex_(int) - Individual column filter is regex or not
        * int: iSortingCols - Number of columns to sort on
        * int: iSortCol_(int) - Column being sorted on (you will need to decode this number for your database)
        * string: sSortDir_(int) - Direction to be sorted - "desc" or "asc". Note that the prefix for this variable is wrong in 1.5.x, but left for backward compatibility)
        * string: sEcho - Information for DataTables to use for rendering
         */

        private const string INDIVIDUAL_DATA_KEY_PREFIX = "mDataProp_";
        private const string INDIVIDUAL_SEARCH_KEY_PREFIX = "sSearch_";
        private const string INDIVIDUAL_SEARCHABLE_KEY_PREFIX = "bSearchable_";
        private const string INDIVIDUAL_SORTABLE_KEY_PREFIX = "bSortable_";
        private const string INDIVIDUAL_SORT_KEY_PREFIX = "iSortCol_";
        private const string INDIVIDUAL_SORT_DIRECTION_KEY_PREFIX = "sSortDir_";
        private const string DISPLAY_START = "iDisplayStart";
        private const string DISPLAY_LENGTH = "iDisplayLength";
        private const string ECHO = "sEcho";
        private const string ASCENDING_SORT = "asc";

        private IQueryable<T> _queriable;
        private readonly HttpRequestBase _httpRequest;
        private readonly Type _type;
        private  PropertyInfo[] _properties;
        private IDictionary<int, PropertyMapping> _propertyMap ;

        //TODO: We may be able to handle other numeric property types if they are translatable
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
            typeof(Nullable<DateTime>) 
            
        };

		public DataTablesParser(HttpRequestBase httpRequest, IQueryable<T> queriable)
        {
            _queriable = queriable;
            _httpRequest = httpRequest;
            _type = typeof(T);

            //user regex instead of throwing exception or using tryparse
             var integerTest = new Regex(@"^\d+$");

            //Is this readable? Well if you can read all this expression stuff then this is nothing~!
            //This associates class properties with relevant datatable configuration options
             _propertyMap = (from key in _httpRequest.Params.AllKeys.Where(k => k.StartsWith(INDIVIDUAL_DATA_KEY_PREFIX))
                             join prop in _type.GetProperties() on _httpRequest[key] equals prop.Name
                             let extractIndex = key.Replace(INDIVIDUAL_DATA_KEY_PREFIX, string.Empty).Trim()
                             let searchable = _httpRequest[INDIVIDUAL_SEARCHABLE_KEY_PREFIX + extractIndex] == null ? true : _httpRequest[INDIVIDUAL_SEARCHABLE_KEY_PREFIX + extractIndex].Trim() == "true"
                             let sortable = _httpRequest[INDIVIDUAL_SORTABLE_KEY_PREFIX + extractIndex] == null ? true : _httpRequest[INDIVIDUAL_SORTABLE_KEY_PREFIX + extractIndex].Trim() == "true" 
                             where integerTest.IsMatch(extractIndex)
                             select new
                             {
                                 index = int.Parse(extractIndex),
                                 map = new PropertyMapping
                                 {
                                     Property = prop,
                                     Searchable = searchable,
                                     Sortable = sortable
                                 }
                             }).Distinct().ToDictionary(k => k.index, v => v.map);
        }

		public DataTablesParser(HttpRequest httpRequest, IQueryable<T> queriable)
            : this(new HttpRequestWrapper(httpRequest), queriable)
        { }

        /// <summary>
        /// Parses the <see cref="HttpRequestBase"/> parameter values for the accepted 
        /// DataTable request values
        /// </summary>
        /// <returns>Formated output for DataTables, which should be serialized to JSON</returns>
       
        public FormatedList<T> Parse()
        {
            var list = new FormatedList<T>();

            // parse the echo property (must be returned as int to prevent XSS-attack)
            list.sEcho = int.Parse(_httpRequest[ECHO]);

            // count the record BEFORE filtering
            list.iTotalRecords =  _queriable.Count();
           
            // apply the sort, if there is one
            ApplySort();

            // parse the paging values
            int skip = 0, take = 10;
            int.TryParse(_httpRequest[DISPLAY_START], out skip);
            int.TryParse(_httpRequest[DISPLAY_LENGTH], out take);

            //This needs to be an expression or else it won't limit results
            Func<T, bool> GenericFind = delegate(T item)
            {
                bool found = false;
                var sSearch = _httpRequest["sSearch"]; 

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

            //Test ofr linq to entities
            //Anyone know of a better way to do this test??
            if (_queriable is ObjectQuery<T> || _queriable is DbQuery<T> ) 
            {

                // setup the data with individual property search, all fields search,
                // paging, and property list selection
                var resultQuery = _queriable.Where(ApplyGenericSearch)
                            .Skip(skip)
                            .Take(take);

                list.aaData = resultQuery.ToList();

                list.SetQuery(resultQuery.ToString());

                // total records that are displayed after filter
                list.iTotalDisplayRecords = _queriable.Count(ApplyGenericSearch);
            }
            else //linq to objects
            {

                // setup the data with individual property search, all fields search,
                // paging, and property list selection
                var resultQuery = _queriable.Where(GenericFind)
                            .Skip(skip)
                            .Take(take);

                list.aaData = resultQuery
                            .ToList();

                list.SetQuery(resultQuery.ToString());

                // total records that are displayed after filter
                list.iTotalDisplayRecords = _queriable.Count(GenericFind);
            }

   

            return list;
        }

        public async Task<FormatedList<T>> ParseAsync()
        {
          return  await Task.Run(() =>
                        {
                            return Parse();
                        });
           
        }
     

        private void ApplySort()
        {
            var sorted = false;
            var paramExpr = Expression.Parameter(typeof(T), "val");

            // Enumerate the keys sort keys in the order we received them
            foreach (string key in _httpRequest.Params.AllKeys.Where(x => x.StartsWith(INDIVIDUAL_SORT_KEY_PREFIX)))
            {
                // column number to sort (same as the array)
                int sortcolumn = int.Parse(_httpRequest[key]);

                // ignore invalid columns 
                if (!_propertyMap.ContainsKey(sortcolumn) || !_propertyMap[sortcolumn].Sortable)
                    continue;

                // get the direction of the sort
                string sortdir = _httpRequest[INDIVIDUAL_SORT_DIRECTION_KEY_PREFIX + key.Replace(INDIVIDUAL_SORT_KEY_PREFIX, string.Empty)];

          
                 var sortProperty = _propertyMap[sortcolumn].Property;
                 var expression1 = Expression.Property(paramExpr, sortProperty);
                 var propType = sortProperty.PropertyType;
                 var delegateType = Expression.GetFuncType(typeof(T), propType);
                 var propertyExpr = Expression.Lambda(delegateType, expression1, paramExpr);
               
                // apply the sort (default is ascending if not specified)
                 string methodName;
                 if (string.IsNullOrEmpty(sortdir) || sortdir.Equals(ASCENDING_SORT, StringComparison.OrdinalIgnoreCase))
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

        /// <summary>
        /// Compound predicate expression with the individual search predicates that will filter the results
        /// per an individual column
        /// </summary>
        //private Expression<Func<T, bool>> IndividualPropertySearch
        //{
        //    get
        //    {
        //        var paramExpr = Expression.Parameter(typeof(T), "val");
        //        Expression whereExpr = Expression.Constant(true); // default is val => True

        //        foreach (string key in _httpRequest.Params.AllKeys.Where(x => x.StartsWith(INDIVIDUAL_SEARCH_KEY_PREFIX)))
        //        {
        //            // parse the property number
        //            int property = -1;
        //            if (!int.TryParse(_httpRequest[key].Replace(INDIVIDUAL_SEARCH_KEY_PREFIX, string.Empty), out property)
        //                || property >= _properties.Length || string.IsNullOrEmpty(_httpRequest[key]))
        //                break; // ignore if the option is invalid

        //            string query = _httpRequest[key].ToLower();

        //            // val.{PropertyName}.ToString().ToLower().Contains({query})
        //            var toStringCall = Expression.Call(
        //                                Expression.Call(
        //                                    Expression.Property(paramExpr, _properties[property]), "ToString", new Type[0]),
        //                                typeof(string).GetMethod("ToLower", new Type[0]));

        //            // reset where expression to also require the current contraint
        //            whereExpr = Expression.And(whereExpr,
        //                                       Expression.Call(toStringCall,
        //                                                       typeof(string).GetMethod("Contains"),
        //                                                       Expression.Constant(query)));

        //        }

        //        return Expression.Lambda<Func<T, bool>>(whereExpr, paramExpr);
        //    }
        //}

        /// <summary>
        /// Expression for an all column search, which will filter the result based on this criterion
        /// </summary>
        private Expression<Func<T, bool>> ApplyGenericSearch
        {
            get
            {
                string search = _httpRequest["sSearch"];

                // default value
                if (string.IsNullOrWhiteSpace(search))
                    return x => true;

                // invariant expressions
                var searchExpression = Expression.Constant(search.ToLower());
                var paramExpression = Expression.Parameter(typeof(T), "val");
                var conversionLength = Expression.Constant(10,typeof(Nullable<int>));
                var conversionDecimals = Expression.Constant(16,typeof(Nullable<int>));
                List<MethodCallExpression> searchProps = new List<MethodCallExpression>();

                foreach (var propMap in _propertyMap)
                {
                    var property = propMap.Value.Property;

                    if (!property.CanWrite || !propMap.Value.Searchable || !_translatable.Any(t => t == property.PropertyType) ) 
                        continue; 

                    Expression stringProp = null; //The result must be a TSQL translatable string expression

                    var propExp = Expression.Property(paramExpression, property);

                    //TODO: find some genius way to categorize numeric properties including their nullable<> variants
                    if (new Type[] {typeof(int),typeof(Nullable<int>), typeof(double), typeof(Nullable<double>), typeof(float),typeof(Nullable<float>)}.Contains( property.PropertyType ))
                    {
                        var toDoubleCall = Expression.Convert(propExp, typeof(Nullable<double>));

                        var doubleConvert = typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(Nullable<double>), typeof(Nullable<int>), typeof(Nullable<int>) });

                        stringProp = Expression.Call(doubleConvert, toDoubleCall,conversionLength,conversionDecimals);

                    }

                    if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(Nullable<decimal>))
                    {
                        var toDoubleCall = Expression.Convert(propExp, typeof(Nullable<decimal>));

                        var doubleConvert = typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(Nullable<decimal>), typeof(Nullable<int>), typeof(Nullable<int>) });

                        stringProp = Expression.Call(doubleConvert, toDoubleCall,conversionLength, conversionDecimals);

                    }

                    //TODO: Either remove this or provide a way to customize 
                    else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(Nullable<DateTime>))
                    {

                        var date = Expression.Convert(propExp, typeof(Nullable<DateTime>));
                        //Only way to get a number for date part
                        var datePart = typeof(SqlFunctions).GetMethod("DatePart", new Type[] { typeof(string), typeof(Nullable<DateTime>) });
                        //get day of month and year which are already strings
                        var dateName = typeof(SqlFunctions).GetMethod("DateName", new Type[] { typeof(string), typeof(Nullable<DateTime>) });
                        //Call to get month part
                        var month = Expression.Call(datePart, Expression.Constant("m"), date);
                        //Convert to double
                        var toDouble = Expression.Convert(month, typeof(Nullable<double>));
                        //convert to string
                        var monthPartToString = typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(Nullable<double>) });
                        //now all three numerical parts of date as string
                        var monthPart = Expression.Call(monthPartToString, toDouble);
                        var dayPart = Expression.Call(dateName, Expression.Constant("d"), date);
                        var yearPart = Expression.Call(dateName, Expression.Constant("yy"), date);
                        var conCat4 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
                        var conCat2 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                        var delim = Expression.Constant("/");
                        stringProp = Expression.Call(conCat2,
                                                        Expression.Call(conCat4, monthPart, delim, dayPart, delim), yearPart);

                    }

                    else if (property.PropertyType == typeof(string))
                    {
                        stringProp = propExp;
                    }

                    if (stringProp != null)
                    {
                        searchProps.Add(Expression.Call(stringProp, typeof(string).GetMethod("Contains"), searchExpression));
                    }

                }
  
                var propertyQuery = searchProps.ToArray();
                // we now need to compound the expression by starting with the first
                // expression and build through the iterator
                Expression compoundExpression = propertyQuery[0];
               
                // add the other expressions
                for (int i = 1; i < propertyQuery.Length; i++)
                    compoundExpression = Expression.Or(compoundExpression, propertyQuery[i]);

                // compile the expression into a lambda 
                return Expression.Lambda<Func<T, bool>>(compoundExpression, paramExpression);
            }
        }

        private class PropertyMapping
        {
            public PropertyInfo Property { get; set; }
            public bool Sortable { get; set; }
            public bool Searchable { get; set; }
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

        public int sEcho { get; set; }
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public List<T> aaData { get; set; }

    }
}