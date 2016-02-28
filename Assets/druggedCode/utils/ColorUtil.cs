using UnityEngine;
using System.Collections;

namespace druggedcode
{
	public class ColorUtil
	{
		//based on UnifyWiki  http://wiki.unity3d.com/index.php?title=HexConverter
		//hex Value:	Pass RGBA hex color values in to set the color property.  IE:   "A0FF8BFF"
		static public Color32 HexToColor (string hex) {
			if (hex.Length < 6)
				return Color.magenta;

			hex = hex.Replace("#", "");
			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			byte a = 0xFF;
			if (hex.Length == 8)
				a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

			return new Color32(r, g, b, a);
		}
	}
}

