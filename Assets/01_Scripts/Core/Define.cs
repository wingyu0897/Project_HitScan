using UnityEngine;

namespace Define
{
	public enum SETTING_TYPE
	{
		BGM,
		SFX,
	}

	public enum MESSAGE_TYPE
	{
		Kill,
	}

	public enum TEAM_TYPE
	{
		Red = 0,
		Blue,
	}

	public static class Utility
	{

		public static Color GetColorByTeam(TEAM_TYPE team)
		{
			return team switch
			{
				TEAM_TYPE.Red => new Color32(238, 0, 2, 255),
				TEAM_TYPE.Blue => new Color32(0, 102, 255, 255),
				_ => Color.black,
			};
		}
	}

	public struct Ease
	{
		public static float OutExpo(float a, float b, float t)
		{
			float factor = t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
			return a + (b - a) * factor;
		}

		public static float OutSuperExpo(float a, float b, float t)
		{
			float factor = t == 1 ? 1 : 1 - Mathf.Pow(2, -50 * t);
			return a + (b - a) * factor;
		}
	}
}
