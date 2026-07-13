using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class Announcement : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPublished { get; set; } = true;
    }
}
