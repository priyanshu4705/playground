using Microsoft.AnalysisServices.Tabular;
using playground;

// Constants
const string host = "localhost:54145";
const string db = "58766809-ac2d-43b1-ada8-ecaad2d9ebb4";
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
// Console.WriteLine("========Columns Not Used in Model===========");
foreach (var column in columnDependency.objectDependency.Keys)
{
   if (columnDependency.objectDependency[column]["isUsedInCalculationGroup"]) Console.WriteLine(column + ": " + columnDependency.objectDependency[column]["isUsedInCalculationGroup"]);
}

// // Unused measures in model
// Console.WriteLine("========Measure Not Used by any other Measure in Model===========");
// foreach (var measure in measureDependency.objectDependency.Keys)
// {
//     if (!measureDependency.objectDependency[measure]["isUsedByMeasure"])
//     {
//         Console.WriteLine(measure);
//     }
// }

// foreach (var expression in measureWithSameExpression)
// {
//     Console.WriteLine(expression.Key + ": " + string.Join(',' , expression.Value));
// }

// disconnect server
server.Disconnect();