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
        private IQueryable<T> _queryable;
        private readonly Dictionary<string,string> _config;
        private readonly Type _type;
        private IDictionary<int, PropertyMap> _propertyMap ;

        //Global configs
        private int _take;
        private int _skip;
        private bool _sortDisabled = false;

        private Dictionary<string,Expression> _converters = new Dictionary<string, Expression>();
    
        private Type[] _convertable = 
        { 
            typeof(int), 
            typeof(Nullable<int>), 
            typeof(decimal), 
            typeof(Nullable<decimal>),
            typeof(float),
            typeof(Nullable<float>),
            typeof(double),
            typeof(Nullable<double>),
            typeof(DateTime), 
            typeof(Nullable<DateTime>) 
        };

        public Parser(IEnumerable<KeyValuePair<string, StringValues>> configParams, IQueryable<T> queryable)
        {
            _queryable = queryable;
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
                             let filterKey = Constants.GetKey(Constants.SEARCH_VALUE_PROPERTY_FORMAT,index)
                             let filter = _config.ContainsKey(filterKey)?_config[filterKey]:string.Empty
                             // Set regex when implemented

                             select new
                             {
                                 index = int.Parse(index),
                                 map = new PropertyMap
                                 {
                                     Property = prop,
                                     Searchable = searchable,
                                     Orderable = orderable,
                                     Filter = filter
                                 }
                             }).Distinct().ToDictionary(k => k.index, v => v.map);


            if(_propertyMap.Count == 0 )
            {
                throw new Exception("No properties were found in request. Please map datatable field names to properties in T");
            }

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

        public Results<T> Parse()
        {
            var list = new Results<T>();

            // parse the echo property
            list.draw = int.Parse(_config[Constants.DRAW]);

            // count the record BEFORE filtering
            list.recordsTotal =  _queryable.Count();

            //sort results if sorting isn't disabled or skip needs to be called
            if(!_sortDisabled || _skip > 0)
            {
                ApplySort();
            }


            IEnumerable<T> resultQuery;
            var hasFilterText = !string.IsNullOrWhiteSpace(_config[Constants.SEARCH_KEY]) || _propertyMap.Any( p => !string.IsNullOrWhiteSpace(p.Value.Filter));
            //Use query expression to return filtered paged list
            //This is a best effort to avoid client evaluation whenever possible
            //No good api to determine support for .ToString() on a type
            if(_queryable.Provider is System.Linq.EnumerableQuery && hasFilterText)
            {     
                 resultQuery = _queryable.Where(EnumerablFilter)
                            .Skip(_skip)
                            .Take(_take);

                list.recordsFiltered =  _queryable.Count(EnumerablFilter);
            }
            else if(hasFilterText)
            {
                var entityFilter = GenerateEntityFilter();
                resultQuery = _queryable.Where(entityFilter)
                            .Skip(_skip)
                            .Take(_take);
                            
                list.recordsFiltered =  _queryable.Count(entityFilter);           
            }
            else
            {
                resultQuery = _queryable
                            .Skip(_skip)
                            .Take(_take);
                            
                list.recordsFiltered =  list.recordsTotal;

            }
            

            list.data = resultQuery.ToList();

            return list;
        }

        ///<summary>
        /// SetConverter accepts a custom expression for converting a property in T to string. 
        /// This will be used during filtering. 
        ///</summary>
        /// <param name="property">A lambda expression with a member expression as the body</param>
        /// <param name="tostring">A lambda given T returns a string by performing a sql translatable operation on property</param>
        public Parser<T> SetConverter(Expression<Func<T,object>> property, Expression<Func<T,string>> tostring)
        {
            Console.WriteLine(property.Body.NodeType);

            var  memberExp =  ((UnaryExpression)property.Body).Operand as MemberExpression;

            if(memberExp == null)
            {
                throw new ArgumentException("Body in property must be a member expression");
            }

            _converters[memberExp.Member.Name] = tostring.Body;
           
            return this;
        }

        private void ApplySort()
        {
            var sorted = false;
            var paramExpr = Expression.Parameter(_type, "val");

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
                var delegateType = Expression.GetFuncType(_type, propType);
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

                _queryable = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                                && method.IsGenericMethodDefinition
                                && method.GetGenericArguments().Length == 2
                                && method.GetParameters().Length == 2)
                        .MakeGenericMethod(_type, propType)
                        .Invoke(null, new object[] { _queryable, propertyExpr }) as IOrderedQueryable<T>;

                     sorted = true;
            }

			//Linq to entities needs a sort to implement skip
            //Not sure if we care about the queriables that come in sorted? IOrderedQueryable does not seem to be a reliable test
            if (!sorted )
            {
                var firstProp = Expression.Property(paramExpr, _propertyMap.First().Value.Property);
                var propType = _propertyMap.First().Value.Property.PropertyType;
                var delegateType = Expression.GetFuncType(_type, propType);
                var propertyExpr = Expression.Lambda(delegateType, firstProp, paramExpr);
         
                _queryable = typeof(Queryable).GetMethods().Single(
             method => method.Name == "OrderBy"
                         && method.IsGenericMethodDefinition
                         && method.GetGenericArguments().Length == 2
                         && method.GetParameters().Length == 2)
                 .MakeGenericMethod(_type, propType)
                 .Invoke(null, new object[] { _queryable, propertyExpr }) as IOrderedQueryable<T>;

            }

        }


        private bool EnumerablFilter(T item)
        {
                bool found = false;

                var filter = _config[Constants.SEARCH_KEY]; 
               
                foreach (var map in _propertyMap)
                {

                    if (map.Value.Searchable && Convert.ToString(map.Value.Property.GetValue(item, null)).ToLower().Contains((filter).ToLower()))
                    {
                        found = true;
                    }
                }
                return found;
        }

        /// <summary>
        /// Generate a lamda expression based on a search filter for all mapped columns
        /// </summary>
        private Expression<Func<T, bool>> GenerateEntityFilter()
        {

                var paramExpression = Expression.Parameter(_type, "val");

                string filter = _config[Constants.SEARCH_KEY];

                ConstantExpression globalFilterConst = null;
                Expression filterExpr = null;
                if(!string.IsNullOrWhiteSpace(filter))
                {
                    globalFilterConst = Expression.Constant(filter.ToLower());
                }

                List<MethodCallExpression> individualConditions = new List<MethodCallExpression>();
                var modifier = new ModifyParam(paramExpression); //map user supplied converters using a visitor

                foreach (var propMap in _propertyMap)
                {
                    var prop = propMap.Value.Property;
                    var isString = prop.PropertyType == typeof(string);
                    var hasCustom = _converters.ContainsKey(prop.Name);

                    if ((!prop.CanWrite || !propMap.Value.Searchable || (!_convertable.Any(t => t == prop.PropertyType) && !isString )) && !hasCustom ) 
                    {
                        continue; 
                    }

                    ConstantExpression individualFilterConst = null;
                    if(!string.IsNullOrWhiteSpace(propMap.Value.Filter))
                    {
                        individualFilterConst = Expression.Constant(propMap.Value.Filter.ToLower());
                    } 
                    
                    Expression propExp = Expression.Property(paramExpression, prop);
                   
                    if(hasCustom)
                    {
                        propExp = modifier.Visit( _converters[prop.Name]);
                    }
                    else if (!isString)
                    {
                        var toString = prop.PropertyType.GetMethod("ToString", Type.EmptyTypes);

                        propExp = Expression.Call(propExp, toString);

                    }
                    
                    var toLower = Expression.Call(propExp,typeof(string).GetMethod("ToLower", Type.EmptyTypes));

                    if(globalFilterConst!=null)
                    {
                        var globalTest = Expression.Call(toLower, typeof(string).GetMethod("Contains"), globalFilterConst);

                        if(filterExpr == null)
                        {
                            filterExpr = globalTest;
                        }
                        else
                        {
                            filterExpr = Expression.Or(filterExpr,globalTest);
                        }
                    }

                    if(individualFilterConst!=null)
                    {
                        individualConditions.Add(Expression.Call(toLower, typeof(string).GetMethod("Contains"), individualFilterConst));

                    }

                }


                foreach(var condition in individualConditions)
                {
                    if(filterExpr == null)
                    {
                        filterExpr = condition;
                    }
                    else
                    {
                        filterExpr = Expression.AndAlso(filterExpr,condition);
                    }   
                }

                // return the expression as a lambda 
                return Expression.Lambda<Func<T, bool>>(filterExpr, paramExpression);
            
        }

        public class ModifyParam : ExpressionVisitor
        {
            private ParameterExpression _replace;

            public ModifyParam(ParameterExpression p)
            {
                _replace = p;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _replace;
            }

        }

        private class PropertyMap
        {
            public PropertyInfo Property { get; set; }
            public bool Orderable { get; set; }
            public bool Searchable { get; set; }
            public string Regex { get; set; } //Not yet implemented
            public string Filter { get; set; } 
        }

        

    }
    public class Results<T>
    {
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
