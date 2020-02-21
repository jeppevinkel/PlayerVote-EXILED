public class PVoteSender : CommandSender
{
    public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
    {
        // eskeiti
    }

    public override void Print(string text)
    {
        // esekjota
    }

    public string Name = "FFAB";
    public override string SenderId => Name;
    public override string Nickname => Name;
    public override ulong Permissions => ServerStatic.GetPermissionsHandler().FullPerm;
    public override byte KickPower => byte.MaxValue;
    public override bool FullPermissions => true;
}