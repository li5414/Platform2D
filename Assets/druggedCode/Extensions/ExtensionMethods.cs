using UnityEngine;
using System.Collections;

namespace druggedcode.extensions.anim
{
	public static class ExtensionMethods 
	{
		/// <summary>
		/// 애니메이터가 특정 파라메터를 포함하는지 알아낸다.
		/// </summary>
		public static bool HasParameterOfType (this Animator self, string name, AnimatorControllerParameterType type) 
		{
			var parameters = self.parameters;
			foreach ( var currParam in parameters )
			{
				if ( currParam.type == type && currParam.name == name )
				{
					return true;
				}
			}
			return false;
		}
	}
}
