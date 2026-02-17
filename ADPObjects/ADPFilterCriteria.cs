using System;
using System.Collections.Generic;
using System.Text;
using Cati.ADP.Common;

namespace Cati.ADP.Objects {
    /// <summary>
    /// A group of conditions to be used to perform filter operations
    /// </summary>
    public class ADPFilterCriteria {
        /// <summary>
        /// Creates a filter criteria
        /// </summary>
        /// <param name="type">
        /// Type of object that will be envolved in the comparision
        /// </param>
        public ADPFilterCriteria(Type type) {
            objectType = type;
            FilterConditions = new List<ADPFilterCondition>();
        }
        /// <summary>
        /// Creates a filter criteria
        /// </summary>
        /// <param name="type">
        /// Type of object that will be envolved in the comparision
        /// </param>
        /// <param name="filterExpression">
        /// Textual representation of the new filter criteria
        /// Ex: (Name starting with 'Rafael') and (Id is not null)
        /// </param>
        public ADPFilterCriteria(Type type, string filterExpression) {
            objectType = type;
            FilterConditions = new List<ADPFilterCondition>();
            FilterExpression = filterExpression;
        }
        /// <summary>
        /// An string that identifies the filter criteria
        /// </summary>
        public string Id;
        /// <summary>
        /// The group of conditions that composes the criteria
        /// </summary>
        public List<ADPFilterCondition> FilterConditions;
        /// <summary>
        /// In the ADPObject load methods, its used to indicate the type of SQL
        /// statement must be automatic generated from this criteria
        /// </summary>
        public ADPSQLStatementType StatementType = ADPSQLStatementType.AutoBuiltCustomSelect;
        /// <summary>
        /// Type of object that will be envolved in the comparision
        /// </summary>
        Type objectType;
        /// <summary>
        /// Textual representation of the filter criteria
        /// </summary>
        internal string FilterExpression {
            get {
                string result = "";
                foreach (ADPFilterCondition c in FilterConditions) {
                    result += " and ";
                    result += c.ConditionExpression;
                }
                //Removes the first 'and '
                result = result.Remove(0, 4);
                return result;
            }
            set {
                string filterExpression = value;
                if ((filterExpression == null) || (filterExpression == "")) {
                    FilterConditions.Clear();
                    return;
                }
                if (!filterExpression.Contains("(")) {
                    filterExpression = "(" + filterExpression + ")";
                }
                List<string> conditionExpressions = new List<string>();
                while (filterExpression.IndexOf("(") > -1) { 
                    int k1 = filterExpression.IndexOf("(");
                    int k2 = filterExpression.IndexOf(")");
                    string conditionExpression = filterExpression.Substring(k1, k2 - k1 + 1);
                    filterExpression = filterExpression.Trim();
                    k1 = filterExpression.IndexOf("(");
                    filterExpression = filterExpression.Remove(0, k1);
                    filterExpression = filterExpression.Trim();
                    ADPFilterCondition condition = new ADPFilterCondition(objectType, conditionExpression);
                    FilterConditions.Add(condition);
                    k2 = filterExpression.IndexOf(")");                    
                    filterExpression = filterExpression.Remove(0, k2 + 1);
                    filterExpression = filterExpression.Trim();
                }
            }
        }
        /// <summary>
        /// SQL kind textual representation of the filter criteria
        /// </summary>
        internal string SqlExpression {
            get {
                string result = "";
                if (FilterConditions.Count > 0) {
                    foreach (ADPFilterCondition c in FilterConditions) {
                        result += " and ";
                        result += c.SqlConditionExpression;
                    }
                    //Removes the first 'and '
                    result = result.Remove(0, 4);
                }
                return result;
            }
        }
        /// <summary>
        /// Properties that must be used to sort the queries. Used with autobuilt statements.
        /// </summary>
        internal string SortExpression = "";
    }
}
