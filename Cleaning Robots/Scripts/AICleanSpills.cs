﻿using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts
{
	[Serializable]
	public class Ava_CleaningRobots_AICleanSpills : IPart
	{
		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == GetInventoryActionsAlwaysEvent.ID || ID == InventoryActionEvent.ID || ID == GetDebugInternalsEvent.ID || ID == AIBoredEvent.ID;
		}

		public override bool HandleEvent(AIBoredEvent E)
		{
			if (CheckForSpills())
				return false;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
		{
			if (!ParentObject.IsPlayerControlled() && !ParentObject.Brain.IsHostileTowards(E.Actor) && !ParentObject.HasPart<SocialRoles>())
				E.AddAction("Configure", "configure", "Configure", Key: 'c');
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "Configure")
			{
			Configure:
				int choice = Popup.PickOption($"{ParentObject.The} {ParentObject.DisplayName} hums.", Options: new List<string>() { $"Configure cleaning threshold (currently {CleaningThreshold.Things("dram")} or lower)", $"Set to clean {(ImpureOnly ? "all liquids (currently impure only)" : "impure liquids only (currently all liquids)")}" }, AllowEscape: true);
				switch (choice)
				{
					case 0:
						int? newThreshold = Popup.AskNumber("Enter a new threshold.", Start: CleaningThreshold, Max: 64);
						if (newThreshold != null)
							CleaningThreshold = (int)newThreshold;
						goto Configure;
					case 1:
						ImpureOnly = !ImpureOnly;
						goto Configure;
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetDebugInternalsEvent E)
		{
			if (ParentObject.HasGoal(nameof(Ava_CleaningRobots_CleanSpillGoal)))
				E.AddEntry(this, "Spill cleaning", "Cleaning a spill");
			else
				E.AddEntry(this, "Spill cleaning", "Idle");
			if (_lastSpill != null)
				E.AddEntry(this, "Last spill location", $"{_lastSpill.CurrentCell.X}, {_lastSpill.CurrentCell.Y} in {_lastSpill.CurrentZone.ZoneID}");
			else
				E.AddEntry(this, "Last spill location", "Has not cleaned a spill");
			return base.HandleEvent(E);
		}

		public override bool WantTurnTick() => true;

		public override void TurnTick(long TurnNumber)
		{
			CheckForSpills();
		}

		private bool CheckForSpills()
		{
			if (ParentObject.CurrentZone == null) // Fix errors on init
				return false;
			if (!ParentObject.FireEvent("CanAIDoIndependentBehavior") || ParentObject.IsPlayerControlled())
				return false;
			if (ParentObject.IsBusy())
			{
				ParentObject.Brain.Think("I'm too busy to be cleaning.");
				return false;
			}
			var spills = ParentObject.CurrentZone.GetObjectsWithPart(nameof(LiquidVolume)).Where(x => ShouldClean(x.LiquidVolume));
			if (spills.IsNullOrEmpty())
			{
				ParentObject.Brain.Think("There's no spills to clean in this zone.");
				return false;
			}
			GameObject _targetSpill = null;
			int distance = int.MaxValue;
			foreach (var spill in spills)
			{
				if (ParentObject.canPathTo(spill.CurrentCell))
				{
					if (ParentObject.DistanceTo(spill) < distance)
					{
						_targetSpill = spill;
						distance = ParentObject.DistanceTo(spill);
					}
				}
			}
			if (_targetSpill == null)
			{
				ParentObject.Brain.Think("I couldn't find a spill to clean in this zone.");
				return false;
			}
			_lastSpill = _targetSpill;
			ParentObject.Brain.DidX("whirr", "studiously");
			ParentObject.Brain.PushGoal(new Ava_CleaningRobots_CleanSpillGoal(_targetSpill));
			ParentObject.Brain.Think("I've acquired a spill to clean up.");
			return true;
		}

		private bool ShouldClean(LiquidVolume lv)
		{
			return lv.IsOpenVolume() && !lv.EffectivelySealed() && lv.Volume <= CleaningThreshold && (!ImpureOnly || !lv.IsPure());
		}

		public int CleaningThreshold = 20;

		public bool ImpureOnly = true;

		private GameObject _lastSpill;
	}
}
