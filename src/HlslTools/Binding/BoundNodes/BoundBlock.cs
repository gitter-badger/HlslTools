using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBlock : BoundStatement
    {
        public ImmutableArray<BoundStatement> Statements { get; set; }

        public BoundBlock(ImmutableArray<BoundStatement> statements)
            : base(BoundNodeKind.Block)
        {
            Statements = statements;
        }
    }
}