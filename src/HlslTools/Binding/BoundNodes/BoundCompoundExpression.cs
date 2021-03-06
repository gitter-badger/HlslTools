using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundCompoundExpression : BoundExpression
    {
        public BoundCompoundExpression(BoundExpression left, BoundExpression right)
            : base(BoundNodeKind.CompoundExpression)
        {
            Type = right.Type;

            Left = left;
            Right = right;
        }

        public override TypeSymbol Type { get; }

        public BoundExpression Left { get; }
        public BoundExpression Right { get; }
    }
}