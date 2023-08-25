﻿using Microsoft.AnalysisServices.Tabular;

namespace playground
{
    class Helper
    {
        public static Dependency CountColumnDependencies(Dictionary<string, Dictionary<string, string>> columnDetails, Dictionary<string, Dictionary<string, string>> measuresDetails, Model model)
        {
            Dependency columnDependency = new();
            RelationshipCollection relationshipCollection = model.Relationships;

            foreach (string columnName in columnDetails.Keys)
            {
                columnDependency.objectDependency.Add(columnName, new() {
                    {"isUsedInMeasure", false},
                    {"isUsedInRelationship", false},
                    {"isUsedInHeirarchy", false},
                    {"isUsedInSortByColumn", false}
                });

                columnDependency.measureDependentOn.Add(columnName, new());
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

                    // is used in sort by column
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
