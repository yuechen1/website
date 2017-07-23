using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HtmlAgilityPack;
using WebApplication1.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public List<Manga> Get(string id)
        {
            var db = new mangaEntities1();
            List<Manga> returnmagnalist = new List<Manga>();

            var searchurl = "http://www.mangareader.net/search/?w=" + id; //for testing
            var htmlWeb = new HtmlWeb();
            var documentNode = htmlWeb.Load(searchurl);
            HtmlNode searchreturn = documentNode.GetElementbyId("mangaresults");
            var searchlist = searchreturn.SelectNodes("//div[@class = 'mangaresultinner']");
            if(searchlist != null)
            {
                foreach(HtmlNode i in searchlist)
                {
                    //get the name
                    var namenode = i.Descendants("a").First();
                    var name = namenode.Attributes["href"].Value;
                    //see if its in the database, if not get the manga
                    var findmanga = db.Mangas.Find(name);
                    if (findmanga == null)
                    {
                        var dname = "<strong>" + namenode.InnerHtml + "</strong>";
                        var imgnode = i.Descendants("div").First(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "imgsearchresults");
                        var imgnot = imgnode.Attributes["style"].Value;
                        var imglist = imgnot.Split('\'');
                        var img = imglist[1];
                        var chapter = i.Descendants("div").First(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "chapter_count");
                        var chapternot = chapter.InnerHtml;
                        var chapterlist = chapternot.Split('&');
                        var chaptercount = chapterlist[0];
                        var chapterint = int.Parse(chaptercount);
                        var mangaurl = "http://www.mangareader.net" + name;
                        Manga manga = new Manga { name = name, displayname = dname, url = mangaurl, img = img, totalchapters = chapterint };
                        try
                        {
                            db.Mangas.Add(manga);
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}",
                                                            validationError.PropertyName,
                                                            validationError.ErrorMessage);
                                }
                            }
                        }
                        returnmagnalist.Add(manga);
                    }
                    else
                    {
                        if (findmanga.img == null)
                        {
                            var imgnode = i.Descendants("div").First(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "imgsearchresults");
                            var imgnot = imgnode.Attributes["style"].Value;
                            var imglist = imgnot.Split('\'');
                            var img = imglist[1];
                        }
                        if (findmanga.totalchapters == null)
                        {
                            var chapter = i.Descendants("div").First(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "chapter_count");
                            var chapternot = chapter.InnerHtml;
                            var chapterlist = chapternot.Split(' ');
                            var chaptercount = chapterlist[0];
                        }
                        try
                        {
                            db.Mangas.Attach(findmanga);
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}",
                                                            validationError.PropertyName,
                                                            validationError.ErrorMessage);
                                }
                            }
                        }
                        returnmagnalist.Add(findmanga);
                    }
                }
            }

            return returnmagnalist;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
