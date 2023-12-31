﻿
using SharpPhysics._2d.ObjectRepresentation.Translation;
using SharpPhysics._2d.Objects;
using SharpPhysics._2d.Physics;
using SharpPhysics._2d.Raycasting;
using SharpPhysics.Simulation.ObjectHierarchy;
using SharpPhysics.Utilities.MISC.Errors;

namespace SharpPhysics._2d.ObjectRepresentation
{
	public class _2dSimulatedObject
	{
		/// <summary>
		/// The object's mesh
		/// </summary>
		public Mesh ObjectMesh { get; set; }

		/// <summary>
		/// The parameters that tell the physics engine how to behave.
		/// </summary>
		public _2dPhysicsParams ObjectPhysicsParams;

		/// <summary>
		/// The translation of the object, with the position, rotation, and scale of the object.
		/// </summary>
		public _2dTranslation Translation;

		/// <summary>
		/// Creates a new _2dSimulatedObject and registers it to the simulation hierarchy in scene 1.
		/// </summary>
		/// <param name="objectMesh"></param>
		/// <param name="objectPhysicsParams"></param>
		/// <param name="translation"></param>
		public _2dSimulatedObject(Mesh objectMesh, _2dPhysicsParams objectPhysicsParams, _2dTranslation translation)
		{
			ObjectMesh = objectMesh;
			ObjectPhysicsParams = objectPhysicsParams;
			Translation = translation;
			try
			{
				SimulationHierarchy.Hierarchies[0].Objects = [.. SimulationHierarchy.Hierarchies[0].Objects, this];
			}
			catch (Exception e)
			{
				ErrorHandler.ThrowError("Error, Unknown error, _2dSimulatedObject.cs _2dSimulatedObject(Mesh,_2dPhysicsParams,_2dTranslation), exact error: " + e, true);
			}
		}

		/// <summary>
		/// Creates a new _2dSimulatedObject and registers it to the simulation hierarchy in scene 1.
		/// </summary>
		/// <param name="objectMesh"></param>
		/// <param name="objectPhysicsParams"></param>
		/// <param name="translation"></param>
		public _2dSimulatedObject()
		{
			ObjectMesh = _2dBaseObjects.LoadSquareMesh();
			ObjectPhysicsParams = new _2dPhysicsParams();
			Translation = new _2dTranslation();
			//SimulationHierarchy.Hierarchies[0].Objects = SimulationHierarchy.Hierarchies[0].Objects.Append(this).ToArray();
		}

		/// <summary>
		/// Registers the object to a scene.
		/// </summary>
		/// <param name="scene"></param>
		public void RegisterToScene(int scene)
		{
			SimulationHierarchy.Hierarchies[scene].Objects = [.. SimulationHierarchy.Hierarchies[0].Objects, this];
		}

		/// <summary>
		/// Registers the object to scene 1.
		/// </summary>
		public void RegisterToScene()
		{
			SimulationHierarchy.Hierarchies[0].Objects = [.. SimulationHierarchy.Hierarchies[0].Objects, this];
		}

		public void ApplyVectorMomentum(_2dVector force)
		{
			_2dLine forceLine = force.ToLine();
			if (ObjectPhysicsParams.Momentum[0] < forceLine.XEnd) ObjectPhysicsParams.Momentum[0] = 0;
			else ObjectPhysicsParams.Momentum[0] = forceLine.XEnd;
			if (ObjectPhysicsParams.Momentum[1] < forceLine.YEnd) ObjectPhysicsParams.Momentum[1] = 0;
			else ObjectPhysicsParams.Momentum[1] = forceLine.YEnd;
		}

		/// <summary>
		/// Start the physics simulation for the object based on ObjectPhysicsParams
		/// </summary>
		/// <returns></returns>
		public _2dPhysicsSimulator StartPhysicsSimulation()
		{
			_2dPhysicsSimulator physicsSimulator = new(this);
			try
			{
				physicsSimulator.StartPhysicsSimulator();
			}
			catch (Exception e) 
			{
				ErrorHandler.ThrowError("Error, Unknown error, _2dSimulatedObject.cs, StartPhysicsSimulation() exact error: " + e, true);
			}
			return physicsSimulator;
		}
	}
}
