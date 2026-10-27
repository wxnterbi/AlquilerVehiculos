
using Microsoft.EntityFrameworkCore;

namespace AlquilerVehiculos.Persistence.Context
{
    public class AlquilerVehiculosContext : DbContext
    {
        public AlquilerVehiculosContext(
            DbContextOptions<AlquilerVehiculosContext> options)
            : base(options)
        {
        }
    }
}
