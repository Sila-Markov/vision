using System;
using System.Collections.Generic;
using System.Text;

namespace GVMP.Module.WestenRoute
{
     class WestenRouteModule : GVMP.Module.Module<WestenRouteModule>
    {
        public static List<WestenRouteModule> farming = new List<WestenRouteModule>();
        public static Dictionary<string, int> farmingprices = new Dictionary<string, int>();

        protected override bool OnLoad()
        {
            var random = new Random();
            farming.Add(new WestenRouteModule());
            return false;
        }

     }
}
