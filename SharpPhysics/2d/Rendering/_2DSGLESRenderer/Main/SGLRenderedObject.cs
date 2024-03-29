﻿using SharpPhysics._2d._2DSGLESRenderer.Shaders;
using SharpPhysics._2d._2DSGLRenderer.Shaders;
using SharpPhysics._2d.ObjectRepresentation;

namespace SharpPhysics._2d.Renderer._2DSGLESRenderer.Main
{
	public class SGLESRenderedObject
	{
		/// <summary>
		/// The vao that contains all the VRAM data
		/// </summary>
		public uint BoundVao;

		/// <summary>
		/// The vbo containing the buffer data
		/// </summary>
		public uint vbo;

		/// <summary>
		/// The contents of the ebo
		/// </summary>
		public uint[] eboContent;

		/// <summary>
		/// Pointer to the ebo in graphics mem
		/// </summary>
		public uint eboPtr;

		/// <summary>
		/// The fragment shader to use. Don't mess with this unless you know what you're doing.
		/// </summary>
		public string FragShader = ShaderCollector.GetShader("FragTextureSupport");

		/// <summary>
		/// The vertex shader to use. Don't mess with this unless you know what you're doing.
		/// </summary>
		public string VrtxShader = ShaderCollector.GetShader("VertexPositionTexture");

		/// <summary>
		/// The pointer to the object texture
		/// </summary>
		public uint TexturePtr;

		/// <summary>
		/// The shader program
		/// </summary>
		public _2d._2DSGLESRenderer.Shaders.ShaderProgram Program = new();

		/// <summary>
		/// The object's texture
		/// </summary>
		public string objTextureLoc = string.Empty;

		/// <summary>
		/// the object to simulate
		/// </summary>
		public SimulatedObject2d objToSim = new();
	}
}
