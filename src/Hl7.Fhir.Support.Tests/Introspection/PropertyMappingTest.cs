﻿/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace Hl7.Fhir.Tests.Introspection
{
    [TestClass]
    public class ModelInspectorMembersTest
    {
        [TestMethod]
        public void TestPrimitiveDataTypeMapping()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Base64Binary), out var mapping));
            Assert.AreEqual("base64Binary", mapping.Name);
            Assert.IsTrue(mapping.HasPrimitiveValueMember);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value
            var valueProp = mapping.PrimitiveValueProperty;
            Assert.IsNotNull(valueProp);
            Assert.AreEqual("value", valueProp.Name);
            Assert.IsFalse(valueProp.IsCollection);     // don't see byte[] as a collection of byte in FHIR
            Assert.IsTrue(valueProp.RepresentsValueElement);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Code<SomeEnum>), out mapping));
            Assert.AreEqual("codeOfT<Hl7.Fhir.Tests.Introspection.SomeEnum>", mapping.Name);
            Assert.IsTrue(mapping.HasPrimitiveValueMember);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value
            valueProp = mapping.PrimitiveValueProperty;
            Assert.IsNotNull(valueProp);
            Assert.IsFalse(valueProp.IsCollection);
            Assert.IsTrue(valueProp.RepresentsValueElement);
            Assert.AreEqual(typeof(SomeEnum), valueProp.ImplementingType);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(FhirUri), out mapping));
            Assert.AreEqual("uri", mapping.Name);
            Assert.IsTrue(mapping.HasPrimitiveValueMember);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value
            valueProp = mapping.PrimitiveValueProperty;
            Assert.IsNotNull(valueProp);
            Assert.IsFalse(valueProp.IsCollection);
            Assert.IsTrue(valueProp.RepresentsValueElement);
            Assert.AreEqual(typeof(string), valueProp.ImplementingType);
        }

        [TestMethod]
        public void TestVersionSpecificMapping()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Meta), out var mapping, Specification.FhirRelease.DSTU1));
            Assert.IsNull(mapping.FindMappedElementByName("source"));
            var profile = mapping.FindMappedElementByName("profile");
            Assert.IsNotNull(profile);
            Assert.AreEqual(typeof(FhirUri), profile.FhirType.Single());

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Meta), out mapping, Specification.FhirRelease.R4));
            Assert.IsNotNull(mapping.FindMappedElementByName("source"));
            profile = mapping.FindMappedElementByName("profile");
            Assert.IsNotNull(profile);
            Assert.AreEqual(typeof(Canonical), profile.FhirType.Single());
        }

        [TestMethod]
        public void TestPropsWithRedirect()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(TypeWithCodeOfT), out var mapping));

            var propMapping = mapping.FindMappedElementByName("type1");
            Assert.AreEqual(typeof(Code<SomeEnum>), propMapping.ImplementingType);
            Assert.AreEqual(typeof(FhirString), propMapping.FhirType.Single());
        }


        [FhirType("TypeWithCodeOfT")]
        public class TypeWithCodeOfT
        {
            [FhirElement("type1")]
            [DeclaredType(Type = typeof(FhirString))]
            public Code<SomeEnum> Type1 { get; set; }
        }

        [TestMethod]
        public void TestPerformanceOfMapping()
        {
            // just a random list of POCO types available in common
            var typesToTest = new Type[] { typeof(BackboneElement), typeof(BackboneType),
                typeof(Base64Binary), typeof(Canonical), typeof(Element), typeof(FhirString),
                typeof(Extension), typeof(Resource), typeof(Meta), typeof(XHtml) };


            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000; i++)
                foreach (var testee in typesToTest)
                    createMapping(testee);
            sw.Stop();
            Console.WriteLine($"No props: {sw.ElapsedMilliseconds}");

            sw.Restart();
            for (int i = 0; i < 1000; i++)
                foreach (var testee in typesToTest)
                    createMapping(testee, touchProps: true);
            sw.Stop();
            Console.WriteLine($"With props: {sw.ElapsedMilliseconds}");

            int createMapping(Type t, bool touchProps = false)
            {
                ClassMapping.TryCreate(t, out var mapping);
                return touchProps ? mapping.PropertyMappings.Count : -1;
            }
        }
    }
}
