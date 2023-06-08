using HtmlAgilityPack;
using Microsoft.AspNetCore.SignalR;
using SharpYaml;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System;

namespace Bb.Middlewares
{
    public class ProxyTransformHtmlResponse : ProxyTransformResponse
    {
        protected override string Transform(HttpContext context, string payload)
        {

            var dom = new HtmlDocument();
            dom.LoadHtml(payload);

            var visitor = new HtmlVisitor(context, base._targetUri, base._aliasUri);
            dom.DocumentNode.Accept(visitor);

            //var documentHeader = dom.DocumentNode.SelectSingleNode("//h1");

            return payload;
        }

    }


    public static class HtmlPackExtension
    {

        public static void Accept(this IEnumerable<HtmlNode> self, IHtmlVisitor visitor)
        {

            foreach (var item in self)
            {
                item.Accept(visitor);

            }

        }

        public static void Accept(this HtmlNode self, IHtmlVisitor visitor)
        {

            switch (self.NodeType)
            {

                case HtmlNodeType.Document:
                    visitor.VisitDocument(self);
                    break;

                case HtmlNodeType.Element:
                    switch (self.Name.ToLower())
                    {
                        case "html":
                            visitor.VisitHtmlElement(self);
                            break;
                        case "meta":
                            visitor.VisitMetaElement(self);
                            break;
                        case "title":
                            visitor.VisitTitleElement(self);
                            break;
                        case "link":
                            visitor.VisitLinkElement(self);
                            break;
                        case "style":
                            visitor.VisitStyleElement(self);
                            break;
                        case "head":
                            visitor.VisitHeadElement(self);
                            break;
                        case "div":
                            visitor.VisitDivElement(self);
                            break;
                        case "script":
                            visitor.VisitScriptElement(self);
                            break;
                        case "body":
                            visitor.VisitBodyElement(self);
                            break;

                        case "img":
                            visitor.VisitImgElement(self);
                            break;

                        case "br":
                            visitor.VisitBrElement(self);
                            break;
                        case "hr":
                            visitor.VisitHrElement(self);
                            break;
                        case "input":
                            visitor.VisitInputElement(self);
                            break;

                        case "abbr":
                            visitor.VisitAbbrElement(self);
                            break;
                        case "blockquote":
                            visitor.VisitBlockquoteElement(self);
                            break;
                        case "q":
                            visitor.VisitQElement(self);
                            break;
                        case "cite":
                            visitor.VisitCiteElement(self);
                            break;
                        case "sub":
                            visitor.VisitSubElement(self);
                            break;
                        case "sup":
                            visitor.VisitSubElement(self);
                            break;
                        case "h1":
                            visitor.VisitH1Element(self);
                            break;
                        case "h2":
                            visitor.VisitH2Element(self);
                            break;
                        case "h3":
                            visitor.VisitH3Element(self);
                            break;
                        case "h4":
                            visitor.VisitH4Element(self);
                            break;
                        case "h5":
                            visitor.VisitH5Element(self);
                            break;
                        case "h6":
                            visitor.VisitH6Element(self);
                            break;
                        case "mark":
                            visitor.VisitMarkElement(self);
                            break;
                        case "strong":
                            visitor.VisitStrongElement(self);
                            break;
                        case "em":
                            visitor.VisitEmElement(self);
                            break;
                        case "figure":
                            visitor.VisitFigureElement(self);
                            break;
                        case "figcaption":
                            visitor.VisitFigCaptionElement(self);
                            break;
                        case "audio":
                            visitor.VisitAudioElement(self);
                            break;
                        case "video":
                            visitor.VisitVideoElement(self);
                            break;
                        case "source":
                            visitor.VisitSourceElement(self);
                            break;
                        case "a":
                            visitor.VisitAElement(self);
                            break;
                        case "p":
                            visitor.VisitPElement(self);
                            break;
                        case "address":
                            visitor.VisitAddressElement(self);
                            break;
                        case "del":
                            visitor.VisitDelElement(self);
                            break;
                        case "ins":
                            visitor.VisitInsElement(self);
                            break;
                        case "dfn":
                            visitor.VisitDfnElement(self);
                            break;
                        case "kbd":
                            visitor.VisitKbdElement(self);
                            break;
                        case "progress":
                            visitor.VisitProgressElement(self);
                            break;
                        case "time":
                            visitor.VisitTimeElement(self);
                            break;
                        case "pre":
                            visitor.VisitPreElement(self);
                            break;
                        case "ul":
                            visitor.VisitUlElement(self);
                            break;
                        case "ol":
                            visitor.VisitOlElement(self);
                            break;
                        case "li":
                            visitor.VisitLiElement(self);
                            break;
                        case "dl":
                            visitor.VisitDlElement(self);
                            break;
                        case "dt":
                            visitor.VisitDtElement(self);
                            break;
                        case "table":
                            visitor.VisitTableElement(self);
                            break;
                        case "caption":
                            visitor.VisitCaptionElement(self);
                            break;
                        case "tr":
                            visitor.VisitTrElement(self);
                            break;
                        case "th":
                            visitor.VisitThElement(self);
                            break;
                        case "td":
                            visitor.VisitTdElement(self);
                            break;
                        case "thead":
                            visitor.VisitTheadElement(self);
                            break;
                        case "tbody":
                            visitor.VisitTBodyElement(self);
                            break;
                        case "tfoot":
                            visitor.VisitTFootElement(self);
                            break;
                        case "form":
                            visitor.VisitFormElement(self);
                            break;
                        case "fieldset":
                            visitor.VisitFieldsetElement(self);
                            break;
                        case "legend":
                            visitor.VisitLegendElement(self);
                            break;
                        case "label":
                            visitor.VisitLabelElement(self);
                            break;
                        case "textarea":
                            visitor.VisitTextAreaElement(self);
                            break;
                        case "select":
                            visitor.VisitSelectElement(self);
                            break;
                        case "option":
                            visitor.VisitOptionElement(self);
                            break;
                        case "optgroup":
                            visitor.VisitOptgroupElement(self);
                            break;
                        case "header":
                            visitor.VisitHeardElement(self);
                            break;
                        case "nav":
                            visitor.VisitNavElement(self);
                            break;
                        case "footer":
                            visitor.VisitfooterElement(self);
                            break;
                        case "section":
                            visitor.VisitSectionElement(self);
                            break;
                        case "article":
                            visitor.VisitArticleElement(self);
                            break;
                        case "aside":
                            visitor.VisitAsideElement(self);
                            break;
                        case "span":
                            visitor.VisitSpanElement(self);
                            break;

                        default:
                            visitor.VisitElement(self);
                            break;
                    }
                    break;

                case HtmlNodeType.Comment:
                    visitor.VisitComment(self);
                    break;

                case HtmlNodeType.Text:
                    visitor.VisitText(self);
                    break;

                default:
                    visitor.Visit(self);
                    break;

            }



        }

    }


}
