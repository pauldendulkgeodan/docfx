// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.MarkdigEngine.Extensions
{
    using Markdig;
    using Markdig.Renderers;
    using Markdig.Renderers.Html;
    using Microsoft.DocAsCode.Common;

    public class HtmlInclusionBlockRenderer : HtmlObjectRenderer<InclusionBlock>
    {
        private MarkdownContext _context;
        private MarkdownPipeline _pipeline;

        public HtmlInclusionBlockRenderer(MarkdownContext context, MarkdownPipeline pipeline)
        {
            _context = context;
            _pipeline = pipeline;
        }

        protected override void Write(HtmlRenderer renderer, InclusionBlock inclusion)
        {
            var (content, includeFilePath) = _context.ReadFile(inclusion.IncludedFilePath, InclusionContext.File);

            if (content == null)
            {
                Logger.LogWarning($"Cannot resolve '{inclusion.IncludedFilePath}' relative to '{InclusionContext.File}'.");
                renderer.Write(inclusion.GetRawToken());
                return;
            }

            if (InclusionContext.IsCircularReference(includeFilePath, out var dependencyChain))
            {
                Logger.LogWarning($"Found circular reference: {string.Join(" -> ", dependencyChain)}\"");
                renderer.Write(inclusion.GetRawToken());
                return;
            }
            using (InclusionContext.PushFile(includeFilePath))
            {
                renderer.Write(Markdown.ToHtml(content, _pipeline));
            }
        }
    }
}