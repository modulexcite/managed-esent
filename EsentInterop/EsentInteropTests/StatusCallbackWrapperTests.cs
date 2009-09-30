﻿//-----------------------------------------------------------------------
// <copyright file="StatusCallbackWrapperTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Threading;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the StatusCallbackWrapper class.
    /// </summary>
    [TestClass]
    public class StatusCallbackWrapperTests
    {
        /// <summary>
        /// Sesid used for testing.
        /// </summary>
        private readonly JET_SESID sesid = new JET_SESID { Value = (IntPtr)0x1 };

        /// <summary>
        /// The wrapper should convert the arguments passed to it.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyStatusCallbackWrapperConvertsArguments()
        {
            var wrapper = new StatusCallbackWrapper(
                (session, snp, snt, snprog) =>
                {
                    Assert.AreEqual(this.sesid, session);
                    Assert.AreEqual(JET_SNP.Backup, snp);
                    Assert.AreEqual(JET_SNT.Progress, snt);
                    Assert.IsNotNull(snprog);
                    Assert.AreEqual(1, snprog.cunitDone);
                    Assert.AreEqual(100, snprog.cunitTotal);
                    return JET_err.Success;
                });

            var native = new NATIVE_SNPROG
            {
                cbStruct = checked((uint) NATIVE_SNPROG.Size),
                cunitDone = 1,
                cunitTotal = 100,
            };

            unsafe
            {
                wrapper.Callback(
                    this.sesid.Value,
                    (uint) JET_SNP.Backup,
                    (uint) JET_SNT.Progress,
                    new IntPtr(&native));
            }

            wrapper.ThrowSavedException();
        }

        /// <summary>
        /// The wrapper should catch exceptions caught by the
        /// real callback and be able to rethrow them.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyStatusCallbackWrapperCatchesExceptions()
        {
            var wrapper = new StatusCallbackWrapper(
                (session, snp, snt, snprog) =>
                {
                    throw new ArgumentException();
                });

            Assert.AreEqual(JET_err.CallbackFailed, wrapper.Callback(this.sesid.Value, 0, 0, IntPtr.Zero));

            try
            {
                wrapper.ThrowSavedException();
                Assert.Fail("Expected an ArgumentException to be thrown");
            }
            catch (ArgumentException)
            {
                // expected
            }
        }

        /// <summary>
        /// The wrapper should catch and stop a thread abort.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyStatusCallbackWrapperCatchesThreadAbort()
        {
            var wrapper = new StatusCallbackWrapper(
                (session, snp, snt, snprog) =>
                {
                    Thread.CurrentThread.Abort();
                    return JET_err.Success;
                });

            Assert.AreEqual(JET_err.CallbackFailed, wrapper.Callback(this.sesid.Value, 0, 0, IntPtr.Zero));

            try
            {
                wrapper.ThrowSavedException();
                Assert.Fail("Expected a ThreadAbortException to be thrown");
            }
            catch (ThreadAbortException)
            {
                // expected
                Thread.ResetAbort();
            }
        }
    }
}