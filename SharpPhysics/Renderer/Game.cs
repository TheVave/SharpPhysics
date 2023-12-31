﻿using GLFW;
namespace SharpPhysics.Renderer
{
	abstract public class Game
	{

		protected int InitialWindowWidth = 800;
		protected int InitialWindowHeight = 600;
		protected string WindowTitle = "SharpPhysics View Port";

		protected Game(int initialWindowWidth, int initialWindowHeight, string windowTitle)
		{
			InitialWindowWidth = initialWindowWidth;
			InitialWindowHeight = initialWindowHeight;
			WindowTitle = windowTitle;
		}
		public void Run()
		{
			Init();

			DisplayManager.CreateWindow(InitialWindowWidth, InitialWindowHeight, WindowTitle);

			LoadContent();

			while (!Glfw.WindowShouldClose(DisplayManager.Window))
			{
				GameTime.DeltaTime = Glfw.Time - GameTime.TotalElapsedSeconds;
				GameTime.TotalElapsedSeconds = Glfw.Time;

				Update();

				Glfw.PollEvents();

				Draw();
			}
			DisplayManager.CloseWindow();
			Environment.Exit(0);
		}

		/// <summary>
		/// You can't use OpenGL here
		/// </summary>
		protected abstract void Init();

		/// <summary>
		/// Called every frame. Execute any position or rotation related things here.
		/// </summary>
		protected abstract void Update();

		/// <summary>
		/// Called every frame.
		/// </summary>
		protected abstract void Draw();

		/// <summary>
		/// Load all the content that is needed for OpenGL here.
		/// </summary>
		protected abstract void LoadContent();
	}
}
