﻿using SharpPhysics._2d._2DSGLRenderer.Main;
using SharpPhysics._2d.ObjectRepresentation;
using SharpPhysics.Utilities;
using SharpPhysics.Utilities.MISC;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpPhysics._2d.Rendering
{
	public static class MainRenderer
	{
		internal static bool IsESCompatible = false;

		/// <summary>
		/// Adds rendered objects
		/// </summary>
		/// <param name="sceneID"></param>
		// DEV NOTE: I'll make either rro do all objs at once or change this to single obj at once.
		public unsafe static void ARO(SGLRenderedObject*[] newObjs, short sceneID)
		{
			if (!IsESCompatible)
			{
				MainRendererSGL.SceneIDToRender = sceneID;
				MainRendererSGL.renderer.ADDOBJSTRNDR(newObjs);
			}
			else
			{
				// TODO: THIS
				ErrorHandler.ThrowError("Not Implemented", true);
			}
		}

		/// <summary>
		/// removes rendered objects
		/// </summary>
		/// <param name="objsToRemove"></param>
		/// <param name="sceneID"></param>
		public static unsafe void RRO(int[] objsToRemove, short sceneID)
		{
			if (!IsESCompatible)
			{
				foreach (int obj in objsToRemove)
				{
					MainRendererSGL.renderer.PauseRender = true;
					SimulatedObject2d object2D = _2dWorld.SceneHierarchies[sceneID].Objects[obj];
					try
					{
						SGLRenderedObject objectRndrd = MainRendererSGL.renderer.ObjectsToRender[obj];
						if (objectRndrd.objToSim != &object2D) throw new();
					}
					catch
					{
						ErrorHandler.ThrowError(14, true);
					}
					MainRendererSGL.renderer.ExecuteRemove = obj;
					fixed (int?* executeRemovePtr = &MainRendererSGL.renderer.ExecuteRemove)
						Utils.AwaitUntilValue(executeRemovePtr, null);
					// remove from SP
					// we can remove some of the down time of the renderer by creating the objects to render and setting that in about a ms of delay.
					SGLRenderedObject[] objs = [];
					Array.Copy(MainRendererSGL.renderer.ObjectsToRender, objs, MainRendererSGL.renderer.ObjectsToRender.Length);
					MainRendererSGL.renderer.PauseRender = false;
					ArrayUtils.RemoveArrayObjFast(objs, obj);
					MainRendererSGL.renderer.PauseRender = true;
					// C# *should* set this is a ptr to the array and not copy array.
					// (and also *should* remove old array)
					MainRendererSGL.renderer.ObjectsToRender = objs;
					MainRendererSGL.renderer.PauseRender = false;
				}
			}
		}

		/// <summary>
		/// Starts rendering.
		/// auto-detects required backend.
		/// </summary>
		public static void InitRendering()
		{
			try
			{
				MainRendererSGL.InitRendering();
			}
			catch
			{
				IsESCompatible = true;
				MainRendererSGLES.InitRendering();
			}

			ErrorHandler.ThrowError(12, true);
		}
	}
}