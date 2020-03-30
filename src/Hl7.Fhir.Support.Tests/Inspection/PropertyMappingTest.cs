﻿/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Hl7.Fhir.Introspection;
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

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Code<Resource.ResourceValidationMode>), out mapping));
            Assert.AreEqual("codeOfT<Hl7.Fhir.Model.Resource+ResourceValidationMode>", mapping.Name);
            Assert.IsTrue(mapping.HasPrimitiveValueMember);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value
            valueProp = mapping.PrimitiveValueProperty;
            Assert.IsNotNull(valueProp);
            Assert.IsFalse(valueProp.IsCollection);
            Assert.IsTrue(valueProp.RepresentsValueElement);
            Assert.AreEqual(typeof(Resource.ResourceValidationMode),valueProp.ElementType);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(FhirUri), out mapping));
            Assert.AreEqual("uri", mapping.Name);
            Assert.IsTrue(mapping.HasPrimitiveValueMember);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value
            valueProp = mapping.PrimitiveValueProperty;
            Assert.IsNotNull(valueProp);
            Assert.IsFalse(valueProp.IsCollection); 
            Assert.IsTrue(valueProp.RepresentsValueElement);
            Assert.AreEqual(typeof(string),valueProp.ElementType);
        }

        [TestMethod]
        public void TestVersionSpecificMapping()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Meta), out var mapping, fhirVersion: "1.0.0"));
            Assert.IsNull(mapping.FindMappedElementByName("source"));
            var profile = mapping.FindMappedElementByName("profile");
            Assert.IsNotNull(profile);
            Assert.AreEqual(typeof(FhirUri), profile.FhirType.Single());

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Meta), out mapping, fhirVersion: "4.0.1"));
            Assert.IsNotNull(mapping.FindMappedElementByName("source"));
            profile = mapping.FindMappedElementByName("profile");
            Assert.IsNotNull(profile);
            Assert.AreEqual(typeof(Canonical), profile.FhirType.Single());
        }
    }
}
