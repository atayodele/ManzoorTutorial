using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSBOL;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSDAL
{
    public class SSDbContext : IdentityDbContext
    {
        public SSDbContext(DbContextOptions<SSDbContext> options) : base(options)
        {
        }

        public DbSet<Story> Stories { get; set; }
    }
}
