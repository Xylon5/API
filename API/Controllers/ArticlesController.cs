using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using API.Models.OData;
using Microsoft.Data.OData;

namespace API.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. 
     * Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using API.Models.ArticleData;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Article>("Articles");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class ArticlesController : ODataController
    {
        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();

        // GET: odata/Articles
        public IHttpActionResult GetArticles(ODataQueryOptions<Article> queryOptions)
        {
            // validate the query.
            List<Article> articles;
            try
            {
                queryOptions.Validate(_validationSettings);

                articles = new List<Article>()
                {
                    new Article() { Code = 1000, FullName = "Article 1", ShortName = "article" },
                    new Article() { Code = 1001, FullName = "Article 2", ShortName = "article" },
                    new Article() { Code = 1002, FullName = "Article 3", ShortName = "article" },
                    new Article() { Code = 1003, FullName = "Article 4", ShortName = "article" }
                };
                
            }
            catch (ODataException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok<IEnumerable<Article>>(articles);
        }

        // GET: odata/Articles(5)
        public IHttpActionResult GetArticle([FromODataUri] int key, ODataQueryOptions<Article> queryOptions)
        {
            // validate the query.
            try
            {
                queryOptions.Validate(_validationSettings);
            }
            catch (ODataException ex)
            {
                return BadRequest(ex.Message);
            }

            // return Ok<Article>(article);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // PUT: odata/Articles(5)
        public IHttpActionResult Put([FromODataUri] int key, Delta<Article> delta)
        {
            Validate(delta.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Get the entity here.

            // delta.Put(article);

            // TODO: Save the patched entity.

            // return Updated(article);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // POST: odata/Articles
        public IHttpActionResult Post(Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Add create logic here.

            // return Created(article);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // PATCH: odata/Articles(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Article> delta)
        {
            Validate(delta.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Get the entity here.

            // delta.Patch(article);

            // TODO: Save the patched entity.

            // return Updated(article);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // DELETE: odata/Articles(5)
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            // TODO: Add delete logic here.

            // return StatusCode(HttpStatusCode.NoContent);
            return StatusCode(HttpStatusCode.NotImplemented);
        }
    }
}
