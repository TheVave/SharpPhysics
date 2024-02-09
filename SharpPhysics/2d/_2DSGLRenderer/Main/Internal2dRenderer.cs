﻿using SharpPhysics._2d._2DSGLRenderer.Shaders;
using SharpPhysics._2d.ObjectRepresentation;
using SharpPhysics.Renderer;
using SharpPhysics.Utilities.MathUtils;
using SharpPhysics.Utilities.MathUtils.DelaunayTriangulator;
using SharpPhysics.Utilities.MISC;
using SharpPhysics.Utilities.MISC.Errors;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;
using System.Numerics;
using System.Runtime.ExceptionServices;

namespace SharpPhysics._2d._2DSGLRenderer.Main
{
	/// <summary>
	/// Handled by the MainRendererSGL class.
	/// Please do not interface directly unless you know what you're doing,
	/// though useful if you want to make custom rendering code.
	/// </summary>
	public class Internal2dRenderer
	{
		/// <summary>
		/// Window ref
		/// </summary>
		public IWindow Wnd;

		/// <summary>
		/// The objects to render
		/// </summary>
		public SGLRenderedObject[] objectToRender = [new(), new()];

		/// <summary>
		/// wnd title
		/// </summary>
		public string title = "SharpPhysics View Port";

		/// <summary>
		/// The size of the window
		/// </summary>
		public Size wndSize;

		/// <summary>
		/// The window options to create Wnd with
		/// </summary>
		public WindowOptions WndOptions = WindowOptions.Default;

		/// <summary>
		/// GL context
		/// </summary>
		public GL gl;

		/// <summary>
		/// The color to clear to
		/// </summary>
		public Color clearBufferBit = new(ColorName.Black);

		/// <summary>
		/// OnRender
		/// </summary>
		public Action<_2dSimulatedObject>[] OR = [];

		/// <summary>
		/// OnUpdate
		/// </summary>
		public Action<_2dSimulatedObject>[] OU = [];

		/// <summary>
		/// OnLoad
		/// </summary>
		public Action OL;

		/// <summary>
		/// Initializes SGL (Silk.net openGL) and the Wnd object
		/// 
		/// </summary>
		public virtual void ISGL() 
		{
			// window init
			SWCNFG();
			INITWND();
			// wnd events
			WES();
			// calling
			CLWND();
		}

		/// <summary>
		/// Sets the window configuration
		/// </summary>
		public virtual void SWCNFG()
		{
			WndOptions.Title = title;
			WndOptions.Size = new(wndSize.Width, wndSize.Height);
		}

		/// <summary>
		/// Sets window events
		/// </summary>
		public virtual void WES()
		{
			Wnd.Update += UDT;
			Wnd.Render += RNDR;
			Wnd.Load += LD;
		}

