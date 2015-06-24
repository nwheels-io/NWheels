using System.Linq.Expressions;

namespace NWheels.Stacks.MongoDb.Impl
{
    public static class MongoQueryExpressionSpecializer
    {
        public static Expression Specialize(Expression general)
        {
            return general;
        }
    }
}
