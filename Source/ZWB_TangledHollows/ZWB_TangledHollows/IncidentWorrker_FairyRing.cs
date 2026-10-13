using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ZWB_TangledHollows
{
    public class FairyRingIncidentDef : IncidentDef
    {
        public ThingDef bigPlant = new ThingDef();
        public ThingDef smallPlant = new ThingDef();

        public SimpleCurve bigPlantCurve = new SimpleCurve();
        public SimpleCurve smallPlantCurve = new SimpleCurve();
        public SimpleCurve noPlantCurve = new SimpleCurve();
    }

    public class IncidentWorker_FairyRing : IncidentWorker
    {
        public FairyRingIncidentDef incidentDef;
        private const int MinRoomCells = 180;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            incidentDef = def as FairyRingIncidentDef;
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            //if (!PlantUtility.GrowthSeasonNow(map, incidentDef.plantDef))
            //{
            //    return false;
            //}

            IntVec3 cell;
            return TryFindRootCell(map, out cell);
        }

        private bool TryFindRootCell(Map map, out IntVec3 cell)
        {
            return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => CanSpawnAt(x, map) && x.GetRoom(map).CellCount >= MinRoomCells, map, out cell);
        }

        private bool CanSpawnAt(IntVec3 c, Map map)
        {
            if (!isValidSpawnCell(c, map))
            {
                return false;
            }
            IEnumerable<IntVec3> testCells = GenRadial.RadialCellsAround(c, 8, true);
            for (int i = 0; i < 20; i++)
            {
                IntVec3 cell = testCells.RandomElement();
                if (!isValidSpawnCell(c, map))
                {
                    return false;
                }
            }

            return true;
        }

        private bool isValidSpawnCell(IntVec3 c, Map map)
        {
            if (!c.Standable(map) || c.Fogged(map) || map.fertilityGrid.FertilityAt(c) < 0.5f || !c.GetRoom(map).PsychologicallyOutdoors || c.GetEdifice(map) != null)
            {
                return false;
            }
            return true;
        }



        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            incidentDef = def as FairyRingIncidentDef;

            Map map = (Map)parms.target;
            if (!TryFindRootCell(map, out var cell))
            {
                return false;
            }

            Thing targetThing = null;


            if (TryFindRootCell(map, out var centerCell))
            {
                IEnumerable<IntVec3> FairyRingCells = GenRadial.RadialCellsAround(centerCell, 13, true);
                List<Thing> fairyShrooms = new List<Thing>();

                foreach (IntVec3 c in FairyRingCells)
                {
                    float dist = (float)Math.Sqrt(Math.Pow(c.x - centerCell.x, 2) + Math.Pow(c.z - centerCell.z, 2));

                    if (Rand.Chance(incidentDef.bigPlantCurve.Evaluate(dist)))
                    {
                        foreach (Thing thing in map.thingGrid.ThingsAt(c).ToList())
                        {
                            if (thing as Plant != null)
                            {
                                thing.Destroy(DestroyMode.Vanish);
                            }
                        }
                        if (incidentDef.bigPlant.CanEverPlantAt(c, map))
                        {

                            Thing bigPlant = GenSpawn.Spawn(incidentDef.bigPlant, c, map);
                            fairyShrooms.Add(bigPlant);
                            if (targetThing == null)
                            {
                                targetThing = bigPlant;
                            }
                        }
                    }

                    else if (Rand.Chance(incidentDef.smallPlantCurve.Evaluate(dist)))
                    {
                        foreach (Thing thing in map.thingGrid.ThingsAt(c).ToList())
                        {
                            if (thing as Plant != null)
                            {
                                thing.Destroy(DestroyMode.Vanish);
                            }
                        }
                        if (incidentDef.smallPlant.CanEverPlantAt(c, map))
                        {

                            GenSpawn.Spawn(incidentDef.smallPlant, c, map);
                        }
                    }

                    else if (Rand.Chance(incidentDef.noPlantCurve.Evaluate(dist)))
                    {
                        foreach (Thing thing in map.thingGrid.ThingsAt(c).ToList())
                        {
                            if (thing as Plant != null)
                            {
                                thing.Destroy(DestroyMode.Vanish);
                            }
                        }

                    }
                }

                FairyRingController controller = (FairyRingController)GenSpawn.Spawn(ZTHDefOf.ZWB_FairyRingController, centerCell, map);
                controller.fairyShrooms = fairyShrooms;
                targetThing = controller;

            }

            if (targetThing == null)
            {
                return false;
            }



            SendStandardLetter(parms, targetThing);
            return true;

        }





    }
}