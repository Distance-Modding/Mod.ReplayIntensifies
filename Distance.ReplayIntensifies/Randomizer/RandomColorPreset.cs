using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Distance.ReplayIntensifies.Randomizer
{
	public class RandomColorPreset : IComparable<RandomColorPreset>
	{
		public static List<RandomColorPreset> LoadAllColorPresets(float defaultWeight = 1f, int defaultMaxCount = 1)
		{
			List<RandomColorPreset> randomColorPresets = new List<RandomColorPreset>();

			// Load resources color presets.
			UnityEngine.Object[] array = Resource.LoadAllInFolder<UnityEngine.Object>(Resource.colorPresetsFolder_);
			foreach (UnityEngine.Object obj in array)
			{
				string fileNameWithoutExtension = Resource.GetFileNameWithoutExtension(obj.name);
				string colorPresetPath = Resource.resourcesColorPresetsDirPath_ + fileNameWithoutExtension + ColorPreset.extension_;

				ColorPreset colorPreset = ColorPreset.Load(colorPresetPath);
				if (colorPreset)
				{
					var randomColorPreset = new RandomColorPreset
					{
						Colors         = colorPreset.CarColors_,
						Name           = colorPreset.Name_,
						IsVanilla      = true,
						MaxCount       = defaultMaxCount,
						RemainingCount = defaultMaxCount,
						Weight         = defaultWeight,
					};
					if (randomColorPreset.IsDefault)
					{
						// Default (car-specific colors) always goes first, we'll track index zero as Default from here on out.
						randomColorPresets.Insert(0, randomColorPreset);
					}
					else
					{
						randomColorPresets.Add(randomColorPreset);
					}

					colorPreset.Destroy();
					UnityEngine.Object.DestroyImmediate(colorPreset);
				}
			}

			// Load personal color presets.
			if (DirectoryEx.Exists(Resource.PersonalColorPresetsDirPath_))
			{
				FileInfo[] directoryFiles = DirectoryEx.GetDirectoryFiles(Resource.PersonalColorPresetsDirPath_, "*.xml");
				foreach (FileInfo fileInfo in directoryFiles)
				{
					if (!fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
					{
						string colorPresetPath = Resource.NormalizePath(fileInfo.FullName);

						ColorPreset colorPreset = ColorPreset.Load(colorPresetPath);
						if (colorPreset)
						{
							randomColorPresets.Add(new RandomColorPreset
							{
								Colors         = colorPreset.CarColors_,
								Name           = colorPreset.Name_,
								IsVanilla      = false,
								MaxCount       = defaultMaxCount,
								RemainingCount = defaultMaxCount,
								Weight         = defaultWeight,
							});

							colorPreset.Destroy();
							UnityEngine.Object.DestroyImmediate(colorPreset);
						}
					}
				}
			}

			randomColorPresets.Sort(); // Sort using RandomCarColors IComparable interface.

			if (randomColorPresets.Count == 0)
			{
				randomColorPresets.Add(new RandomColorPreset
				{
					Colors         = new CarColors(Color.white, Color.white, Color.white, Color.white),
					Name           = "Default",
					IsVanilla      = true,
					MaxCount       = defaultMaxCount,
					RemainingCount = defaultMaxCount,
					Weight         = defaultWeight,
				});
			}

			return randomColorPresets;
		}

		public CarColors Colors { get; set; }
		// If this is from a color scheme file.
		public string Name { get; set; }
		// Index of the vanilla color preset.
		//public int Index { get; set; }
		// If this color scheme is from a resources file.
		public bool IsVanilla { get; set; }
		// Default color scheme unique to each car.
		public bool IsDefault => this.IsVanilla && this.Name == "Default";

		// Max number of cars that can use this before its removed from the list of choices.
		public int MaxCount { get; set; }

		public int RemainingCount { get; set; }

		public float Weight { get; set; }

		// Is this a color scheme preset, or custom-input car colors.
		public bool IsPreset => this.Name != null;


		public int CompareTo(RandomColorPreset other)
		{
			if (this.IsPreset != other.IsPreset)
			{
				// Presets before custom-input colors.
				return other.IsPreset.CompareTo(this.IsPreset);
			}
			else if (this.IsPreset)
			{
				if (this.IsDefault != other.IsDefault)
				{
					// Default preset before all other presets.
					return other.IsDefault.CompareTo(this.IsDefault);
				}
				else if (this.IsVanilla != other.IsVanilla)
				{
					// Vanilla presets before custom presets.
					return other.IsVanilla.CompareTo(this.IsVanilla);
				}
				/*else if (this.IsVanilla)
				{
					return this.Index.CompareTo(other.Index);
				}*/
				else
				{
					// Alphabetical order for custom presets (case sensitive, invariant).
					return string.Compare(this.Name, other.Name, StringComparison.InvariantCulture);
				}
			}
			else
			{
				// Compare each part of the car colors in order.
				for (ColorChanger.ColorType i = 0; i < ColorChanger.ColorType.Size_; i++)
				{
					int result = CompareColor(this.Colors[i], other.Colors[i]);
					if (result != 0)
					{
						return result;
					}
				}
				return 0;
			}
		}

		private static int CompareColor(Color a, Color b)
		{
			// Order comparison by channels: r, g, b, a.
			for (int i = 0; i < 4; i++)
			{
				// Higher channel values before lower values.
				int result = b[i].CompareTo(a[i]);
				if (result != 0)
				{
					return result;
				}
			}
			return 0;
		}

		public void ResetRemainingCount()
		{
			this.RemainingCount = this.MaxCount;
		}

		public static List<RandomColorPreset> CloneListAndReset(List<RandomColorPreset> colorPresets)
		{
			foreach (var colorPreset in colorPresets)
			{
				colorPreset.ResetRemainingCount();
			}
			return new List<RandomColorPreset>(colorPresets);
		}
	}
}
