using System;
using System.Collections.Generic;
using UnityEditor;
using Vexe.Runtime.Extensions;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Others
{
	/// <summary>
	/// A simple utility class that memorizes object selection.
	/// Press Ctrl+Shift+- to go back, Ctrl+Shift+= to go forward
	/// </summary>
	[InitializeOnLoad]
	public static class SelectionMemorizer
	{
		private static UnityObject[] previous = new UnityObject[1] { null };
		private static bool isRunning;
		private const string MenuPath = "Tools/Vexe/SelectionMemorizer";
		private static Stack<SelOp> undo = new Stack<SelOp>();
		private static Stack<SelOp> redo = new Stack<SelOp>();

		static SelectionMemorizer()
		{
			ToggleActive();
		}

		[MenuItem(MenuPath + "/Toggle StartStop")]
		public static void ToggleActive()
		{
			if (isRunning)
				EditorApplication.update -= Update;
			else
				EditorApplication.update += Update;
			isRunning = !isRunning;
		}

		[MenuItem(MenuPath + "/Select Last Object (Back) %#-")]
		public static void Back()
		{
			if (undo.Count == 0)
				return;

			var op = undo.Pop();
			op.Undo();
			redo.Push(op);
		}

		[MenuItem(MenuPath + "/Forward %#=")]
		public static void Forward()
		{
			if (redo.Count == 0)
				return;

			var op = redo.Pop();
			op.Perform();
			undo.Push(op);
		}

		static private void Update()
		{
			var current = Selection.objects;
			if (current != null && !current.IsEqualTo(previous))
			{
				Action a = () => previous = Selection.objects;

				var so = new SelOp
				{
					ToSelect = current,
					ToGoBackTo = previous,
					OnPerformed = a,
					OnUndone = a
				};

				undo.Push(so);
				redo.Clear();

				previous = current;
			}
		}

		public struct SelOp
		{
			public UnityObject[] ToSelect, ToGoBackTo;
			public Action OnPerformed, OnUndone;

			public void Perform()
			{
				Selection.objects = ToSelect;
				OnPerformed();
			}

			public void Undo()
			{
				Selection.objects = ToGoBackTo;
				OnUndone();
			}
		}
	}
}