using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WhatPillsBot.Infrastructure
{
    public class WhatPillsContext:DbContext
    {
        public WhatPillsContext():base("PillsDB") { }
    
        public DbSet<Model.Color> Colors { get; set; }

        public DbSet<Model.Shape> Shape { get; set; }
    }
}