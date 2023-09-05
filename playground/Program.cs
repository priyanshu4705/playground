using Microsoft.AnalysisServices.Tabular;
using playground;

// Constants
const string host = "localhost:63486";
const string db = "11a9c3eb-7576-4c80-9b4e-ce8910b6c3cc";
const string ConnectionString = "DataSource=" + host;

// Connect to Server
Server server = new();
server.Connect(ConnectionString);

var database = server.Databases.GetByName(db);
var model = database.Model;

Helper helper = new(model);
// get column dependencies
var columnDependency = helper.CountColumnDependencies();

// get measure dependencies
var measureDependency = helper.CountMeasureDependencies();

// get measure with same expression
var measureWithSameExpression = helper.GetMeasureWithSameExpression();

// Unused Columns in model
Console.WriteLine("========Columns Not Used in Model===========");
foreach (var column in columnDependency.objectDependency.Keys)
{
    if (!(columnDependency.objectDependency[column]["isUsedInSortByColumn"] || columnDependency.objectDependency[column]["isUsedInHeirarchy"] || columnDependency.objectDependency[column]["isUsedInMeasure"] || columnDependency.objectDependency[column]["isUsedInRelationship"] || columnDependency.objectDependency[column]["isUsedInRoles"] || columnDependency.objectDependency[column]["isUsedInIncrementalRefersh"]))
    {
        Console.WriteLine(column);
    }
}

// Unused measures in model
Console.WriteLine("========Measure Not Used by any other Measure in Model===========");
foreach (var measure in measureDependency.objectDependency.Keys)
{
    if (!measureDependency.objectDependency[measure]["isUsedByMeasure"])
    {
        Console.WriteLine(measure);
    }
}

foreach (var expression in measureWithSameExpression)
{
    Console.WriteLine(expression.Key + ": " + string.Join(',' , expression.Value));
}

// disconnect server
server.Disconnect();