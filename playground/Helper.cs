using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json;

namespace playground
{
    public class Helper
    {
        private readonly Dictionary<string, Dictionary<string, string>> rolesDetails;
        private readonly Dictionary<string, Dictionary<string, string>> measuresDetails;
        private readonly Dictionary<string, Dictionary<string, string>> calcGroupDetails;

        private readonly Dictionary<string, string> IRDetails;
        private readonly Dependency measureDependency;
        private readonly Dependency columnDependency;
        private readonly TableCollection tables;
        private readonly List<SingleColumnRelationship> relationships;

        public Helper(Model model)
        {
            measuresDetails = new();
            rolesDetails = new();
            IRDetails = new();
            calcGroupDetails = new();
            measureDependency = new();
            columnDependency = new();
            tables = model.Tables;
            relationships = model.Relationships.Cast<SingleColumnRelationship>().ToList();

            foreach (var table in tables)
            {
                // intialize IRDetails
                if (table.RefreshPolicy != null)
                {
                    BasicRefreshPolicy rp = (BasicRefreshPolicy)table.RefreshPolicy;
                    IRDetails.Add(rp.Table.Name, rp.PollingExpression.Trim());
                }

                // intialize measuresDetails
                foreach (var measure in table.Measures)
                {
                    measuresDetails.Add(measure.Name, new()
                    {
                        { "table", table.Name },
                        { "expression", measure.Expression.Trim() }
                    });
                }

                if (table.CalculationGroup != null)
                {
                    foreach (var calcGrp in table.CalculationGroup.CalculationItems)
                    {
                        calcGroupDetails.Add(calcGrp.Name, new() {
                            {"table", table.Name},
                            {"expression", calcGrp.Expression.Trim()}
                        });

                        Console.WriteLine(calcGrp.Name + " = " + calcGrp.Expression);
                    }
                }

                // intialize columnDepencency
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
                                {"isUsedInIncrementalRefersh",false },
                                {"isUsedInCalculationGroup", false}
                            });
                    }

                    columnDependency.measureDependentOn.Add(columnName, new());
                }
            }

            // initialize roles
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

            // intialize measureDepencency
            foreach (string measureName in measuresDetails.Keys)
            {
                measureDependency.objectDependency.Add(measureName, new() {
                    { "isUsedByMeasure", false }
                });
                measureDependency.measureDependentOn.Add(measureName, new());
            }
        }

        public Dependency CountColumnDependencies()
        {
            foreach (var table in tables)
            {
                // Checking and Adding Sort by columns dependency
                foreach (var column in table.Columns)
                {
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
            foreach (var relationship in relationships)
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

                foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePairInner in calcGroupDetails)
                {
                    if (keyValuePairInner.Value["expression"].IndexOf("'" + name[0] + "'[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1
                        || keyValuePairInner.Value["expression"].IndexOf("" + name[0] + "[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1
                        || (keyValuePairInner.Value["expression"].IndexOf(name[1], StringComparison.OrdinalIgnoreCase) > -1 && keyValuePairInner.Value["table"].Equals(name[0])))
                    {
                        columnDependency.objectDependency[columnUsed]["isUsedInCalculationGroup"] = true;
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
                    if ((keyValuePairInner.Value.IndexOf("\"" + name[0] + "\"[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1) || (keyValuePairInner.Value.IndexOf("" + name[0] + "[" + name[1] + "]", StringComparison.OrdinalIgnoreCase) > -1))
                    {
                        columnDependency.objectDependency[columnUsed]["isUsedInIncrementalRefersh"] = true;
                    }
                }
            }

            return columnDependency;
        }

        public Dependency CountMeasureDependencies()
        {
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

            return measureDependency;
        }

        public Dictionary<string, HashSet<string>> GetMeasureWithSameExpression()
        {
            Dictionary<string, HashSet<string>> measureWithSameExpression = new();

            foreach (var measure in measuresDetails)
            {
                if (!measureWithSameExpression.ContainsKey(measure.Value["expression"]))
                {
                    measureWithSameExpression.Add(measure.Value["expression"], new() { measure.Key });
                }
                else
                {
                    measureWithSameExpression[measure.Value["expression"]].Add(measure.Key);
                }
            }

            return measureWithSameExpression.Where(x => x.Value.Count > 1).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}