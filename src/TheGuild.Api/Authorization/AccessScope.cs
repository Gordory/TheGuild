namespace TheGuild.Api.Authorization;

/// <summary>
/// How much of a collection the actor may see. Exists because a yes/no answer cannot shape a query:
/// asking "may I read warnings" has three useful answers, and the middle one has to reach the
/// repository as a filter rather than discard rows after loading them.
/// </summary>
public enum AccessScope
{
    None,
    Own,
    Any,
}
