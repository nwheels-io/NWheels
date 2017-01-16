using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class ClassSyntaxEmitterTests
    {
        [Fact]
        public void Empty()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public class ClassOne { }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithBase()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.BaseType = new TypeMember(typeof(System.IO.Stream));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public class ClassOne : System.IO.Stream { }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithOneInterface()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Interfaces.Add(new TypeMember(typeof(System.IDisposable)));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public class ClassOne : System.IDisposable { }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithBaseAndManyInterfaces()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.BaseType = new TypeMember(typeof(System.IO.Stream));
            classMember.Interfaces.Add(new TypeMember(typeof(System.IDisposable)));
            classMember.Interfaces.Add(new TypeMember(typeof(System.IFormattable)));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "public class ClassOne : System.IO.Stream, System.IDisposable, System.IFormattable { }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithGenericBaseTypes()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.BaseType = new TypeMember(typeof(List<string>));
            classMember.Interfaces.Add(new TypeMember(typeof(Dictionary<int, DateTime>)));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                public class ClassOne : 
                    System.Collections.Generic.List<System.String>, 
                    System.Collections.Generic.Dictionary<System.Int32, System.DateTime> 
                {  }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithMethods()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Members.Add(new MethodMember(MemberVisibility.Public, MethodParameter.Void, "First"));
            classMember.Members.Add(new MethodMember(MemberVisibility.Public, MethodParameter.Void, "Second"));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                public class ClassOne 
                { 
                    public void First() { }
                    public void Second() { } 
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithOneAttribute()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Attributes.Add(new AttributeInfo() {
                AttributeType = new TypeMember(typeof(System.Diagnostics.DebuggerStepThroughAttribute))
            });

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(
                "[System.Diagnostics.DebuggerStepThroughAttribute] public class ClassOne { }"
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithMultipleAttributes()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            classMember.Attributes.Add(new AttributeInfo() {
                AttributeType = new TypeMember(typeof(System.Diagnostics.DebuggerStepThroughAttribute))
            });
            classMember.Attributes.Add(new AttributeInfo() {
                AttributeType = new TypeMember(typeof(System.ComponentModel.LocalizableAttribute))
            });

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                [System.Diagnostics.DebuggerStepThroughAttribute, System.ComponentModel.LocalizableAttribute] 
                public class ClassOne { }
            ");
        }
    }
}
