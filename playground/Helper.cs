using Microsoft.AnalysisServices.Tabular;

namespace playground
{
    class Helper
    {
        public static Dictionary<string, Dictionary<string, string>> rolesDetails = new();
        public static Dictionary<string, Dictionary<string, string>> measuresDetails = new();
        public static Dictionary<string, string> IRDetails = new();

        public static Dependency CountColumnDependencies(Model model)
        {
            Dependency columnDependency = new();

            //Create Dictionary to hold all Roles and their Expressions
            ModelRoleCollection roleCollection = model.Roles;
            foreach (ModelRole role in roleCollection)
            {
                foreach (TablePermission tp in role.TablePermissions)
                {
                    rolesDetails.Add(tp.Table.Name + "." + role.Name, new()
                    {
                        { "expression", tp.FilterExpression.Trim() },
                        { "table", tp.Table.Name }
                    });
                }
            }

            //Create Dictionary to hold all IR Expressions and Measures Expressions
            foreach (var table in model.Tables)
            {
                if (table.RefreshPolicy != null)
                {
                    BasicRefreshPolicy rp = (BasicRefreshPolicy)table.RefreshPolicy;
                    IRDetails.Add(rp.Table.Name, rp.PollingExpression.Trim());
                }

                foreach (var measure in table.Measures)
                {
                    measuresDetails.Add(measure.Name, new()
                    {
                        { "table", table.Name },
                        { "expression", measure.Expression.Trim() }
                    });
                }
            }

            foreach (var table in model.Tables)
            {
                // Also adding all the column properties with default value is false
                foreach (var column in table.Columns)
                {
                    string columnName = table.Name + "." + column.Name;

                    if (!columnName.Contains("RowNumber"))
                    {
                        columnDependency.objectDependency.Add(columnName, new() {
                                {"isUsedInMeasure", false},
                                {"isUsedInRelationship", false},
                                {"isUsedInHeirarchy", false},
                                {"isUsedInSortByColumn", false},
                                {"isUsedInRoles",false },
                                {"isUsedInIncrementalRefersh",false }
                            });
                    }
                    columnDependency.measureDependentOn.Add(columnName, new());
                }

                foreach (var column in table.Columns)
                {
                    // Checking and Adding Sort by columns dependency
                    if (column.SortByColumn != null)
                        columnDependency.objectDependency[table.Name + "." + column.SortByColumn.Name]["isUsedInSortByColumn"] = true;
                }

                // Checking and Adding Hierarchy columns dependency
                foreach (var hierarchy in table.Hierarchies)
                {
                    foreach (var column in hierarchy.Levels)
                    {
                        columnDependency.objectDependency[table.Name + "." + column.Name]["isUsedInHeirarchy"] = true;
                    }
                }
            }

            // Checking and Adding Relationship columns dependency
            foreach (SingleColumnRelationship relationship in model.Relationships.Cast<SingleColumnRelationship>())
            {
                columnDependency.objectDependency[relationship.FromTable.Name + "." + relationship.FromColumn.Name]["isUsedInRelationship"] = true;
                columnDependency.objectDependency[relationship.ToTable.Name + "." + relationship.ToColumn.Name]["isUsedInRelationship"] = true;
            }

            // Check the column used in measure and add to the dependency column
            foreach (string columnUsed in columnDependency.objectDependency.Keys)
            {
                string[] name = columnUsed.Split('.');
                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in measuresDetails)
                {
                    if (keyValuePairInner.Value["expression"].IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1
                        || keyValuePairInner.Value["expression"].IndexOf("" + name[0] + "[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1
                        || (keyValuePairInner.Value["expression"].IndexOf(name[1], StringComparison.OrdinalIgnoreCase) > -1 && keyValuePairInner.Value["table"].Equals(name[0])))
                    {
                        columnDependency.objectDependency[columnUsed]["isUsedInMeasure"] = true;
                        columnDependency.measureDependentOn[columnUsed].Add(keyValuePairInner.Key);
                    }
                }

                //Check for columns used in Roles
                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in rolesDetails)
                {
                    if (
                        (keyValuePairInner.Value["expression"].IndexOf(name[1], StringComparison.OrdinalIgnoreCase) > -1 && keyValuePairInner.Value["table"].Equals(name[0])) || (keyValuePairInner.Value["expression"].IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
                        || (keyValuePairInner.Value["expression"].IndexOf("" + name[0] + "[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1))
                    {
                        columnDependency.objectDependency[columnUsed]["isUsedInRoles"] = true;

                    }
                }

                //Check for columns used in Incremental Refresh
                foreach (KeyValuePair<string, string> keyValuePairInner in IRDetails)
                {
                    if ((keyValuePairInner.Value.IndexOf("\"" + name[0] + "\"[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1)
                        || (keyValuePairInner.Value.IndexOf("" + name[0] + "[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1))

                    {
                        columnDependency.objectDependency[columnUsed]["isUsedInIncrementalRefersh"] = true;

                    }
                }

            }

            return columnDependency;
        }

        public static Dependency CountMeasureDependencies()
        {
            Dependency measureDependency = new();

            //Declaring and initializing variables

            foreach (string measureName in measuresDetails.Keys)
            {
                measureDependency.objectDependency.Add(measureName, new() {
                    { "isUsedByMeasure", false }
                });
                measureDependency.measureDependentOn.Add(measureName, new());
            }

            //Applying the logic
            foreach (string measureUsed in measuresDetails.Keys)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in measuresDetails)
                {
                    if (keyValuePairInner.Value["expression"].IndexOf("[" + measureUsed + "]", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        measureDependency.objectDependency[measureUsed]["isUsedByMeasure"] = true;
                        measureDependency.measureDependentOn[keyValuePairInner.Key].Add(measureUsed);
                    }
                }
            }
            
            //returning the dictionary Counts
            return measureDependency;
        }
    }
}
