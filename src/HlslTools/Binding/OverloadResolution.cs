﻿using System.Collections.Generic;
using System.Linq;
using HlslTools.Binding.Signatures;
using HlslTools.Compilation;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal static class OverloadResolution
    {
        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, params TypeSymbol[] argumentTypes)
            where T : Signature
        {
            return Perform(signatures, (IReadOnlyList<TypeSymbol>) argumentTypes);
        }

        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, IReadOnlyList<TypeSymbol> argumentTypes)
            where T : Signature
        {
            // >= 64 arguments will overflow the bitwise scoring system we use to compare arguments. So just give up...
            if (argumentTypes.Count >= (1 << Conversion.NumConversionBits))
                return new OverloadResolutionResult<T>(null, null, Enumerable.Empty<OverloadResolutionCandidate<T>>());

            var candidates = new List<OverloadResolutionCandidate<T>>();
            foreach (var signature in signatures)
            {
                Conversion[] conversions;
                ulong score;
                if (!TryRankArguments(signature, argumentTypes, out conversions, out score))
                    continue;

                candidates.Add(new OverloadResolutionCandidate<T>(signature, conversions, score));
            }

            candidates.Sort((l, r) => l.Score.CompareTo(r.Score));

            var selected = candidates.FirstOrDefault();

            var best = (selected != null && (candidates.Count < 2 || candidates[1].Score > selected.Score))
                ? selected
                : null;

            return new OverloadResolutionResult<T>(best, selected, candidates);
        }

        private static bool TryRankArguments<T>(T signature, IReadOnlyList<TypeSymbol> argumentTypes, out Conversion[] conversions, out ulong score)
            where T : Signature
        {
            conversions = new Conversion[signature.ParameterCount];
            score = 0;

            var argumentCount = argumentTypes.Count;

            if (signature.HasVariadicParameter)
            {
                if (argumentCount < signature.ParameterCount)
                    return false;
            }
            else
            {
                if (signature.ParameterCount != argumentCount)
                    return false;
            }
            
            for (var i = 0; i < signature.ParameterCount; i++)
            {
                var parameterType = signature.GetParameterType(i);
                var argumentType = argumentTypes[i];

                var conversion = Conversion.Classify(argumentType, parameterType, signature.GetParameterDirection(i));
                if (!conversion.Exists)
                    return false;

                conversions[i] = conversion;

                score += (ulong) conversion.ImplicitConversionType;
            }

            return true;
        }
    }
}