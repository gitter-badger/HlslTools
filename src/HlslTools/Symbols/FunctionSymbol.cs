﻿using System;
using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public class FunctionSymbol : InvocableSymbol
    {
        public FunctionSymbol(string name, string documentation, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(SymbolKind.Function, name, documentation, parent, returnType, lazyParameters)
        {
            
        }
    }
}