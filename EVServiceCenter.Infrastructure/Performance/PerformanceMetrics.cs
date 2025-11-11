using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EVServiceCenter.Infrastructure.Performance
{
    /// <summary>
    /// ✅ FIX GAP #16: Performance logging and metrics collection
    /// Helper để track performance metrics của critical operations
    /// </summary>
    public class PerformanceMetrics : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private readonly Dictionary<string, object> _metadata;

        public PerformanceMetrics(ILogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
            _metadata = new Dictionary<string, object>();

            _logger.LogDebug("📊 GAP #16 - Started tracking: {Operation}", operationName);
        }

        /// <summary>
        /// Add metadata to performance log
        /// </summary>
        public void AddMetadata(string key, object value)
        {
            _metadata[key] = value;
        }

        /// <summary>
        /// Log intermediate checkpoint
        /// </summary>
        public void Checkpoint(string checkpointName)
        {
            _logger.LogDebug(
                "🔍 GAP #16 - Checkpoint '{Checkpoint}' in {Operation}: {Elapsed}ms",
                checkpointName, _operationName, _stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Dispose - log final metrics
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();

            var metadataString = string.Join(", ",
                _metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            if (_stopwatch.ElapsedMilliseconds > 1000) // Slow query threshold: 1s
            {
                _logger.LogWarning(
                    "⚠️ GAP #16 - SLOW OPERATION: {Operation} took {Elapsed}ms | Metadata: {Metadata}",
                    _operationName, _stopwatch.ElapsedMilliseconds, metadataString);
            }
            else
            {
                _logger.LogInformation(
                    "✅ GAP #16 - Performance: {Operation} completed in {Elapsed}ms | Metadata: {Metadata}",
                    _operationName, _stopwatch.ElapsedMilliseconds, metadataString);
            }
        }

        /// <summary>
        /// Static factory method for clean usage
        /// </summary>
        public static PerformanceMetrics Track(ILogger logger, string operationName)
        {
            return new PerformanceMetrics(logger, operationName);
        }
    }
}
