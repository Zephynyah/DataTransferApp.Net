using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataTransferApp.Net.Helpers;

/// <summary>
/// Provides retry logic with exponential backoff and jitter for transient failures.
/// </summary>
public static class RetryHelper
{
    /// <summary>
    /// Executes an async operation with exponential backoff retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="baseDelaySeconds">Base delay in seconds for exponential backoff (default: 5).</param>
    /// <param name="useJitter">Whether to add random jitter to prevent retry storms (default: true).</param>
    /// <param name="onRetry">Optional callback invoked before each retry with attempt number and delay.</param>
    /// <param name="cancellationToken">Cancellation token to cancel retry attempts.</param>
    /// <returns>The result of the operation if successful.</returns>
    /// <exception cref="Exception">Throws the last exception if all retries fail.</exception>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelaySeconds = 5,
        bool useJitter = true,
        Action<int, TimeSpan>? onRetry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (maxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries must be non-negative.");
        }

        if (baseDelaySeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseDelaySeconds), "Base delay must be non-negative.");
        }

        Exception? lastException = null;
        var random = useJitter ? new Random() : null;

        for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await operation();
            }
            catch (OperationCanceledException)
            {
                // Don't retry if operation was cancelled
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                // If this was the last attempt, don't delay and rethrow
                if (attempt > maxRetries)
                {
                    break;
                }

                // Calculate exponential backoff delay: baseDelay * 2^(attempt-1)
                var exponentialDelay = baseDelaySeconds * Math.Pow(2, attempt - 1);

                // Add jitter: ±25% random variation to prevent retry storms
                if (useJitter && random != null)
                {
                    var jitterPercent = 0.25;
                    var jitterFactor = 1.0 + ((random.NextDouble() * 2 - 1) * jitterPercent);
                    exponentialDelay *= jitterFactor;
                }

                var delay = TimeSpan.FromSeconds(exponentialDelay);

                // Invoke retry callback if provided
                onRetry?.Invoke(attempt, delay);

                // Wait before retrying
                await Task.Delay(delay, cancellationToken);
            }
        }

        // All retries exhausted, throw the last exception
        throw lastException ?? new InvalidOperationException("Retry operation failed with no exception captured.");
    }

    /// <summary>
    /// Executes an async operation with exponential backoff retry logic (void return).
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="baseDelaySeconds">Base delay in seconds for exponential backoff (default: 5).</param>
    /// <param name="useJitter">Whether to add random jitter to prevent retry storms (default: true).</param>
    /// <param name="onRetry">Optional callback invoked before each retry with attempt number and delay.</param>
    /// <param name="cancellationToken">Cancellation token to cancel retry attempts.</param>
    /// <exception cref="Exception">Throws the last exception if all retries fail.</exception>
    public static async Task ExecuteWithRetryAsync(
        Func<Task> operation,
        int maxRetries = 3,
        int baseDelaySeconds = 5,
        bool useJitter = true,
        Action<int, TimeSpan>? onRetry = null,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithRetryAsync(
            async () =>
            {
                await operation();
                return 0; // Dummy return value
            },
            maxRetries,
            baseDelaySeconds,
            useJitter,
            onRetry,
            cancellationToken);
    }
}
