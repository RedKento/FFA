using Discord;
using Discord.Commands;
using FFA.Common;
using FFA.Database.Models;
using FFA.Entities.CustomCmd;
using FFA.Extensions.Database;
using FFA.Extensions.Discord;
using FFA.Preconditions.Command;
using FFA.Preconditions.Parameter;
using FFA.Services;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FFA.Modules
{
    [Name("General")]
    [Summary("The best memes in town start with these commands.")]
    [NotMuted]
    public sealed class General : ModuleBase<Context>
    {
        private readonly IMongoCollection<CustomCmd> _dbCustomCmds;
        private readonly CustomCmdService _customCmdService;
        private readonly ColorRoleService _colorRoleService;

        public General(IMongoCollection<CustomCmd> dbCustomCmds, CustomCmdService customCmdService, ColorRoleService colorRoleService)
        {
            _dbCustomCmds = dbCustomCmds;
            _customCmdService = customCmdService;
            _colorRoleService = colorRoleService;
        }

        [Command("Color")]
        [Alias("colour")]
        [Summary("Give yourself a role with any color you please.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Top(Config.TOP_COLOR)]
        [Cooldown(Config.COLOR_CD)]
        public async Task ColorAsync(
            [Summary("#FF0000")] [Remainder] Color color)
        {
            var role = await _colorRoleService.GetOrCreateAsync(Context.Guild, _colorRoleService.FormatColor(color), color);
            var existingColorRoles = Context.GuildUser.GetRoles().Where(x => x.Name.StartsWith('#'));

            await Context.GuildUser.RemoveRolesAsync(existingColorRoles);
            await Context.GuildUser.AddRoleAsync(role);
            await Context.ReplyAsync("You have successfully set your role color.");
        }

        [Command("AddCommand")]
        [Alias("addcmd", "createcommand", "createcmd")]
        [Summary("Add any custom command you please.")]
        public async Task AddCommandAsync(
            [Summary("retarded")] [UniqueCustomCmd] string name,
            [Summary("VIM2META LOL DUDE IS THICC AS BALLS")] [Remainder] [MaxLength(Config.MAX_CMD_LENGTH)] CmdResponse response)
        {
            var newCmd = new CustomCmd(Context.User.Id, Context.Guild.Id, name.ToLower(), response.Value);
            await _dbCustomCmds.InsertOneAsync(newCmd);
            await Context.ReplyAsync("You have successfully added a new custom command.");
        }

        [Command("ModifyCommand")]
        [Alias("modcommand", "modcmd", "modifycmd")]
        [Summary("Modify an existing custom command.")]
        [Top(Config.TOP_MOD_CMD)]
        [Cooldown(Config.MOD_CMD_CD)]
        public async Task ModifyCommandAsync(
            [Summary("vim2meta")] CustomCmd command,
            [Summary("RETARD THAT'S AS BLIND AS ME GRAN")] [Remainder] [MaxLength(Config.MAX_CMD_LENGTH)] CmdResponse response = null)
        {
            await _dbCustomCmds.UpdateAsync(command, x => x.Response = response.Value);
            await Context.ReplyAsync("You have successfully updated this command.");
        }

        [Command("RemoveCommand")]
        [Alias("removecmd", "deletecommand", "deletecmd")]
        [Summary("Delete an existing custom command.")]
        [Top(Config.TOP_REMOVE_CMD)]
        [Cooldown(Config.REMOVE_CMD_CD)]
        public async Task RemoveCommandAsync(
            [Summary("vim2meta")] CustomCmd command)
        {
            await _dbCustomCmds.DeleteOneAsync(command);
            await Context.ReplyAsync("You have successfully deleted this command.");
        }
    }
}
