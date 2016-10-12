﻿using System;
using System.Linq;
using NUnit.Framework;

namespace Proteus.Retry.Test
{
    [TestFixture]
    public class WhenManipulatingConstrainedTypesList
    {
        [Test]
        public void CanAddTypesDerivedFromConstrainedType()
        {
            Assume.That(typeof(ExpectableTestException).IsSubclassOf(typeof(Exception)), "Unable to validate required inheritance hierarchy.");

            var list = new ConstrainedTypesList<Exception>();
            list.Add(typeof(Exception));
            list.Add(typeof(ExpectableTestException));
            list.Insert(0, (typeof(ExpectableTestException)));
            list[0] = (typeof(ExpectableTestException));

            Assert.That(list, Has.Member(typeof(Exception)), "Cannot successfully add Exception type.");
            Assert.That(list, Has.Member(typeof(ExpectableTestException)), "Cannot successfully add Exception-derived type.");
        }

        [Test]
        public void CanDefendAgainstAddingTypeNotDerivedFromConstrainedType()
        {
            Assume.That(this.GetType().IsSubclassOf(typeof(Exception)), Is.False, "Unable to validate required inheritance hierarchy.");

            var list = new ConstrainedTypesList<Exception>();

            Assert.Throws<ArgumentException>(() => list.Add(this.GetType()), ".Add(...) method did not prevent adding non-Exception-derived type.");
            Assert.Throws<ArgumentException>(() => list[0] = this.GetType(), "Index[n] method did not prevent adding non-Exception-derived type.");
            Assert.Throws<ArgumentException>(() => list.Insert(0, this.GetType()), "Insert(...) method did not prevent adding non-Exception-derived type.");
        }
    }
}