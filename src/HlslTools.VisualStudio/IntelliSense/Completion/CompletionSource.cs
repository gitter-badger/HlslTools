using System.Collections.Generic;
using HlslTools.VisualStudio.Glyphs;
using Microsoft.VisualStudio.Language.Intellisense;

namespace HlslTools.VisualStudio.IntelliSense.Completion
{
    internal sealed class CompletionSource : ICompletionSource
    {
        private readonly DispatcherGlyphService _glyphService;

        public CompletionSource(DispatcherGlyphService glyphService)
        {
            _glyphService = glyphService;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var completionModel = session.Properties.GetProperty<CompletionModelManager>(typeof(CompletionModelManager));
            var completionSet = new HlslCompletionSet(session, completionModel, _glyphService);
            completionSets.Add(completionSet);
        }

        public void Dispose()
        {
        }
    }
}