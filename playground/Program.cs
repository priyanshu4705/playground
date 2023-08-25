using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json;
using playground;

// Constants
const string host = "localhost:62574";
const string db = "43d5f005-d68d-4697-8dac-8ffad64ce9fe";
const string ConnectionString = "DataSource=" + host;

// Connect to Server
Server server = new();
server.Connect(ConnectionString);

var database = server.Databases.GetByName(db);
var model = database.Model;

// Global variables
Dictionary<string, Dictionary<string, string>> measuresDetails = new();
Dictionary<string, Dictionary<string, string>> columnDetails = new();

// Initialize variables
foreach (var table in model.Tables)
{
    foreach (var measure in table.Measures)
    {
        measuresDetails.Add(measure.Name, new()
        {
            { "expression", measure.Expression.Trim() },
            { "table", table.Name },
            { "isHidden", measure.IsHidden.ToString() }
        });
    }

    foreach (var column in table.Columns)
    {
        string columnName = table.Name + "." + column.Name;

        if (column.SortByColumn == null)
            continue;

        columnDetails.Add(columnName, new() {
            { "table", table.Name },
            { "isHidden", column.IsHidden.ToString() },
            { "isKey", column.IsKey.ToString() },
            //{ "isSortBy", column.SortByColumn == null ? "false": "true"},

        });
    }
}

// get column dependencies
var columnDependency = Helper.CountColumnDependencies(columnDetails, measuresDetails, model);

// get measure dependencies
// var measureDependency = Helper.CountMeasureDependencies(columnDetails, measuresDetails);


// outputs

Console.WriteLine("========column details===========");

//var columns = JsonConvert.SerializeObject(columnDetails, Formatting.Indented);
//Console.WriteLine(columns);

var columns1 = JsonConvert.SerializeObject(columnDependency.objectDependency, Formatting.Indented);
Console.WriteLine(columns1);

//var columns2 = JsonConvert.SerializeObject(columnDependency.measureDependentOn, Formatting.Indented);
//Console.WriteLine(columns2);

//Console.WriteLine("========measure details===========");
//var measures = JsonConvert.SerializeObject(measuresDetails, Formatting.Indented);
//Console.WriteLine(measures);

//var measures1 = JsonConvert.SerializeObject(measureDependency.columnDependencyCount, Formatting.Indented);
//Console.WriteLine(measures1);

//var measures2 = JsonConvert.SerializeObject(measureDependency.measureDependentOn, Formatting.Indented);
//Console.WriteLine(measures2);

// disconnect server
server.Disconnect();