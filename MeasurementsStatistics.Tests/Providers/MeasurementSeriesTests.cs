using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MeasurementsStatistics.Providers;
using MeasurementsStatistics.Interfaces;
using System.Linq;

namespace MeasurementsStatistics.Tests.Providers
{
    [TestClass]
    public class MeasurementSeriesTests
    {
        List<double> _measurementValues = new List<double>() { 3.4, 3.2, 2.6, 2.8, 3.15, 3.25, 2.15, 3.35, 2.25, 2.95 };

        [TestMethod]
        public void CalculateStatistics_MeasurementSeries_Correct()
        {
            IMeasurementSeries sut = new MeasurementSeries();

            foreach (double val in _measurementValues)
            {
                sut.Add(val);
            }

            double average = sut.Average;
            Assert.AreEqual(_measurementValues.Average(), average);
            double median = sut.Median;
            Assert.AreEqual(3.05, median);
            double sigma = sut.Sigma;
            Assert.AreEqual(0.42591078878093713, sigma);
            double relativeSigma = sut.RelativeSigma;
            Assert.AreEqual(0.14636109580100934, relativeSigma);
            double slope = sut.Slope;
            Assert.AreEqual(-0.053333333333333337, slope);
            IEnumerable<double> measurementValues = sut.MeasurementValues;
            Assert.AreEqual(_measurementValues.Count, measurementValues.Count());
            Tuple<double, double> linearInterpolationCoefficients = sut.LinearInterpolationCoefficients;
            Assert.IsTrue(linearInterpolationCoefficients.Item1 == slope && linearInterpolationCoefficients.Item2 == 3.15);
        }
    }
}
