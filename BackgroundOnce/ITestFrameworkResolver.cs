namespace BackgroundOnce
{
    /// <summary>
    /// Resolve the test framework in which the specflow features will execute
    /// </summary>
    public interface ITestFrameworkResolver
    {
        TestFramework ResolveTestFramework();
    }
}