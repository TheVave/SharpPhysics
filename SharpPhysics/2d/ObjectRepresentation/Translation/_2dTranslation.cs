﻿namespace SharpPhysics._2d.ObjectRepresentation.Translation
{
	public class _2dTranslation
	{
		public _2dTranslation(double xPos, double yPos, double zPos)
		{
			ObjectPosition = new _2dPosition(xPos, yPos, zPos);
			ObjectRotation = new _2dRotation(0);
			ObjectScale = new _2dScale(128, 128);
		}

		public _2dTranslation()
		{
			ObjectPosition = new _2dPosition(0, 0, 0);
			ObjectRotation = new _2dRotation(0);
			ObjectScale = new _2dScale(128, 128);
		}
		public override string ToString()
		{
			return $"{ObjectPosition}, {ObjectRotation}, {ObjectScale}";
		}
		public _2dPosition ObjectPosition;
		public _2dRotation ObjectRotation;
		public _2dScale ObjectScale;
	}
}