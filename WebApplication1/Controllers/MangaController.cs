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
    public class MangaController : ApiController
    {

        // GET: api/Manga
        public List<newlisting> Get()
        {

            string Url = "http://www.mangareader.net";

            //get the list of new released chapters
            var htmlWeb = new HtmlWeb();
            var documentNode = htmlWeb.Load(Url);
            HtmlNode newchapters = documentNode.GetElementbyId("latestchapters");
            var tempnode = newchapters.SelectNodes("//td[@class = 'c5']");
            List<HtmlNode> tempnodes = new List<HtmlNode>();
            foreach (HtmlNode i in tempnode)
            {
                tempnodes.Add(i.ParentNode);
            }
            var db = new mangaEntities1();


            //split the list into each manga and their chapters
            List<newlisting> NC = new List<newlisting>();
            foreach (HtmlNode k in tempnodes)
            {
                newlisting templisting = new newlisting();

                var urlname = k.Descendants("a").First(x => x.Attributes["class"] != null && x.Attributes["class"].Value == "chapter");   //contains the href and the display name
                var name = urlname.Attributes["href"].Value;    //get the context url for name
                var url = "http://www.mangareader.net" + name;  //get the full url to the page
                var displayname = urlname.InnerHtml;    //get the displayname of the manga, with proper captialisation
                Manga manga = new Manga { name = name, displayname = displayname, url = url, img = null };

                try
                {
                    if (db.Mangas.Find(name) == null)
                    {
                        db.Mangas.Add(manga);
                        db.SaveChanges();
                    }
                }
                catch(DbEntityValidationException dbEx)
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

                //get each chapter as its own object
                templisting.manga = manga;
                List<Chapter> templist = new List<Chapter>();
                var chapters = k.Descendants("a"); //contains the chapter url and a display string
                foreach (HtmlNode i in chapters)
                {
                    if (i.Attributes["class"] == null || i.Attributes["class"].Value != "chaptersrec")
                    {
                        continue;
                    }
                    var chapterurl = i.Attributes["href"].Value;    //get the ref url
                    var split = chapterurl.Split('/');              //split the string to get the chapter number
                    var number = split[split.Count() - 1];
                    chapterurl = "http://www.mangareader.net" + chapterurl; //get the full url
                    Chapter chapter = new Chapter { manganame = name, number = int.Parse(number), url = null, spacing = null };
                    templist.Add(chapter);

                    try
                    {
                        if (db.Chapters.Find(name, int.Parse(number)) == null)
                        {
                            db.Chapters.Add(chapter);
                            db.SaveChanges();
                        }

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
                }


                templisting.chapters = templist;
                NC.Add(templisting);
            }

            return NC;
        }

        // GET: api/Manga/5
        public Manga Get(string id)
        {
            var db = new mangaEntities1();
            var manga = db.Mangas.Find(id);  //try this
            var Url = manga.url;

            //search database for the manga

            //if exsists, check the chapter count
            if (manga != null)
            {
                //might not need, only need the number of chapters
                var htmlWeb = new HtmlWeb();
                var documentNode = htmlWeb.Load(Url);
                HtmlNode newchapters = documentNode.GetElementbyId("bodyust");

                if (manga.img == null || manga.totalchapters == null)
                {
                    db.Mangas.Attach(manga);
                    if (manga.img == null)
                    {
                        var imgholder = newchapters.SelectSingleNode("//div[@id = 'mangaimg']");
                        var img = imgholder.FirstChild.Attributes["src"].Value;
                        manga.img = img;
                    }

                    if (manga.totalchapters == null)
                    {
                        var lastchapter = newchapters.SelectSingleNode("//div[@class = 'chico_manga']").ParentNode;
                        var latestchapter = lastchapter.Descendants("a").First(x => x.Attributes["href"] != null);
                        var chapterurl = latestchapter.Attributes["href"].Value;
                        var lastnumber = chapterurl.Split('/').Last<string>();
                        try
                        {
                            int totalchapters = int.Parse(lastnumber);
                            manga.totalchapters = totalchapters;
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    db.SaveChanges();
                }
            }
            
            return manga;
        }

        // POST: api/Manga
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Manga/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Manga/5
        public void Delete(int id)
        {
        }
    }
}
