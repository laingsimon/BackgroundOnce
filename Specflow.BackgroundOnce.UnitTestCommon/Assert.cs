using System;
using System.Linq;

namespace Specflow.BackgroundOnce.UnitTestCommon
{
    internal static class Assert
    {
        public static void AreEqual<T>(T expected, T actual)
        {
            if (Equals(expected, actual))
            {
                return;
            }

            throw new InvalidOperationException($"Actual ({actual}) doesn't equal expected ({expected})");
        }

        public static void AreEquivalentTo(string[] expected, string[] actual)
        {
            var missing = expected.Except(actual).ToArray();
            if (missing.Any())
            {
                throw new InvalidOperationException($"The following items are missing: {string.Join(", ", missing)}");
            }

            var unexpected = actual.Except(expected).ToArray();
            if (unexpected.Any())
            {
                throw new InvalidOperationException($"The following items were unexpected: {string.Join(", ", unexpected)}");
            }
        }
    }
}