using System;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers
{
	[Serializable]
	public class Ceres_CleaningRobots_CleanSpillGoal : GoalHandler
	{
		public Ceres_CleaningRobots_CleanSpillGoal(GameObject Target)
		{
			this.Target = Target;
		}

		public override bool Finished() => Target.LiquidVolume == null || Target.LiquidVolume.Volume <= 0;

		public override void TakeAction()
		{
			if (Target != null)
			{
				if (GameObject.Validate(Target) && ParentObject.InZone(Target.CurrentZone?.ZoneID))
				{
					CleanSpill();
					return;
				}
				Think("I don't have a valid target anymore!");
				Target = null;
			}
			FailToParent();
		}

		/// <summary>
		/// Removes a random amount of drams from the currently targeted liquid volume.
		/// </summary>
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

		/// <summary>
		/// The liquid volume that we're currently targeting.
		/// The goal will be ended early in <see cref="Finished"/> if this is an invalid object, or if its liquid volume is empty.
		/// </summary>
		public GameObject Target;
	}
}
