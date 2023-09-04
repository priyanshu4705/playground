using Microsoft.AnalysisServices.Tabular;
using playground;

// Constants
const string host = "localhost:51862";
const string db = "1eee6f89-89b4-470c-812b-6383c2d0c20d";
const string ConnectionString = "DataSource=" + host;

// Connect to Server
Server server = new();
server.Connect(ConnectionString);

var database = server.Databases.GetByName(db);
var model = database.Model;

// get column dependencies
var columnDependency = Helper.CountColumnDependencies(model);

// get measure dependencies
//var measureDependency = Helper.CountMeasureDependencies( measuresDetails );

//var columns1 = JsonConvert.SerializeObject(columnDependency.objectDependency, Formatting.Indented);
//Console.WriteLine(columns1);

// Unused Columns in model

Console.WriteLine("========Columns Not Used in Model===========");
foreach (var column in columnDependency.objectDependency.Keys)
{
    if (!columnDependency.objectDependency[column]["isUsedInSortByColumn"] && !columnDependency.objectDependency[column]["isUsedInHeirarchy"] && !columnDependency.objectDependency[column]["isUsedInMeasure"] && !columnDependency.objectDependency[column]["isUsedInRelationship"] && !columnDependency.objectDependency[column]["isUsedInRoles"] && !columnDependency.objectDependency[column]["isUsedInIncrementalRefersh"])
    {
        Console.WriteLine(column);
    }
}

// disconnect server
server.Disconnect();