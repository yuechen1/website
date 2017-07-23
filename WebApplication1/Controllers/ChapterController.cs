using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebApplication1.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using HtmlAgilityPack;

namespace WebApplication1.Controllers
{

    public class ChapterController : ApiController
    {
        // GET: api/Chapter
        public void Get()
        {
            
        }

        // GET: api/Chapter/5
        public findchapter Get(string id)
        {
            int chapternum = -1;
            findchapter returnchapter = new findchapter { dname = null, thischapter = null };

            id = HttpUtility.UrlDecode(id);
            var manga_chapter = id.Split(' ');
            if (manga_chapter.Count() != 2)
            {
                return returnchapter;
            }
            var manganame = manga_chapter[0];
            var chapternumber = manga_chapter[1];

            try
            {
                chapternum = int.Parse(chapternumber);
            }
            catch(FormatException)
            {
                Console.WriteLine("Chapter: chapter number cannot be found");
            }
            catch(OverflowException)
            {
                Console.WriteLine("Chapter: invalid chapter number");
            }

            if (manganame != null && chapternumber != null)
            {
                var db = new mangaEntities1();

                Manga manga = db.Mangas.Find('/' + manganame);
                if (manga != null)
                {
                    bool newchapter = false;

                    Chapter chapter = db.Chapters.Find(manga.name, chapternum);
                    if(chapter == null)
                    {
                        if(chapternum <= manga.totalchapters)
                        {
                            newchapter = true;
                            chapter = new Chapter { manganame = manga.name, number = chapternum, url = null};
                        }
                        else
                        {
                            return returnchapter;
                        }
                    }

                    //get the chapter spacing from the site
                    if (chapter.spacing == null || chapter.url == null)
                    {
                        //get the img src of the first img, and store that in url
                        var htmlWeb = new HtmlWeb();
                        string loadurl = "http://www.mangareader.net" + manga.name + '/' + chapternum;
                        loadurl = loadurl.Replace(" ", String.Empty);
                        var documentNode = htmlWeb.Load(loadurl);
                        HtmlNode img = documentNode.GetElementbyId("imgholder");
                        var tempnode = img.Descendants("a").First();
                        var imgnode = tempnode.SelectSingleNode("//img[@id='img']");
                        if (imgnode == null)
                        {
                            return returnchapter;
                        }
                        string img1src = imgnode.Attributes["src"].Value;
                        chapter.url = img1src;

                        //get the img src of the second img and compair the difference in img number
                        var nextimg = tempnode.Attributes["href"].Value;
                        documentNode = htmlWeb.Load("http://www.mangareader.net" + nextimg);
                        img = documentNode.GetElementbyId("imgholder");
                        tempnode = img.Descendants("a").First();
                        imgnode = tempnode.SelectSingleNode("//img[@id='img']");
                        if (imgnode == null)
                        {
                            return returnchapter;
                        }
                        string img2src = imgnode.Attributes["src"].Value;

                        //compare the difference in img number for this chapter
                        var img1 = img1src.Split('-').Last();
                        img1 = img1.Split('.').First();
                        var img2 = img2src.Split('-').Last();
                        img2 = img2.Split('.').First();
                        int diff = 0;

                        try
                        {
                            diff = int.Parse(img2) - int.Parse(img1);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Chapter: chapter dif number cannot be found");
                        }
                        catch (OverflowException)
                        {
                            Console.WriteLine("Chapter: invalid img number");
                        }
                        chapter.spacing = diff.ToString();

                        try
                        {
                            if (newchapter)
                            {
                                db.Chapters.Add(chapter);
                            }
                            else
                            {
                                db.Chapters.Attach(chapter);
                            }
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
                    }

                    returnchapter.dname = manga.displayname;
                    returnchapter.thischapter = chapter;
                }
                else
                {
                    return returnchapter;
                }


            }
            else
            {
                return returnchapter;
            }


            return returnchapter;
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
