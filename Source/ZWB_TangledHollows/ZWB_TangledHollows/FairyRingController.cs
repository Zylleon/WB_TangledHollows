using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ZWB_TangledHollows
{
    public  class FairyRingController : ThingWithComps
    {
        public List<Thing> fairyShrooms = new List<Thing>();

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {

            //if (floodedCells.Any())
            //{
            //    int num = floodedCells.Values.Min();
            //    foreach (KeyValuePair<IntVec3, int> floodedCell in floodedCells)
            //    {
            //        floodedCell.Deconstruct(out var key, out var value);
            //        IntVec3 c = key;
            //        int num2 = value;
            //        base.Map.tempTerrain.QueueRemoveTerrain(c, Find.TickManager.TicksGame + 2000 + num2 - num);
            //    }
            //}
            base.Destroy(mode);
        }
    }
}