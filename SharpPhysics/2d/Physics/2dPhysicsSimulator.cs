﻿
namespace SharpPhysics
{
	/// <summary>
	/// used the same PhyscisParams class as the PhyscisSimulator for compatibility
	/// </summary>
	public class _2dPhysicsSimulator
	{

		/// <summary>
		/// The object to simulate
		/// </summary>
		public _2dSimulatedObject ObjectToSimulate;

		/// <summary>
		/// bouncy multiplier
		/// </summary>
		public int ShocksMultiplier = 0;

		/// <summary>
		/// Stops calculation
		/// </summary>
		public bool StopSignal = false;

		internal bool alwaysOverride = true;

		public bool TickSignal = false;

		/// <summary>
		/// WARNING: this has a maximum of 1000
		/// </summary>
		public int TickSpeed = 500;
		public _2dMovmentRepresenter CurrentMovement { get; private set; } = new(new _2dPosition(0, 0));
		public int SpeedMultiplier = 1;
		public virtual void ExecuteAtCollision(SimulatedObject hitObject, SimulatedObject self) { }
		public int DelayAmount;
		public double TimePerSimulationTick = 0.001;

		private readonly bool DoManualTicking = false;

		readonly SUVATEquations sUVATEquations = new SUVATEquations();

		double displacement;

		//calculating the position based on the moving position
		private double[] speedDirection = new double[2];

		public _2dPhysicsSimulator(_2dSimulatedObject objectToSimulate) => ObjectToSimulate = objectToSimulate;


		public _2dPhysicsSimulator(_2dSimulatedObject objectToSimulate, int shocksMultiplier)
		{
			ShocksMultiplier = shocksMultiplier;
			ObjectToSimulate = objectToSimulate;
		}

		public _2dPhysicsSimulator(_2dSimulatedObject objectToSimulate, int shocksMultiplier, bool doManualTicking)
		{
			DoManualTicking = doManualTicking;
			ObjectToSimulate = objectToSimulate;
			ShocksMultiplier = shocksMultiplier;
		}

		internal void Tick()
		{
			//TODO find if the object collides with another object
			_2dCollisionManager.CheckIfCollidedWithObject(ObjectToSimulate.ObjectPhysicsParams.CollidableObjects, ObjectToSimulate);

			//Starting math for moving 1d

			sUVATEquations.T = TimePerSimulationTick;
			sUVATEquations.VS = ObjectToSimulate.ObjectPhysicsParams.Speed;
			sUVATEquations.A = ObjectToSimulate.ObjectPhysicsParams.SpeedAcceleration;

			displacement = sUVATEquations.NSWVSTA();

			//calculating the position based on the moving position
			speedDirection = ObjectToSimulate.ObjectPhysicsParams.Acceleration;

			// do standard calculations to find the displacement in a given direction
			ObjectToSimulate.Translation.ObjectPosition.xPos += ((speedDirection[0]) * displacement) + ObjectToSimulate.ObjectPhysicsParams.Momentum[0];
			ObjectToSimulate.Translation.ObjectPosition.yPos += (((speedDirection[1]) * displacement) + ObjectToSimulate.ObjectPhysicsParams.Momentum[1]) - ObjectToSimulate.ObjectPhysicsParams.GravityMultiplier * 9.8 * ObjectToSimulate.ObjectPhysicsParams.Mass;

			// add momentum
			ObjectToSimulate.ObjectPhysicsParams.Momentum[0] += (((speedDirection[0]) * displacement / sUVATEquations.T * ObjectToSimulate.ObjectPhysicsParams.Mass))/* - ObjectToSimulate.ObjectPhysicsParams.SpeedResistance */;
			ObjectToSimulate.ObjectPhysicsParams.Momentum[1] += ((speedDirection[1]) * displacement / sUVATEquations.T * ObjectToSimulate.ObjectPhysicsParams.Mass) - (ObjectToSimulate.ObjectPhysicsParams.GravityMultiplier * 9.8 * ObjectToSimulate.ObjectPhysicsParams.Mass);

			if (ObjectToSimulate.ObjectPhysicsParams.Momentum[0] > 0)
				if (ObjectToSimulate.ObjectPhysicsParams.Momentum[0] > ObjectToSimulate.ObjectPhysicsParams.SpeedResistance)
					ObjectToSimulate.ObjectPhysicsParams.Momentum[0] -= ObjectToSimulate.ObjectPhysicsParams.SpeedResistance;
				else
					ObjectToSimulate.ObjectPhysicsParams.Momentum[0] = 0;
			else if (ObjectToSimulate.ObjectPhysicsParams.Momentum[0] < 0)

				if (ObjectToSimulate.ObjectPhysicsParams.Momentum[0] < ObjectToSimulate.ObjectPhysicsParams.SpeedResistance)
					ObjectToSimulate.ObjectPhysicsParams.Momentum[0] += ObjectToSimulate.ObjectPhysicsParams.SpeedResistance;

				else
					ObjectToSimulate.ObjectPhysicsParams.Momentum[0] = 0;

			if (ObjectToSimulate.ObjectPhysicsParams.Momentum[1] > 0)
				if (ObjectToSimulate.ObjectPhysicsParams.Momentum[1] > ObjectToSimulate.ObjectPhysicsParams.SpeedResistance)
					ObjectToSimulate.ObjectPhysicsParams.Momentum[1] -= ObjectToSimulate.ObjectPhysicsParams.SpeedResistance;
				else
					ObjectToSimulate.ObjectPhysicsParams.Momentum[1] = 0;

			else if (ObjectToSimulate.ObjectPhysicsParams.Momentum[1] < 0)
				if (ObjectToSimulate.ObjectPhysicsParams.Momentum[1] < ObjectToSimulate.ObjectPhysicsParams.SpeedResistance)
					ObjectToSimulate.ObjectPhysicsParams.Momentum[1] += ObjectToSimulate.ObjectPhysicsParams.SpeedResistance;
				else
					ObjectToSimulate.ObjectPhysicsParams.Momentum[1] = 0;

			// set mesh points for collision
			for (int i = 0; i < ObjectToSimulate.ObjectMesh.MeshPointsX.Length; i++)
			{
				ObjectToSimulate.ObjectMesh.MeshPointsX[i] = ObjectToSimulate.ObjectMesh.MeshPointsActualX[i] + ObjectToSimulate.Translation.ObjectPosition.xPos;
				ObjectToSimulate.ObjectMesh.MeshPointsY[i] = ObjectToSimulate.ObjectMesh.MeshPointsActualY[i] + ObjectToSimulate.Translation.ObjectPosition.yPos;
			}


			// update CurrentMovment value
			CurrentMovement.StartPosition = CurrentMovement.EndPosition;
			CurrentMovement.EndPosition = ObjectToSimulate.Translation.ObjectPosition;
		}

		internal void StartPhysicsSimulator()
		{
			Thread thread = new Thread(() =>
			{
				TimePerSimulationTick = ObjectToSimulate.ObjectPhysicsParams.TimeMultiplier / ObjectToSimulate.ObjectPhysicsParams.TicksPerSecond;
				DelayAmount = (int)Math.Ceiling(1000d / TickSpeed);
				while (true)
				{
					if (!GlobalDeclerations.IsSolvingPhysics) break;
					if (StopSignal) break;
					if (!DoManualTicking)
					{
						Tick();
					}
					else
					{
						if (TickSignal)
						{
							Tick();
							TickSignal = false;
						}
					}

					Task.Delay(DelayAmount).Wait();
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}
	}
}