using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Generic helper class used to compare two objects on sorting operations
    /// </summary>
    /// <typeparam name="T">
    /// Type of the objects to be compared
    /// </typeparam>
    public sealed class ADPSortComparer<T> : IComparer<T> {
        /// <summary>
        /// Collection of ListSortDescription to be used in multiple sorting comparitions
        /// </summary>
        private ListSortDescriptionCollection comparerSortCollection = null;
        /// <summary>
        /// Property to be evaluated in a single sort comparition
        /// </summary>
        private PropertyDescriptor comparerProperty = null;
        /// <summary>
        /// Direction of the sort when performing single sorting operations
        /// </summary>
        private ListSortDirection comparerDirection = ListSortDirection.Ascending;

        /// <summary>
        /// Creates a sort comparer to perform single sort operation
        /// </summary>
        /// <param name="propDesc">
        /// Property to be evaluated
        /// </param>
        /// <param name="direction">
        /// Sort direction
        /// </param>
        public ADPSortComparer(PropertyDescriptor propDesc, ListSortDirection direction) {
            comparerProperty = propDesc;
            comparerDirection = direction;
        }
        /// <summary>
        /// Creates a sort comparer to perform multiple sort operation
        /// </summary>
        /// <param name="sortCollection">
        /// Collection of ListSortDescription to be used in multiple sorting comparitions
        /// </param>
        public ADPSortComparer(ListSortDescriptionCollection sortCollection) {
            comparerSortCollection = sortCollection;
        }

        /// <summary>
        /// Compare two objects according to the sort settings
        /// </summary>
        /// <param name="x">
        /// First object to be evaluated
        /// </param>
        /// <param name="y">
        /// Second object to be evaluated
        /// </param>
        /// <returns>
        /// -1 if the object x is less than the object y
        /// 0 if the object x is equal to the object y
        /// 1 if the objet x is greater than the object y
        /// </returns>
        int IComparer<T>.Compare(T x, T y) {
            if (comparerProperty != null) {
                object xValue = comparerProperty.GetValue(x);
                object yValue = comparerProperty.GetValue(y);
                return CompareValues(xValue, yValue, comparerDirection);
            } else {
                if ((comparerSortCollection != null) &&  (comparerSortCollection.Count > 0)) {
                    return RecursiveCompareInternal(x, y, 0);
                } else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Compare the two given values according to the sort direction
        /// </summary>
        /// <param name="xValue">
        /// First value to be evaluated
        /// </param>
        /// <param name="yValue">
        /// Second value to be evaluated
        /// </param>
        /// <param name="direction">
        /// Sort direction to be considered
        /// </param>
        /// <returns>
        /// If the sort direction is ascending:
        ///   -1 if the value x is less than the value y
        ///   0 if the value x is equal to the value y
        ///   1 if the value x is greater than the value y
        /// If the sort direction is descending:
        ///   1 if the value x is less than the value y
        ///   0 if the value x is equal to the value y
        ///   -1 if the value x is greater than the value y
        /// </returns>
        private int CompareValues(object xValue, object yValue, ListSortDirection direction) {
            int retValue = 0;
            // Can ask the x value
            if (xValue is IComparable)  {
                retValue = ((IComparable)xValue).CompareTo(yValue);
            } else {
                //Can ask the y value
                if (yValue is IComparable) {
                    retValue = ((IComparable)yValue).CompareTo(xValue);
                } else {
                    // not comparable, compare String representations
                    if (!xValue.Equals(yValue)) {
                        retValue = xValue.ToString().CompareTo(yValue.ToString());
                    }
                }
            }
            if (direction == ListSortDirection.Ascending) {
                return retValue;
            } else {
                return retValue * -1;
            }
        }
        /// <summary>
        /// Performs a recursive comparition to allow multiple sorts
        /// </summary>
        /// <param name="x">
        /// First object to be evaluated
        /// </param>
        /// <param name="y">
        /// Second object to be evaluated
        /// </param>
        /// <param name="index">
        /// The index of the sorting operation
        /// </param>
        /// <returns>
        /// If the sort direction is ascending:
        ///   -1 if the object x is less than the object y
        ///   0 if the object x is equal to the object y
        ///   1 if the object x is greater than the object y
        /// If the sort direction is descending:
        ///   1 if the object x is less than the object y
        ///   0 if the object x is equal to the object y
        ///   -1 if the object x is greater than the object y
        /// </returns>
        private int RecursiveCompareInternal(T x, T y, int index) {
            if (index >= comparerSortCollection.Count) {
                return 0; // termination condition
            }
            ListSortDescription listSortDesc = comparerSortCollection[index];
            object xValue = listSortDesc.PropertyDescriptor.GetValue(x);
            object yValue = listSortDesc.PropertyDescriptor.GetValue(y);

            int retValue = CompareValues(xValue, yValue, listSortDesc.SortDirection);
            if (retValue == 0) {
                return RecursiveCompareInternal(x, y, ++index);
            } else {
                return retValue;
            }
        }
    }
}
