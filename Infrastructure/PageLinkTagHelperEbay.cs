using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Store.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Store.Infrastructure
{
    [HtmlTargetElement("ol", Attributes = "page-model")]
    public class PageLinkTagHelperEbay : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelperEbay(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }

        public PagingInfo? PageModel {  get; set; }
        public string? PageAction { get; set; }

        public string? QueryName {  get; set; }

        public int EbayCategory {  get; set; }
        
        public int LowPrice { get; set; }

        public int UpPrice { get; set; }

        [HtmlAttributeName(DictionaryAttributePrefix = "page-url-")]
        public Dictionary<string, object> PageUrlValues { get; set; }
            = new Dictionary<string, object>();

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? Context { get; set; }

        public bool PageClassesEnabled { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Context != null && PageModel != null)
            {
                IUrlHelper urlHelper =
                    urlHelperFactory.GetUrlHelper(Context);

                TagBuilder result = new TagBuilder("div");

                if (PageModel.ItemsCount > 0 && 
                    PageModel.CurrentPage <= PageModel.TotalPages &&
                    PageModel.TotalPages > 1)
                {
                    TagBuilder prev = new TagBuilder("li");
                    prev.Attributes["class"] = "page-item";

                    TagBuilder a1 = new TagBuilder("a");
                    a1.Attributes["class"] = "page-link";

                    PageUrlValues["queryName"] = QueryName;  // Replace with dynamic values
                    PageUrlValues["categoryNumber"] = EbayCategory;          // Replace with dynamic values
                    PageUrlValues["priceLow"] = LowPrice;                   // Replace with dynamic values
                    PageUrlValues["priceHigh"] = UpPrice;

                    PageUrlValues["itemPage"] = (PageModel.CurrentPage - 1 == 0 ? 1 :
                        PageModel.CurrentPage - 1);
                    a1.Attributes["href"] = urlHelper.Action(PageAction,
                        PageUrlValues);
                    a1.Attributes["aria-label"] = "Previous";
                    //a1.Attributes["page-url-categoryId"] = PageModel.CurrentCategory.ToString(); 

                    TagBuilder span1 = new TagBuilder("span");
                    TagBuilder span2 = new TagBuilder("span");

                    span1.Attributes["aria-hidden"] = "true";
                    span1.InnerHtml.Append("\u00AB");

                    span2.Attributes["class"] = "sr-only";

                    a1.InnerHtml.AppendHtml(span1);
                    a1.InnerHtml.AppendHtml(span2);

                    prev.InnerHtml.AppendHtml(a1);

                    result.InnerHtml.AppendHtml(prev);
                }
                for (int i = 1; i <= PageModel.TotalPages &&
                    PageModel.CurrentPage <= PageModel.TotalPages; i++)
                {
                    TagBuilder li = new TagBuilder("li");
                    li.Attributes["class"] = "page-item";

                    if (PageClassesEnabled && i == PageModel.CurrentPage)
                    {
                        li.Attributes["class"] = "page-item active";
                    }
                    TagBuilder a = new TagBuilder("a");
                    a.Attributes["class"] = "page-link";

                    PageUrlValues["queryName"] = QueryName;  // Replace with dynamic values
                    PageUrlValues["categoryNumber"] = EbayCategory;          // Replace with dynamic values
                    PageUrlValues["priceLow"] = LowPrice;                   // Replace with dynamic values
                    PageUrlValues["priceHigh"] = UpPrice;

                    PageUrlValues["itemPage"] = i;
                    a.Attributes["href"] = urlHelper.Action(PageAction,
                        PageUrlValues);
                    
                    a.InnerHtml.Append(i.ToString());
                    li.InnerHtml.AppendHtml(a);
                    result.InnerHtml.AppendHtml(li);
                }
                if (PageModel.ItemsCount > 0 &&
                    PageModel.CurrentPage <= PageModel.TotalPages &&
                    PageModel.TotalPages > 1)
                {
                    TagBuilder next = new TagBuilder("li");
                    next.Attributes["class"] = "page-item";

                    TagBuilder a2 = new TagBuilder("a");
                    a2.Attributes["class"] = "page-link";

                    PageUrlValues["queryName"] = QueryName;  // Replace with dynamic values
                    PageUrlValues["categoryNumber"] = EbayCategory;          // Replace with dynamic values
                    PageUrlValues["priceLow"] = LowPrice;                   // Replace with dynamic values
                    PageUrlValues["priceHigh"] = UpPrice;

                    PageUrlValues["itemPage"] = (PageModel.CurrentPage + 1 >
                        PageModel.TotalPages ? PageModel.TotalPages
                        : PageModel.CurrentPage + 1);

                    a2.Attributes["href"] = urlHelper.Action(PageAction,
                        PageUrlValues);
                    a2.Attributes["aria-label"] = "Next";

                    TagBuilder span3 = new TagBuilder("span");
                    TagBuilder span4 = new TagBuilder("span");

                    span3.Attributes["aria-hidden"] = "true";
                    span4.InnerHtml.Append("\u00BB");

                    span3.Attributes["class"] = "sr-only";

                    a2.InnerHtml.AppendHtml(span3);
                    a2.InnerHtml.AppendHtml(span4);

                    next.InnerHtml.AppendHtml(a2);

                    result.InnerHtml.AppendHtml(next);
                }
                output.Content.AppendHtml(result.InnerHtml);
            }
        }
    }
}
