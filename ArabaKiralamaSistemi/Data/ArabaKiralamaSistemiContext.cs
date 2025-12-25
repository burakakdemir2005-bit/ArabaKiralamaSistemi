using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArabaKiralamaSistemi.Models;

namespace ArabaKiralamaSistemi.Data
{
    public class ArabaKiralamaSistemiContext : DbContext
    {
        public ArabaKiralamaSistemiContext (DbContextOptions<ArabaKiralamaSistemiContext> options)
            : base(options)
        {
        }

        public DbSet<ArabaKiralamaSistemi.Models.Car> Car { get; set; } = default!;
        public DbSet<ArabaKiralamaSistemi.Models.Rental> Rental { get; set; }
    }
}
