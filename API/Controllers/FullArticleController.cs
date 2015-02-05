using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using API.Models.OData;

namespace API.Controllers
{
    public class FullArticleController : ApiController
    {
        public IEnumerable<Article> GetArticleById(int id)
        {
            var article = new API.Models.OData.Article() { Code = id, FullName = "FullName", ShortName = "ShortName" };

            var result = new List<Article>();
            result.Add(article);

            return result;
        }
    }
}
