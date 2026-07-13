using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true; // soft delete icin
    }
}
