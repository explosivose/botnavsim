//#define PROFILE
//#define DBG

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Vexe.Editor.Drawers;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.GUIs
{
	public abstract partial class BaseGUI
	{
		public bool Member(MemberInfo info, object rawTarget, UnityObject unityTarget, string key, bool ignoreComposition)
		{
			if (info.MemberType == MemberTypes.Method)
			{
				var method = info as MethodInfo;
				var methodKey = Cache.GetMethodKey(Tuple.Create(key, method));
				var methodDrawer = MemberDrawersHandler.Instance.GetMethodDrawer(methodKey);
				methodDrawer.Initialize(method, rawTarget, unityTarget, methodKey, this);
				return methodDrawer.OnGUI();
			}
			else
			{
				var dm = Cache.GetMember(Tuple.Create(info, key));
				dm.Target = rawTarget;
				dm.UnityTarget = unityTarget;

				return Member(dm, ignoreComposition);
			}
		}

		public bool Member(EditorMember member)
		{
			return Member(member, false);
		}

		public bool Member(EditorMember member, bool ignoreComposition)
		{
			return Member(member, member.Attributes, ignoreComposition);
		}

		public bool Member(EditorMember member, Attribute[] attributes, bool ignoreComposition)
		{
			var handler = MemberDrawersHandler.Instance;
			var memberDrawer = handler.GetMemberDrawer(member, attributes, this);
			memberDrawer.Initialize(member, attributes, this);
			return Member(member, attributes, memberDrawer, ignoreComposition);
		}

		public bool Member(EditorMember member, Attribute[] attributes, BaseDrawer memberDrawer, bool ignoreComposition)
		{
			var handler = MemberDrawersHandler.Instance;

			List<BaseDrawer> composites = null;

			if (!ignoreComposition)
				composites = handler.GetCompositeDrawers(member, attributes, this);

			#if DBG
			Label(member.Id);
			Debug.Log("Got drawer " + memberDrawer.GetType().Name + " for member " + member.Name + " Key: " + handlerKey);
			#endif

			if (composites == null || composites.Count == 0)
			{
				#if PROFILE
				Profiler.BeginSample(memberDrawer.GetType().Name + " OnGUI (C)");
				#endif

				BeginCheck();
				{
					memberDrawer.OnGUI();
				}
				#if PROFILE
				Profiler.EndSample();
				#endif
				return HasChanged();
			}

			for (int i = 0; i < composites.Count; i++)
				composites[i].Initialize(member, attributes, this);

			Action<List<BaseDrawer>, Action<BaseDrawer>> drawSection = (drawers, section) =>
			{
				for (int i = 0; i < drawers.Count; i++)
					section(drawers[i]);
			};

			bool changed = false;

			#if PROFILE
			Profiler.BeginSample("OnUpperGUI " + member.Name);
			#endif
			drawSection(composites, d => d.OnUpperGUI());
			#if PROFILE
			Profiler.EndSample();
			#endif
			using (Horizontal())
			{
				#if PROFILE
				Profiler.BeginSample("OnLeftGUI " + member.Name);
				#endif
				drawSection(composites, d => d.OnLeftGUI());
				#if PROFILE
				Profiler.EndSample();
				#endif
				using (Vertical())
				{
					#if PROFILE
					Profiler.BeginSample(memberDrawer.GetType().Name + " OnGUI");
					#endif
					BeginCheck();
					{
						memberDrawer.OnGUI();
					}
					#if PROFILE
					Profiler.EndSample();
					#endif
					changed = HasChanged();

					#if PROFILE
					Profiler.BeginSample("OnMemberDrawn" + member.Name);
					#endif
					drawSection(composites, d => d.OnMemberDrawn(LastRect));
					#if PROFILE
					Profiler.EndSample();
					#endif
				}

				#if PROFILE
				Profiler.BeginSample("OnRightGUI " + member.Name);
				#endif
				drawSection(composites, d => d.OnRightGUI());
				#if PROFILE
				Profiler.EndSample();
				#endif
			}

			#if PROFILE
			Profiler.BeginSample("OnLowerGUI " + member.Name);
			#endif
			drawSection(composites, d => d.OnLowerGUI());
			#if PROFILE
			Profiler.EndSample();
			#endif
			return changed;
		}

		private static class Cache
		{
			private static Func<Tuple<string, MethodInfo>, string> getMethodKey;
			public static Func<Tuple<string, MethodInfo>, string> GetMethodKey
			{
				get { return getMethodKey ?? (getMethodKey = new Func<Tuple<string, MethodInfo>, string>(x => x.Item1 + x.Item2.GetNiceName()).Memoize()); }
			}

			private static Func<Tuple<MemberInfo, string>, EditorMember> _getMember;
			public static Func<Tuple<MemberInfo, string>, EditorMember> GetMember
			{
				get
				{
					return _getMember ?? (_getMember = new Func<Tuple<MemberInfo, string>, EditorMember>(x =>
						new EditorMember(x.Item1, null, null, x.Item2)).Memoize());
				}
			}
		}
	}
}
