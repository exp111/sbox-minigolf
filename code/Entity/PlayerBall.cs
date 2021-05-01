using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minigolf
{
	/// <summary>
	/// Player golf ball
	/// </summary>
	[Library("minigolf_ball")]
	public partial class PlayerBall : ModelEntity
	{
		[Net] public bool IsMoving { get; set; }
		public bool InHole { get; set; }

		static readonly SoundEvent BounceSound = new("sounds/minigolf.ball_bounce1.vsnd");

		// Clientside only
		public ModelEntity Quad { get; set; }
		public Particles Trail { get; set; }
		public Particles PowerArrows { get; set; }


		public override void Spawn()
		{
			base.Spawn();

			SetModel("models/golf_ball.vmdl");
			SetupPhysicsFromModel(PhysicsMotionType.Dynamic, false);

			MoveType = MoveType.Physics;
			CollisionGroup = CollisionGroup.Debris;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;

			Transmit = TransmitType.Always;
		}

        public override void OnNewModel(Model model)
        {
            base.OnNewModel(model);

			if (Host.IsServer)
				return;

			Quad = new ModelEntity();
			Quad.SetModel("models/minigolf.ball_quad.vmdl");

			// Trail = Particles.Create("particles/ball_trail.vpcf");
			// Trail.SetEntity(0, this);
		}

		[Event( "frame" )]
		public void OnFrame()
        {
			if (Quad == null)
				return;

			var player = Player.Local as GolfPlayer;
			if (player == null) return;

			// only do stuff with your own ball
			if (Owner != player)
            {
				Quad.RenderAlpha = 0.0f;
				return;
			}

			// keep the quad under the ball
			Quad.WorldPos = WorldPos - (Vector3.Up * 7.99f);
			Quad.RenderAlpha = IsMoving ? 0.0f : 1.0f;

			var camera = player.BallCamera;
			if (camera == null) return;

			Quad.WorldRot = Rotation.FromYaw(player.BallCamera.Angles.yaw + 180);

			var power = player.ShotPower;
			var powerS = power / 100.0f; // 0-1

			var yawRadians = player.BallCamera.Angles.yaw * (MathF.PI / 180);

			if (power <= 0)
			{
				if (PowerArrows == null)
					return;

				PowerArrows.Destroy(true);
				PowerArrows = null;

				return;
			}

			if (PowerArrows == null)
				PowerArrows = Particles.Create("particles/power_arrow.vpcf");

			var moveDir = Angles.AngleVector(new Angles(0, player.BallCamera.Angles.yaw, 0)) * (0.1f + powerS);

			PowerArrows.SetPos(0, WorldPos - Vector3.Up * 7.5f);
			PowerArrows.SetPos(1, new Vector3(powerS, 0, yawRadians));
			PowerArrows.SetPos(2, moveDir);
		}

		/// <summary>
		/// Do our own physics collisions, we create a fun bouncing effect this way and handle collision sounds.
		/// </summary>
		/// <param name="eventData"></param>
		protected override void OnPhysicsCollision(CollisionEventData eventData)
		{
			// Walls are non world cause it's fucked
			if (eventData.Entity.IsWorld)
				return;

			DebugOverlay.Text(eventData.Pos, $"{eventData.Speed}", 5f);

			// Don't do ridiculous bounces upwards, just bounce off walls mainly
			if (Vector3.Up.Dot(eventData.Normal) >= -0.35)
            {
				var reflect = Vector3.Reflect(eventData.PreVelocity.Normal, eventData.Normal.Normal).Normal;
				var newSpeed = Math.Max(eventData.PreVelocity.Length, eventData.Speed);

				DebugOverlay.Line(eventData.Pos, eventData.Pos - (eventData.PreVelocity.Normal * 64.0f), 5);
				DebugOverlay.Line(eventData.Pos, eventData.Pos + (reflect * 64.0f), 5);

				PhysicsBody.Velocity = reflect * newSpeed * 0.8f;
				PhysicsBody.AngularVelocity = Vector3.Zero;

				// Get a scalar of how hard we hit the wall, 50 is a light tap, 1000 is hard as fuck

				// Go a minimum speed to do effects
				if (eventData.Speed < 50)
					return;

				var particle = Particles.Create("particles/ball_hit.vpcf", eventData.Pos);
				particle.SetPos(0, eventData.Pos);
				particle.SetForward(0, reflect);
				// todo: pass scalar to particle
				particle.Destroy(false);

				// Collision sound happens at this point, not entity
				var sound = Sound.FromWorld(BounceSound.Name, eventData.Pos);
				sound.SetVolume(1.0f); // todo: scale this based on speed (it can go above 1.0)
				sound.SetPitch(0.5f + Math.Clamp(eventData.Speed / 2000, 0.0f, 0.5f));
			}
		}
	}
}
