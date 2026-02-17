using System;
using System.Collections.Generic;
using System.Text;

namespace Cati.ADP.Common {
    /// <summary>
    /// Create a list of strings from a separated text
    /// </summary>
    public class ADPStringList : List<string> {
        /// <summary>
        /// Creates a new ADPStringList
        /// </summary>
        /// <param name="separatedText">
        /// Separated text containing the values to be used to populate the string list
        /// </param>
        /// <param name="separator">
        /// Separator string
        /// </param>
        /// <param name="trimItems">
        /// If true, remove the spaces before and after each value
        /// </param>
        public ADPStringList(string separatedText, string separator, bool trimItems)
            : base() {
            Initialize(separatedText, separator, trimItems);
        }
        /// <summary>
        /// Initializes a ADPStringList
        /// </summary>
        /// <param name="separatedText">
        /// Separated text containing the values to be used to populate the string list
        /// </param>
        /// <param name="separator">
        /// Separator string
        /// </param>
        /// <param name="trimItems">
        /// If true, remove the spaces before and after each value
        /// </param>
        private void Initialize(string separatedText, string separator, bool trimItems) {
            separatorText = separator;
            canTrimItems = trimItems;
            DelimitedText = separatedText;
        }
        /// <summary>
        /// If true, remove the spaces before and after each value
        /// </summary>
        private bool canTrimItems;
        /// <summary>
        /// Separator string
        /// </summary>
        private string separatorText;
        /// <summary>
        /// Gets and sets a delimited text containing the values used to populate the string list
        /// </summary>
        public string DelimitedText {
            get {
                string result = "";
                foreach (string s in this) {
                    result += separatorText + s;
                }
                result = result.Remove(0, separatorText.Length);
                return result;
            }
            set {
                //A,,B
                string separatedText = value;
                while (separatedText != "") {
                    int k = separatedText.IndexOf(separatorText);
                    if (k == -1) {
                        k = separatedText.Length;
                    }
                    string s = separatedText.Substring(0, k);
                    if (k == separatedText.Length) {
                        k = k - 1;
                    }
                    separatedText = separatedText.Substring(k + 1);
                    if (canTrimItems) {
                        s = s.Trim();
                    }
                    this.Add(s);
                }
            }
        }
    }
}
