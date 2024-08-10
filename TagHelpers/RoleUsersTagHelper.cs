using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IdentityApp.TagHelpers
{
    [HtmlTargetElement("td", Attributes ="asp-role-users")]
    public class RoleUsersTagHelper : TagHelper
    {

        [HtmlAttributeName("asp-role-users")]
        public string RoleId { get; set; } = null!;

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public RoleUsersTagHelper(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
           var userNames = new List<string>();
           var role = await _roleManager.FindByIdAsync(RoleId);

            if(role != null && role.Name != null)
            {
                foreach (var user in _userManager.Users)
                {
                    if(await _userManager.IsInRoleAsync(user, role.Name))
                    {
                         userNames.Add(user.UserName ?? ""); 
                    }
                }
                output.Content.SetHtmlContent(userNames.Count == 0 ? "No User" : setHtml(userNames));
            }


        }

        private string setHtml(List<string> userNames)
        {
            var html = "<ul>";

            foreach (var item in userNames)
            {
                html += "<li>" + item + "</li>";
            }
            html += "</ul>";

            return html;
        }
    }
}
