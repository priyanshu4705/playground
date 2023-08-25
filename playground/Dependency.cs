namespace playground
{
    public class Dependency
    {
        public Dictionary<string, Dictionary<string, bool>> columnDependencyCount;
        public Dictionary<string, List<string>> measureDependentOn;

        public Dependency()
        {
            columnDependencyCount = new();
            measureDependentOn = new();
        }
    }
}