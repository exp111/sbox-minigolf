﻿using System.Linq;
using System.Collections.Generic;
using Sandbox;
using Sandbox.Internal;

namespace Minigolf
{
	[Library( "minigolf_out_of_bounds", Description = "Out of bounds" )]
	[Hammer.Solid]
	[Hammer.AutoApplyMaterial]
	public partial class OutOfBoundsArea : ModelEntity
	{
		/// <summary>
		/// When the ball enters this out of bounds area, how much time until we declare out of bounds?
		/// </summary>
		[Property(Title = "Forgiveness Time")]
		public int ForgiveTime { get; set; } = 3;

		public IEnumerable<GolfBall> TouchingBalls => touchingBalls;
		private readonly List<GolfBall> touchingBalls = new();

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;

			Transmit = TransmitType.Never;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is GolfBall ball )
			{
				AddTouchingBall( ball );

				// TODO: forgiveness time
				(Game.Current as GolfGame).BallOutOfBounds( ball, GolfGame.OutOfBoundsType.Normal );
			}
		}

		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			if ( other is not GolfBall )
				return;

			var ball = other as GolfBall;

			if ( touchingBalls.Contains( ball ) )
				touchingBalls.Remove( ball );
		}

		protected void AddTouchingBall( GolfBall ball )
		{
			if ( !ball.IsValid() )
				return;

			if ( !touchingBalls.Contains( ball ) )
				touchingBalls.Add( ball );
		}
	}
}
