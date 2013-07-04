using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DataTablesParser.Tests
{
    public class MockHttpRequest : HttpRequestBase
    {
        private NameValueCollection _params;

        public MockHttpRequest()
        {
             _params = new NameValueCollection();
        }

        public MockHttpRequest(NameValueCollection startParams)
        {
            _params = startParams;
        }

        public override NameValueCollection Params
        {
            get
            {
                return _params;
            }
        }

        public override string this[string key]
        {
            get
            {
                return _params[key];
            }
        }

        public void Add(NameValueCollection addParams)
        {
            _params.Add(addParams);
        }
    }
}
