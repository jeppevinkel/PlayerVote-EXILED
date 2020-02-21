using EXILED;
using EXILED.Extensions;

namespace PlayerVote
{
	public static class Extensions
	{
		//public static void RAMessage(this CommandSender sender, string message, bool success = true) =>
		//	sender.RaReply("PlayerVote#" + message, success, true, string.Empty);

		//public static void Broadcast(this ReferenceHub rh, uint time, string message) => rh.GetComponent<Broadcast>().TargetAddElement(rh.scp079PlayerScript.connectionToClient, message, time, false);
	
		public static void BC(uint time, string msg)
		{
			foreach (ReferenceHub p in Player.GetHubs())
				p.Broadcast(time, msg);
		}
	}
}