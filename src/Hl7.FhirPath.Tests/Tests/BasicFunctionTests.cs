﻿/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

// To introduce the DSTU2 FHIR specification
//extern alias dstu2;

using Hl7.Fhir.ElementModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class BasicFunctionsTest
    {     
        [TestMethod]
        public void IsDistinctWorksOnEmptyCollections()
        {
            ITypedElement dummy = ElementNode.ForPrimitive(true);

            Assert.IsTrue(dummy.IsBoolean("{}.isDistinct()", true));
            Assert.IsTrue(dummy.IsBoolean("(2).isDistinct()", true));
        }

        [TestMethod]
        public void StringConcatenationAndEmpty()
        {
            ITypedElement dummy = ElementNode.ForPrimitive(true);

            Assert.AreEqual("ABCDEF", dummy.Scalar("'ABC' + '' + 'DEF'"));
            Assert.AreEqual("DEF", dummy.Scalar("'' + 'DEF'"));
            Assert.AreEqual("DEF", dummy.Scalar("'DEF' + ''"));

            Assert.IsNull(dummy.Scalar("{} + 'DEF'"));
            Assert.IsNull(dummy.Scalar("'ABC' + {} + 'DEF'"));
            Assert.IsNull(dummy.Scalar("'ABC' + {}"));

            Assert.AreEqual("ABCDEF", dummy.Scalar("'ABC' & '' & 'DEF'"));
            Assert.AreEqual("DEF", dummy.Scalar("'' & 'DEF'"));
            Assert.AreEqual("DEF", dummy.Scalar("'DEF' & ''"));

            Assert.AreEqual("DEF", dummy.Scalar("{} & 'DEF'"));
            Assert.AreEqual("ABCDEF", dummy.Scalar("'ABC' & {} & 'DEF'"));
            Assert.AreEqual("ABC", dummy.Scalar("'ABC' & {}"));

            Assert.IsNull(dummy.Scalar("'ABC' & {} & 'DEF' + {}"));
        }

        [TestMethod]
        public void TestStringSplit()
        {
            ITypedElement dummy = ElementNode.ForPrimitive("a,b,c,d");
            var result = dummy.Select("split(',')");
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new[] { "a", "b", "c", "d" }, result.Select(r => r.Value.ToString()).ToArray());

            dummy = ElementNode.ForPrimitive("a,,b,c,d"); // Empty element should be removed
            result = dummy.Select("split(',')");
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new[] { "a", "b", "c", "d" }, result.Select(r => r.Value.ToString()).ToArray());

            dummy = ElementNode.ForPrimitive("");
            result = dummy.Select("split(',')");
            Assert.IsNotNull(result);

            dummy = ElementNode.ForPrimitive("[stop]ONE[stop][stop]TWO[stop][stop][stop]THREE[stop][stop]");
            result = dummy.Select("split('[stop]')");
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new[] { "ONE", "TWO", "THREE" }, result.Select(r => r.Value.ToString()).ToArray());
        }
    }
        
}