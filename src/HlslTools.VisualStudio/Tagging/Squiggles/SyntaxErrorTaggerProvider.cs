﻿using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SyntaxErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public IOptionsService OptionsService { get; set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<SyntaxErrorTagger, T>(buffer,
                () => new SyntaxErrorTagger(textView, buffer.GetBackgroundParser(SourceTextFactory), OptionsService, ServiceProvider, TextDocumentFactoryService),
                SourceTextFactory);
        }
    }
}