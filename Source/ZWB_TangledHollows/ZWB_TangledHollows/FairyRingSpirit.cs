using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace ZWB_TangledHollows
{
    public class FairyRingSpirit : ThingWithComps
    {
        public List<Thing> fairyShrooms = new List<Thing>();
        public int shroomCount = 0;
        public int violationCount = 0;
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shroomCount, "shroomCount");
            Scribe_Values.Look(ref violationCount, "violationCount");
            Scribe_Collections.Look(ref fairyShrooms, "fairyShrooms", LookMode.Reference);
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
        public GameConditionDef badOutcome = GameConditionDefOf.ColdSnap;
        public FloatRange badOutcomeDuration = new FloatRange(1,2);
        public string badLetterLabel = "";
        public string badLetterText = "";
        public string midLetterText = "";
        public string midLetterLabel = "";
        public GameConditionDef goodOutcome = GameConditionDefOf.ColdSnap;
        public FloatRange goodOutcomeDuration = new FloatRange(1, 2);
        public string goodLetterLabel = "";
        public string goodLetterText = "";
    }

    public class HediffEffect
    {
        public HediffDef hediffDef = ZTHDefOf.ZWB_Hediff_FRBlocker;
        public float severity = 0.5f;
        public float maxSeverity = 0.8f;
        public float commonality = 1.0f;
        public string messageText = "";
    }


    public class CompFairyRing : ThingComp
    {

        public CompProperties_FairyRing PropsFRController => (CompProperties_FairyRing)props;
        //public List<Thing> fairyShrooms = new List<Thing>();

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
                return;
            }
            FairyRingSpirit controller = (FairyRingSpirit)parent;
            controller.fairyShrooms = controller.fairyShrooms.Where(s => s.Spawned).ToList();
            if(controller.fairyShrooms.NullOrEmpty())
            {
                DespawnController();
                return;
            }

            else
            {
                List<Pawn> victimList = parent.Map.mapPawns.AllHumanlikeSpawned;
                victimList = victimList.Where(p => p.Position.InHorDistOf(parent.Position, PropsFRController.effectRadius)).ToList();

                foreach (Pawn p in victimList)
                {
                    if (p.health.hediffSet.GetFirstHediffOfDef(ZTHDefOf.ZWB_Hediff_FRBlocker) == null)
                    {
                        controller.violationCount += 1;
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
                        Hediff protection = HediffMaker.MakeHediff(ZTHDefOf.ZWB_Hediff_FRBlocker, p);
                        p.health.AddHediff(protection);
                    }
                }
            }
        }

        public void DespawnController()
        {
            Log.Message("Fairy ring danger is gone");
            FairyRingSpirit controller = (FairyRingSpirit)parent;

            controller.fairyShrooms = controller.fairyShrooms.Where(s => s.Spawned).ToList();

            Log.Message(String.Format("Found this many shrooms remaining: {0} out of {1}", controller.fairyShrooms.Count, controller.shroomCount));
            parent.Destroy();

            if (controller.fairyShrooms.Count < controller.shroomCount * 0.25)
            {
                // fail
                Log.Message("The fairies are displeased");
                int duration = Mathf.RoundToInt(PropsFRController.badOutcomeDuration.RandomInRange * 60000f);
                GameCondition gameCondition = GameConditionMaker.MakeCondition(PropsFRController.badOutcome, duration);
                Find.LetterStack.ReceiveLetter(PropsFRController.badLetterLabel, PropsFRController.badLetterText, LetterDefOf.NegativeEvent, new TargetInfo(controller.Position, controller.Map)); 
            }
            else if (controller.fairyShrooms.Count > controller.shroomCount * 0.9 && controller.violationCount < 4)
            {
                // good
                Log.Message("The fairies are pleased");
                int duration = Mathf.RoundToInt(PropsFRController.goodOutcomeDuration.RandomInRange * 60000f);
                GameCondition gameCondition = GameConditionMaker.MakeCondition(PropsFRController.goodOutcome, duration);
                Find.LetterStack.ReceiveLetter(PropsFRController.goodLetterText, PropsFRController.goodLetterText, LetterDefOf.PositiveEvent, new TargetInfo(controller.Position, controller.Map));
            }
            else
            {
                //mid
                Find.LetterStack.ReceiveLetter(PropsFRController.midLetterText, PropsFRController.midLetterText, LetterDefOf.NeutralEvent, new TargetInfo(controller.Position, controller.Map));

            }

            Messages.Message("The spirit of the fairy ring has gone", MessageTypeDefOf.PositiveEvent);
        }

    }
}




