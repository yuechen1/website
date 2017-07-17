using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class newlisting
    {
        public Manga manga { get; set; }
        public List<Chapter> chapters { get; set; }
    }
}