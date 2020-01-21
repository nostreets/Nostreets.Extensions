using RazorGenerator.Mvc;

using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Nostreets.Extensions.Utilities
{
    public class CSharpRazorViewEngine : RazorViewEngine
    {
        public CSharpRazorViewEngine()
        {
            AreaViewLocationFormats = new[]
             {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            AreaMasterLocationFormats = new[]
             {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            AreaPartialViewLocationFormats = new[]
             {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            ViewLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml",
             "~/Views/Shared/{0}.cshtml"
             };
            MasterLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml",
             "~/Views/Shared/{0}.cshtml"
             };
            PartialViewLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml",
             "~/Views/Shared/{0}.cshtml"
             };
        }
    }

    public class PrecompiledViewEngine : PrecompiledMvcEngine
    {
        public PrecompiledViewEngine(Assembly assembly) : base(assembly)
        {
            AreaViewLocationFormats = new[]
               {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            AreaMasterLocationFormats = new[]
             {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            AreaPartialViewLocationFormats = new[]
             {
             "~/Areas/{2}/Views/{1}/{0}.cshtml",
             "~/Areas/{2}/Views/Shared/{0}.cshtml"
             };
            ViewLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml",
             "~/Views/Shared/{0}.cshtml"
             };
            MasterLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml",
             "~/Views/Shared/{0}.cshtml"
             };
            PartialViewLocationFormats = new[]
             {
             "~/Views/{1}/{0}.cshtml",
             "~/Views/Shared/{0}.cshtml"
             };
            PreemptPhysicalFiles = true;
            UsePhysicalViewsIfNewer = HttpContext.Current?.Request.IsLocal ?? false;
        }
    }
}