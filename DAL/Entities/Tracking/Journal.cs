using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities.Core;

namespace DAL.Entities.Tracking
{
    public class Journal
    {
        public int Id { get; set; }
        public string PatientId { get; set; }
        public PatientProfile Patient { get; set; }

        public DateTime CreatedAt { get; set; }
        public string Content { get; set; }
        public string? Mood { get; set; }
    }
}