		/// <summary>
		/// Compiles the shader with the specified name in shaders.resx
		/// </summary>
		/// <param name="name"></param>
		public virtual uint CMPLSHDRN(string name, Silk.NET.OpenGL.ShaderType type, int objID)
		{
			uint ptr = gl.CreateShader(type);
			gl.ShaderSource(ptr, ShaderCollector.GetShader(name));

			gl.CompileShader(ptr);
			gl.GetShader(ptr, ShaderParameterName.CompileStatus, out int status);
			if (status != /* if it has failed */ 1)
			{
				ErrorHandler.ThrowError($"Error, Internal/External error, shader compilation failed with name {name}.", true);
			}
			if (type is Silk.NET.OpenGL.ShaderType.VertexShader)
			{
				objectToRender[objID].Program.Vrtx.ShaderCode = ShaderCollector.GetShader(name);
				objectToRender[objID].Program.Vrtx.ShaderCompilePtr = ptr;
				objectToRender[objID].Program.Vrtx.ShaderType = type;
			}
			else if (type is Silk.NET.OpenGL.ShaderType.FragmentShader)
			{
				objectToRender[objID].Program.Frag.ShaderCode = ShaderCollector.GetShader(name);
				objectToRender[objID].Program.Frag.ShaderCompilePtr = ptr;
				objectToRender[objID].Program.Frag.ShaderType = type;
			}
			return ptr;
		}
		public virtual uint CMPLSHDRC(string code, Silk.NET.OpenGL.ShaderType type, int objID)
		{
			uint ptr = gl.CreateShader(type);
			gl.ShaderSource(ptr, code);

			gl.CompileShader(ptr);
			gl.GetShader(ptr, ShaderParameterName.CompileStatus, out int status);
			if (status != /* if it has failed */ 1)
			{
				ErrorHandler.ThrowError($"Error, Internal/External error, shader compilation failed with code\n {code}.", true);
			}
			if (type is Silk.NET.OpenGL.ShaderType.VertexShader)
			{
				objectToRender[objID].Program.Vrtx.ShaderCode = code;
				objectToRender[objID].Program.Vrtx.ShaderCompilePtr = ptr;
				objectToRender[objID].Program.Vrtx.ShaderType = type;
			}
			else if (type is Silk.NET.OpenGL.ShaderType.FragmentShader)
			{
				objectToRender[objID].Program.Frag.ShaderCode = code;
				objectToRender[objID].Program.Frag.ShaderCompilePtr = ptr;
				objectToRender[objID].Program.Frag.ShaderType = type;
			}
			return ptr;
		}

		/// <summary>
		/// Compiles a shader program with the specified names
		/// </summary>
		/// <param name="name"></param>
		/// <param name="name2"></param>
		/// <returns></returns>
		public virtual uint CMPLPROGN(string name, string name2, int objID)
		{
			uint shdr1 = CMPLSHDRN(name,Silk.NET.OpenGL.ShaderType.VertexShader, objID);
			uint shdr2 = CMPLSHDRN(name2, Silk.NET.OpenGL.ShaderType.FragmentShader, objID);

			uint prog;
			prog = gl.CreateProgram();

			gl.AttachShader(prog, shdr1);
			gl.AttachShader(prog, shdr2);

			gl.LinkProgram(prog);

			gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out int status);
			// 1s good.
			if (status is not 1)
			{
				ErrorHandler.ThrowError($"Internal Error, Compiler link failed with input names {name},{name2}.", true);
			}

			gl.DeleteShader(shdr1);
			gl.DeleteShader(shdr2);

			gl.DetachShader(prog, shdr1);
			gl.DetachShader(prog, shdr2);

			objectToRender[objID].Program.ProgramPtr = prog;

			return prog;
		}

		/// <summary>
		/// Creates a shader program with the specified code.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="code2"></param>
		/// <returns></returns>
		public virtual uint CMPLPROGC(string code, string code2, int objID)
		{
			uint shdr1 = CMPLSHDRC(code, Silk.NET.OpenGL.ShaderType.VertexShader, objID);
			uint shdr2 = CMPLSHDRC(code2, Silk.NET.OpenGL.ShaderType.FragmentShader, objID);

			uint prog;
			prog = gl.CreateProgram();

			gl.AttachShader(prog, shdr1);
			gl.AttachShader(prog, shdr2);

			gl.LinkProgram(prog);

			gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out int status);
			// 1s good.
			if (status is not 1)
			{
				ErrorHandler.ThrowError($"Internal Error, Compiler link failed with input code\n {code}\n\n{code2}.", true);
			}

			gl.DeleteShader(shdr1);
			gl.DeleteShader(shdr2);

			gl.DetachShader(prog, shdr1);
			gl.DetachShader(prog, shdr2);

			objectToRender[objID].Program.ProgramPtr = prog;

