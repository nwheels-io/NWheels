using FluentAssertions;
using NWheels.Compilation.Mechanism.Syntax;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Visit = System.Tuple<string, object>;

namespace NWheels.Core.UnitTests.Compilation.Mechanism.Syntax
{
    public class MemberVisitorTests
    {
        [Fact]
        public void CanVisitTypeMembers()
        {
            //-- arrange

            var class1 = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "Class1");
            var field1 = new FieldMember(class1, MemberVisibility.Private, MemberModifier.None, typeof(int), "_field1");
            var field2 = new FieldMember(class1, MemberVisibility.Private, MemberModifier.None, typeof(int), "_field2");
            var constructor1 = new ConstructorMember(MemberVisibility.Public, MemberModifier.None, "Class1", new MethodSignature());
            var constructor2 = new ConstructorMember(MemberVisibility.Public, MemberModifier.Static, "Class1", new MethodSignature());
            var method1 = new MethodMember(MemberVisibility.Public, MemberModifier.None, "M1", new MethodSignature());
            var method2 = new MethodMember(MemberVisibility.Public, MemberModifier.None, "M2", new MethodSignature());
            var property1 = new PropertyMember(class1, MemberVisibility.Public, MemberModifier.None, typeof(int), "P1");
            var property2 = new PropertyMember(class1, MemberVisibility.Public, MemberModifier.None, typeof(int), "P2");
            var event1 = new EventMember(MemberVisibility.Public, MemberModifier.None, typeof(Action), "E1");
            var event2 = new EventMember(MemberVisibility.Public, MemberModifier.None, typeof(Action), "E2");

            class1.Members.AddRange(new AbstractMember[] {
                field1, field2, constructor1, constructor2, method1, method2, property1, property2, event1, event2
            });

            property2.Getter = new MethodMember(property2.Visibility, "get_" + property2.Name);
            property2.Setter = new MethodMember(property2.Visibility, "set_" + property2.Name);

            event2.Adder = new MethodMember(property2.Visibility, "add_" + event2.Name);
            event2.Remover = new MethodMember(property2.Visibility, "remove_" + event2.Name);

            var visitLog = new List<Visit>();
            var visitor = new TestMemberVisitor(visitLog);

            //-- act

            class1.AcceptVisitor(visitor);

            //-- assert

            visitLog.Should().Equal(
                new Visit(nameof(MemberVisitor.VisitAbstractMember), class1),
                new Visit(nameof(MemberVisitor.VisitTypeMember), class1),
                new Visit(nameof(MemberVisitor.VisitClassType), class1),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), field1),
                new Visit(nameof(MemberVisitor.VisitField), field1),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), field2),
                new Visit(nameof(MemberVisitor.VisitField), field2),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), constructor1),
                new Visit(nameof(MemberVisitor.VisitMethodBase), constructor1),
                new Visit(nameof(MemberVisitor.VisitConstructor), constructor1),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), constructor2),
                new Visit(nameof(MemberVisitor.VisitMethodBase), constructor2),
                new Visit(nameof(MemberVisitor.VisitConstructor), constructor2),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), method1),
                new Visit(nameof(MemberVisitor.VisitMethodBase), method1),
                new Visit(nameof(MemberVisitor.VisitMethod), method1),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), method2),
                new Visit(nameof(MemberVisitor.VisitMethodBase), method2),
                new Visit(nameof(MemberVisitor.VisitMethod), method2),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), property1),
                new Visit(nameof(MemberVisitor.VisitProperty), property1),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), property2),
                new Visit(nameof(MemberVisitor.VisitProperty), property2),
                new Visit(nameof(MemberVisitor.VisitAbstractMember), property2.Getter),
                new Visit(nameof(MemberVisitor.VisitMethodBase), property2.Getter),
                new Visit(nameof(MemberVisitor.VisitMethod), property2.Getter),
                new Visit(nameof(MemberVisitor.VisitAbstractMember), property2.Setter),
                new Visit(nameof(MemberVisitor.VisitMethodBase), property2.Setter),
                new Visit(nameof(MemberVisitor.VisitMethod), property2.Setter),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), event1),
                new Visit(nameof(MemberVisitor.VisitEvent), event1),

                new Visit(nameof(MemberVisitor.VisitAbstractMember), event2),
                new Visit(nameof(MemberVisitor.VisitEvent), event2),
                new Visit(nameof(MemberVisitor.VisitAbstractMember), event2.Adder),
                new Visit(nameof(MemberVisitor.VisitMethodBase), event2.Adder),
                new Visit(nameof(MemberVisitor.VisitMethod), event2.Adder),
                new Visit(nameof(MemberVisitor.VisitAbstractMember), event2.Remover),
                new Visit(nameof(MemberVisitor.VisitMethodBase), event2.Remover),
                new Visit(nameof(MemberVisitor.VisitMethod), event2.Remover)
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestMemberVisitor : MemberVisitor
        {
            private readonly List<Visit> _visitLog;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestMemberVisitor(List<Visit> visitLog)
            {
                _visitLog = visitLog;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitInterfaceType(TypeMember type)
            {
                base.VisitInterfaceType(type);
                _visitLog.Add(new Visit(nameof(VisitInterfaceType), type));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitAttribute(AttributeDescription attribute)
            {
                base.VisitAttribute(attribute);
                _visitLog.Add(new Visit(nameof(VisitAttribute), attribute));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitClassType(TypeMember type)
            {
                base.VisitClassType(type);
                _visitLog.Add(new Visit(nameof(VisitClassType), type));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitStructType(TypeMember type)
            {
                base.VisitStructType(type);
                _visitLog.Add(new Visit(nameof(VisitStructType), type));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitEnumType(TypeMember type)
            {
                base.VisitEnumType(type);
                _visitLog.Add(new Visit(nameof(VisitEnumType), type));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitEnumMember(EnumMember member)
            {
                base.VisitEnumMember(member);
                _visitLog.Add(new Visit(nameof(VisitEnumMember), member));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitField(FieldMember field)
            {
                base.VisitField(field);
                _visitLog.Add(new Visit(nameof(VisitField), field));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitConstructor(ConstructorMember constructor)
            {
                base.VisitConstructor(constructor);
                _visitLog.Add(new Visit(nameof(VisitConstructor), constructor));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitMethod(MethodMember method)
            {
                base.VisitMethod(method);
                _visitLog.Add(new Visit(nameof(VisitMethod), method));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitProperty(PropertyMember property)
            {
                base.VisitProperty(property);
                _visitLog.Add(new Visit(nameof(VisitProperty), property));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void VisitEvent(EventMember @event)
            {
                base.VisitEvent(@event);
                _visitLog.Add(new Visit(nameof(VisitEvent), @event));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal protected override void VisitAbstractMember(AbstractMember member)
            {
                base.VisitAbstractMember(member);
                _visitLog.Add(new Visit(nameof(VisitAbstractMember), member));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal protected override void VisitTypeMember(TypeMember type)
            {
                base.VisitTypeMember(type);
                _visitLog.Add(new Visit(nameof(VisitTypeMember), type));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal protected override void VisitMethodBase(MethodMemberBase method)
            {
                base.VisitMethodBase(method);
                _visitLog.Add(new Visit(nameof(VisitMethodBase), method));
            }
        }
    }
}
