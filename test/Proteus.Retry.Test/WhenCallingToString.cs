using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenCallingToString
    {
        [Test]
        public void RetryPolicvyCanReportDetailsOfSettings()
        {
            var policy = new RetryPolicy();

            policy.RegisterRetriableException<ArgumentException>();
            policy.RegisterRetriableException<InvalidCastException>();

            Debug.WriteLine(policy);
        }

        [Test]
        public void ConstrainedTypesListCanReportContainedTypes()
        {
            var list = new ConstrainedTypesList<Exception>();
            list.Add(typeof(Exception));
            list.Add(typeof(ArgumentException));
            list.Add(typeof(InvalidCastException));
            Debug.WriteLine(list);
        }

        [Test]
        public void RetryCanReportPolicy()
        {
            var retry = new Retry();
            retry.Policy = new RetryPolicy() { MaxRetries = 10 };
            retry.Policy.RegisterRetriableException<InvalidCastException>();
            Debug.WriteLine(retry);
        }
    }
}