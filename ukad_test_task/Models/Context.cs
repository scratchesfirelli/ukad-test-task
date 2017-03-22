using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ukad_test_task.Models
{
    public class Context
    {
        public class UrlResponseContext : DbContext
        {
            public DbSet<UrlResponseTime> UrlResponseTime { get; set; }
        }
    }
}