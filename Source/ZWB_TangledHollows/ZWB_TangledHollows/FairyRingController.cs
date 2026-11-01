using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ZWB_TangledHollows
{
    public class FairyRingController : ThingWithComps
    {
        //public List<Thing> fairyShrooms = new List<Thing>();
        public int tickerTime = 260;
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


    public class CompProperties_FairyRing : Verse.CompProperties
    {
        public float daysToDespawn = 7f;
        public float triggerChance = 0.1f;
        public float effectRadius = 6f;
        public List<HediffEffect> hediffEffects = new List<HediffEffect>();
        public CompProperties_FairyRing()
        {
            compClass = typeof(CompFairyRing);
        }
    }

    public class HediffEffect
    {
        public HediffDef hediffDef = ZTHDefOf.ZWB_Hediff_FRVictim;
        public float severity = 0.5f;
        public float maxSeverity = 0.8f;
        public float commonality = 1.0f;

        public string messageText = "";
    }


    public class CompFairyRing : ThingComp
    {

        public CompProperties_FairyRing PropsFRController => (CompProperties_FairyRing)props;
        public List<Thing> fairyShrooms = new List<Thing>();

        public override void CompTickRare()
        {
            if (!parent.Spawned)
            {
                return;
            }
            else if (parent.Position.Fogged(parent.Map))
            {
                return;
            }

            if (Find.TickManager.TicksGame - parent.TickSpawned >= PropsFRController.daysToDespawn * 60000)
            {
                DespawnController();
            }
            else
            {
                List<Pawn> victimList = parent.Map.mapPawns.AllHumanlikeSpawned;
                victimList = victimList.Where(p => p.Position.InHorDistOf(parent.Position, PropsFRController.effectRadius)).ToList();

                foreach (Pawn p in victimList)
                {
                    if (p.health.hediffSet.GetFirstHediffOfDef(ZTHDefOf.ZWB_Hediff_FRVictim) == null)
                    {
                        if (Rand.Chance(PropsFRController.triggerChance))
                        {
                            HediffEffect effect = PropsFRController.hediffEffects.RandomElementByWeight(h => h.commonality);

                            Hediff existing = p.health.hediffSet.GetFirstHediffOfDef(effect.hediffDef);
                            if (existing != null)
                            {
                                float sev = existing.Severity;
                                existing.Severity = Math.Min(existing.Severity + effect.severity, effect.maxSeverity);
                                p.health.hediffSet.GetFirstHediffOfDef(effect.hediffDef).Severity += effect.severity;
                            }
                            else
                            {
                                Hediff addEffect = HediffMaker.MakeHediff(effect.hediffDef, p);
                                addEffect.Severity = effect.severity;
                                p.health.AddHediff(addEffect);
                            }
                            Messages.Message(String.Format(effect.messageText, p.Name), p, MessageTypeDefOf.NegativeEvent);
                        }
                        Hediff protection = HediffMaker.MakeHediff(ZTHDefOf.ZWB_Hediff_FRVictim, p);
                        p.health.AddHediff(protection);
                    }
                }
            }
        }

        public void DespawnController()
        {
            Log.Message("Fairy ring danger is gone");
            FairyRingController controller = (FairyRingController)parent;

            fairyShrooms = fairyShrooms.Where(s => s.Spawned).ToList();

            Log.Message("Found this many shrooms remaining: " + fairyShrooms.Count);
            parent.DeSpawn();

            Messages.Message("The danger of the fairy ring has gone", MessageTypeDefOf.PositiveEvent);
        }

    }
}




