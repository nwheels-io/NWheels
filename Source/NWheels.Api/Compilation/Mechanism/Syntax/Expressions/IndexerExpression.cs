using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Expressions
{
    public class IndexerExpression : AbstractExpression
    {
        public IndexerExpression()
        {
            this.IndexArguments = new List<AbstractExpression>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitIndexerExpression(this);

            if (Target != null)
            {
                Target.AcceptVisitor(visitor);
            }

            foreach (var argument in IndexArguments)
            {
                argument.AcceptVisitor(visitor);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Target { get; set; }
        public List<AbstractExpression> IndexArguments { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractExpression Index
        {
            get
            {
                if (IndexArguments.Count == 0)
                {
                    throw new InvalidOperationException("Index arguments were not set");
                }

                if (IndexArguments.Count != 1)
                {
                    throw new InvalidOperationException("This is a multiple-argument indexer");
                }

                return IndexArguments[0];
            }
            set
            {
                IndexArguments.Clear();
                IndexArguments.Add(value);
            }
        }
    }
}
