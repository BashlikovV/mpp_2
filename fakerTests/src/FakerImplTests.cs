using Microsoft.VisualStudio.TestTools.UnitTesting;
using faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fakerTests;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace faker.Tests
{
    [TestClass()]
    public class FakerImplTests
    {

        [TestMethod]
        public void defaultClassTest()
        {
            var faker = new FakerImpl();
            var obj = faker.create<ExampleClass>();
            obj.field.Should().NotBe(float.NaN);
            obj.property.Should().NotBe(float.NaN);
            foreach (var list in obj.list)
            {
                list.Should().NotBeNull();
                foreach (var item in list)
                {
                    item.Should().NotBeNull();
                    item.field.Should().NotBe(float.NaN);
                    item.property.Should().NotBe(float.NaN);
                }
            }
        }

        [TestMethod]
        public void recursiveDetectionTest()
        {
            var action = () =>
            {
                var faker = new FakerImpl();
                faker.create<RecursiveClass1>();
            };
            action.Should().Throw<Exception>();
        }
    }
}