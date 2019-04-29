using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using ModMail.Preconditions;
using ModMail.Services;

namespace ModMail.Commands
{
    public class DeveloperCommands : BaseCommandModule
    {
        private readonly EvaluationService _evalService;

        public DeveloperCommands(EvaluationService evalService)
        {
            _evalService = evalService;
        }
        
        [Command("eval")]
        public async Task EvaluateCode(CommandContext ctx, [RemainingText] string code)
        {
            var watch = new Stopwatch();
            watch.Start();

            var formattedMessage = Formatter.BlockCode("Evaluating...", "diff");

            var msg = await ctx.RespondAsync(formattedMessage);
            
            var result = await _evalService.EvaluateAsync(ctx, code);

            watch.Stop();

            formattedMessage = Formatter.BlockCode($"+ Evaluated in {watch.ElapsedMilliseconds} ms\n"
                                                   + $"+ Result: {(result.Result ?? result.Exception).GetType().Name}\n\n"
                                                   + $"{result.Result ?? result.Exception.Message}\n", "diff");

            await msg.ModifyAsync(content: formattedMessage);
        }
        
    }
}