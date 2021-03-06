﻿using System.Collections.Generic;
using System.Linq;
using HlslTools.Binding;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Compilation
{
    public sealed class SemanticModel
    {
        private readonly BindingResult _bindingResult;

        public Compilation Compilation { get; }

        internal SemanticModel(Compilation compilation, BindingResult bindingResult)
        {
            Compilation = compilation;
            _bindingResult = bindingResult;
        }

        public ParameterSymbol GetDeclaredSymbol(ParameterSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax.Declarator) as BoundVariableDeclaration;
            return result?.VariableSymbol as ParameterSymbol;
        }

        public NamespaceSymbol GetDeclaredSymbol(NamespaceSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundNamespace;
            return result?.NamespaceSymbol;
        }

        public ClassSymbol GetDeclaredSymbol(ClassTypeSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundClassType;
            return result?.ClassSymbol;
        }

        public StructSymbol GetDeclaredSymbol(StructTypeSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundStructType;
            return result?.StructSymbol;
        }

        public VariableSymbol GetDeclaredSymbol(VariableDeclaratorSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundVariableDeclaration;
            return result?.VariableSymbol;
        }

        public FunctionSymbol GetDeclaredSymbol(FunctionDeclarationSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundFunction;
            return result?.FunctionSymbol;
        }

        public FunctionSymbol GetDeclaredSymbol(FunctionDefinitionSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundFunction;
            return result?.FunctionSymbol;
        }

        public TechniqueSymbol GetDeclaredSymbol(TechniqueSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundTechnique;
            return result?.TechniqueSymbol;
        }

        public Symbol GetSymbol(IdentifierDeclarationNameSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundName;
            return result?.Symbol;
        }

        public Symbol GetSymbol(SemanticSyntax syntax)
        {
            var result = _bindingResult.GetBoundNode(syntax) as BoundSemantic;
            return result?.SemanticSymbol;
        }

        public Symbol GetSymbol(ExpressionSyntax expression)
        {
            var boundExpression = GetBoundExpression(expression);
            return boundExpression == null ? null : GetSymbol(boundExpression);
        }

        private static Symbol GetSymbol(BoundExpression expression)
        {
            switch (expression.Kind)
            {
                case BoundNodeKind.VariableExpression:
                    return GetSymbol((BoundVariableExpression) expression);
                case BoundNodeKind.FunctionInvocationExpression:
                    return GetSymbol((BoundFunctionInvocationExpression) expression);
                case BoundNodeKind.MethodInvocationExpression:
                    return GetSymbol((BoundMethodInvocationExpression) expression);
                case BoundNodeKind.FieldExpression:
                    return GetSymbol((BoundFieldExpression) expression);
                case BoundNodeKind.Name:
                    return GetSymbol((BoundName) expression);
                case BoundNodeKind.IntrinsicGenericMatrixType:
                case BoundNodeKind.IntrinsicGenericVectorType:
                case BoundNodeKind.IntrinsicMatrixType:
                case BoundNodeKind.IntrinsicObjectType:
                case BoundNodeKind.IntrinsicScalarType:
                case BoundNodeKind.IntrinsicVectorType:
                    return GetSymbol((BoundType) expression);
                default:
                    // TODO: More bound expression types.
                    return null;
            }
        }

        private static Symbol GetSymbol(BoundVariableExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundFunctionInvocationExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundMethodInvocationExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundFieldExpression expression)
        {
            return expression.Field;
        }

        private static Symbol GetSymbol(BoundType expression)
        {
            return expression.TypeSymbol;
        }

        private static Symbol GetSymbol(BoundName expression)
        {
            return expression.Symbol;
        }

        public TypeSymbol GetExpressionType(ExpressionSyntax expression)
        {
            var boundExpression = GetBoundExpression(expression);
            return boundExpression?.Type;
        }

        private BoundExpression GetBoundExpression(ExpressionSyntax expression)
        {
            return _bindingResult.GetBoundNode(expression) as BoundExpression;
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _bindingResult.Diagnostics;
        }

        public IEnumerable<Symbol> LookupSymbols(SourceLocation position)
        {
            // TODO
            return IntrinsicFunctions.AllFunctions
                .AsEnumerable<Symbol>()
                .Union(IntrinsicSemantics.AllSemantics);

            //var node = FindClosestNodeWithBinder(_bindingResult.Root, position);
            //var binder = node == null ? null : _bindingResult.GetBinder(node);
            //return binder == null
            //    ? Enumerable.Empty<Symbol>()
            //    : LookupSymbols(binder);
        }

        //private static IEnumerable<Symbol> LookupSymbols(Binder binder)
        //{
        //    // NOTE: We want to only show the *available* symbols. That means, we need to
        //    //       hide symbols from the parent binder that have same name as the ones
        //    //       from a nested binder.
        //    //
        //    //       We do this by simply recording which names we've already seen.
        //    //       Please note that we *do* want to see duplicate names within the
        //    //       *same* binder.

        //    var allNames = new HashSet<string>();

        //    while (binder != null)
        //    {
        //        var localNames = new HashSet<string>();
        //        var localSymbols = binder.LocalSymbols
        //            .Where(s => !string.IsNullOrEmpty(s.Name));

        //        foreach (var symbol in localSymbols)
        //        {
        //            if (!allNames.Contains(symbol.Name))
        //            {
        //                localNames.Add(symbol.Name);
        //                yield return symbol;
        //            }
        //        }

        //        allNames.UnionWith(localNames);
        //        binder = binder.Parent;
        //    }
        //}

        //private SyntaxNode FindClosestNodeWithBinder(SyntaxNode root, SourceLocation position)
        //{
        //    var token = root.FindTokenContext(position);
        //    return (from n in token.Parent.AncestorsAndSelf()
        //            let bc = _bindingResult.GetBinder(n)
        //            where bc != null
        //            select n).FirstOrDefault();
        //}

        //protected abstract Binder GetEnclosingBinder(TextLocation location);

        //private TextSpan CheckAndAdjustPosition(int position, out SyntaxToken token)
        //{
        //    token = Root.FindToken(position);
        //    return token.Span;
        //}

        //public Symbol GetDeclaredSymbol(FunctionDefinitionSyntax syntax)
        //{
        //    return _bindingResult.GetSymbol(syntax);
        //}

        //public Symbol GetDeclaredSymbol(VariableDeclaratorSyntax syntax)
        //{
        //    return _bindingResult.GetSymbol(syntax);
        //}

        //public Symbol GetSymbol(ExpressionSyntax expression)
        //{
        //    var boundExpression = GetBoundExpression(expression);
        //    return boundExpression == null ? null : GetSymbol(boundExpression);
        //}

        //private static Symbol GetSymbol(BoundExpression expression)
        //{
        //    switch (expression.Kind)
        //    {
        //        case BoundNodeKind.FunctionInvocationExpression:
        //            return GetSymbol((BoundFunctionInvocationExpression)expression);
        //        default:
        //            return null;
        //    }
        //}

        //private static Symbol GetSymbol(BoundFunctionInvocationExpression expression)
        //{
        //    return expression.Symbol;
        //}

        //private BoundExpression GetBoundExpression(ExpressionSyntax expression)
        //{
        //    return _bindingResult.GetBoundNode(expression) as BoundExpression;
        //}
    }
}