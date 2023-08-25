using Microsoft.AnalysisServices.Tabular;

namespace playground
{
    class Helper
    {
        public static Dependency CountColumnDependencies(Dictionary<string, Dictionary<string, string>> columnDetails, Dictionary<string, Dictionary<string, string>> measuresDetails, Model model)
        {
            Dependency columnDependency = new();
            Dictionary<string, string> hierarchColumns = new();
            Dictionary<string, string> sortByColumn = new();
            RelationshipCollection relationshipCollection = model.Relationships;

            foreach (string columnName in columnDetails.Keys)
            {
                if (!columnName.Contains("RowNumber"))
                {
                    columnDependency.objectDependency.Add(columnName, new() {
                        {"isUsedInMeasure", false},
                        {"isUsedInRelationship", false},
                        {"isUsedInHeirarchy", false},
                        {"isUsedInSortByColumn", false}
                    });
                }
                columnDependency.measureDependentOn.Add(columnName, new());
            }


            foreach (var table in model.Tables)
            {
                // Sort by columns
                foreach (var column in table.Columns)
                {
                    if (column.SortByColumn == null)
                        continue;

                    sortByColumn.Add(column.SortByColumn.Name, table.Name);
                }

                // Hierarchy columns
                foreach (var hierarchy in table.Hierarchies)
                {
                    foreach (var column in hierarchy.Levels)
                    {
                        if (!hierarchColumns.ContainsKey(column.Name))
                        {
                            hierarchColumns.Add(column.Name, hierarchy.Name);
                        }
                    }
                }
            }

            try
            {
                //Applying the logic
                foreach (string columnUsed in columnDetails.Keys)
                {

                    // is used in measure

                    string[] name = columnUsed.Split('.');
                    foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in measuresDetails)
                    {
                        if (keyValuePairInner.Value["expression"].IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            columnDependency.objectDependency[columnUsed]["isUsedInMeasure"] = true;
                            columnDependency.measureDependentOn[columnUsed].Add(keyValuePairInner.Key);
                        }
                    }

                    //get relationship dependencies

                    foreach (SingleColumnRelationship relationship in relationshipCollection.Cast<SingleColumnRelationship>())
                    {
                        if ((relationship.FromTable.Name + "." + relationship.FromColumn.Name).Equals(columnUsed) || (relationship.ToTable.Name + "." + relationship.ToColumn.Name).Equals(columnUsed))
                            columnDependency.objectDependency[columnUsed]["isUsedInRelationship"] = true;
                    }

                    // is used in heirarchy

                    if (hierarchColumns.ContainsKey(name[1]))
                    {
                        columnDependency.objectDependency[columnUsed]["isUsedInHeirarchy"] = true;
                    }

                    // is used in sort by column

                    if (sortByColumn.ContainsKey(name[1]))
                    {
                        columnDependency.objectDependency[columnUsed]["isUsedInSortByColumn"] = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return columnDependency;
        }



        public static Dependency CountMeasureDependencies(Dictionary<string, Dictionary<string, string>> columnDetails, Dictionary<string, Dictionary<string, string>> measuresDetails)
        {
            Dependency measureDependency = new();

            //Declaring and initializing variables

            foreach (string measureName in measuresDetails.Keys)
            {
                measureDependency.objectDependency.Add(measureName, new());
                measureDependency.measureDependentOn.Add(measureName, new());
            }

            try
            {
                //Applying the logic
                foreach (string measureUsed in measuresDetails.Keys)
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in measuresDetails)
                    {
                        if (keyValuePairInner.Value["expression"].IndexOf("[" + measureUsed + "]", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            measureDependency.objectDependency[measureUsed].Add("isUsedByMeasure", true);
                            measureDependency.measureDependentOn[keyValuePairInner.Key].Add(measureUsed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //returning the dictionary Counts
            return measureDependency;
        }
    }
}
