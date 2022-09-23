using System;
using System.Collections.Generic;
using GTANetworkAPI;
using System.Text;


namespace GVMP.Module.WestenRoute
{
     class WestenRoute
     {
        public int ID { get; set; }
        public Vector3 Sammler { get; set; }
        public string Item { get; set; }
        public int PickupCount { get; set; }
        public int AddCount { get; set; }
        public float ColShapeRange { get; set; } = 5f;
    

        public WestenRoute  () { }
     }


}
