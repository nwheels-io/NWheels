using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class AttributeSyntaxEmitterTests
    {
        [Fact]
        public void AttributeWithNoValues()
        {
            //-- arrange 

            var attribute = new AttributeDescription() {
                AttributeType = typeof(TestAttributes.AttributeOne)
            };

            //-- act

            var syntax = AttributeSyntaxEmitter.EmitSyntax(attribute);

            //-- assert

            syntax.Should().BeEquivalentToCode("TestAttributes.AttributeOne");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void AttributeWithConstructorArguments()
        {
            //-- arrange

            var attribute = new AttributeDescription() {
                AttributeType = typeof(TestAttributes.AttributeOne)
            };

            attribute.ConstructorArguments.Add(new ConstantExpression() { Value = 123 });
            attribute.ConstructorArguments.Add(new ConstantExpression() { Value = "ABC" });

            //-- act

            var syntax = AttributeSyntaxEmitter.EmitSyntax(attribute);

            //-- assert

            syntax.Should().BeEquivalentToCode("TestAttributes.AttributeOne(123, \"ABC\")");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void AttributeWithPropertyValues()
        {
            //-- arrange

            var attribute = new AttributeDescription() {
                AttributeType = typeof(TestAttributes.AttributeOne)
            };

            attribute.PropertyValues.Add(new PropertyValue() { Name = "First", Value = new ConstantExpression() { Value = 123 } });
            attribute.PropertyValues.Add(new PropertyValue() { Name = "Second", Value = new ConstantExpression() { Value = "ABC" } });

            //-- act

            var syntax = AttributeSyntaxEmitter.EmitSyntax(attribute);

            //-- assert

            syntax.Should().BeEquivalentToCode("TestAttributes.AttributeOne(First = 123, Second = \"ABC\")");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void AttributeWithMixOfArgumentsAndProperties()
        {
            //-- arrange

            var attribute = new AttributeDescription() {
                AttributeType = typeof(TestAttributes.AttributeOne)
            };

            attribute.ConstructorArguments.Add(new ConstantExpression() { Value = 123 });
            attribute.ConstructorArguments.Add(new ConstantExpression() { Value = "ABC" });
            attribute.PropertyValues.Add(new PropertyValue() { Name = "First", Value = new ConstantExpression() { Value = 456 } });
            attribute.PropertyValues.Add(new PropertyValue() { Name = "Second", Value = new ConstantExpression() { Value = "DEF" } });

            //-- act

            var syntax = AttributeSyntaxEmitter.EmitSyntax(attribute);

            //-- assert

            syntax.Should().BeEquivalentToCode("TestAttributes.AttributeOne(123, \"ABC\", First = 456, Second = \"DEF\")");
        }
    }
}

namespace TestAttributes
{
    public class AttributeOne : Attribute
    {
    } 
}
