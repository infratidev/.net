using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSSInfraTI
{
    public class RSS
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime PublishDate { get; set; }
        public string Description { get; set; }

        public RSS()
        {
            Title = "";
            Link = "";
            Description = "";
            PublishDate = DateTime.Now;
         
        }
    }
}
