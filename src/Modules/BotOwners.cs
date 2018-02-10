using Discord.Commands;
using FFA.Common;
using FFA.Database;
using FFA.Extensions;
using FFA.Preconditions;
using FFA.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FFA.Modules
{
    [BotOwner]
    public sealed class BotOwners : ModuleBase<Context>
    {
        private readonly FFAContext _ffaContext;
        private readonly SendingService _sender;
        private readonly RulesService _rulesService;
        private readonly ReputationService _repService;

        public BotOwners(FFAContext ffaContext, SendingService sender, RulesService rulesService, ReputationService repService)
        {
            _ffaContext = ffaContext;
            _sender = sender;
            _rulesService = rulesService;
            _repService = repService;
        }

        [Command("Eval")]
        [Summary("Evaluate C# code in a command context.")]
        public async Task EvalAsync([Summary("Client.Token")] [Remainder] string code)
        {
            var script = CSharpScript.Create(code, Configuration.SCRIPT_OPTIONS, typeof(Globals));
            var diagnostics = script.Compile();
            var compilerError = diagnostics.FirstOrDefault(x => x.Severity == DiagnosticSeverity.Error);

            if (compilerError != default(Diagnostic))
            {
                await Context.SendFieldsErrorAsync("Eval", $"```cs\n{code}```", "Compilation Error", $"```{compilerError.GetMessage()}```");
            }
            else
            {
                try
                {
                    var result = await script.RunAsync(new Globals(Context, _ffaContext, _sender, _rulesService, _repService));
                    await Context.SendFieldsAsync(null, "Eval", $"```cs\n{code}```", "Result", $"```{result.ReturnValue?.ToString() ?? "Success."}```");
                }
                catch (Exception ex)
                {
                    await Context.SendFieldsErrorAsync("Eval", $"```cs\n{code}```", "Runtime Error", $"```{ex.LastMessage()}```");
                }
            }
        }
    }

    public class Globals
    {
        public Globals(Context context, FFAContext ffaContext, SendingService sender, RulesService rulesService, ReputationService reputationService)
        {
            Context = context;
            FFAContext = ffaContext;
            Sender = sender;
            RulesService = rulesService;
            ReputationService = reputationService;
        }

        public Context Context { get; }
        public FFAContext FFAContext { get; }
        public SendingService Sender { get; }
        public RulesService RulesService { get; }
        public ReputationService ReputationService { get; }
    }
}
