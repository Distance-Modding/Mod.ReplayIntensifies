using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Distance.ReplayIntensifies.Helpers
{
	/// <summary>
	/// Optimized and organized reflection for use with Steamworks,
	/// which is needed due to the lack of <c>Assembly-CSharp-firstpass.dll</c> dependency.
	/// <para/>
	/// <see cref="SteamworksHelper.Init()"/> must be called before using.
	/// </summary>
	public static class SteamworksHelper
	{
		/// <summary>
		/// Access: <see cref="SteamworksLeaderboard.Entry"/><c>.steamID_</c> -> <c>CSteamID.m_SteamID</c>
		/// </summary>
		public static Func<SteamworksLeaderboard.Entry, ulong> GetLeaderboardEntrySteamID { get; private set; }


		public static void Init()
		{
			GetLeaderboardEntrySteamID = CreateAccessor<SteamworksLeaderboard.Entry, ulong>("steamID_.m_SteamID");
		}


		// Using this allows for compiling expressions that may need to access non-public fields or properties.
		// see: <https://stackoverflow.com/a/16208620/7517185>
		private static Delegate CreateAccessor(Type type, string accessorPath)
		{
			Mod.Instance.Logger.Debug($"CreateAccessor: {type.Name}.{accessorPath}");
			Stopwatch watch = Stopwatch.StartNew();

			ParameterExpression param = Expression.Parameter(type, "x");
			Expression body = param;
			foreach (string member in accessorPath.Split('.'))
			{
				body = Expression.PropertyOrField(body, member);
			}
			Delegate compiled = Expression.Lambda(body, param).Compile();

			watch.Stop();
			Mod.Instance.Logger.Debug($"CreateAccessor: {watch.ElapsedMilliseconds}ms");
			return compiled;
		}

		private static Func<T, R> CreateAccessor<T, R>(string accessorPath)
		{
			return (Func<T, R>)CreateAccessor(typeof(T), accessorPath);
		}
	}
}
