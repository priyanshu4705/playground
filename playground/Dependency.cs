namespace playground
{
    public class Dependency
    {
        public Dictionary<string, Dictionary<string, bool>> objectDependency;
        public Dictionary<string, List<string>> measureDependentOn;

        public Dependency()
        {
            objectDependency = new();
            measureDependentOn = new();
        }
    }
}