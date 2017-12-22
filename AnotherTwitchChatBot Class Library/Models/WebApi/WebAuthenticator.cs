using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.WebApi
{
    public class WebAuthenticator
    {
        private Lazy<HttpClient> httpClientLazyCache;
        private const string baseUrl = "http://bot.sandhead.stream/api/";

        public WebAuthenticator()
        {
            httpClientLazyCache = new Lazy<HttpClient>();
        }


    }
}
