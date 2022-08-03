using System;
using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Randomizer
{
	public class RandomCarType : IComparable<RandomCarType>
	{
		public const string BackerCar = "Catalyst";

		public static Dictionary<string, float> VanillaCarChances { get; } = new Dictionary<string, float>
		{
			{ "Spectrum",    1.00f },
			{ "Archive",     0.75f },
			{ "Interceptor", 0.50f },
			{ "Encryptor",   0.08f }, // (Doot car)
			{ "Halcyon",     0.05f }, // (Shiny Spectrum car)
			{ "Catalyst",    0.00f }, // (Kickstarter Backer car, disabled by default)
		};

		public static RandomCarType DefaultCarType
		{
			get
			{
				string defaultCarName = VanillaCarChances.First().Key;
				if (!KnownCars.TryGetValue(defaultCarName, out int defaultCarIndex))
				{
					var firstKnownCar = KnownCars.First(); // If the known car names have somehow changed, default to the first known car.
					defaultCarName = firstKnownCar.Key;
					defaultCarIndex = firstKnownCar.Value;
				}
				return new RandomCarType
				{
					Name           = defaultCarName,
					Index          = defaultCarIndex,
					IsVanilla      = true,
					MaxCount       = -1,
					RemainingCount = -1,
					Weight         = 1.0f,
				};
			}
		}

		public static Dictionary<string, int> KnownCars => G.Sys.ProfileManager_.knownCars_;

		public static IEnumerable<string> VanillaCarNames => VanillaCarChances.Keys;

		public static IEnumerable<string> CustomCarNames
		{
			get
			{
				foreach (string carName in KnownCars.Keys)
				{
					if (!VanillaCarChances.ContainsKey(carName))
					{
						yield return carName;
					}
				}
			}
		}

		public static int VanillaCarsCount => VanillaCarChances.Count;

		public static int CustomCarsCount => KnownCars.Count - VanillaCarChances.Count;

		public static bool IsBackerCar(string carName) => carName == BackerCar;

		public static bool IsVanillaCar(string carName) => VanillaCarChances.ContainsKey(carName);

		public static bool TryCreate(string carName, float weight, int maxCount, out RandomCarType carType)
		{
			if (weight > 0f && KnownCars.TryGetValue(carName, out int carIndex))
			{
				bool isVanilla = VanillaCarChances.ContainsKey(carName);
				carType = new RandomCarType
				{
					Name           = carName,
					Index          = carIndex,
					IsVanilla      = isVanilla,
					MaxCount       = maxCount,
					RemainingCount = maxCount,
					Weight         = weight,
				};
				return true;
			}
			carType = null;
			return false;
		}

		public static List<RandomCarType> LoadAllCarTypes(float defaultWeight = 1f, int defaultMaxCount = 1)
		{
			List<RandomCarType> randomCarTypes = new List<RandomCarType>();

			foreach (var knownCarPair in KnownCars)
			{
				randomCarTypes.Add(new RandomCarType
				{
					Name           = knownCarPair.Key,
					Index          = knownCarPair.Value,
					IsVanilla      = VanillaCarChances.ContainsKey(knownCarPair.Key),
					MaxCount       = defaultMaxCount,
					RemainingCount = defaultMaxCount,
					Weight         = defaultWeight,
				});
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

		public CarColors DefaultColors => G.Sys.ProfileManager_.CarInfos_[this.Index].colors_;


		public int CompareTo(RandomCarType other)
		{
			// Vanilla before custom.
			// Alphabetical order (case sensitive, invariant).
			if (this.IsVanilla == other.IsVanilla)
			{
				return other.IsVanilla.CompareTo(this.IsVanilla);
			}
			else
			{
				return string.Compare(this.Name, other.Name, StringComparison.InvariantCulture);
			}
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
