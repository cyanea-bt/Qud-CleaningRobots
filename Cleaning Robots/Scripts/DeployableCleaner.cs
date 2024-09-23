using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class Ceres_CleaningRobots_DeployableCleaner : IScribedPart
	{
		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == GetInventoryActionsAlwaysEvent.ID || ID == InventoryActionEvent.ID;
		}

		public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
		{
			E.AddAction("Activate", "activate", "Activate", Key: 'a');
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "Activate" && AttemptDeploy(E.Actor))
			{
				E.RequestInterfaceExit();
			}
			return base.HandleEvent(E);
		}

		private bool AttemptDeploy(GameObject Actor)
		{
			if (Actor.OnWorldMap())
				return Actor.Fail("Your attempt to tidy up all of Qud is admirable, but alas. You cannot do this on the world map.");
			if (ParentObject.IsRusted())
				return Actor.Fail($"{ParentObject.Itis} rusted...");
			if (ParentObject.IsBroken())
				return Actor.Fail($"{ParentObject.Itis} broken...");
			Cell toDeploy = PickDirection($"Deploying {ParentObject.GetDisplayName()}", Actor);
			if (toDeploy == null)
				return false;
			if (!toDeploy.IsPassable() || toDeploy.HasObjectWithTag("ExcavatoryTerrainFeature"))
				return Actor.Fail("You can't deploy cleaners into a wall! That would make no sense. How is it going to fit?");
			ParentObject.SystemMoveTo(toDeploy, forced: true);
			GameObject newCleaner = ParentObject.ReplaceWith(GameObject.CreateUnmodified("Ceres_CleaningRobots_Cleaner"));
			newCleaner.Brain.Wanders = false;
			newCleaner.Brain.StartingCell = newCleaner.CurrentCell.GetGlobalLocation();
			newCleaner.PlayWorldSound("Sounds/Robot/sfx_turret_deploy");
			XDidYToZ(Actor, "deploy", newCleaner, EndMark: "! Beep boop.");
			Actor.UseEnergy(2000);
			return true;
		}
	}
}
