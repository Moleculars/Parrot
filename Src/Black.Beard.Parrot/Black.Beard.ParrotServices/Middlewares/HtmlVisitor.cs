using Bb.Projects;
using HtmlAgilityPack;
using Flurl;

namespace Bb.Middlewares
{
    public class HtmlVisitor : IHtmlVisitor
    {

        public HtmlVisitor(HttpContext context, Uri targetUri1, string aliasUri)
        {

            _context = context;

            this._targetUri1 = targetUri1;
            this._aliasUri = aliasUri;
        }

        public void VisitLinkElement(HtmlNode s)
        {

            if (s.Attributes.Contains("href"))
            {
                var value = s.Attributes["href"];
                s.Attributes.Remove("href");


                _context.Request.Path.StartsWithSegments(this._aliasUri, out var q);

                var url = new Url(new Uri(_context.Request.Host.ToString()))
                    .AppendPathSegment(this._aliasUri)
                    .AppendPathSegment(q)      
                    .AppendPathSegment(value.Value);

                //var u = new Uri(_targetUri1, q, value.Value);

                s.Attributes.Add("href", url.ToString());

            }


            s.ChildNodes.Accept(this);
        }

        public void VisitScriptElement(HtmlNode s)
        {

            if (s.Attributes.Contains("src"))
            {

                var value = s.Attributes["src"];
                s.Attributes.Remove("src");


                _context.Request.Path.StartsWithSegments(this._aliasUri, out var q);

                var url = new Url(new Uri(_context.Request.Host.ToString()))
                    .AppendPathSegment(this._aliasUri)
                    .AppendPathSegment(q)
                    .AppendPathSegment(value.Value);

                //var u = new Uri(_targetUri1, q, value.Value);

                s.Attributes.Add("src", url.ToString());

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
        private Uri _targetUri1;
        private string _aliasUri;

    }


}
