using Microsoft.AnalysisServices.Tabular;
using playground;

// Constants
const string host = "localhost:52699";
const string db = "5c69ad18-ab81-4aff-92c3-6e9bb77b60bc";
const string ConnectionString = "DataSource=" + host;

// Connect to Server
Server server = new();
server.Connect(ConnectionString);

var database = server.Databases.GetByName(db);
var model = database.Model;

// get column dependencies
var columnDependency = Helper.CountColumnDependencies(model);

// get measure dependencies
var measureDependency = Helper.CountMeasureDependencies();

// Unused Columns in model

Console.WriteLine("========Columns Not Used in Model===========");
foreach (var column in columnDependency.objectDependency.Keys)
{
    if (!(columnDependency.objectDependency[column]["isUsedInSortByColumn"] || columnDependency.objectDependency[column]["isUsedInHeirarchy"] || columnDependency.objectDependency[column]["isUsedInMeasure"] || columnDependency.objectDependency[column]["isUsedInRelationship"] || columnDependency.objectDependency[column]["isUsedInRoles"] || columnDependency.objectDependency[column]["isUsedInIncrementalRefersh"]))
    {
        Console.WriteLine(column);
    }
}

Console.WriteLine("========Measure Not Used by any other Measure in Model===========");
foreach (var measure in measureDependency.objectDependency.Keys)
{
    if (!measureDependency.objectDependency[measure]["isUsedByMeasure"])
    {
        Console.WriteLine(measure);
    }
}

// disconnect server
server.Disconnect();