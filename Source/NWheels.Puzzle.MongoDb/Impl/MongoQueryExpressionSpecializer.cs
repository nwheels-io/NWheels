using System.Linq.Expressions;

namespace NWheels.Puzzle.MongoDb.Impl
{
    public static class MongoQueryExpressionSpecializer
    {
        public static Expression Specialize(Expression general)
        {
            return general;
        }
    }
}
