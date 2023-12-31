﻿using SharpPhysics._2d.ObjectRepresentation;

namespace SharpPhysics.Utilities.MathUtils.DelaunayTriangulator
{

	public class Triangle
	{
		public Point Vertex1 { get; }
		public Point Vertex2 { get; }
		public Point Vertex3 { get; }
		internal bool IsToBeRemoved = false;

		public Edge[] Edges { get; }

		public Triangle(Point vertex1, Point vertex2, Point vertex3)
		{
			Vertex1 = vertex1;
			Vertex2 = vertex2;
			Vertex3 = vertex3;

			Edges = [new Edge(Vertex1, Vertex2), new Edge(Vertex2, Vertex3), new Edge(Vertex3, Vertex1)];
		}

		private const double Epsilon = 1e-6;

		public bool IsPointInsideCircumcircle(Point point)
		{
			double ax = Vertex1.X - point.X;
			double ay = Vertex1.Y - point.Y;
			double bx = Vertex2.X - point.X;
			double by = Vertex2.Y - point.Y;
			double cx = Vertex3.X - point.X;
			double cy = Vertex3.Y - point.Y;

			double ab = ax * by - ay * bx;
			double bc = bx * cy - by * cx;
			double ca = cx * ay - cy * ax;

			double alift = ax * ax + ay * ay;
			double blift = bx * bx + by * by;
			double clift = cx * cx + cy * cy;

			// Check if point is inside the circumcircle
			return alift * bc + blift * ca + clift * ab > Epsilon;
		}
		public override string ToString()
		{
			return $"Triangle: ({Vertex1.X}, {Vertex1.Y}), ({Vertex2.X}, {Vertex2.Y}), ({Vertex3.X}, {Vertex3.Y})";
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			Triangle other = (Triangle)obj;

			// Check for set equality, considering both orientations of triangles
			return (AreVerticesEqual(Vertex1, other.Vertex1) || AreVerticesEqual(Vertex1, other.Vertex2) || AreVerticesEqual(Vertex1, other.Vertex3)) &&
				   (AreVerticesEqual(Vertex2, other.Vertex1) || AreVerticesEqual(Vertex2, other.Vertex2) || AreVerticesEqual(Vertex2, other.Vertex3)) &&
				   (AreVerticesEqual(Vertex3, other.Vertex1) || AreVerticesEqual(Vertex3, other.Vertex2) || AreVerticesEqual(Vertex3, other.Vertex3));
		}

		public override int GetHashCode() => Vertex1.GetHashCode() + Vertex2.GetHashCode() + Vertex3.GetHashCode();

		private bool AreVerticesEqual(Point v1, Point v2)
		{
			return Math.Abs(v1.X - v2.X) < Epsilon && Math.Abs(v1.Y - v2.Y) < Epsilon;
		}

		public bool HasVertex(Point vertex)
		{
			if (vertex == null) { return false; }
			if (vertex == Vertex1) return true;
			if (vertex == Vertex2) return true;
			if (vertex == Vertex3) return true;
			return false;
		}

		public Circle Circumcircle
		{
			get
			{
				double x1 = Vertex1.X;
				double y1 = Vertex1.Y;
				double x2 = Vertex2.X;
				double y2 = Vertex2.Y;
				double x3 = Vertex3.X;
				double y3 = Vertex3.Y;

				double D = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
				double Ux = ((x1 * x1 + y1 * y1) * (y2 - y3) + (x2 * x2 + y2 * y2) * (y3 - y1) + (x3 * x3 + y3 * y3) * (y1 - y2)) / D;
				double Uy = ((x1 * x1 + y1 * y1) * (x3 - x2) + (x2 * x2 + y2 * y2) * (x1 - x3) + (x3 * x3 + y3 * y3) * (x2 - x1)) / D;

				Point center = new Point(Ux, Uy);
				double radius = Math.Sqrt((x1 - Ux) * (x1 - Ux) + (y1 - Uy) * (y1 - Uy));

				return new Circle(center, radius);
			}
		}

	}
}
