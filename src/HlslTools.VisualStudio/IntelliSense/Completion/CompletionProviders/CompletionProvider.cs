using System.Collections.Generic;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders
{
    internal abstract class CompletionProvider<T> : ICompletionProvider
        where T : SyntaxNode
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var node = token.Parent.AncestorsAndSelf()
                .OfType<T>()
                .FirstOrDefault();

            return node == null
                ? Enumerable.Empty<CompletionItem>()
                : GetItems(semanticModel, position, node);
        }

        protected abstract IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position, T node);
    }
}