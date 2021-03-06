﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;

using SiliconStudio.Core;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    class TestUpdate
    {
        #region Test class definitions
        public class SimpleClass
        {
            public SimpleClass(int firstValue, string secondValue)
            {
                FirstValue = firstValue;
                SecondValue = secondValue;
            }

            [DataMember(1)]
            public int FirstValue;

            [DataMember(2)]
            public string SecondValue { get; set; }
        }

        public struct SimpleStruct
        {
            private string secondValue;

            public SimpleStruct(double firstValue, string secondValue)
            {
                FirstValue = firstValue;
                this.secondValue = secondValue;
            }
            [DataMember(1)]
            public double FirstValue;

            [DataMember(2)]
            public string SecondValue { get { return secondValue; } set { secondValue = value; } }
        }

        public class SimpleClassWithSimpleStruct
        {
            public SimpleClassWithSimpleStruct(double structFirstValue, string structSecondValue)
            {
                Struct = new SimpleStruct(structFirstValue, structSecondValue);
            }

            [DataMember(1)]
            public SimpleStruct Struct { get; set; }
        }

        public struct NestedStruct
        {
            private string secondValue;

            public NestedStruct(double firstValue, string secondValue, double innerStructFirstValue, string innerStructSecondValue)
            {
                FirstValue = firstValue;
                this.secondValue = secondValue;
                InnerStruct = new SimpleStruct(innerStructFirstValue, innerStructSecondValue);
            }

            [DataMember(1)]
            public double FirstValue;

            [DataMember(2)]
            public string SecondValue { get { return secondValue; } set { secondValue = value; } }

            [DataMember(3)]
            public SimpleStruct InnerStruct;
        }

        public class SimpleClassWithNestedStruct
        {
            public SimpleClassWithNestedStruct(double structFirstValue, string structSecondValue, double innerStructFirstValue, string innerStructSecondValue)
            {
                Struct = new NestedStruct(structFirstValue, structSecondValue, innerStructFirstValue, innerStructSecondValue);
            }

            [DataMember(1)]
            public NestedStruct Struct { get; set; }
        }
        #endregion Test class definitions

        [Test]
        public void TestSimpleContent()
        {
            var obj = new SimpleClass(1, "test");
            Assert.That(obj.FirstValue, Is.EqualTo(1));
            Assert.That(obj.SecondValue, Is.EqualTo("test"));

            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            model.GetChild("FirstValue").Content.Value = 2;
            model.GetChild("SecondValue").Content.Value = "new value";

            Assert.That(obj.FirstValue, Is.EqualTo(2));
            Assert.That(obj.SecondValue, Is.EqualTo("new value"));
        }

        [Test]
        public void TestSimpleNestedStruct()
        {
            var obj = new SimpleClassWithSimpleStruct(1.0, "test");
            Assert.That(obj.Struct.FirstValue, Is.EqualTo(1.0));
            Assert.That(obj.Struct.SecondValue, Is.EqualTo("test"));

            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            model.GetChild("Struct").GetChild("FirstValue").Content.Value = 2.0;
            model.GetChild("Struct").GetChild("SecondValue").Content.Value = "new value";

            Assert.That(obj.Struct.FirstValue, Is.EqualTo(2.0));
            Assert.That(obj.Struct.SecondValue, Is.EqualTo("new value"));
        }

        [Test]
        public void TestMultipleNestedStruct()
        {
            var obj = new SimpleClassWithNestedStruct(1.0, "test", 5.0, "inner value");
            Assert.That(obj.Struct.FirstValue, Is.EqualTo(1.0));
            Assert.That(obj.Struct.SecondValue, Is.EqualTo("test"));
            Assert.That(obj.Struct.InnerStruct.FirstValue, Is.EqualTo(5.0));
            Assert.That(obj.Struct.InnerStruct.SecondValue, Is.EqualTo("inner value"));

            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            model.GetChild("Struct").GetChild("FirstValue").Content.Value = 2.0;
            model.GetChild("Struct").GetChild("SecondValue").Content.Value = "new value";
            model.GetChild("Struct").GetChild("InnerStruct").GetChild("FirstValue").Content.Value = 7.0;
            model.GetChild("Struct").GetChild("InnerStruct").GetChild("SecondValue").Content.Value = "new inner value";

            Assert.That(obj.Struct.FirstValue, Is.EqualTo(2.0));
            Assert.That(obj.Struct.SecondValue, Is.EqualTo("new value"));
            Assert.That(obj.Struct.InnerStruct.FirstValue, Is.EqualTo(7.0));
            Assert.That(obj.Struct.InnerStruct.SecondValue, Is.EqualTo("new inner value"));
        }
    }
}
