using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SharedModel.Model;

namespace SharedModel;

public class ProxygenContextFactory : IDesignTimeDbContextFactory<ProxygenContext>
{
    public ProxygenContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProxygenContext>();
        optionsBuilder.UseSqlite("Data Source=proxygen.db", x => x.UseNodaTime());

        return new ProxygenContext(optionsBuilder.Options);
    }
}