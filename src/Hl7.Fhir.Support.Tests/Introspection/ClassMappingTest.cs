﻿/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using Hl7.Fhir.Introspection;
using System.Linq;

namespace Hl7.Fhir.Tests.Introspection
{
    [TestClass]
    public class ClassMappingTest
    {
        [TestMethod]
        public void TestReflectionCache()
        {
            var way = ReflectionCache.Current.Get(typeof(Way));
            var way2 = ReflectionCache.Current.Get(typeof(Way2));
            var way3 = ReflectionCache.Current.Get(typeof(Way));
            _ = ReflectionCache.Current.Get(typeof(string));
            _ = ReflectionCache.Current.Get(typeof(int));

            Assert.AreSame(way, way3, "Reflected types don't seem to be cached");
            Assert.AreNotSame(way, way2);
        }

        [TestMethod]
        public void Test‎‎‎ReflectedTypeCreation()
        {
            var way = new ReflectedType(typeof(Way));
            Assert.AreEqual(typeof(Way), way.Reflected);
            Assert.AreEqual(4, way.Attributes.Count);    // my 3 + 1 DebuggerDisplayAttribute

            // Test properties are reflected only once
            var wayProps = way.Properties;
            var wayProps2 = way.Properties;
            Assert.AreSame(wayProps, wayProps2, "Properties don't seem to be cached");

            // Test list all properties
            var onlyMyProps = wayProps.Where(p => p.Reflected.DeclaringType == typeof(Way));
            Assert.AreEqual(1, onlyMyProps.Count());
            Assert.IsTrue(wayProps.Count > 1);

            // Test get property by name
            Assert.IsFalse(way.TryGetProperty("xxx", out var _));
            Assert.IsTrue(way.TryGetProperty("Member", out var memberProp));
            Assert.IsTrue(onlyMyProps.Contains(memberProp));
            Assert.AreEqual("Member", memberProp.Name);

            // Test getter/setter
            var wayInstance = new Way();
            memberProp.Set(wayInstance, "test");
            Assert.AreEqual("test", memberProp.Get(wayInstance));
            Assert.AreEqual("test", wayInstance.Member);
        }

        [TestMethod]
        public void TestResourceMappingCreation()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way), out var mapping));
            Assert.IsTrue(mapping.IsResource);
            Assert.AreEqual("Way", mapping.Name);
            Assert.AreEqual(typeof(Way), mapping.NativeType);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way2), out mapping));
            Assert.IsTrue(mapping.IsResource);
            Assert.AreEqual("Way2", mapping.Name);
            Assert.AreEqual(typeof(Way2), mapping.NativeType);

            Assert.IsFalse(ClassMapping.TryCreate(typeof(Way2), out _, 1));
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way2), out _, 2));
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way2), out _, 3));
        }


        [TestMethod]
        public void TestDatatypeMappingCreation()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(AnimalName), out var mapping, 10));
            Assert.IsFalse(mapping.IsResource);
            Assert.AreEqual("AnimalName", mapping.Name);
            Assert.AreEqual(typeof(AnimalName), mapping.NativeType);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(NewAnimalName), out mapping));
            Assert.IsFalse(mapping.IsResource);
            Assert.AreEqual("AnimalName", mapping.Name);
            Assert.AreEqual(typeof(NewAnimalName), mapping.NativeType);

            Assert.IsFalse(ClassMapping.TryCreate(typeof(ComplexNumber), out _, 1));
            Assert.IsTrue(ClassMapping.TryCreate(typeof(ComplexNumber), out mapping, 5));
            Assert.IsFalse(mapping.IsResource);
            Assert.AreEqual("Complex", mapping.Name);
            Assert.AreEqual(typeof(ComplexNumber), mapping.NativeType);
        }
    }


    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class TestAttribute : Attribute
    {
        public TestAttribute(string data) => PositionalString = data;

        public string PositionalString { get; private set; }
    }


    /*
     * Resource classes for tests 
     */
    [FhirType("Way")]
    [Test("One")]
    [Test("Two")]
    public class Way : Resource
    {
        [Test("AttrA")]
        public string Member { get; set; }
        public override IDeepCopyable DeepCopy() => throw new NotImplementedException(); 
    }

    [FhirType("Way2", Since = 2)]
    public class Way2 : Resource 
    {
        public override IDeepCopyable DeepCopy() { throw new NotImplementedException(); } 
    }

    /* 
     * Datatype classes for tests
     */
    [FhirType("AnimalName")]
    public class AnimalName  { }

    [FhirType("AnimalName")]
    public class NewAnimalName : AnimalName { }

    [FhirType("Complex", Since=3)]
    public class ComplexNumber { }
}
