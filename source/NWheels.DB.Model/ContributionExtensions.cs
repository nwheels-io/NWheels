using System;
using NWheels.Composition.Model;

namespace NWheels.DB.Model
{
    public static class ContributionExtensions
    {
        public static void IncludeDatabase<TDB>(this IContributions contributions, Func<DatabaseConfig> config)
            where TDB : DatabaseModel
        {
        }
    }
}
