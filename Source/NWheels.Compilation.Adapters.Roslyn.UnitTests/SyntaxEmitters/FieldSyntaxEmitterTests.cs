using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests.SyntaxEmitters
{
    public class FieldSyntaxEmitterTests
    {
        [Fact]
        public void ClassWithFields()
        {
            //-- arrange

            var classMember = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");

            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Public, MemberModifier.None, typeof(int), "PublicNumber"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Protected, MemberModifier.None, typeof(int), "ProtectedNumber"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Internal, MemberModifier.None, typeof(int), "InternalNumber"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.InternalProtected, MemberModifier.None, typeof(int), "InternalProtectedNumber"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Private, MemberModifier.None, typeof(int), "_privateNumber"));

            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Public, MemberModifier.Static, typeof(string), "PublicString"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Protected, MemberModifier.Static, typeof(string), "ProtectedString"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Internal, MemberModifier.Static, typeof(string), "InternalString"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.InternalProtected, MemberModifier.Static, typeof(string), "InternalProtectedString"));
            classMember.Members.Add(new FieldMember(classMember, MemberVisibility.Private, MemberModifier.Static, typeof(string), "_s_privateString"));

            var emitter = new ClassSyntaxEmitter(classMember);

            //-- act

            var syntax = emitter.EmitSyntax();

            //-- assert

            syntax.Should().BeEquivalentToCode(@"
                public class ClassOne 
                { 
                    public int PublicNumber;
                    protected int ProtectedNumber;
                    internal int InternalNumber;
                    internal protected int InternalProtectedNumber;
                    private int _privateNumber;

                    public static string PublicString;
                    protected static string ProtectedString;
                    internal static string InternalString;
                    internal protected static string InternalProtectedString;
                    private static string _s_privateString;
                }
            ");
        }
    }
}
