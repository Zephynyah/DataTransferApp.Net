using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataTransferApp.Net.Helpers;
using Xunit;

namespace DataTransferApp.Net.Tests
{
    /// <summary>
    /// Unit tests for RetryHelper exponential backoff retry logic.
    /// </summary>
    public class RetryHelperTests
    {
        #region Success Cases

        [Fact]
        public async Task ExecuteWithRetryAsync_SuccessOnFirstAttempt_ReturnsResult()
        {
            // Arrange
            var expectedResult = 42;
            var attemptCount = 0;

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();
                return expectedResult;
            };

            // Act
            var result = await RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 3);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(1, attemptCount);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_Void_SuccessOnFirstAttempt_Completes()
        {
            // Arrange
            var executed = false;

            Func<Task> operation = async () =>
            {
                executed = true;
                await Task.Yield();
            };

            // Act
            await RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 3);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_FailOnceThenSucceed_ReturnsResult()
        {
            // Arrange
            var attemptCount = 0;
            var expectedResult = "success";

            Func<Task<string>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount == 1)
                {
                    throw new InvalidOperationException("First attempt fails");
                }

                return expectedResult;
            };

            // Act
            var result = await RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 3, baseDelaySeconds: 0);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(2, attemptCount);
        }

        #endregion

        #region Retry Logic Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_FailsMultipleTimes_RetriesUpToMaxCount()
        {
            // Arrange
            var attemptCount = 0;
            const int maxRetries = 3;

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount <= 2)
                {
                    throw new InvalidOperationException($"Attempt {attemptCount} fails");
                }

                return attemptCount;
            };

            // Act
            var result = await RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: maxRetries, baseDelaySeconds: 0);

            // Assert
            Assert.Equal(3, result);
            Assert.Equal(3, attemptCount);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_AllAttemptsFail_ThrowsLastException()
        {
            // Arrange
            var attemptCount = 0;
            const int maxRetries = 3;
            var expectedMessage = "Final failure";

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount < maxRetries + 1)
                {
                    throw new InvalidOperationException($"Attempt {attemptCount}");
                }

                throw new InvalidOperationException(expectedMessage);
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: maxRetries, baseDelaySeconds: 0));

            Assert.Equal(expectedMessage, exception.Message);
            Assert.Equal(maxRetries + 1, attemptCount); // Initial + 3 retries = 4 attempts
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_ZeroRetries_FailsImmediately()
        {
            // Arrange
            var attemptCount = 0;

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();
                throw new InvalidOperationException("Always fails");
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 0, baseDelaySeconds: 0));

            Assert.Equal(1, attemptCount); // Only one attempt, no retries
        }

        #endregion

        #region Exponential Backoff Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_ExponentialBackoff_IncreasesDelay()
        {
            // Arrange
            var attemptCount = 0;
            var retryDelays = new List<TimeSpan>();

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount <= 3)
                {
                    throw new InvalidOperationException($"Attempt {attemptCount}");
                }

                return attemptCount;
            };

            Action<int, TimeSpan> onRetry = (attempt, delay) =>
            {
                retryDelays.Add(delay);
            };

            // Act
            await RetryHelper.ExecuteWithRetryAsync(
                operation,
                maxRetries: 3,
                baseDelaySeconds: 1,
                useJitter: false, // Disable jitter for predictable delays
                onRetry: onRetry);

            // Assert
            Assert.Equal(3, retryDelays.Count);
            // Exponential backoff: 1s, 2s, 4s
            Assert.True(retryDelays[0].TotalSeconds >= 0.9 && retryDelays[0].TotalSeconds <= 1.1);
            Assert.True(retryDelays[1].TotalSeconds >= 1.9 && retryDelays[1].TotalSeconds <= 2.1);
            Assert.True(retryDelays[2].TotalSeconds >= 3.9 && retryDelays[2].TotalSeconds <= 4.1);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WithJitter_AddsRandomness()
        {
            // Arrange
            var attemptCount = 0;
            var retryDelays = new List<TimeSpan>();

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount <= 2)
                {
                    throw new InvalidOperationException($"Attempt {attemptCount}");
                }

                return attemptCount;
            };

            Action<int, TimeSpan> onRetry = (attempt, delay) =>
            {
                retryDelays.Add(delay);
            };

            // Act
            await RetryHelper.ExecuteWithRetryAsync(
                operation,
                maxRetries: 2,
                baseDelaySeconds: 1,
                useJitter: true, // Enable jitter
                onRetry: onRetry);

            // Assert
            Assert.Equal(2, retryDelays.Count);
            // With jitter, delays should vary by ±25% from base exponential values
            // First retry: 1s ± 25% = 0.75s to 1.25s
            Assert.True(retryDelays[0].TotalSeconds >= 0.7 && retryDelays[0].TotalSeconds <= 1.3);
            // Second retry: 2s ± 25% = 1.5s to 2.5s
            Assert.True(retryDelays[1].TotalSeconds >= 1.4 && retryDelays[1].TotalSeconds <= 2.6);
        }

        #endregion

        #region Cancellation Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var attemptCount = 0;

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount == 1)
                {
                    cts.Cancel(); // Cancel after first attempt
                }

                throw new InvalidOperationException("Fails");
            };

            // Act & Assert
            // TaskCanceledException is a subclass of OperationCanceledException
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 3, baseDelaySeconds: 0, cancellationToken: cts.Token));

            Assert.Equal(1, attemptCount); // Should stop retrying after cancellation
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_CancelledDuringDelay_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var attemptCount = 0;

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();
                throw new InvalidOperationException("Always fails");
            };

            Action<int, TimeSpan> onRetry = (attempt, delay) =>
            {
                cts.Cancel(); // Cancel during retry delay
            };

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => RetryHelper.ExecuteWithRetryAsync(
                    operation,
                    maxRetries: 3,
                    baseDelaySeconds: 1,
                    onRetry: onRetry,
                    cancellationToken: cts.Token));

            Assert.Equal(1, attemptCount); // Should stop before next attempt
        }

        #endregion

        #region Callback Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_InvokesOnRetryCallback()
        {
            // Arrange
            var attemptCount = 0;
            var callbackInvocations = new List<(int attempt, TimeSpan delay)>();

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount <= 2)
                {
                    throw new InvalidOperationException($"Attempt {attemptCount}");
                }

                return attemptCount;
            };

            Action<int, TimeSpan> onRetry = (attempt, delay) =>
            {
                callbackInvocations.Add((attempt, delay));
            };

            // Act
            await RetryHelper.ExecuteWithRetryAsync(
                operation,
                maxRetries: 3,
                baseDelaySeconds: 0,
                onRetry: onRetry);

            // Assert
            Assert.Equal(2, callbackInvocations.Count); // 2 failures = 2 retry callbacks
            Assert.Equal(1, callbackInvocations[0].attempt);
            Assert.Equal(2, callbackInvocations[1].attempt);
        }

        #endregion

        #region Argument Validation Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_NullOperation_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => RetryHelper.ExecuteWithRetryAsync<int>(null!));
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_NegativeMaxRetries_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Func<Task<int>> operation = async () =>
            {
                await Task.Yield();
                return 42;
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: -1));
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_NegativeBaseDelay_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Func<Task<int>> operation = async () =>
            {
                await Task.Yield();
                return 42;
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 3, baseDelaySeconds: -5));
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ExecuteWithRetryAsync_ZeroBaseDelay_NoDelayBetweenRetries()
        {
            // Arrange
            var attemptCount = 0;
            var startTime = DateTime.Now;

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();

                if (attemptCount <= 3)
                {
                    throw new InvalidOperationException($"Attempt {attemptCount}");
                }

                return attemptCount;
            };

            // Act
            await RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 3, baseDelaySeconds: 0);
            var elapsed = DateTime.Now - startTime;

            // Assert
            Assert.Equal(4, attemptCount);
            Assert.True(elapsed.TotalSeconds < 1, "Should complete quickly with zero delay");
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_OperationThrowsOperationCanceledException_DoesNotRetry()
        {
            // Arrange
            var attemptCount = 0;

            Func<Task<int>> operation = async () =>
            {
                attemptCount++;
                await Task.Yield();
                throw new OperationCanceledException("Cancelled by operation");
            };

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => RetryHelper.ExecuteWithRetryAsync(operation, maxRetries: 3, baseDelaySeconds: 0));

            Assert.Equal(1, attemptCount); // Should not retry when operation itself throws OCE
        }

        #endregion
    }
}
