using Sandbox;

namespace Minigolf
{
	public partial class PowerArrow : RenderEntity
	{
		public Material Material = Material.Load( "materials/minigolf.arrow.vmat" );

		public Vector3 Direction = Vector3.Zero;
		public float Power = 0.0f;

		protected void DrawArrow( SceneObject obj, Vector3 startPos, Vector3 endPos, Vector3 direction, Vector3 size, Color color, bool drawTip )
		{
			var vertexBuffer = Render.GetDynamicVB( true );
			
			// We need the relative position not the absolute
			var offset = endPos - startPos;
			
			// Line
			Vertex a = new( -size, Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
			Vertex b = new( size, Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
			Vertex c = new( offset + size, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
			Vertex d = new( offset - size, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

			vertexBuffer.Add( a );
			vertexBuffer.Add( b );
			vertexBuffer.Add( c );
			vertexBuffer.Add( d );

			vertexBuffer.AddTriangleIndex( 4, 3, 2 );
			vertexBuffer.AddTriangleIndex( 2, 1, 4 );

			if (drawTip)
			{
				// Add the arrow tip
				Vertex e = new( offset + size * 2, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
				Vertex f = new( offset - size * 2, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );
				Vertex g = new( offset + direction * 16 * Power, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );

				vertexBuffer.Add( e );
				vertexBuffer.Add( f );
				vertexBuffer.Add( g );
				vertexBuffer.AddTriangleIndex( 1, 2, 3 );
			}

			Render.Set( "color", color );

			vertexBuffer.Draw( Material );
		}

		public override void DoRender( SceneObject obj )
		{
			if ( Power == 0.0f )
				return;

			Render.SetLighting( obj );

			var startPos = Position;
			var endPos = Position += Direction * Power * 100;

			var offset = Vector3.Cross( Direction, Vector3.Up ) * (1 + 2 * Power);

			// Trace to check if we go against a wall
			var trace = Trace.Ray( startPos, endPos );
			var result = trace.Run();

			var remainingLength = (result.EndPos - endPos).Length;

			// Draw single arrow if no trace/no reflection
			if ( remainingLength.AlmostEqual(0.0f) )
			{
				var color = ColorConvert.HSLToRGB( 120 - (int)(Power * Power * 120), 1.0f, 0.5f );
				DrawArrow( obj, startPos, endPos, Direction, offset, color, true );
				return;
			}

			// Draw two arrows
			var color2 = ColorConvert.HSLToRGB( 120 - (int)(Power * Power * 120), 1.0f, 0.5f );
			DrawArrow( obj, startPos, result.EndPos, Direction, offset, color2, false );

			var direction2 = Vector3.Reflect( Direction, result.Normal );

			var endPos2 = result.EndPos + direction2 * remainingLength;
			var offset2 = Vector3.Cross( direction2, Vector3.Up ) * (1 + 2 * Power);
			DrawArrow( obj, result.EndPos, endPos2, direction2, offset2, color2, true );

			DebugOverlay.Line( result.EndPos, endPos2 );
		}
	}
}
