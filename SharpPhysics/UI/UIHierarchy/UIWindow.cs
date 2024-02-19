﻿namespace SharpPhysics.UI.UIHierarchy
{
	public class UIWindow
	{
		public string Title { get; set; }
		public IUIElement[] Elements = [];

		public UIWindow(string title, IUIElement[] elements)
		{
			Title = title;
			Elements = elements;
		}
	}
}
