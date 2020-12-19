using DOTAPI;
using DOTFluentAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DOTFluentAPI
{
	internal enum Interest
	{
		node, source, target
	}
	public class Wrapper<T>
	{
		internal StringBuilder text = new StringBuilder();
		internal bool isDirected;
		internal List<string> nodes = new List<string>();
		internal List<string> edgeSources = new List<string>();
		internal List<string> edgeTargets = new List<string>();
		internal List<string> attributes = new List<string>();
		internal Interest currentInterest;
		public string End<API>() where API : T => BeginDOT._Flush(this).text.Append("}").ToString();
	}
	public static class BeginDOT
	{
		public static Wrapper<digraph<BOTTOM>> Digraph(string name) =>
			new Wrapper<digraph<BOTTOM>>()._Digraph(name);
		public static Wrapper<graph<BOTTOM>> Graph(string name) =>
			new Wrapper<graph<BOTTOM>>()._Graph(name);
		public static Wrapper<node<T>> Node<T>(this Wrapper<T> w, string name) =>
			w._Cast<T, node<T>>()._Flush()._Node(name);
		public static Wrapper<edge<T>> Edge<T>(this Wrapper<T> w, string name) =>
			w._Cast<T, edge<T>>()._Flush()._Edge(name);
		public static Wrapper<to<T>> To<T>(this Wrapper<T> w, string name) =>
			w._Cast<T, to<T>>()._To(name);
		public static Wrapper<and<T>> And<T>(this Wrapper<T> w, string name) =>
			w._Cast<T, and<T>>()._And(name);
		public static Wrapper<color<T>> Color<T>(this Wrapper<T> w, string name) =>
			w._Cast<T, color<T>>()._Attribute($"color={name}");
		public static Wrapper<shape<T>> Shape<T>(this Wrapper<T> w, string name) =>
			w._Cast<T, shape<T>>()._Attribute($"shape={name}");
		public static Wrapper<style<T>> Style<T>(this Wrapper<T> w, string name) =>
			w._Cast<T, style<T>>()._Attribute($"style={name}");
		internal static Wrapper<S> _Cast<T, S>(this Wrapper<T> w)
		{
			return new Wrapper<S>
			{
				text = w.text,
				isDirected = w.isDirected,
				nodes = w.nodes,
				edgeSources = w.edgeSources,
				edgeTargets = w.edgeTargets,
				attributes = w.attributes,
				currentInterest = w.currentInterest
			};
		}
		internal static Wrapper<T> _Graph<T>(this Wrapper<T> w, string name)
		{
			w.isDirected = false;
			w.text.Append($"graph {name} {{\n");
			return w;
		}
		internal static Wrapper<T> _Digraph<T>(this Wrapper<T> w, string name)
		{
			w.isDirected = false;
			w.text.Append($"graph {name} {{\n");
			return w;
		}
		internal static Wrapper<T> _Node<T>(this Wrapper<T> w, string name)
		{
			w.currentInterest = Interest.node;
			w.nodes.Add(name);
			return w;
		}
		internal static Wrapper<T> _Edge<T>(this Wrapper<T> w, string name)
		{
			w.currentInterest = Interest.source;
			w.edgeSources.Add(name);
			return w;
		}
		internal static Wrapper<T> _To<T>(this Wrapper<T> w, string name)
		{
			w.currentInterest = Interest.target;
			w.edgeTargets.Add(name);
			return w;
		}
		internal static Wrapper<T> _And<T>(this Wrapper<T> w, string name)
		{
			switch (w.currentInterest)
			{
				case Interest.node:
					w.nodes.Add(name);
					break;
				case Interest.source:
					w.edgeSources.Add(name);
					break;
				case Interest.target:
					w.edgeTargets.Add(name);
					break;
			}
			return w;
		}
		internal static Wrapper<T> _Attribute<T>(this Wrapper<T> w, string name)
		{
			w.attributes.Add(name);
			return w;
		}
		internal static Wrapper<T> _Flush<T>(this Wrapper<T> w)
		{
			string attributesText = w.attributes.Count == 0 ? "" : $" [{string.Join(", ", w.attributes)}]";
			if (w.nodes.Count > 0)
			{
				string nodesText = w.nodes.Count == 1 ? w.nodes[0] : $"{{{string.Join(", ", w.nodes)}}}";
				w.text.Append($"\t{nodesText}{attributesText};\n");
			}
			else if (w.edgeSources.Count > 0)
			{
				string sourcesText = w.edgeSources.Count == 1 ? w.edgeSources[0] : $"{{{string.Join(", ", w.edgeSources)}}}";
				string targetsText = w.edgeTargets.Count == 1 ? w.edgeTargets[0] : $"{{{string.Join(", ", w.edgeTargets)}}}";
				string edgeText = w.isDirected ? "->" : "--";
				w.text.Append($"\t{sourcesText} {edgeText} {targetsText}{attributesText};\n");
			}
			w.nodes.Clear();
			w.edgeSources.Clear();
			w.edgeTargets.Clear();
			w.attributes.Clear();
			return w;
		}
	}
}

namespace sandbox
{
	class AnotherExample
    {
        public static void OtherMain(string[] _)
        {
			var graph = BeginDOT.Digraph("small_graph")
				.Node("A").Shape("rectangle")
				.Node("B")
				.Node("C").Shape("doublecircle")
				.Edge("A").To("B").Style("dotted")
				.Edge("A").And("B").To("C")
				.End<DOT>();
			Console.WriteLine(graph);
		}
    }
}
