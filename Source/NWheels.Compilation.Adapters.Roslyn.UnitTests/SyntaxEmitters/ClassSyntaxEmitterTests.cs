using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class ClassSyntaxEmitterTests : SyntaxEmittingTestBase
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
            classMember.Interfaces.Add(new TypeMember(typeof(IDictionary<int, DateTime>)));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                public class ClassOne : 
                    System.Collections.Generic.List<string>, 
                    System.Collections.Generic.IDictionary<int, System.DateTime> 
                {  }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithMethods()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Members.Add(new MethodMember(MemberVisibility.Public, "First"));
            classMember.Members.Add(new MethodMember(MemberVisibility.Public, "Second"));

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
        public void WithFields()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Public, MemberModifier.None, typeof(int), "Number"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Public, MemberModifier.None, typeof(string), "Text"));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                public class ClassOne 
                { 
                    public int Number;
                    public string Text;
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithProperties()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Members.Add(new PropertyMember(classMember, MemberVisibility.Public, MemberModifier.None, typeof(int), "Number"));
            classMember.Members.Add(new PropertyMember(classMember, MemberVisibility.Public, MemberModifier.None, typeof(string), "Text"));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                public class ClassOne 
                { 
                    public int Number { get; }
                    public string Text { get; }
                }
            ");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void WithOneAttribute()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            classMember.Attributes.Add(new AttributeDescription() {
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

            classMember.Attributes.Add(new AttributeDescription() {
                AttributeType = new TypeMember(typeof(System.Diagnostics.DebuggerStepThroughAttribute))
            });
            classMember.Attributes.Add(new AttributeDescription() {
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
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanOrderMembersOfType()
        {
            //-- arrange

            #region Expected Code

            var expectedCode = @"
                public class ClassOne 
                { 
                    public int PublicField;
                    internal protected int _protectedInternalField;
                    protected int _protectedField;
                    internal int _internalField;
                    private int _privateField;

                    public ClassOne(string s) { }
                    internal protected ClassOne(byte b) { }
                    protected ClassOne(int n) { }
                    internal ClassOne(float f) { }
                    private ClassOne(decimal m) { }
                    
                    public void M1() { }
                    public int P1 { get; }

                    internal protected void M2() { }
                    internal protected int P2 { get; }

                    protected void M3() { }
                    protected int P3 { get; }

                    internal void M4() { }
                    internal int P4 { get; }

                    private void M5() { }
                    private int P5 { get; }

                    public static int PublicStaticField;
                    internal protected static int _s_protectedInternalStaticField;
                    protected static int _s_protectedStaticField;
                    internal static int _s_internalStaticField;
                    private static int _s_privateStaticField;

                    static ClassOne() { }

                    public static void StaticM1() { }
                    public static int StaticP1 { get; }

                    internal protected static void StaticM2() { }
                    internal protected static int StaticP2 { get; }

                    protected static void StaticM3() { }
                    protected static int StaticP3 { get; }

                    internal static void StaticM4() { }
                    internal static int StaticP4 { get; }

                    private static void StaticM5() { }
                    private static int StaticP5 { get; }
                }";

            #endregion

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            #region Add Members

            classMember.Members.AddRange(new PropertyMember[] {
                new PropertyMember(classMember, MemberVisibility.Protected, MemberModifier.Static, typeof(int), "StaticP3"),
                new PropertyMember(classMember, MemberVisibility.Private, MemberModifier.Static, typeof(int), "StaticP5"),
                new PropertyMember(classMember, MemberVisibility.InternalProtected, MemberModifier.Static, typeof(int), "StaticP2"),
                new PropertyMember(classMember, MemberVisibility.Public, MemberModifier.Static, typeof(int), "StaticP1"),
                new PropertyMember(classMember, MemberVisibility.Internal, MemberModifier.Static, typeof(int), "StaticP4"),
            });
            classMember.Members.AddRange(new AbstractMember[] {
                new ConstructorMember(MemberVisibility.Public, MemberModifier.Static, "ClassOne", new MethodSignature()),
                new ConstructorMember(MemberVisibility.Protected, MemberModifier.None, "ClassOne", new MethodSignature(
                    new[] { new MethodParameter("n", 1, typeof(int)) }, returnValue: null, isAsync: false
                )),
                new ConstructorMember(MemberVisibility.Private, MemberModifier.None, "ClassOne", new MethodSignature(
                    new[] { new MethodParameter("m", 1, typeof(decimal)) }, returnValue: null, isAsync: false
                )),
                new ConstructorMember(MemberVisibility.InternalProtected, MemberModifier.None, "ClassOne", new MethodSignature(
                    new[] { new MethodParameter("b", 1, typeof(byte)) }, returnValue: null, isAsync: false
                )),
                new ConstructorMember(MemberVisibility.Public, MemberModifier.None, "ClassOne", new MethodSignature(
                    new[] { new MethodParameter("s", 1, typeof(string)) }, returnValue: null, isAsync: false
                )),
                new ConstructorMember(MemberVisibility.Internal, MemberModifier.None, "ClassOne", new MethodSignature(
                    new[] { new MethodParameter("f", 1, typeof(float)) }, returnValue: null, isAsync: false
                )),
            });
            classMember.Members.AddRange(new AbstractMember[] {
                new MethodMember(MemberVisibility.Protected, MemberModifier.Static, "StaticM3", new MethodSignature()),
                new MethodMember(MemberVisibility.Private, MemberModifier.Static, "StaticM5", new MethodSignature()),
                new MethodMember(MemberVisibility.Internal, MemberModifier.Static, "StaticM4", new MethodSignature()),
                new MethodMember(MemberVisibility.Public, MemberModifier.Static, "StaticM1", new MethodSignature()),
                new MethodMember(MemberVisibility.InternalProtected, MemberModifier.Static, "StaticM2", new MethodSignature()),
            });
            classMember.Members.AddRange(new AbstractMember[] {
                new FieldMember(classMember, MemberVisibility.Protected, MemberModifier.Static, typeof(int), "_s_protectedStaticField"),
                new FieldMember(classMember, MemberVisibility.Private, MemberModifier.Static, typeof(int), "_s_privateStaticField"),
                new FieldMember(classMember, MemberVisibility.InternalProtected, MemberModifier.Static, typeof(int), "_s_protectedInternalStaticField"),
                new FieldMember(classMember, MemberVisibility.Public, MemberModifier.Static, typeof(int), "PublicStaticField"),
                new FieldMember(classMember, MemberVisibility.Internal, MemberModifier.Static, typeof(int), "_s_internalStaticField"),
            });
            classMember.Members.AddRange(new AbstractMember[] {
                new FieldMember(classMember, MemberVisibility.Protected, MemberModifier.None, typeof(int), "_protectedField"),
                new FieldMember(classMember, MemberVisibility.Private, MemberModifier.None, typeof(int), "_privateField"),
                new FieldMember(classMember, MemberVisibility.InternalProtected, MemberModifier.None, typeof(int), "_protectedInternalField"),
                new FieldMember(classMember, MemberVisibility.Public, MemberModifier.None, typeof(int), "PublicField"),
                new FieldMember(classMember, MemberVisibility.Internal, MemberModifier.None, typeof(int), "_internalField"),
            });
            classMember.Members.AddRange(new PropertyMember[] {
                new PropertyMember(classMember, MemberVisibility.Private, MemberModifier.None, typeof(int), "P5"),
                new PropertyMember(classMember, MemberVisibility.Protected, MemberModifier.None, typeof(int), "P3"),
                new PropertyMember(classMember, MemberVisibility.Internal, MemberModifier.None, typeof(int), "P4"),
                new PropertyMember(classMember, MemberVisibility.Public, MemberModifier.None, typeof(int), "P1"),
                new PropertyMember(classMember, MemberVisibility.InternalProtected, MemberModifier.None, typeof(int), "P2"),
            });
            classMember.Members.AddRange(new AbstractMember[] {
                new MethodMember(MemberVisibility.Private, MemberModifier.None, "M5", new MethodSignature()),
                new MethodMember(MemberVisibility.Protected, MemberModifier.None, "M3", new MethodSignature()),
                new MethodMember(MemberVisibility.Internal, MemberModifier.None, "M4", new MethodSignature()),
                new MethodMember(MemberVisibility.Public, MemberModifier.None, "M1", new MethodSignature()),
                new MethodMember(MemberVisibility.InternalProtected, MemberModifier.None, "M2", new MethodSignature()),
            });

            #endregion

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(expectedCode);
        }
    }
}
