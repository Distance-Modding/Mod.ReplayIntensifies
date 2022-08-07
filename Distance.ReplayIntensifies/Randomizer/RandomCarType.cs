using System;
using System.Collections.Generic;

namespace Distance.ReplayIntensifies.Randomizer
{
	public class RandomCarType : IComparable<RandomCarType>
	{
		private static Dictionary<string, int> KnownCars => G.Sys.ProfileManager_.knownCars_;
		private static CarInfo[] CarInfos => G.Sys.ProfileManager_.CarInfos_;

		private static bool? allowBackerCar;
		public static bool AllowBackerCar
		{
			get
			{
				if (!allowBackerCar.HasValue)
				{
					if (Mod.Debug_TestBackerCars)
					{
						allowBackerCar = true;
					}
					else
					{
						// Lazy loading so we don't constantly send requests to steam about the DLC
						//  and so Steam is initialized when this is first called.
						allowBackerCar = SteamworksManager.IsSteamBuild_ && G.Sys.SteamworksManager_.OwnsCatalystDLC();
					}
				}
				return allowBackerCar.Value;
			}
		}

		private static List<string> backerCarNames;
		public static List<string> BackerCarNames
		{
			get
			{
				if (backerCarNames == null)
				{
					// Lazy loading so that we can wait for the ProfileManager to initialize.
					backerCarNames = new List<string>();
					foreach (var unlockableCar in ProfileManager.unlockableCars_)
					{
						if (unlockableCar.backerVehicle_)
						{
							string carName = CarInfos[unlockableCar.index_].name_;
							if (KnownCars.ContainsKey(carName))
							{
								backerCarNames.Add(carName);
							}
						}
					}
				}
				return backerCarNames;
			}
		}

		private static List<string> vanillaCarNames;
		public static List<string> VanillaCarNames
		{
			get
			{
				if (vanillaCarNames == null)
				{
					// Lazy loading so that we can wait for the ProfileManager to initialize.
					vanillaCarNames = new List<string>();
					foreach (var unlockableCar in ProfileManager.unlockableCars_)
					{
						string carName = CarInfos[unlockableCar.index_].name_;
						if (KnownCars.ContainsKey(carName))
						{
							vanillaCarNames.Add(carName);
						}
					}
				}
				return vanillaCarNames;
			}
		}

		public static IEnumerable<string> AllowedVanillaCarNames
		{
			get
			{
				foreach (string carName in VanillaCarNames)
				{
					if (IsAllowedCar(carName))
					{
						yield return carName;
					}
				}
			}
		}

		public static IEnumerable<string> CustomCarNames
		{
			get
			{
				foreach (string carName in KnownCars.Keys)
				{
					if (!IsVanillaCar(carName))
					{
						yield return carName;
					}
				}
			}
		}

		public static RandomCarType DefaultCarType
		{
			get
			{
				string defaultCarName = CarInfos[ProfileManager.defaultCarIndex_].name_;
				return new RandomCarType
				{
					Name           = defaultCarName,
					Index          = ProfileManager.defaultCarIndex_,
					IsVanilla      = IsVanillaCar(defaultCarName),
					MaxCount       = -1,
					RemainingCount = -1,
					Weight         = 1.0f,
				};
			}
		}

		public static int VanillaCarsCount => VanillaCarNames.Count;

		public static int CustomCarsCount => KnownCars.Count - VanillaCarsCount;

		public static bool IsBackerCar(string carName) => BackerCarNames.Contains(carName);

		public static bool IsVanillaCar(string carName) => VanillaCarNames.Contains(carName);

		// Use IsUnlocked for if the user overrides unlock behavior (helpful for debugging or Steam issues).
		public static bool IsAllowedCar(string carName) => AllowBackerCar || !IsBackerCar(carName) || IsUnlocked(carName);

		public static bool IsUnlocked(string carName) => G.Sys.ProfileManager_.IsUnlocked(carName);

		public static bool TryCreate(string carName, float weight, int maxCount, bool requireUnlock, out RandomCarType carType)
		{
			if (weight > 0f && KnownCars.TryGetValue(carName, out int carIndex))
			{
				if (IsAllowedCar(carName) && (!requireUnlock || IsUnlocked(carName)))
				{
					carType = new RandomCarType
					{
						Name           = carName,
						Index          = carIndex,
						IsVanilla      = IsVanillaCar(carName),
						MaxCount       = maxCount,
						RemainingCount = maxCount,
						Weight         = weight,
					};
					return true;
				}
			}
			carType = null;
			return false;
		}

		public static List<RandomCarType> LoadAllCarTypes(bool requireUnlock, float defaultWeight = 1f, int defaultMaxCount = 1)
		{
			List<RandomCarType> randomCarTypes = new List<RandomCarType>();

			foreach (var knownCarPair in KnownCars)
			{
				if (IsAllowedCar(knownCarPair.Key) && (!requireUnlock || IsUnlocked(knownCarPair.Key)))
				{
					randomCarTypes.Add(new RandomCarType
					{
						Name           = knownCarPair.Key,
						Index          = knownCarPair.Value,
						IsVanilla      = IsVanillaCar(knownCarPair.Key),
						MaxCount       = defaultMaxCount,
						RemainingCount = defaultMaxCount,
						Weight         = defaultWeight,
					});
				}
			}

			randomCarTypes.Sort(); // Sort using RandomCarType IComparable interface.

			return randomCarTypes;
		}


		public string Name { get; set; }
		// Only needed for looking up default colors.
		public int Index { get; set; }

		public bool IsVanilla { get; set; }

		public bool IsBacker => IsBackerCar(this.Name);

		// Max number of cars that can use this before its removed from the list of choices.
		public int MaxCount { get; set; }

		public int RemainingCount { get; set; }

		public float Weight { get; set; }

		public CarColors DefaultColors => CarInfos[this.Index].colors_;


		public int CompareTo(RandomCarType other)
		{
			// Car index order (don't do alphabetical order so that we preserve the same order as seen during car selection).
			return this.Index.CompareTo(other.Index);

			/*// Vanilla before custom.
			// Alphabetical order for custom cars (case insensitive, invariant, fallback to case sensitive).
			if (this.IsVanilla != other.IsVanilla)
			{
				return other.IsVanilla.CompareTo(this.IsVanilla);
			}
			else
			{
				return this.Index.CompareTo(other.Index);
				//int cmp = string.Compare(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
				//if (cmp == 0)
				//{
				//	cmp = string.Compare(this.Name, other.Name, StringComparison.InvariantCulture);
				//}
				//return cmp;
			}*/
		}

		public void ResetRemainingCount()
		{
			this.RemainingCount = this.MaxCount;
		}

		public static List<RandomCarType> CloneListAndReset(List<RandomCarType> carTypes)
		{
			foreach (var carType in carTypes)
			{
				carType.ResetRemainingCount();
			}
			return new List<RandomCarType>(carTypes);
		}
	}
}
