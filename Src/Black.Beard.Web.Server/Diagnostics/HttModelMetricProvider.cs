using System;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Bb.Diagnostics
{


    /// <summary>
    /// Managing metrics source.
    /// </summary>
    public static class HttModelMetricProvider
    {

        /// <summary>
        /// initialize the metrics
        /// </summary>
        static HttModelMetricProvider()
        {
            Name = nameof(HttModelMetricProvider);
            Name = Name.Substring(0, Name.Length - "Provider".Length);
            Version = typeof(HttModelMetricProvider).Assembly.GetName().Version;

            Source = new Meter(Name, Version?.ToString());

            HttModelMetricProvider.Counter = Source.CreateCounter<long>("ComponentModelMetric", "unit", "description");
            HttModelMetricProvider.Histogram = Source.CreateHistogram<long>("ComponentModelMetric", "unit", "description");

            WithTelemetry = HttModelMetricProvider.Counter.Enabled;

        }


        public static Counter<long> Counter;
        public static Histogram<long> Histogram;


        internal static Meter Source;
        public static readonly string Name;
        public static readonly Version Version;
        private static bool WithTelemetry = true;

    }


}
