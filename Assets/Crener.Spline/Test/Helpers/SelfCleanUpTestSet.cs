using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Crener.Spline.Test.Helpers
{
    /// <summary>
    /// Allows for automated tests to dump disposables into here for automatic disposal once the test is over
    /// </summary>
    public class SelfCleanUpTestSet
    {
        protected readonly List<IDisposable> m_disposables = new List<IDisposable>(2);

        [TearDown]
        public void PostTest()
        {
            foreach (IDisposable disposable in m_disposables)
            {
                disposable?.Dispose();
            }

            m_disposables.Clear();
        }
    }
}