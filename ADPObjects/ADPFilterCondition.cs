using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Cati.ADP.Common;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Specifies the comparision operation that will compose the filter condition
    /// </summary>
    public enum ADPOperator {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        Between,
        StartingWith,
        Containing,
        IsNull,
        IsNotNull
    }

    /// <summary>
    /// A condition to be applied to a filter operation.
    /// Extends ADPPropertyValue.
    /// </summary>
    public sealed class ADPFilterCondition {
        /// <summary>
        /// Create a filter condition
        /// </summary>
        /// <param name="type">
        /// Type of object that will be envolved in the comparision
        /// </param>
        public ADPFilterCondition(Type type) {
            objectType = type;
        }
        /// <summary>
        /// Create a filter condition
        /// </summary>
        /// <param name="type">
        /// Type of object that will be envolved in the comparision
        /// </param>
        /// <param name="conditionExpression">
        /// Textual representation of the new filter condition
        /// Ex: (Name starting with 'Rafael')
        /// </param>
        public ADPFilterCondition(Type type, string conditionExpression) {
            objectType = type;
            ConditionExpression = conditionExpression;
        }
        /// <summary>
        /// Type of object that will be envolved in the comparision
        /// </summary>
        private Type objectType;
        /// <summary>
        /// Operator to be used in the comparision
        /// </summary>
        public ADPOperator Operator;
        /// <summary>
        /// Converts a ADPOperator to its textual representation
        /// </summary>
        /// <param name="op">
        /// ADPOperator to be converted
        /// </param>
        /// <returns>
        /// Textual representation of the given ADPOperator
        /// </returns>
        private string GetOperator(ADPOperator op) {
            switch (op) {
                case ADPOperator.Between:
                    return "between";
                case ADPOperator.Containing:
                    return "containing";
                case ADPOperator.GreaterThan:
                    return ">";
                case ADPOperator.GreaterThanOrEqualTo:
                    return ">=";
                case ADPOperator.LessThan:
                    return "<";
                case ADPOperator.LessThanOrEqualTo:
                    return "<=";
                case ADPOperator.NotEquals:
                    return "!=";
                case ADPOperator.StartingWith:
                    return "starting with";
                case ADPOperator.IsNull:
                    return "is null";
                case ADPOperator.IsNotNull:
                    return "is not null";
                default:
                    return "==";
            }
        }
        /// <summary>
        /// Converts the textual representation of 
        /// an operator to a ADPOperator
        /// </summary>
        /// <param name="op">
        /// Textual representation of an ADPOperator
        /// </param>
        /// <returns>
        /// ADPOperator represented by the given textual representation
        /// </returns>
        private ADPOperator GetOperator(string op) {
            switch (op) {
                case "between":
                    return ADPOperator.Between;
                case "containing":
                    return ADPOperator.Containing;
                case ">":
                    return ADPOperator.GreaterThan;
                case ">=":
                    return ADPOperator.GreaterThanOrEqualTo;
                case "<":
                    return ADPOperator.LessThan;
                case "<=":
                    return ADPOperator.LessThanOrEqualTo;
                case "!=":
                    return ADPOperator.NotEquals;
                case "startingwith":
                    return ADPOperator.StartingWith;
                case "isnull":
                    return ADPOperator.IsNull;
                case "isnotnull":
                    return ADPOperator.IsNotNull;
                case "==":
                    return ADPOperator.Equals;
                default:
                    throw new ADPException(String.Format("The operator {0} is not supported!", op));
            }
        }
        /// <summary>
        /// Start value to be considered in comparisions
        /// using the "between" operator
        /// </summary>
        public object StartValue;
        /// <summary>
        /// Final value to be considered in comparisions
        /// using the "between" operator
        /// </summary>
        public object FinalValue;
        /// <summary>
        /// Value to be considered in comparisions
        /// </summary>
        public object Value;

        /// <summary>
        /// Property name or expression to reach the property to be evaluated
        /// </summary>
        public string PropertyName = "";

        /// <summary>
        /// Gets and sets the textual representation of the filter condition
        /// </summary>
        public string ConditionExpression {
            get {
                switch (Operator) {
                    case ADPOperator.Between:
                        return String.Format("({0} {1} {2} and {3})", PropertyName, GetOperator(Operator), StartValue, FinalValue);
                    case ADPOperator.IsNull:
                        return String.Format("({0} {1})", PropertyName, GetOperator(Operator));
                    case ADPOperator.IsNotNull:
                        return String.Format("({0} {1})", PropertyName, GetOperator(Operator));
                    default:
                        return String.Format("({0} {1} {2})", PropertyName, GetOperator(Operator), Value);
                }
            }
            set { 
                //Remove brakets
                int k1, k2;
                string conditionExpression = value.Substring(1,value.Length-2);
                //Remove the quoted property value from the conditionExpression
                //Tip: It is implemented to provide support for property values 
                //containing supported wrong operator names, such as "Name = 'is null'"
                k1 = conditionExpression.IndexOf("'");
                k2 = conditionExpression.LastIndexOf("'");
                string propValue = "";
                string propValueIdentifier = Convert.ToString(Guid.NewGuid());
                if ((k1 > -1) && (k2 > k1)) {
                    propValue = conditionExpression.Substring(k1, k2 - k1 + 1);
                    conditionExpression = conditionExpression.Remove(k1, k2 - k1 + 1);
                    conditionExpression = conditionExpression.Insert(k1, propValueIdentifier);
                }
                //Handle supported wrong operator names
                conditionExpression = conditionExpression.Replace(" = ", " == ");
                conditionExpression = conditionExpression.Replace("<>", "!=");
                conditionExpression = conditionExpression.Replace("starting with", "startingwith");
                conditionExpression = conditionExpression.Replace("is null", "isnull");
                conditionExpression = conditionExpression.Replace("is not null", "isnotnull");
                conditionExpression = conditionExpression.Replace("'", "");
                if ((k1 > -1) && (k2 > k1)) {
                    //Bring the propValue back to the conditionExpression
                    conditionExpression = conditionExpression.Replace(propValueIdentifier, propValue);
                }
                //Get property name
                k1 = conditionExpression.IndexOf(" ");
                string propertyName = conditionExpression.Substring(0, k1);
                conditionExpression = conditionExpression.Remove(0, k1);
                conditionExpression = conditionExpression.Trim();
                PropertyName = propertyName;
                //Get operator
                k2 = conditionExpression.IndexOf(" ");
                if (k2 == -1) {
                    k2 = conditionExpression.Length;
                }
                string operatorName = conditionExpression.Substring(0, k2);
                conditionExpression = conditionExpression.Remove(0, k2);
                conditionExpression = conditionExpression.Trim();
                Operator = GetOperator(operatorName);
                //Get value
                if (Operator == ADPOperator.Between) {
                    int k3 = conditionExpression.IndexOf(" ");
                    StartValue = conditionExpression.Substring(0, k3);
                    conditionExpression = conditionExpression.Remove(0, k1);
                    conditionExpression = conditionExpression.Trim();
                    FinalValue = conditionExpression.Trim();
                } else {
                    Value = conditionExpression;
                }
            }
        }
        /// <summary>
        /// Gets a SQL kind textual representation of the filter condition
        /// </summary>
        public string SqlConditionExpression {
            get {
                switch (Operator) {
                    case ADPOperator.Between:
                        return String.Format("({0} {1} {2} and {3})", PropertyName, GetOperator(Operator), StartValue, FinalValue);
                    case ADPOperator.IsNull:
                        return String.Format("({0} {1})", PropertyName, GetOperator(Operator));
                    case ADPOperator.IsNotNull:
                        return String.Format("({0} {1})", PropertyName, GetOperator(Operator));
                    case ADPOperator.Containing:
                        return String.Format("({0} LIKE '%{1}%')", PropertyName, ADPUtils.TrimQuotes(Convert.ToString(Value)));
                    case ADPOperator.NotEquals:
                        return String.Format("({0} <> {1})", PropertyName, Value);
                    case ADPOperator.Equals:
                        return String.Format("({0} = {1})", PropertyName, Value);
                    default:
                        return String.Format("({0} {1} {2})", PropertyName, GetOperator(Operator), Value);
                }
            }
        }
        /// <summary>
        /// Evaluate the condition agains a given object
        /// </summary>
        /// <param name="item">
        /// Object to be evaluated
        /// </param>
        /// <returns>
        /// True if the comparision is true
        /// </returns>
        internal bool Evaluate(object item) {
            //Check if the property is comparable
            object o = ADPObject.GetExpressionValue(PropertyName, item);
            if (!(o is IComparable)) {
                return false;
            }
            IComparable currentValue = (IComparable)o;
            //Get the values to be compared
            bool result = false;
            object evaluateStartValue = ADPUtils.TrimQuotes(Convert.ToString(StartValue));
            object evaluateFinalValue = ADPUtils.TrimQuotes(Convert.ToString(FinalValue));
            object evaluateValue = ADPUtils.TrimQuotes(Convert.ToString(Value));
            if (currentValue != null) {
                //Convert start and final value to the correct type
                if (Operator == ADPOperator.Between) {
                    evaluateFinalValue = Convert.ChangeType(evaluateFinalValue, currentValue.GetType());
                    evaluateStartValue = Convert.ChangeType(evaluateStartValue, currentValue.GetType());
                }
                //Convert the value to the correct type
                evaluateValue = Convert.ChangeType(evaluateValue, currentValue.GetType());
            }
            //Performs comparison
            string s;
            switch (Operator) {
                case ADPOperator.Between:
                    result = (currentValue.CompareTo(evaluateStartValue) >= 0) && (currentValue.CompareTo(evaluateFinalValue) <= 0);
                    return result;
                case ADPOperator.Containing:
                    s = Convert.ToString(currentValue);
                    result = s.Contains(Convert.ToString(evaluateValue));
                    return result;
                case ADPOperator.GreaterThan:
                    return (currentValue.CompareTo(evaluateValue) > 0);
                case ADPOperator.GreaterThanOrEqualTo:
                    return (currentValue.CompareTo(evaluateValue) >= 0);
                case ADPOperator.LessThan:
                    return (currentValue.CompareTo(evaluateValue) < 0);
                case ADPOperator.LessThanOrEqualTo:
                    return (currentValue.CompareTo(evaluateValue) <= 0);
                case ADPOperator.NotEquals:
                    return (!currentValue.Equals(evaluateValue));
                case ADPOperator.StartingWith:
                    s = Convert.ToString(currentValue);
                    result = s.IndexOf(Convert.ToString(evaluateValue)) == 0;
                    return result;
                case ADPOperator.IsNull:
                    return currentValue == null;
                case ADPOperator.IsNotNull:
                    return currentValue != null;
                default:
                    return currentValue.Equals(evaluateValue);
            }
        }
    }
}
