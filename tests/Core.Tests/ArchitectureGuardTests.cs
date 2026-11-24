using System.Reflection;

namespace Core.Tests;

public class ArchitectureGuardTests
{
    [Fact]
    public void CoreAssembly_ShouldNotReference_EntityFrameworkOrInfrastructure()
    {
        var coreAssembly = Assembly.Load("Core");
        var referencedNames = coreAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name is not null)
            .ToList();

        Assert.DoesNotContain(
            referencedNames,
            name => name!.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)
                 || name.StartsWith("Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase)
                 || name.Equals("Infrastructure", StringComparison.OrdinalIgnoreCase));
    }
}
