using MeasurementsStatistics.Defaults;
using MeasurementsStatistics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementsStatistics.Providers
{
        /// <summary>
        /// Class storing measurement values, offering statistical information about the measurements
        /// </summary>
        /// <remarks>
        /// There is need for at least 2 measurement values in order to obtain relevant statistical information (otherwise exception will be thrown)
        /// </remarks>
        public class MeasurementSeries : IMeasurementSeries
        {
            #region Member Variables
            private List<double> _measurementValues = new List<double>();
            private double _average = UninitializedValues.UninitializedDouble;
            private double _median = UninitializedValues.UninitializedDouble;
            private double _sigma = UninitializedValues.UninitializedDouble;
            private Tuple<double, double> _linearInterpolationCoefficients = null;
            #endregion

            #region Properties
            public IEnumerable<double> MeasurementValues
            {
                get
                {
                    return _measurementValues as IReadOnlyCollection<double>;
                }
            }

            public double Average
            {
                get
                {
                    if (_average == UninitializedValues.UninitializedDouble)
                    {
                        _average = CalculateAverage();
                    }

                    return _average;
                }
            }

            public double Median
            {
                get
                {
                    if (_median == UninitializedValues.UninitializedDouble)
                    {
                        _median = CalculateMedian();
                    }

                    return _median;
                }
            }

            /// <summary>
            /// Standard Deviation
            /// </summary>
            public double Sigma
            {
                get
                {
                    if (_sigma == UninitializedValues.UninitializedDouble)
                    {
                        _sigma = CalculateStandardDeviation();
                    }

                    return _sigma;
                }
            }

            /// <summary>
            /// Standard Deviation relative to the Average (the imprecision of the measurements relative to the measured value)
            /// </summary>
            public double RelativeSigma
            {
                get
                {
                    return Sigma / Math.Abs(Average);
                }
            }

            /// <summary>
            /// The slope of the trend line (tangent alpha, where alpha is the angle between the trend line the the X axis)
            /// </summary>
            public double Slope
            {
                get
                {
                    return LinearInterpolationCoefficients.Item1;
                }
            }

            public Tuple<double, double> LinearInterpolationCoefficients
            {
                get
                {
                    if (_linearInterpolationCoefficients == null)
                    {
                        _linearInterpolationCoefficients = CalculateLinearInterpolationCoefficients();
                    }

                    return _linearInterpolationCoefficients;
                }
            }
            #endregion

            #region Constructor, Initialization
            public MeasurementSeries()
            {
            }

            /// <summary>
            /// Invalidates any calculated and cached information
            /// </summary>
            virtual protected void InvalidateStatistics()
            {
                _linearInterpolationCoefficients = null;
                _average = UninitializedValues.UninitializedDouble;
                _median = UninitializedValues.UninitializedDouble;
                _sigma = UninitializedValues.UninitializedDouble;
            }
            #endregion

            #region APIs
            /// <summary>
            /// Allows collecting the measurement data so that statistical information can be extracted later on
            /// </summary>
            /// <param name="measurementValue">
            /// Measurement data
            /// </param>
            virtual public void Add(double measurementValue)
            {
                _measurementValues.Add(measurementValue);

                InvalidateStatistics();
            }
            #endregion

            #region Helpers
            private double CalculateAverage()
            {
                ThrowExceptionIfThereAreNoValues();

                return _measurementValues.Average();
            }

            private double CalculateMedian()
            {
                ThrowExceptionIfThereAreNoValues();

                var sortedValues = from value in _measurementValues
                                   orderby value
                                   select value;
                int count = sortedValues.Count();
                int middleIdx = count / 2;

                if (count % 2 == 0) // Even number of measurement values 
                {
                    return (sortedValues.ElementAt(middleIdx) + sortedValues.ElementAt(middleIdx - 1)) / 2;
                }
                else // Odd number of measurement values
                {
                    return sortedValues.ElementAt(middleIdx);
                }
            }

            private double CalculateStandardDeviation()
            {
                ThrowExceptionIfThereAreNoValues();

                double average = Average;
                double diff2s = _measurementValues.Select(value => (value - average) * (value - average)).Sum();

                int valueCount = _measurementValues.Count();

                return Math.Sqrt(diff2s / valueCount);
            }

            private Tuple<double, double> CalculateLinearInterpolationCoefficients()
            {
                ThrowExceptionIfThereAreNoValues();

                // create a 2 dimensional representation of the data series
                List<Tuple<double, double>> xyValues = new List<Tuple<double, double>>();

                int idx = 0;
                foreach (double value in _measurementValues)
                {
                    xyValues.Add(new Tuple<double, double>((double)idx, value));

                    ++idx;
                }

                Tuple<double, double> linearEquParams = CalculateLinearInterpolationCoefficients(xyValues);

                return linearEquParams;
            }

            /// <summary>
            /// y = a*x + b (a = slope of the line, b = vertical offset of the line in x = 0)
            /// </summary>
            /// <param name="xyData">
            /// The x and y coordinates of the 2 dimensional representation of the data
            /// </param>
            /// <remarks>
            /// a of the linear equation is tangent alpha, where alpha is the angle of the line relative to the x axis.
            /// - a ~ 0 - constant trend, good
            /// - a &gt; 0 - increasing trend (if time - slow down, performance degradation; if throughput - performance increase)
            /// - a &lt; 0 - decreasing trend (if time - performance increase; if troughput - performance degradation
            /// </remarks>
            /// <returns></returns>
            private /*static*/ Tuple<double, double> CalculateLinearInterpolationCoefficients(IEnumerable<Tuple<double, double>> xyData)
            {
                ThrowExceptionIfThereAreNoValues();

                int xyDataCount = 0;
                double xSum = 0;
                double ySum = 0;
                double xxSum = 0;
                double xySum = 0;

                foreach (var xy in xyData)
                {
                    ++xyDataCount;

                    xSum += xy.Item1;
                    ySum += xy.Item2;
                    xxSum += xy.Item1 * xy.Item1;
                    xySum += xy.Item1 * xy.Item2;
                }

                if (xyDataCount < 2)
                {
                    //throw new InvalidOperationException("There should be at least 2 points for calculating the linear equation parameters");

                    return new Tuple<double, double>(0, 0);
                }

                double denominator = xyDataCount * xxSum - xSum * xSum;
                double b = (xxSum * ySum - xSum * xySum) / denominator;
                double a = (xyDataCount * xySum - xSum * ySum) / denominator;

                return new Tuple<double, double>(a, b);
            }

            protected void ThrowExceptionIfThereAreNoValues()
            {
                if (!_measurementValues.Any() || _measurementValues.Count < 2)
                {
                    throw new InvalidOperationException("There should be some measurement values for providing statistical information");
                }
            }
            #endregion
        }
    }
