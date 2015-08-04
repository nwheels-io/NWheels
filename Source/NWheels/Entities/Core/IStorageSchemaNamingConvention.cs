using NWheels.DataObjects;
using NWheels.DataObjects.Core;

namespace NWheels.Entities.Core
{
    public interface IStorageSchemaNamingConvention
    {
        string NameTypePrimaryTable(TypeMetadataBuilder type);
        string NameTypeRelationTable(TypeMetadataBuilder type1, TypeMetadataBuilder type2);
        string NamePropertyColumnTable(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        string NamePropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        string NamePropertyColumnType(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        string NameKeyPropertyColumnType(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property);
        string NameKeyPropertyColumn(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property);
        string NameForeignKeyPropertyColumn(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        string NameRelationTableForeignKeyColumn(TypeMetadataBuilder type);
        string NameKeyPropertyColumnTable(TypeMetadataBuilder type, KeyMetadataBuilder key, PropertyMetadataBuilder property);
        string NameEntityPartColumnPrefix(TypeMetadataBuilder type, PropertyMetadataBuilder property);
        string QualifyTableNameWithNamespace(ITypeMetadata type, string tableName, string namespaceName);
    }
}