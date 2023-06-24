using HtmlAgilityPack;

namespace Bb.Middlewares
{
    public interface IHtmlVisitor
    {
        void Visit(HtmlNode s);
        void VisitAbbrElement(HtmlNode s);
        void VisitAddressElement(HtmlNode s);
        void VisitAElement(HtmlNode s);
        void VisitArticleElement(HtmlNode s);
        void VisitAsideElement(HtmlNode s);
        void VisitAudioElement(HtmlNode s);
        void VisitBlockquoteElement(HtmlNode s);
        void VisitBodyElement(HtmlNode s);
        void VisitBrElement(HtmlNode s);
        void VisitCaptionElement(HtmlNode s);
        void VisitCiteElement(HtmlNode s);
        void VisitComment(HtmlNode s);
        void VisitDelElement(HtmlNode s);
        void VisitDfnElement(HtmlNode s);
        void VisitDivElement(HtmlNode s);
        void VisitDlElement(HtmlNode s);
        void VisitDocument(HtmlNode s);
        void VisitDtElement(HtmlNode s);
        void VisitElement(HtmlNode s);
        void VisitEmElement(HtmlNode s);
        void VisitFieldsetElement(HtmlNode s);
        void VisitFigCaptionElement(HtmlNode s);
        void VisitFigureElement(HtmlNode s);
        void VisitfooterElement(HtmlNode s);
        void VisitFormElement(HtmlNode s);
        void VisitH1Element(HtmlNode s);
        void VisitH2Element(HtmlNode s);
        void VisitH3Element(HtmlNode s);
        void VisitH4Element(HtmlNode s);
        void VisitH5Element(HtmlNode s);
        void VisitH6Element(HtmlNode s);
        void VisitHeadElement(HtmlNode s);
        void VisitHeardElement(HtmlNode s);
        void VisitHrElement(HtmlNode s);
        void VisitHtmlElement(HtmlNode s);
        void VisitImgElement(HtmlNode s);
        void VisitInputElement(HtmlNode s);
        void VisitInsElement(HtmlNode s);
        void VisitKbdElement(HtmlNode s);
        void VisitLabelElement(HtmlNode s);
        void VisitLegendElement(HtmlNode s);
        void VisitLiElement(HtmlNode s);
        void VisitLinkElement(HtmlNode s);
        void VisitMarkElement(HtmlNode s);
        void VisitMetaElement(HtmlNode s);
        void VisitNavElement(HtmlNode s);
        void VisitOlElement(HtmlNode s);
        void VisitOptgroupElement(HtmlNode s);
        void VisitOptionElement(HtmlNode s);
        void VisitPElement(HtmlNode s);
        void VisitPreElement(HtmlNode s);
        void VisitProgressElement(HtmlNode s);
        void VisitQElement(HtmlNode s);
        void VisitScriptElement(HtmlNode s);
        void VisitSectionElement(HtmlNode s);
        void VisitSelectElement(HtmlNode s);
        void VisitSourceElement(HtmlNode s);
        void VisitSpanElement(HtmlNode s);
        void VisitStrongElement(HtmlNode s);
        void VisitStyleElement(HtmlNode s);
        void VisitSubElement(HtmlNode s);
        void VisitTableElement(HtmlNode s);
        void VisitTBodyElement(HtmlNode s);
        void VisitTdElement(HtmlNode s);
        void VisitText(HtmlNode s);
        void VisitTextAreaElement(HtmlNode s);
        void VisitTFootElement(HtmlNode s);
        void VisitTheadElement(HtmlNode s);
        void VisitThElement(HtmlNode s);
        void VisitTimeElement(HtmlNode s);
        void VisitTitleElement(HtmlNode s);
        void VisitTrElement(HtmlNode s);
        void VisitUlElement(HtmlNode s);
        void VisitVideoElement(HtmlNode s);
    }


}
