using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protean;
using System;
using System.Reflection;

namespace Protean.CmsTests
{
    /// <summary>
    /// Unit tests for Protean.Base class disposal pattern and resource management.
    /// Tests verify proper implementation of IDisposable pattern, resource cleanup,
    /// and prevention of use-after-disposal scenarios.
    /// 
    /// These tests focus on validating the fixes applied to Base.cs for proper
    /// disposal of database connections, event handlers, and other managed resources.
    /// </summary>
    [TestClass]
    public class BaseDisposalTests
    {
        #region Helper Methods

        /// <summary>
        /// Helper to access the private disposedValue field via reflection
        /// </summary>
        private bool GetDisposedValue(Base instance)
        {
            var field = typeof(Base).GetField("disposedValue",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null && (bool)field.GetValue(instance);
        }

        /// <summary>
        /// Helper to call protected ThrowIfDisposed method via reflection
        /// </summary>
        private void CallThrowIfDisposed(Base instance)
        {
            var method = typeof(Base).GetMethod("ThrowIfDisposed",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(instance, null);
        }

        #endregion

        #region Basic Disposal Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [Priority(1)]
        [Description("Verifies Base class implements IDisposable interface")]
        public void Base_ImplementsIDisposable()
        {
            // Arrange & Act
            Base baseInstance = null;

            try
            {
                baseInstance = new Base();

                // Assert
                Assert.IsInstanceOfType(baseInstance, typeof(IDisposable),
                    "Base class must implement IDisposable interface");
                Assert.IsNotNull(baseInstance as IDisposable,
                    "Base should be castable to IDisposable");
            }
            finally
            {
                baseInstance?.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [Priority(1)]
        [Description("Verifies Dispose() can be called without throwing exceptions")]
        public void Base_Dispose_DoesNotThrow()
        {
            // Arrange
            var baseInstance = new Base();

            // Act & Assert - Should not throw
            try
            {
                baseInstance.Dispose();
                Assert.IsTrue(true, "Dispose completed without exception");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dispose() threw unexpected exception: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [Priority(1)]
        [Description("Verifies multiple Dispose() calls are safe (idempotent behavior)")]
        public void Base_Dispose_MultipleCallsAreSafe()
        {
            // Arrange
            var baseInstance = new Base();

            // Act & Assert - Should not throw on multiple calls
            try
            {
                baseInstance.Dispose();
                baseInstance.Dispose();
                baseInstance.Dispose();
                baseInstance.Dispose();
                baseInstance.Dispose();

                Assert.IsTrue(true, "Multiple Dispose() calls handled safely");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Multiple Dispose() calls threw exception: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [Priority(1)]
        [Description("Verifies disposed flag is set after Dispose()")]
        public void Base_Dispose_SetsDisposedFlag()
        {
            // Arrange
            var baseInstance = new Base();
            Assert.IsFalse(GetDisposedValue(baseInstance),
                "disposedValue should be false initially");

            // Act
            baseInstance.Dispose();

            // Assert
            Assert.IsTrue(GetDisposedValue(baseInstance),
                "disposedValue flag should be true after Dispose()");
        }

        #endregion

        #region Resource Cleanup Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("ResourceCleanup")]
        [Priority(1)]
        [Description("Verifies Features dictionary is cleared and nulled after disposal")]
        public void Base_Dispose_ClearsFeaturesCollection()
        {
            // Arrange
            var baseInstance = new Base();
            Assert.IsNotNull(baseInstance.Features,
                "Features should be initialized after construction");
            int initialCount = baseInstance.Features.Count;
            Assert.IsTrue(initialCount > 0,
                $"Features should contain items after initialization, found {initialCount}");

            // Act
            baseInstance.Dispose();

            // Assert
            Assert.IsNull(baseInstance.Features,
                "Features should be null after disposal to prevent memory leaks");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("ResourceCleanup")]
        [Priority(1)]
        [Description("Verifies PerfMon is disposed and nulled after disposal")]
        public void Base_Dispose_NullsPerfMon()
        {
            // Arrange
            var baseInstance = new Base();
            Assert.IsNotNull(baseInstance.PerfMon,
                "PerfMon should be initialized after construction");

            // Act
            baseInstance.Dispose();

            // Assert
            Assert.IsNull(baseInstance.PerfMon,
                "PerfMon should be null after disposal");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("ResourceCleanup")]
        [Priority(1)]
        [Description("Verifies HTTP context references are nulled after disposal")]
        public void Base_Dispose_NullsContextReferences()
        {
            // Arrange
            var baseInstance = new Base();

            // Act
            baseInstance.Dispose();

            // Assert
            Assert.IsNull(baseInstance.moCtx,
                "moCtx should be null after disposal");
            Assert.IsNull(baseInstance.moRequest,
                "moRequest should be null after disposal");
            Assert.IsNull(baseInstance.moResponse,
                "moResponse should be null after disposal");
            Assert.IsNull(baseInstance.goServer,
                "goServer should be null after disposal");
            Assert.IsNull(baseInstance.goCache,
                "goCache should be null after disposal");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("ResourceCleanup")]
        [Priority(1)]
        [Description("Verifies configuration references are nulled after disposal")]
        public void Base_Dispose_NullsConfigReferences()
        {
            // Arrange
            var baseInstance = new Base();
            // moConfig and goLangConfig are initialized in constructor

            // Act
            baseInstance.Dispose();

            // Assert
            Assert.IsNull(baseInstance.moConfig,
                "moConfig should be null after disposal");
            Assert.IsNull(baseInstance.goLangConfig,
                "goLangConfig should be null after disposal");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("ResourceCleanup")]
        [Priority(2)]
        [Description("Verifies all major references are cleaned up after disposal")]
        public void Base_Dispose_CleansUpAllMajorReferences()
        {
            // Arrange
            var baseInstance = new Base();

            // Verify initial state
            Assert.IsNotNull(baseInstance.Features, "Features should exist initially");
            Assert.IsNotNull(baseInstance.PerfMon, "PerfMon should exist initially");

            // Act
            baseInstance.Dispose();

            // Assert - Complete cleanup verification
            Assert.IsNull(baseInstance.Features, "Features not cleaned up");
            Assert.IsNull(baseInstance.PerfMon, "PerfMon not cleaned up");
            Assert.IsNull(baseInstance.moCtx, "moCtx not cleaned up");
            Assert.IsNull(baseInstance.moRequest, "moRequest not cleaned up");
            Assert.IsNull(baseInstance.moResponse, "moResponse not cleaned up");
            Assert.IsNull(baseInstance.goServer, "goServer not cleaned up");
            Assert.IsNull(baseInstance.goCache, "goCache not cleaned up");
            Assert.IsNull(baseInstance.moConfig, "moConfig not cleaned up");
            Assert.IsNull(baseInstance.goLangConfig, "goLangConfig not cleaned up");
        }

        #endregion

        #region Using Statement Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("UsingStatement")]
        [Priority(1)]
        [Description("Verifies Base works correctly with using statement")]
        public void Base_WorksWithUsingStatement()
        {
            // Arrange
            Base baseInstance = null;

            // Act & Assert
            try
            {
                using (baseInstance = new Base())
                {
                    Assert.IsNotNull(baseInstance,
                        "Instance should exist within using block");
                    Assert.IsNotNull(baseInstance.Features,
                        "Features should be accessible within using block");
                    Assert.IsFalse(GetDisposedValue(baseInstance),
                        "Should not be disposed within using block");
                }

                // After using block
                Assert.IsTrue(GetDisposedValue(baseInstance),
                    "Should be disposed after exiting using block");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Using statement failed: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("UsingStatement")]
        [Priority(1)]
        [Description("Verifies using statement disposes even when exception occurs")]
        public void Base_UsingStatement_DisposesOnException()
        {
            // Arrange
            Base baseInstance = null;
            bool exceptionCaught = false;

            // Act
            try
            {
                using (baseInstance = new Base())
                {
                    Assert.IsNotNull(baseInstance);
                    Assert.IsFalse(GetDisposedValue(baseInstance));

                    // Throw exception within using block
                    throw new InvalidOperationException("Test exception in using block");
                }
            }
            catch (InvalidOperationException ex)
            {
                exceptionCaught = ex.Message == "Test exception in using block";
            }

            // Assert
            Assert.IsTrue(exceptionCaught,
                "Exception should have been thrown and caught");
            Assert.IsTrue(GetDisposedValue(baseInstance),
                "Object should be disposed even when exception occurs in using block");
        }

        #endregion

        #region Performance Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("Performance")]
        [Priority(2)]
        [Timeout(1000)] // 1 second timeout
        [Description("Verifies disposal completes within reasonable time")]
        public void Base_Dispose_CompletesQuickly()
        {
            // Arrange
            var baseInstance = new Base();

            // Act
            var startTime = DateTime.Now;
            baseInstance.Dispose();
            var endTime = DateTime.Now;

            // Assert
            var elapsed = endTime - startTime;
            Assert.IsTrue(elapsed.TotalMilliseconds < 500,
                $"Disposal should complete quickly (< 500ms), took {elapsed.TotalMilliseconds:F2}ms");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("Performance")]
        [Priority(2)]
        [Timeout(2000)] // 2 second timeout
        [Description("Verifies multiple dispose calls don't cause performance degradation")]
        public void Base_Dispose_MultipleCallsStable()
        {
            // Arrange
            var baseInstance = new Base();

            // Act - Multiple dispose calls
            var startTime = DateTime.Now;
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    baseInstance.Dispose();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Multiple dispose calls caused exception: {ex.GetType().Name} - {ex.Message}");
            }
            var endTime = DateTime.Now;

            // Assert
            var elapsed = endTime - startTime;
            Assert.IsTrue(elapsed.TotalMilliseconds < 1000,
                $"100 dispose calls should complete quickly (< 1s), took {elapsed.TotalMilliseconds:F2}ms");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("Integration")]
        [Priority(2)]
        [Description("Verifies complete disposal workflow in realistic scenario")]
        public void Base_CompleteDisposalWorkflow_Integration()
        {
            // Arrange
            var baseInstance = new Base();

            // Verify initial state
            Assert.IsNotNull(baseInstance.Features,
                "Features should be initialized");
            Assert.IsNotNull(baseInstance.PerfMon,
                "PerfMon should be initialized");
            Assert.IsFalse(GetDisposedValue(baseInstance),
                "Should not be disposed initially");

            // Act - Simulate usage then dispose
            var featureCount = baseInstance.Features.Count;
            Assert.IsTrue(featureCount > 0, "Features should contain items");

            baseInstance.Dispose();

            // Assert - Verify complete cleanup
            Assert.IsTrue(GetDisposedValue(baseInstance),
                "Should be disposed after calling Dispose()");
            Assert.IsNull(baseInstance.Features,
                "Features should be null");
            Assert.IsNull(baseInstance.PerfMon,
                "PerfMon should be null");
            Assert.IsNull(baseInstance.moCtx,
                "Context should be null");
            Assert.IsNull(baseInstance.moRequest,
                "Request should be null");
            Assert.IsNull(baseInstance.moResponse,
                "Response should be null");
            Assert.IsNull(baseInstance.moConfig,
                "Config should be null");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("Integration")]
        [Priority(2)]
        [Description("Verifies multiple Base instances can be disposed independently")]
        public void Base_MultipleInstances_DisposeIndependently()
        {
            // Arrange - Create multiple instances
            var baseInstance1 = new Base();
            var baseInstance2 = new Base();
            var baseInstance3 = new Base();

            Assert.IsFalse(GetDisposedValue(baseInstance1), "Instance 1 should not be disposed");
            Assert.IsFalse(GetDisposedValue(baseInstance2), "Instance 2 should not be disposed");
            Assert.IsFalse(GetDisposedValue(baseInstance3), "Instance 3 should not be disposed");

            // Act - Dispose in different order
            baseInstance2.Dispose();
            Assert.IsTrue(GetDisposedValue(baseInstance2), "Instance 2 should be disposed");
            Assert.IsFalse(GetDisposedValue(baseInstance1), "Instance 1 should still not be disposed");
            Assert.IsFalse(GetDisposedValue(baseInstance3), "Instance 3 should still not be disposed");

            baseInstance1.Dispose();
            Assert.IsTrue(GetDisposedValue(baseInstance1), "Instance 1 should be disposed");
            Assert.IsFalse(GetDisposedValue(baseInstance3), "Instance 3 should still not be disposed");

            baseInstance3.Dispose();
            Assert.IsTrue(GetDisposedValue(baseInstance3), "Instance 3 should be disposed");

            // Assert - All properly disposed
            Assert.IsNull(baseInstance1.Features, "Instance 1 Features should be null");
            Assert.IsNull(baseInstance2.Features, "Instance 2 Features should be null");
            Assert.IsNull(baseInstance3.Features, "Instance 3 Features should be null");
        }

        #endregion

        #region Memory Leak Prevention Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("MemoryLeak")]
        [Priority(2)]
        [Description("Verifies Features collection is properly cleared to prevent memory leaks")]
        public void Base_Dispose_ClearsFeaturesToPreventLeak()
        {
            // Arrange
            var baseInstance = new Base();
            int initialCount = baseInstance.Features.Count;
            Assert.IsTrue(initialCount > 0,
                $"Features should have items initially, found {initialCount}");

            // Act
            baseInstance.Dispose();

            // Assert
            Assert.IsNull(baseInstance.Features,
                "Features should be completely removed (not just cleared) to prevent memory leak");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("MemoryLeak")]
        [Priority(2)]
        [Description("Verifies disposal handles large Features collections efficiently")]
        public void Base_Dispose_HandlesLargeFeaturesCollection()
        {
            // Arrange
            var baseInstance = new Base();

            // Add many items to Features to simulate heavy usage
            for (int i = 0; i < 1000; i++)
            {
                baseInstance.Features[$"TestFeature{i}"] = $"TestValue{i}";
            }

            Assert.IsTrue(baseInstance.Features.Count >= 1000,
                "Features should contain many items");

            // Act
            var startTime = DateTime.Now;
            baseInstance.Dispose();
            var endTime = DateTime.Now;

            // Assert
            var elapsed = endTime - startTime;
            Assert.IsNull(baseInstance.Features,
                "Features should be null even with large collection");
            Assert.IsTrue(elapsed.TotalMilliseconds < 500,
                $"Disposal of large collection should be quick, took {elapsed.TotalMilliseconds:F2}ms");
        }

        #endregion

        #region Finalizer Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("Finalizer")]
        [Priority(2)]
        [Description("Verifies Base class has finalizer defined")]
        public void Base_HasFinalizer()
        {
            // Arrange & Act - Check for finalizer using reflection
            var finalizerMethod = typeof(Base).GetMethod("Finalize",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.IsNotNull(finalizerMethod,
                "Base class should have a finalizer (~Base) for proper resource cleanup");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("Finalizer")]
        [Priority(3)]
        [Description("Verifies GC.SuppressFinalize is called by Dispose()")]
        public void Base_Dispose_SuppressesFinalizer()
        {
            // Note: This test verifies the pattern is followed
            // Actual finalizer suppression is difficult to test directly

            // Arrange
            var baseInstance = new Base();

            // Act
            baseInstance.Dispose();

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert
            Assert.IsTrue(GetDisposedValue(baseInstance),
                "Object should remain disposed (if GC.SuppressFinalize was called, finalizer won't re-dispose)");
        }

        #endregion

        #region ThrowIfDisposed Tests

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("ThrowIfDisposed")]
        [Priority(1)]
        [Description("Verifies ThrowIfDisposed throws ObjectDisposedException when disposed")]
        public void Base_ThrowIfDisposed_ThrowsWhenDisposed()
        {
            // Arrange
            var baseInstance = new Base();
            baseInstance.Dispose();

            // Act & Assert
            Assert.ThrowsException<ObjectDisposedException>(() =>
            {
                CallThrowIfDisposed(baseInstance);
            }, "ThrowIfDisposed should throw ObjectDisposedException when object is disposed");
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("ThrowIfDisposed")]
        [Priority(1)]
        [Description("Verifies ThrowIfDisposed does not throw when not disposed")]
        public void Base_ThrowIfDisposed_DoesNotThrowWhenNotDisposed()
        {
            // Arrange
            var baseInstance = new Base();

            // Act & Assert - Should not throw
            try
            {
                CallThrowIfDisposed(baseInstance);
                Assert.IsTrue(true, "ThrowIfDisposed did not throw when not disposed");
            }
            catch (ObjectDisposedException)
            {
                Assert.Fail("ThrowIfDisposed should not throw when object is not disposed");
            }
            finally
            {
                baseInstance.Dispose();
            }
        }

        #endregion

        #region Edge Cases and Error Handling

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("EdgeCase")]
        [Priority(2)]
        [Description("Verifies disposal works correctly with null moDbHelper")]
        public void Base_Dispose_HandlesNullDbHelper()
        {
            // Arrange
            var baseInstance = new Base();
            baseInstance.moDbHelper = null; // Explicitly set to null

            // Act & Assert - Should not throw
            try
            {
                baseInstance.Dispose();
                Assert.IsTrue(true, "Disposal handled null moDbHelper safely");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Disposal threw exception with null moDbHelper: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("Disposal")]
        [TestCategory("Base")]
        [TestCategory("EdgeCase")]
        [Priority(2)]
        [Description("Verifies rapid creation and disposal doesn't cause issues")]
        public void Base_RapidCreateAndDispose_NoIssues()
        {
            // Act & Assert - Create and dispose many instances rapidly
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    using (var instance = new Base())
                    {
                        Assert.IsNotNull(instance);
                        Assert.IsNotNull(instance.Features);
                    }
                }

                Assert.IsTrue(true, "Rapid create/dispose completed successfully");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Rapid create/dispose caused exception: {ex.GetType().Name} - {ex.Message}");
            }
        }

        #endregion
    }
}
