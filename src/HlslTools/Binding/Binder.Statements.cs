﻿using System;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal partial class Binder
    {
        private BoundBlock BindBlock(BlockSyntax syntax, Symbol parent)
        {
            var blockBinder = new Binder(_sharedBinderState, this);
            return new BoundBlock(syntax.Statements.Select(x => blockBinder.Bind(x, y => blockBinder.BindStatement(y, parent))).ToImmutableArray());
        }

        private BoundStatement BindStatement(StatementSyntax syntax, Symbol parent)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.Block:
                    return BindBlock((BlockSyntax) syntax, parent);
                case SyntaxKind.BreakStatement:
                    return BindBreakStatement((BreakStatementSyntax) syntax);
                case SyntaxKind.DiscardStatement:
                    return BindDiscardStatement((DiscardStatementSyntax) syntax);
                case SyntaxKind.DoStatement:
                    return BindDoStatement((DoStatementSyntax) syntax, parent);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax) syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax) syntax, parent);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax) syntax, parent);
                case SyntaxKind.ReturnStatement:
                    return BindReturnStatement((ReturnStatementSyntax) syntax);
                case SyntaxKind.VariableDeclarationStatement:
                    return BindVariableDeclarationStatement((VariableDeclarationStatementSyntax) syntax, parent);
                case SyntaxKind.SwitchStatement:
                    return BindSwitchStatement((SwitchStatementSyntax) syntax, parent);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax) syntax, parent);
                case SyntaxKind.EmptyStatement:
                    return BindEmptyStatement((EmptyStatementSyntax) syntax);
                default:
                    throw new NotSupportedException("Not supported: " + syntax.Kind);
            }
        }

        private BoundStatement BindEmptyStatement(EmptyStatementSyntax syntax)
        {
            return new BoundNoOpStatement();
        }

        private BoundStatement BindDoStatement(DoStatementSyntax syntax, Symbol parent)
        {
            return new BoundDoStatement(
                Bind(syntax.Condition, BindExpression),
                Bind(syntax.Statement, x => BindStatement(x, parent)));
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax, Symbol parent)
        {
            return new BoundWhileStatement(
                Bind(syntax.Condition, BindExpression),
                Bind(syntax.Statement, x => BindStatement(x, parent)));
        }

        private BoundStatement BindSwitchStatement(SwitchStatementSyntax syntax, Symbol parent)
        {
            var switchBinder = new Binder(_sharedBinderState, this);
            var boundSections = syntax.Sections.Select(x => switchBinder.Bind(x, y => switchBinder.BindSwitchSection(y, parent))).ToImmutableArray();

            return new BoundSwitchStatement(
                Bind(syntax.Expression, BindExpression),
                boundSections);
        }

        private BoundSwitchSection BindSwitchSection(SwitchSectionSyntax syntax, Symbol parent)
        {
            return new BoundSwitchSection(
                syntax.Labels.Select(x => Bind(x, BindSwitchLabel)).ToImmutableArray(),
                syntax.Statements.Select(x => Bind(x, y => BindStatement(y, parent))).ToImmutableArray());
        }

        private BoundSwitchLabel BindSwitchLabel(SwitchLabelSyntax syntax)
        {
            BoundExpression boundExpression;
            switch (syntax.Kind)
            {
                case SyntaxKind.CaseSwitchLabel:
                    var caseSwitchLabel = (CaseSwitchLabelSyntax) syntax;
                    boundExpression = Bind(caseSwitchLabel.Value, BindExpression);
                    break;
                case SyntaxKind.DefaultSwitchLabel:
                    boundExpression = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return new BoundSwitchLabel(boundExpression);
        }

        private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
        {
            return new BoundBreakStatement();
        }

        private BoundStatement BindDiscardStatement(DiscardStatementSyntax syntax)
        {
            return new BoundDiscardStatement();
        }

        private BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax syntax) 
        {
            return new BoundExpressionStatement(Bind(syntax.Expression, BindExpression));
        }

        private BoundForStatement BindForStatement(ForStatementSyntax syntax, Symbol parent)
        {
            var forStatementBinder = new Binder(_sharedBinderState, this);

            // Note that we bind declarations in the current scope, not the for statement scope.

            return new BoundForStatement(
                syntax.Declaration != null ? Bind(syntax.Declaration, x => BindForStatementDeclaration(x, parent)) : null,
                syntax.Initializer != null ? forStatementBinder.Bind(syntax.Initializer, forStatementBinder.BindExpression) : null,
                forStatementBinder.Bind(syntax.Condition, forStatementBinder.BindExpression),
                syntax.Incrementor != null ? forStatementBinder.Bind(syntax.Incrementor, forStatementBinder.BindExpression) : null,
                forStatementBinder.Bind(syntax.Statement, x => forStatementBinder.BindStatement(x, parent)));
        }

        private BoundMultipleVariableDeclarations BindForStatementDeclaration(VariableDeclarationSyntax syntax, Symbol parent)
        {
            // When binding for loop declarations, allow redefinition of variables from enclosing scope. (X3078)
            // Use most recently declared variable. Add a warning to diagnostics.

            return BindVariableDeclaration(syntax, parent, (d, t) =>
            {
                var existingSymbol = _symbols.FirstOrDefault(x => x.Name == d.Identifier.Text);
                if (existingSymbol != null)
                {
                    _symbols.Remove(existingSymbol);
                    Diagnostics.ReportLoopControlVariableConflict(d);
                }
                return new VariableSymbol(d, parent, t);
            });
        }

        private BoundIfStatement BindIfStatement(IfStatementSyntax syntax, Symbol parent)
        {
            return new BoundIfStatement(
                Bind(syntax.Condition, BindExpression),
                Bind(syntax.Statement, x => BindStatement(x, parent)),
                syntax.Else != null ? Bind(syntax.Else.Statement, x => BindStatement(x, parent)) : null);
        }

        private BoundReturnStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            return new BoundReturnStatement(syntax.Expression != null ? Bind(syntax.Expression, BindExpression) : null);
        }

        private BoundMultipleVariableDeclarations BindVariableDeclarationStatement(VariableDeclarationStatementSyntax syntax, Symbol parent)
        {
            return BindVariableDeclaration(syntax.Declaration, parent);
        }
    }
}