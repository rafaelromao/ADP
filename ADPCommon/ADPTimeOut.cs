using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cati.ADP.Common {
    /// <summary>
    /// Controls timeout operations
    /// </summary>
    public class ADPTimeOut {
        /// <summary>
        /// Time were the counting has began
        /// </summary>
        private DateTime startTime;
        /// <summary>
        /// Amount of seconds before to consider the timeout exceeded
        /// </summary>
        private int seconds;
        /// <summary>
        /// Amount of seconds before to consider the timeout exceeded
        /// </summary>
        public int Interval {
            get { return seconds; }
        }
        /// <summary>
        /// Indicates whether the counting is running
        /// </summary>
        private bool enabled = false;
        /// <summary>
        /// Checks if the timeout has been exceeded
        /// </summary>
        /// <returns>
        /// True if exceeded
        /// </returns>
        public bool TimeOutExceeded() {
            if (!enabled) {
                return false;
            } else {
                TimeSpan ts = (DateTime.Now - startTime);
                if (ts.TotalSeconds > seconds) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        /// <summary>
        /// Starts the counting
        /// </summary>
        /// <param name="seconds">
        /// Amount of seconds before to consider the timeout exceeded
        /// </param>
        public void Start(int seconds) {
            if (!enabled) {
                this.seconds = seconds;
                startTime = DateTime.Now;
                enabled = true;
            }
        }
        /// <summary>
        /// Stops the counting
        /// </summary>
        public void Stop() {
            enabled = false;
        }
    }
}
