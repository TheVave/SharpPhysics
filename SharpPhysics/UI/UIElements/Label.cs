﻿using ImGuiNET;
using System.Numerics;

namespace SharpPhysics.UI.UIElements
{
	public class Label : IUIElement
	{
		public string Txt;
		public bool Visible { get; set; } = true;
		public Action OnDraw = () => { };
		public Vector2 Position { get; set; } = Vector2.Zero;

		public Label(string txt)
		{
			Txt = txt;
		}

		public Label(string txt, Vector2 position)
		{
			Txt = txt;
			Position = position;
		}

		public bool Draw()
		{
			try
			{
				ImGui.Text(Txt);
				OnDraw.Invoke();
				return true;
			}
			catch
			{
				return false;
			}

		}
	}
}
