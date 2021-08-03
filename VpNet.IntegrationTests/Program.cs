using System;
using System.Threading.Tasks;
using VpNet.EventData;

namespace VpNet.IntegrationTests
{
    internal static class Program
    {
        private static VirtualParadiseClient s_client;

        private static async Task Main()
        {
            Console.WriteLine("Hello World!");
            
            s_client = new VirtualParadiseClient(new VirtualParadiseConfiguration
            {
                Username = "<<username>>",
                Password = "<<password>>",
                BotName = "<<botname>>"
            });
            
            s_client.AvatarJoined += ClientOnAvatarJoined;
            s_client.AvatarLeft += ClientOnAvatarLeft;

            await s_client.ConnectAsync();
            await s_client.LoginAsync();
            await s_client.EnterAsync("<<world name>>");

            Console.ReadKey();
        }

        private static Task ClientOnAvatarJoined(VirtualParadiseClient sender, AvatarJoinedEventArgs args)
        {
            return args.Avatar.SendConsoleMessageAsync("greetings", $"Welcome to {s_client.CurrentWorld.Name}, {args.Avatar.Name}.");
        }

        private static Task ClientOnAvatarLeft(VirtualParadiseClient sender, AvatarLeftEventArgs args)
        {
            return sender.BroadcastConsoleMessageAsync($"{args.Avatar.Name} has left {s_client.CurrentWorld.Name}");
        }
    }
}
