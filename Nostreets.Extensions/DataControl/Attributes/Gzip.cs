using System.IO.Compression;
using System.Web.Mvc;

namespace Nostreets.Extensions.DataControl.Attributes
{

    public class GzipAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            var context = filterContext.HttpContext;
            if (filterContext.Exception == null &&
                context.Response.Filter != null &&
                !filterContext.ActionDescriptor.IsDefined(typeof(GzipAttribute), true))
            {
                string acceptEncoding = context.Request.Headers["Accept-Encoding"]?.ToLower(); ;

                if (acceptEncoding != null)
                {
                    if (acceptEncoding.Contains("gzip"))
                    {
                        context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                        context.Response.AppendHeader("Content-Encoding", "gzip");
                    }
                    else if (acceptEncoding.Contains("deflate"))
                    {
                        context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                        context.Response.AppendHeader("Content-encoding", "deflate");
                    } 
                }
            }
        }
    }
}