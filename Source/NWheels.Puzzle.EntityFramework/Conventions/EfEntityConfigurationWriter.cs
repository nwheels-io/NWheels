using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Entities;
using NWheels.DataObjects;
using System.Linq.Expressions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    internal class EfEntityConfigurationWriter
    {
        private readonly ITypeMetadata _metadata;
        private readonly MethodWriterBase _method;
        private readonly Operand<DbModelBuilder> _model;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfEntityConfigurationWriter(ITypeMetadata metadata, MethodWriterBase method, Operand<DbModelBuilder> model)
        {
            _model = model;
            _method = method;
            _metadata = metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteEntityTypeConfiguration()
        {
            var m = _method;

            using ( TT.CreateScope<TT.TImpl>(_metadata.ImplementationType) )
            {
                var typeConfig = _method.Local(initialValue: _model.Func<EntityTypeConfiguration<TT.TImpl>>(x => x.Entity<TT.TImpl>));

                typeConfig.Func<string, EntityTypeConfiguration<TT.TImpl>>(x => x.ToTable, m.Const(_metadata.RelationalMapping.PrimaryTableName));
                typeConfig.Func<string, EntityTypeConfiguration<TT.TImpl>>(x => x.HasEntitySetName, m.Const(_metadata.Name));

                //foreach ( var property in _metadata.Properties )
                //{
                //    switch ( property.Kind )
                //    {
                //        case PropertyKind.Scalar:
                //            WriteScalarPropertyConfiguration(property, typeConfig);
                //            break;
                //        case PropertyKind.Relation:
                //            WriteRelationPropertyConfiguration(property, typeConfig);
                //            break;
                //    }
                //}
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteRelationPropertyConfiguration(IPropertyMetadata property, Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            if ( property.Relation.RelationKind == RelationKind.ManyToOne && property.Relation.ThisPartyKind == RelationPartyKind.Dependent )
            {
                WriteManyToOnePropertyConfiguration(property, typeConfig);
            }
            else if ( property.Relation.RelationKind == RelationKind.OneToMany && property.Relation.ThisPartyKind == RelationPartyKind.Principal )
            {
                WriteOneToManyPropertyConfiguration(property, typeConfig);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteOneToManyPropertyConfiguration(IPropertyMetadata property, Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteManyToOnePropertyConfiguration(IPropertyMetadata property, Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteScalarPropertyConfiguration(IPropertyMetadata property, Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            var m = _method;

            using ( TT.CreateScope<TT.TValue>(property.ClrType) )
            {
                //typeConfig.Func<Expression<Func<TT.TImpl, TT.TValue>>, PrimitivePropertyConfiguration>(x => x.Property<TT.TValue>, WriteNewPropertyExpression(property).CastTo<Expression<Func<TT.TImpl, TT.TValue>>>())
                
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IOperand<LambdaExpression> WriteNewPropertyExpression(IPropertyMetadata property)
        {
            throw new NotImplementedException();
        }
    }
}
