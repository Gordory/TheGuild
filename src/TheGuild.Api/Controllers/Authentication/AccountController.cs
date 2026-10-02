using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheGuild.Api.Authentication.External;
using TheGuild.DataLayer.Models.Identity;

namespace TheGuild.Api.Controllers.Authentication;

[ApiController]
[Route("auth")]
public class AccountController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet("{provider}/login")]
    public IActionResult Login(string provider)
    {
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            provider,
            Url.Action(nameof(LoginCallback)));

        return Challenge(properties, provider);
    }

    [AllowAnonymous]
    [HttpGet("login/callback")]
    public async Task<IActionResult> LoginCallback()
    {
        var login = await _signInManager.GetExternalLoginInfoAsync();
        if (login is null)
        {
            return BadRequest(new ProblemDetails { Title = "No external login to complete" });
        }

        // Known provider identity: the ordinary path, and the only one that signs anybody in without
        // further proof.
        var signIn = await _signInManager.ExternalLoginSignInAsync(
            login.LoginProvider,
            login.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (signIn.Succeeded)
        {
            await RefreshDisplayNameAsync(login);
            return Ok(new { status = "signed-in" });
        }

        var verifiedEmail = VerifiedEmail(login);

        if (verifiedEmail is not null && await _userManager.FindByEmailAsync(verifiedEmail) is not null)
        {
            // An account already holds this address. Signing in here would hand it over on the
            // strength of the provider's word alone, so refuse and offer linking instead: whoever
            // owns the account proves it by signing in with a provider already attached to it, then
            // attaches this one from POST auth/link/{provider}.
            return Conflict(new
            {
                status = "link-required",
                provider = login.LoginProvider,
                email = verifiedEmail,
            });
        }

        return await CreateAccountAsync(login, verifiedEmail);
    }

    [Authorize]
    [HttpPost("link/{provider}")]
    public IActionResult Link(string provider)
    {
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            provider,
            Url.Action(nameof(LinkCallback)),
            _userManager.GetUserId(User));

        return Challenge(properties, provider);
    }

    [Authorize]
    [HttpGet("link/callback")]
    public async Task<IActionResult> LinkCallback()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var login = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
        if (login is null)
        {
            return BadRequest(new ProblemDetails { Title = "No external login to attach" });
        }

        var linked = await _userManager.AddLoginAsync(user, login);
        if (!linked.Succeeded)
        {
            return Problem(Describe(linked), statusCode: StatusCodes.Status400BadRequest);
        }

        // The external cookie has served its purpose; leaving it would let a later request attach the
        // same identity again.
        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(user, isPersistent: true);

        return Ok(new { status = "linked", provider = login.LoginProvider });
    }

    [Authorize]
    [HttpDelete("logins/{provider}")]
    public async Task<IActionResult> Unlink(string provider, [FromQuery] string providerKey)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var logins = await _userManager.GetLoginsAsync(user);

        // Removing the last provider would leave an account nobody can ever sign into again, and we
        // have no password to fall back on.
        if (logins.Count <= 1)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cannot remove the only way to sign in",
                Detail = "Attach another provider first, or delete the account.",
            });
        }

        var removed = await _userManager.RemoveLoginAsync(user, provider, providerKey);

        return removed.Succeeded
            ? Ok(new { status = "unlinked", provider })
            : Problem(Describe(removed), statusCode: StatusCodes.Status400BadRequest);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var logins = await _userManager.GetLoginsAsync(user);

        return Ok(new
        {
            id = user.Id,
            displayName = user.DisplayName,
            email = user.Email,
            logins = logins.Select(login => new { login.LoginProvider, login.ProviderKey }),
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return Ok(new { status = "signed-out" });
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> Delete()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var deleted = await _userManager.DeleteAsync(user);
        if (!deleted.Succeeded)
        {
            return Problem(Describe(deleted), statusCode: StatusCodes.Status400BadRequest);
        }

        await _signInManager.SignOutAsync();

        return NoContent();
    }

    private async Task<IActionResult> CreateAccountAsync(ExternalLoginInfo login, string? verifiedEmail)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            // Built from the provider identity rather than from the person's chosen name: Identity
            // requires UserName to be unique, and provider display names collide and change.
            UserName = $"{login.LoginProvider}:{login.ProviderKey}",
            Email = verifiedEmail,
            EmailConfirmed = verifiedEmail is not null,
            DisplayName = DisplayName(login),
        };

        var created = await _userManager.CreateAsync(user);
        if (!created.Succeeded)
        {
            return Problem(Describe(created), statusCode: StatusCodes.Status400BadRequest);
        }

        var linked = await _userManager.AddLoginAsync(user, login);
        if (!linked.Succeeded)
        {
            return Problem(Describe(linked), statusCode: StatusCodes.Status400BadRequest);
        }

        await _signInManager.SignInAsync(user, isPersistent: true);

        return Ok(new { status = "signed-in", created = true });
    }

    private async Task RefreshDisplayNameAsync(ExternalLoginInfo login)
    {
        var displayName = DisplayName(login);
        if (displayName is null)
        {
            return;
        }

        var user = await _userManager.FindByLoginAsync(login.LoginProvider, login.ProviderKey);
        if (user is null || user.DisplayName == displayName)
        {
            return;
        }

        user.DisplayName = displayName;
        await _userManager.UpdateAsync(user);
    }

    private static string? DisplayName(ExternalLoginInfo login)
    {
        return login.Principal.FindFirstValue(ClaimTypes.Name) ?? login.ProviderDisplayName;
    }

    private static string? VerifiedEmail(ExternalLoginInfo login)
    {
        var email = login.Principal.FindFirstValue(ClaimTypes.Email);
        if (email is null)
        {
            return null;
        }

        // An unverified address proves nothing about who controls it, so it is treated as no address
        // at all rather than as a weaker hint.
        var verified = login.Principal.FindFirstValue(ExternalLoginClaims.EmailVerified);

        return bool.TryParse(verified, out var isVerified) && isVerified ? email : null;
    }

    private static string Describe(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
