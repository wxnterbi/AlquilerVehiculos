
using Microsoft.EntityFrameworkCore;

namespace SistemadeRenta.Persistence.Context
{
    public class SistemadeRentaContext : DbContext
    {
        public SistemadeRentaContext(
            DbContextOptions<SistemadeRentaContext> options)
            : base(options)
        {
        }
    }
}
