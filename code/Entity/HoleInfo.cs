﻿using Sandbox;

namespace Minigolf
{
	[Library("minigolf_hole_info")]
	public partial class HoleInfoEntity : ModelEntity
	{
		[HammerProp("hole_number")]
		public int Number { get; set; }

		[HammerProp("hole_name")]
		public string Name { get; set; }

		[HammerProp("hole_par")]
		public int Par { get; set; }

		public override void Spawn()
		{
			base.Spawn();
		}
	}
}
