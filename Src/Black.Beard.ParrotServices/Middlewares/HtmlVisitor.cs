using Bb.Projects;
using HtmlAgilityPack;
using Flurl;
using Bb.Services;

namespace Bb.Middlewares
{
    public class HtmlVisitor : IHtmlVisitor
    {

        public HtmlVisitor(HttpContext context, AddressTranslator translator)
        {

            _context = context;
            var req = context.Request;
            _current = new Url(req.Scheme, req.Host.Host, req.Host.Port.HasValue ? req.Host.Port.Value : 80)
                .AppendPathSegments(translator.QuerySource, "swagger");
            _translator = translator;
        }

        public void VisitLinkElement(HtmlNode s)
        {

            if (s.Attributes.Contains("href"))
            {
                Url url = null;

                var value = s.Attributes["href"];
                s.Attributes.Remove("href");

                var extension = value.Value;
                if (extension.EndsWith(".css") || extension.EndsWith(".png"))
                {
                    url = new Url(_translator.QuerySource)
                        .AppendPathSegment(value.Value);
                    s.Attributes.Add("href", url.ToString());
                }
                else
                {

                    _context.Request.Path.StartsWithSegments(this._translator.QuerySource, out var q);

                    url = new Url(new Uri(_translator.TargetUrl))
                        .AppendPathSegment(q)
                        .AppendPathSegment(value.Value);

                    s.Attributes.Add("href", url.ToString());

                }

            }

            s.ChildNodes.Accept(this);
        }

        public void VisitScriptElement(HtmlNode s)
        {

            if (s.Attributes.Contains("src"))
            {

                Url url = null;

                var value = s.Attributes["src"];
                s.Attributes.Remove("src");

                var extension = value.Value;
                if (extension.EndsWith(".js"))
                {
                    url = new Url(_translator.QuerySource)
                        .AppendPathSegment(value.Value);
                    s.Attributes.Add("href", url.ToString());
                }
                else
                {
                    _context.Request.Path.StartsWithSegments(this._translator.QuerySource, out var q);

                    url = new Url(new Uri(_translator.TargetUrl))
                        .AppendPathSegment(q)
                        .AppendPathSegment(value.Value);

                    s.Attributes.Add("src", url.ToString());
                }
            }

            s.ChildNodes.Accept(this);
        }



        public void Visit(HtmlNode s)
        {

        }

        public void VisitComment(HtmlNode s)
        {
        }

        public void VisitDocument(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
            Console.WriteLine("case \"" + s.Name.ToLower() + "\"");
        }

        public void VisitText(HtmlNode s)
        {
        }

        public void VisitAbbrElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitAddressElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitAElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitArticleElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitAsideElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitAudioElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitBlockquoteElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitBodyElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitBrElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitCaptionElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitCiteElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitDelElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitDfnElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitDivElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitDlElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitDtElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitEmElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitFieldsetElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitFigCaptionElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitFigureElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitfooterElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitFormElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitH1Element(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitH2Element(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitH3Element(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitH4Element(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitH5Element(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitH6Element(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitHeadElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitHeardElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitHrElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitHtmlElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitImgElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitInputElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitInsElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitKbdElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitLabelElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitLegendElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitLiElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitMarkElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitMetaElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitNavElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitOlElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitOptgroupElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitOptionElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitPElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitPreElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitProgressElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitQElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitSectionElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitSelectElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitSourceElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitSpanElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitStrongElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitStyleElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitSubElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTableElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTBodyElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTdElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTextAreaElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTFootElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTheadElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitThElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTimeElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTitleElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitTrElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitUlElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }

        public void VisitVideoElement(HtmlNode s)
        {
            s.ChildNodes.Accept(this);
        }


        private readonly HttpContext _context;
        private readonly Url _current;
        private readonly AddressTranslator _translator;
    }


}
