﻿
namespace SharpPhysics._2d.ObjectRepresentation.Translation
{
	public class _2dScale
	{
		public override string ToString()
		{
			return $"Scale:{xSca},{ySca},{zSca}";
		}
		public _2dScale(short x, short y)
		{
			xSca = x;
			ySca = y;
		}
		public _2dScale()
		{
			xSca = 0;
			ySca = 0;
		}
		/// <summary>
		/// Scale in pixels
		/// </summary>
		public short xSca, ySca, zSca = 0;
	}
}
