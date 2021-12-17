using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    public static class EfCoreExtensions
    {
        public static ICollection<T> AsCollection<T>(this DbSet<T> set)
            where T : class
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            return new DbSetCollection<T>(set);
        }
    }
}