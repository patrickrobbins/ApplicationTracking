using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace DependencyTracker.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new Bundle("~/bundles/jquery").Include(
                        "~/Scripts/lib/jquery-3.7.1.min.js"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                        "~/Scripts/lib/bootstrap.bundle.min.js"));

            bundles.Add(new Bundle("~/bundles/jqueryval").Include(
                        "~/Scripts/lib/jquery.validate.min.js",
                        "~/Scripts/lib/jquery.validate.unobtrusive.min.js"));

            bundles.Add(new Bundle("~/bundles/cytoscape").Include(
                        "~/Scripts/lib/cytoscape.min.js"));

            bundles.Add(new Bundle("~/bundles/graph").Include(
                        "~/Scripts/app/graph-config.js",
                        "~/Scripts/app/graph.js"));

            bundles.Add(new Bundle("~/Content/css").Include(
                      "~/Content/css/lib/bootstrap.min.css",
                      "~/Content/css/site.css",
                      "~/Content/css/graph.css"));
        }
    }
}
