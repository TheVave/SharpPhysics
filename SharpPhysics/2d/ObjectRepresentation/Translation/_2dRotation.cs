﻿
namespace SharpPhysics._2d.ObjectRepresentation.Translation
{
	public class _2dRotation
	{
		public override string ToString()
		{
			return $"Rot:{xRot}";
		}
		public _2dRotation(float x)
		{
			xRot = x;
		}
		public _2dRotation()
		{
			xRot = 0;
		}
		public float xRot = 0;
	}
}
