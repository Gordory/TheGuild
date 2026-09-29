# Guild authorization: catalogued permissions, roles over Discord

Design record for the permission system. Written after the design dialogue, before implementation,
and kept as the explanation of why the model looks the way it does.

## The goal

One place where an officer grants a member access, after which it applies across every layer at
once — the API, the Discord bot, and eventually the addon.

That goal is what rules out the obvious alternative. If Discord owned both membership *and* the
permission grants, the single place to grant would be Discord, which is the opposite of the goal.

## What was wrong with the previous model

1. **`Self` / `ForOthers` were not permissions.** They encoded a *relation between the actor and the
   record* inside a flag. One relation fits; the moment "own raid", "within 24 hours" or "lower rank"
   arrive, the vocabulary grows as verbs × resources × relations.
2. **Roles were anonymous.** `GuildRole` had no `Id` and no `Name`, so a role could not be referenced
   from a UI, attributed in an audit, or explained to an officer.
3. **The vocabulary was closed in enums.** It could not be handed to the addon as a snapshot, and a
   settings screen could not be drawn from it.

## The model

**A role belongs to the guild and is the only thing that grants.** It carries `Id`, `Name`, the
Discord roles it draws membership from (many-to-many), members named outright, and its grants.

**Membership is never stored.** It follows from the linked Discord roles, so Discord remains the
single answer to "who is a raid leader". A role that links nothing and names its members outright is
how a role with no Discord counterpart works — no third concept needed.

**Permissions are atomic ids from a catalogue declared in code.** A permission exists only where code
checks for it: a row added to a table grants nothing and enforces nothing, so the catalogue cannot
outrun the code. `Implies` carries over what the composite flag members expressed (`read.any`
subsumes `read.own`) with the value computed rather than hand-written.

**The catalogue is projected into MongoDB, one direction only.** The settings screen needs to list
what can be granted, and a label should not need a release to fix. Code stays the authority; checks
and grant validation read the code catalogue, never the rows, so an edit in the database cannot
quietly disagree with what is enforced.

**Conditions live in the evaluator, not in the vocabulary.** A permission names a verb; ownership of
the loaded record picks which half of the `own`/`any` pair a request needs.

**Collections resolve to a scope, not a verdict.** A yes/no answer cannot shape a query. `Own`
reaches the repository as a filter on the acting member instead of rows being loaded and discarded.

## Why a domain evaluator rather than ASP.NET policies

1. **The bot is a separate process.** It needs to know permissions *before* offering a slash command,
   and it cannot reuse an evaluation that lives inside the web pipeline.
2. **Lists.** The framework is built for yes/no on one resource; filtering a collection through it
   means loading everything and discarding, or putting the scope logic outside it anyway.
3. **A dynamic catalogue fights startup-registered named policies.** It would need a custom
   `IAuthorizationPolicyProvider` — writing our own layer inside someone else's model.

## Deliberately out of scope

**Seniority.** No operation in the domain uses it. Kick, ban and role management stay with Discord,
which applies its own hierarchy. Defining "rank = max(Position)" before a rule needs that semantics
would be defining it by guess. Cheap to add later: an absent `int` in an embedded document
deserialises to `0`, which is the right default, so no migration.

**Explicit deny.** Grant-only. With permissions unioned across several roles, deny creates a
precedence puzzle that cannot be explained to an officer in one sentence. To take something away,
remove the member from the role or narrow the role.

**Writing to Discord.** Channel access stays Discord's. Pushing role assignments there would need
`MANAGE_ROLES` and a bot positioned above the roles it manages, which turns our API key into a key to
the server, and brings reconciliation, echo loops and ambiguous deletion with it. Two-way sync is
weeks of work whose complexity never leaves operations.

**Guild-authored permissions.** A permission a guild invents is checked by nobody. The model does not
block it: grants are already data keyed by string ids, so a new grant source or condition type is an
implementation behind `IGuildAuthorizer`.

## Shape of a grant

Stored as a document rather than a bare string:

```
Grants: [ { PermissionId: "attendance.warning.read.any" } ]
```

This is the one hedge taken deliberately. Parameterised permissions ("edit own within N hours") then
arrive as a field, where a flat array of strings would have meant migrating embedded arrays across
every guild document. Hedge where the retrofit is expensive; do not hedge where it is free — which is
exactly why seniority was not hedged.

## The addon cannot enforce

A WoW addon has no network access: it reaches the service only through `SavedVariables` read by a
desktop companion, or a copy-paste export string. And its files belong to the user. So permissions in
the addon are interface hints — hide a button, skip a field — and the real check happens server-side
when data comes back. This is also why the vocabulary is strings: the snapshot has to serialise and
be versioned so an old addon does not break on new permissions.
