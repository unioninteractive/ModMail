using System;
using System.Threading.Tasks;

namespace ModMail
{
    class Program
    {
        static void Main(string[] args) => Program.MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            var bot = new Bot();
            await bot.StartAsync(args);
        }
    }
}