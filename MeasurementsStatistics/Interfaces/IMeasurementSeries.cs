using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementsStatistics.Interfaces
{
    /// <summary>
    /// Interface defining the expectations against an implementation supporting statistical data interpretation
    /// </summary>
    public interface IMeasurementSeries
    {
        #region APIs
        /// <summary>
        /// Allows collecting the measurement data so that statistical information can be extracted later on
        /// </summary>
        /// <param name="measurementValue">
        /// Measurement result
        /// </param>
        void Add(double measurementValue);
        #endregion

        #region Properties
        IEnumerable<double> MeasurementValues { get; }
        double Average { get; }
        double Median { get; }
        /// <summary>
        /// Standard Deviation
        /// </summary>
        double Sigma { get; }
        /// <summary>
        /// Standard Deviation relative to the Average (the imprecision of the measurements relative to the measured value)
        /// </summary>
        double RelativeSigma { get; }
        /// <summary>
        /// The slope of the trend line (tangent alpha, where alpha is the angle between the trend line the the X axis)
        /// </summary>
        double Slope { get; }
        Tuple<double, double> LinearInterpolationCoefficients { get; }
        #endregion
    }
}
