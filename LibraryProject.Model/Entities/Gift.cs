using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class Gift : BaseEntity
    {
        public string Name { get; set; }
        public int RequiredPoints { get; set; }
        public int Stock { get; set; }
    }
}