			return prog;
		}

		/// <summary>
		/// Initializes the window
		/// </summary>
		public virtual void INITWND()
		{
			Wnd = Silk.NET.Windowing.Window.Create(WndOptions);
		}

		/// <summary>
		/// Starts the window
		/// </summary>
		public virtual void CLWND()
		{
			Wnd.Run();
		}

		/// <summary>
		/// Called every frame to render the object(s)
		/// </summary>
		public unsafe virtual void RNDR(double deltaTime)
		{
			CLR();

			for (int i = 0; i < objectToRender.Length; i++)
			{
				SELOBJ(i);
				DRWOBJ(i);
			}
		}

		/// <summary>
		/// Invokes user draw code
		/// </summary>
		public unsafe virtual void IVKUSRRNDRS()
		{
			for (int obj = 0; obj < objectToRender.Length; obj++)
			{
				OR[obj].Invoke(objectToRender[obj].objToSim);
			}
		}

		/// <summary>
		/// Selects an object to draw
		/// </summary>
		/// <param name="objectID"></param>
		public unsafe virtual void SELOBJ(int objectID)
		{
			// select vao
			gl.BindVertexArray(objectToRender[objectID].BoundVao);
			gl.UseProgram(objectToRender[objectID].Program.ProgramPtr);

			// use texture
			gl.ActiveTexture(TextureUnit.Texture0);
			gl.BindTexture(TextureTarget.Texture2D, objectToRender[objectID].TexturePtr);
		}

		/// <summary>
		/// Draws the object at objectID
		/// </summary>
		/// <param name="objectID"></param>
		public unsafe virtual void DRWOBJ(int objectID)
		{
			STTRNSFRMM4(objectID, "mod");
			gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)objectToRender[objectID].objToSim.ObjectMesh.MeshTriangles.Length * 3);
		}

		/// <summary>
		/// Gets a transform matrix
		/// </summary>
		/// <param name="objectID"></param>
		/// <returns></returns>
		public unsafe virtual Matrix4x4 GTTRNSFRMMTRX(int objectID)
		{
			Vector3 vctr3 = new Vector3((float)objectToRender[objectID].objToSim.Translation.ObjectPosition.xPos, (float)objectToRender[objectID].objToSim.Translation.ObjectPosition.yPos, 0);
			Matrix4x4 model = Matrix4x4.CreateRotationZ((float)GenericMathUtils.DegreesToRadians(objectToRender[objectID].objToSim.Translation.ObjectRotation.xRot)) * Matrix4x4.CreateTranslation(vctr3);
			return model;
		}

		/// <summary>
		/// Sets transform matrix
		/// </summary>
		/// <param name="objectID"></param>
		/// <param name="name"></param>
		public unsafe virtual void STTRNSFRMM4(int objectID, string name)
		{
			int pos = gl.GetUniformLocation(objectToRender[objectID].Program.ProgramPtr, name);
			gl.UniformMatrix4(pos, false, GetMatrix4x4Values(GTTRNSFRMMTRX(objectID)));
		}

		/// <summary>
		/// Initializes some info for objects
		/// </summary>
		public unsafe virtual void INITOBJS()
		{
			int objid = 0;
			foreach (SGLRenderedObject obj in objectToRender)
			{
				objectToRender[objid].objToSim.ObjectMesh.MeshTriangles = DelaunayTriangulator.DelaunayTriangulation(objectToRender[0].objToSim.ObjectMesh.MeshPoints).ToArray();
				objid++;
			}
		}

		/// <summary>
		/// Update. Called before render.
		/// </summary>
		public virtual void UDT(double deltaTime)
		{
			ParallelFor.ParallelForLoop((int obj) =>
			{
				try
				{
					OU[obj].Invoke(objectToRender[obj].objToSim);
				}
				catch
				{

				}
			}, objectToRender.Length);
		}

		/// <summary>
		/// Called before anything else (other than OpenGL.Init), only called once.
		/// </summary>
		public virtual void LD()
		{
			// loads some necessary info for the objects.
			INITOBJS();
			// inits the OpenGL context
			INTGLCNTXT();
			// sets clear color
			STCLRCOLR(ColorName.Blue);
			// sets texture settings
			TXRHINTS();

			for (int i = 0; i < objectToRender.Length; i++)
			{
				// binds vao
				BNDVAO(i);
				// creates vbo
				INITVBO(i);
				// sets vbo data
				STVBO(i);
				// compiles shaders and shader progs
				CMPLPROGC(objectToRender[i].VrtxShader, objectToRender[i].FragShader, i);
				// sets the texture supporting attributes
				STSTDATTRIB();
				// sets vbo and vao
				BFRST((uint)i);
				// binds texture info
				TXINIT(i);
				// sets the texture info
				TXST(i);
				// generates mipmaps
				GNMPMPS();
				// sets the texture info to the shader
				STTXTRUNI(i);
				// loads user-defined info
				OL.Invoke();
			}

			// old code:

			// loads some necessary info for the objects.
			//INITOBJS();
			//// inits the OpenGL context
			//INTGLCNTXT();
			//// binds vao
			//BNDVAO();
			//// creates vbo
			//INITVBO();
			//// sets vbo data
			//STVBO();
			//// compiles shaders and shader progs
			//CMPLPROGC(objectToRender[0].VrtxShader, objectToRender[0].FragShader, 0);
			//// sets clear color
			//STCLRCOLR(ColorName.Blue);
			//// sets the texture supporting attributes
			//STSTDATTRIB();
			//// cleans up some stuff mid-way
			//CLNUP();
			//// binds texture info
			//TXINIT();
			//// sets the texture info
			//TXST();
			//// sets texture settings
			//TXRHINTS();
			//// generates mipmaps
			//GNMPMPS();
			//// sets the texture info to the shader
			//STTXTRUNI();
			//// loads user-defined info
			//OL.Invoke();
		}

		/// <summary>
		/// Initializes some info for the textures
		/// </summary>
		public virtual void TXINIT(int objid)
		{
			objectToRender[objid].TexturePtr = gl.GenTexture();
			gl.ActiveTexture(TextureUnit.Texture0);
			gl.BindTexture(TextureTarget.Texture2D, objectToRender[objid].TexturePtr);
		}

		/// <summary>
		/// Sets the texture info
		/// </summary>
		public unsafe virtual void TXST(int objid)
		{
			ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(@$"{Environment.CurrentDirectory}\Ctnt\Txtrs\Enemy Thing.png"), ColorComponents.RedGreenBlueAlpha);
			fixed (byte* ptr = result.Data)
			{
				gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
					(uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
			}
		}

		/// <summary>
		/// Sets some settings for OpenGL and textures.
		/// </summary>
		public unsafe virtual void TXRHINTS()
		{
			gl.TextureParameter(objectToRender[0].TexturePtr, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			gl.TextureParameter(objectToRender[0].TexturePtr, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			gl.TextureParameter(objectToRender[0].TexturePtr, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			gl.TextureParameter(objectToRender[0].TexturePtr, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		}

		/// <summary>
		/// Generates all the mipmaps for OpenGL
		/// </summary>
		public virtual void GNMPMPS()
		{
			gl.GenerateMipmap(TextureTarget.Texture2D);
		}

		/// <summary>
		/// Sets the texture info in the shader
		/// </summary>
		public virtual void STTXTRUNI(int objid)
		{
			gl.BindTexture(TextureTarget.Texture2D, (uint)objid);

			int location = gl.GetUniformLocation(objectToRender[objid].Program.ProgramPtr, "uTexture");
			gl.Uniform1(location, 0);
		}

		/// <summary>
		/// Enables blending
		/// </summary>
		public virtual void ENABLBLEND()
		{
			gl.Enable(EnableCap.Blend);
			gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		/// <summary>
		/// Selects buffers
		/// </summary>
		public virtual void BFRST(uint objid)
		{
			gl.BindVertexArray(objid);
			gl.BindBuffer(BufferTargetARB.ArrayBuffer, objid);
			gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, objid);
		}

		/// <summary>
		/// Gets a float[] containing the points from a mesh object
		/// </summary>
		public virtual float[] GVFPS(Mesh msh) =>
			Triangle.ToFloats3D(objectToRender[0].objToSim.ObjectMesh.MeshTriangles);

		/// <summary>
		/// Connects the mesh and texture cords
		/// </summary>
		/// <param name="points"></param>
		/// <param name="msh"></param>
		/// <returns></returns>
		public virtual float[] MSHTXCRDS(float[] points, Mesh msh) =>
			RenderingUtils.MashMeshTextureFloats(points, RenderingUtils.GetTXCords(msh));

		/// <summary>
		/// Binds a VAO object
		/// </summary>
		public virtual void BNDVAO(int objid)
		{
			objectToRender[objid].BoundVao = gl.GenVertexArray();
			gl.BindVertexArray(objectToRender[objid].BoundVao);
		}

		/// <summary>
		/// Sets standard attribs for support with textures
		/// </summary>
		public unsafe virtual void STSTDATTRIB()
		{
			const uint stride = (3 * sizeof(float)) + (2 * sizeof(float));

			const uint positionLoc = 0;
			gl.EnableVertexAttribArray(positionLoc);
			gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

			const uint textureLoc = 1;
			gl.EnableVertexAttribArray(textureLoc);
			gl.VertexAttribPointer(textureLoc, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
		}

		/// <summary>
		/// Gets the shader with the specified name
		/// </summary>
		/// <param name="nm"></param>
		public virtual void GTSHDR(string nm) =>
			ShaderCollector.GetShader(nm);

		/// <summary>
		/// Sets the inside of the vbo to objectToRender[0].triangles
		/// </summary>
		public virtual unsafe void STVBO(int objid)
		{
			float[] data = MSHTXCRDS(GVFPS(objectToRender[objid].objToSim.ObjectMesh), objectToRender[objid].objToSim.ObjectMesh);
			fixed (float* buf = data)
				gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(float) * data.Length), buf, BufferUsageARB.StaticDraw);
		}

		/// <summary>
		/// Creates a vbo
		/// </summary>
		public virtual void INITVBO(int objid)
		{
			objectToRender[objid].vbo = gl.GenBuffer();
			gl.BindBuffer(BufferTargetARB.ArrayBuffer, objectToRender[objid].vbo);
		}

		/// <summary>
		/// Initializes the OpenGL context
		/// </summary>
		public virtual void INTGLCNTXT()
		{
			gl = Wnd.CreateOpenGL();
		}
		/// <summary>
		/// sets the clear color.
		/// </summary>
		/// <param name="name">
		/// The color to set to.
		/// </param>
		public virtual void STCLRCOLR(ColorName name)
		{
			Color clr = new Color(name);
			gl.ClearColor(clr.R, clr.G, clr.B, clr.A);
			clearBufferBit = clr;
		}

		/// <summary>
		/// Uses the rgba syntax to set the buffer bit
		/// </summary>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <param name="a"></param>
		public virtual void STCLRCOLR(byte r, byte g, byte b, byte a)
		{
			gl.ClearColor(r, g, b, a);
			clearBufferBit = new Color(r, g, b, a);
		}

		/// <summary>
		/// Clears the screen to clearBufferBit
		/// </summary>
		public virtual void CLR()
		{
			gl.Clear(ClearBufferMask.ColorBufferBit);
		}

		/// <summary>
		/// Gets float[] from matrix4x4
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		private float[] GetMatrix4x4Values(Matrix4x4 m) =>
			[
				m.M11, m.M12, m.M13, m.M14,
				m.M21, m.M22, m.M23, m.M24,
				m.M31, m.M32, m.M33, m.M34,
				m.M41, m.M42, m.M43, m.M44
			];
	}
}
