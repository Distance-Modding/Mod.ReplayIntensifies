using Distance.ReplayIntensifies.Randomizer;
using System.Collections.Generic;
using UnityEngine;

namespace Distance.ReplayIntensifies.Scripts
{
	/// <summary>
	/// Storage used by all replays when determining random factors.
	/// </summary>
	public class ReplayManagerCompoundData : MonoBehaviour
	{
		public List<RandomCarType> OriginalCarTypes { get; internal set; }
		public List<RandomColorPreset> OriginalCarPresets { get; internal set; }

		public List<RandomCarType> CarTypes { get; internal set; }
		public List<RandomColorPreset> CarPresets { get; internal set; }


		public CarReplayData.CarData ChooseRandomCarData(CarReplayData.CarData origCarData, int seed, int placement)
		{
			// Reset any lists that have been exhausted by the 'avoid duplicate' settings.
			if (this.CarTypes != null && this.CarTypes.Count == 0)
			{
				this.CarTypes = RandomCarType.CloneListAndReset(this.OriginalCarTypes); // reset remaining counts
			}
			if (this.CarPresets != null && this.CarPresets.Count == 0)
			{
				this.CarPresets = RandomColorPreset.CloneListAndReset(this.OriginalCarPresets); // reset remaining counts
			}

			return Mod.Instance.Config.ChooseRandomCarData(origCarData, seed, placement, this.CarTypes, this.CarPresets);
		}

		public static ReplayManagerCompoundData Create(ReplayManager replayManager)
		{
			var rmCompoundData = replayManager.gameObject.GetOrAddComponent<ReplayManagerCompoundData>();

			// Prepare lists for random cars and color presets to choose from.
			if (Mod.Instance.Config.RandomCarMethod.IsCarTypes())
			{
				rmCompoundData.CarTypes = rmCompoundData.OriginalCarTypes = Mod.Instance.Config.LoadRandomCarTypes();
				if (Mod.Instance.Config.RandomCarMethod.IsAvoidDuplicates())
				{
					rmCompoundData.CarTypes = RandomCarType.CloneListAndReset(rmCompoundData.OriginalCarTypes); // reset remaining counts
				}
			}
			else
			{
				rmCompoundData.CarTypes = rmCompoundData.OriginalCarTypes = null;
			}

			if (Mod.Instance.Config.RandomColorMethod.IsColorPresets())
			{
				rmCompoundData.CarPresets = rmCompoundData.OriginalCarPresets = Mod.Instance.Config.LoadRandomColorPresets();
				if (Mod.Instance.Config.RandomColorMethod.IsAvoidDuplicates())
				{
					rmCompoundData.CarPresets = RandomColorPreset.CloneListAndReset(rmCompoundData.OriginalCarPresets); // reset remaining counts
				}
			}
			else
			{
				rmCompoundData.CarPresets = rmCompoundData.OriginalCarPresets = null;
			}


			if (Mod.Instance.Config.RandomCarByPlacement || Mod.Instance.Config.RandomColorByPlacement)
			{
				// Gather all replay cars to evaluate their placement.
				List<CarReplayData> placements = new List<CarReplayData>();

				if (replayManager.IsReplayMode_)
				{
					if (replayManager.PlayingPickedReplay_)
					{
						AddPlacements(placements, replayManager.normalFinishReplays_, false);
						AddPlacements(placements, replayManager.pickedReplays_, false);
					}
					else if (replayManager.finishedNormally_)
					{
						AddPlacements(placements, replayManager.normalFinishReplays_, false);
						AddPlacements(placements, replayManager.didNotFinishReplays_, true);
					}
					else
					{
						AddPlacements(placements, replayManager.didNotFinishReplays_, false, true);
					}
				}
				else
				{
					AddPlacements(placements, replayManager.pickedReplays_, true);
					AddPlacements(placements, replayManager.loadedReplays_, true);
				}

				// Sort cars by placement (note this *should* only happen before the `IsMyPlayer` replay is added).
				if (replayManager.gm_.NextGameModeID_.IsPointsBased())
				{
					// Larger FinishValues comes first.
					placements.Sort(ComparePlacementPoints);
				}
				else
				{
					// Smaller FinishValues comes first.
					placements.Sort(ComparePlacementTime);
				}

				// Assign placements for all cars that are not the most-recent run.
				// Start placement index at 1 so that 0 can be treated as 'no placement'.
				for (int i = 0, placement = 1; i < placements.Count; i++)
				{
					var replayData = placements[i];
					var carCompoundData = replayData.GetComponent<CarReplayDataCompoundData>();
					if (carCompoundData && !carCompoundData.IsMyPlayer)
					{
						carCompoundData.Placement = placement++;
					}
				}
			}

			return rmCompoundData;
		}

		#region Helpers

		private static void AddPlacements(List<CarReplayData> placements, GameObject group, bool isGhost, bool spawnLast = false)
		{
			CarReplayData[] componentsInChildren = group.GetComponentsInChildren<CarReplayData>();
			if (!spawnLast)
			{
				foreach (CarReplayData replayData in componentsInChildren)
				{
					AddPlacement(replayData, placements, isGhost);
				}
			}
			else if (componentsInChildren.Length > 0)
			{
				CarReplayData replayData = componentsInChildren.Last<CarReplayData>();
				AddPlacement(replayData, placements, isGhost);
			}
		}

		private static void AddPlacement(CarReplayData replayData, List<CarReplayData> placements, bool isGhost)
		{
			if (ReplayManager.SaveLoadReplays_)
			{
				if (replayData.enabled)
				{
					placements.Add(replayData);
				}
			}
		}

		private static int ComparePlacementPoints(CarReplayData x, CarReplayData y)
		{
			bool x_gtZero = x.FinishValue_ > 0;
			bool y_gtZero = y.FinishValue_ > 0;
			if (x_gtZero != y_gtZero)
			{
				// Non-zero FinishValues before zero FinishValues.
				return y_gtZero.CompareTo(x_gtZero);
			}
			else
			{
				// Higher points before lower points.
				return y.FinishValue_.CompareTo(x.FinishValue_);
			}
		}

		private static int ComparePlacementTime(CarReplayData x, CarReplayData y)
		{
			bool x_gtZero = x.FinishValue_ > 0;
			bool y_gtZero = y.FinishValue_ > 0;
			if (x_gtZero != y_gtZero)
			{
				// Non-zero FinishValues before zero FinishValues.
				return y_gtZero.CompareTo(x_gtZero);
			}
			else
			{
				// Either lowest time first, or closest distance towards endzone first.
				return x.FinishValue_.CompareTo(y.FinishValue_);
			}
		}

		#endregion
	}
}
