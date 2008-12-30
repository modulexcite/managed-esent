﻿//-----------------------------------------------------------------------
// <copyright file="SessionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the disposable Session class, which wraps a JET_SESSION.
    /// </summary>
    [TestClass]
    public class SessionTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Allocate a session and let it be disposed.
        /// </summary>
        [TestMethod]
        public void CreateSession()
        {
           using (Session session = new Session(this.instance))
            {
                Assert.AreNotEqual(JET_SESID.Nil, session.JetSesid);
                Api.JetBeginTransaction(session.JetSesid);
                Api.JetCommitTransaction(session.JetSesid, CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Test that a Session can be converted to a JET_SESID
        /// </summary>
        [TestMethod]
        public void SessionCanConvertToJetSesid()
        {
            using (Session session = new Session(this.instance))
            {
                JET_SESID sesid = session;
                Assert.AreEqual(sesid, session.JetSesid);
            }
        }

        /// <summary>
        /// Allocate a session and end it.
        /// </summary>
        [TestMethod]
        public void CreateAndEndSession()
        {
            using (Session session = new Session(this.instance))
            {
                session.End();
            }
        }

        /// <summary>
        /// Check that ending a session zeroes the JetSesid member.
        /// </summary>
        [TestMethod]
        public void CheckThatEndSessionZeroesJetSesid()
        {
            Session session = new Session(this.instance);
            session.End();
            Assert.AreEqual(JET_SESID.Nil, session.JetSesid);
        }

        /// <summary>
        /// Check that calling End on a disposed session throws
        /// an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void EndThrowsExceptionWhenSessionIsDisposed()
        {
            Session session = new Session(this.instance);
            session.Dispose();
            session.End();
        }

        /// <summary>
        /// Check that accessing the JetSesid property on a disposed
        /// session throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void JetSesidThrowsExceptionWhenSessionIsDisposed()
        {
            Session session = new Session(this.instance);
            session.Dispose();
            var x = session.JetSesid;
        }
    }
}