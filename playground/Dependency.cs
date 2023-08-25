namespace playground
{
    public class Dependency
    {
        public Dictionary<string, Dictionary<string, bool>> columnDependency;
        public Dictionary<string, List<string>> measureDependentOn;

        public Dependency()
        {
            columnDependency = new();
            measureDependentOn = new();
        }
    }
}