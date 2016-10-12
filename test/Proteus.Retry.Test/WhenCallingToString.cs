#region License

/*
 * Copyright © 2014-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenCallingToString
    {
        [Test]
        public void RetryPolicyCanReportDetailsOfSettings()
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