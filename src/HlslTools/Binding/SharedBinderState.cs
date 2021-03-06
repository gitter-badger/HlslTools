﻿using System.Collections.Generic;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal sealed class SharedBinderState
    {
        public Dictionary<SyntaxNode, BoundNode> BoundNodeFromSyntaxNode { get; } = new Dictionary<SyntaxNode, BoundNode>();
        public Dictionary<BoundNode, Binder> BinderFromBoundNode { get; } = new Dictionary<BoundNode, Binder>();
        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();
    }
}