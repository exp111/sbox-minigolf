﻿using Sandbox;
using System.Linq;

namespace Minigolf
{
    public class StaticCamera : ICamera
    {
		public Vector3 Position;
		public Rotation Rotation;
		public float FieldOfView;

        public StaticCamera( Vector3 position, Angles angles )
        {
            Position = new Vector3(position);
            Rotation = Rotation.From(angles);
            FieldOfView = 90;
        }

		public override void Build( ref CameraSetup camSetup )
		{
			camSetup.Position = Position;
			camSetup.Rotation = Rotation;
			camSetup.FieldOfView = FieldOfView;
		}

		public override void BuildInput( InputBuilder builder )
		{

		}
    }
}
