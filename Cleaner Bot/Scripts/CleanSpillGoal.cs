using System;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers
{
	[Serializable]
	public class Ava_CleanerBot_CleanSpillGoal : GoalHandler
	{
		public Ava_CleanerBot_CleanSpillGoal(GameObject Target)
		{
			this.Target = Target;
		}

		public override bool Finished()
		{
			return Target.LiquidVolume == null || Target.LiquidVolume.Volume <= 0 || Target.LiquidVolume.IsPure();
		}

		public override void TakeAction()
		{
			if (Target != null && ParentObject.InZone(Target.CurrentZone.ZoneID))
			{
				CleanSpill();
				return;
			}
			FailToParent();
		}

		private void CleanSpill()
		{
			if (!ParentObject.InSameOrAdjacentCellTo(Target))
			{
				Think("I'm moving to a spill to clean up.");
				ParentBrain.PushGoal(new MoveTo(Target.CurrentCell, shortBy: 1));
				return;
			}
			Think("I'm cleaning up a spill.");
			int toVac = Stat.Random(1, 3);
			ParentBrain.DidXToY("drain", Target);
			Target.LiquidVolume.UseDrams(toVac);
			ParentObject.UseEnergy(1000, "Cleaning");
		}

		public GameObject Target;
	}
}
