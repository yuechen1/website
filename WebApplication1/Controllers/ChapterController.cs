using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using HtmlAgilityPack;

namespace WebApplication1.Controllers
{
    public class findchapter
    {
        public string dname { get; set; }
        public Chapter thischapter { get; set; }
    }

    public class ChapterController : ApiController
    {
        // GET: api/Chapter
        public void Get()
        {
            
        }

        // GET: api/Chapter/5
        public findchapter Get(int id)
        {
            return null;
        }

        // POST: api/Chapter
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Chapter/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Chapter/5
        public void Delete(int id)
        {
        }
    }
}
