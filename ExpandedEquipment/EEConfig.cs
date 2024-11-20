using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.ExpandedEquipment
{
	public class EEConfig
	{		
		public enum ConfigEntries {
			[ConfigEntry("Item Magnet Power Cost", typeof(float), 0.1F, 0, 10, 0)]MAGNET_COST,
			[ConfigEntry("Turbo Item Magnet Power Cost", typeof(float), 0.4F, 0, 100, 0)]MAGNET_V2_COST,
			[ConfigEntry("Night Vision Power Cost", typeof(float), 0.4F, 0, 10, 0)]NV_COST,
			[ConfigEntry("Night Vision Lighting Strength", typeof(float), 0.3F, 0, 1, 0)]NV_STRENGTH,
			[ConfigEntry("Spring Boots Damage Reduction Power Cost (Minimum)", typeof(float), 16, 0, 512, 0)]FALL_BOOT_COST_MIN,
			[ConfigEntry("Spring Boots Damage Reduction Power Cost (Maximum)", typeof(float), 64, 0, 512, 0)]FALL_BOOT_COST_MAX,
			[ConfigEntry("Sand Blast Power Cost", typeof(float), 10F, 0, 1024, 0)]SAND_BLAST_COST,
			[ConfigEntry("Sand Blast Radius", typeof(int), 2, 1, 8, 0)]SAND_BLAST_RADIUS,
		}
	}
}
