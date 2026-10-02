# Accounts on ASP.NET Core Identity, and the move to net10

Continues [the guild authorization design](2026-09-29-guild-authorization-design.md), which assumed a
Discord id identified a person. That assumption is replaced here.

## The identity model

Accounts are ours. There is no password: every sign-in is an external handshake, through Discord now
and Battle.net or Google later. An unknown provider identity creates an account; several providers can
point at one account, so a person keeps their history after switching.

This is exactly Identity's external-login model — a table of `(provider, subject key) → account` — so
the ready implementation fits with no custom store work beyond pointing it at MongoDB.

## Identity owns identity; guild permissions stay ours

`IdentityRole` is flat and has no tenant dimension. "Officer" means something only inside one guild, so
`[Authorize(Roles = "Officer")]` could only ever mean officer *everywhere*. `ApplicationRole` is
therefore reserved for application-wide roles such as platform support, and guild roles stay in
`GuildRole` with their own permission grants.

That split is not a concession: it is what every multi-tenant application on Identity does, and it is
the seam that lets the guild permission system sit on Identity as a plugin.

## Why an account identifies a member

A Discord id cannot answer "who is this person" once Discord is one provider among several: a member
who later signs in with Battle.net must still own the records written about them. So warnings and the
members a role names outright carry account ids.

Discord ids do not go away. Guild membership is still defined by Discord roles — deliberately, so that
membership is never duplicated — and a bot can name nobody else. `GuildActor` therefore carries both,
with the account nullable: somebody who has never signed in still holds what their Discord roles grant,
just nothing granted to them personally.

### Why this creates no duplicate accounts

An account made for a member who never signed in is created **together with its
`(Discord, <discord id>)` login row**. That member's first real sign-in is then an ordinary
`FindByLoginAsync` hit on the account that already exists. The duplicate-account problem belongs to a
different design — one that creates an empty account and later tries to match it by email or name.

Resolution has two modes, and the split is the design:

- **Look up, do not create** for the acting member. Resolving an actor happens on every request,
  including plain reads, which must not leave accounts behind.
- **Create if missing** for the subject of a new record, and only *after* the permission check. A
  refused request must not leave an account behind for whoever it named.

## Linking by email is an offer, never an action

When an unknown provider identity arrives carrying an address an account already holds, the callback
answers `409 link-required` and signs nobody in. Whoever owns that account proves it by signing in with
a provider already attached, then attaches the new one.

Three rules hold this up:

1. **Never link on email alone.** Otherwise registering a Discord account on someone else's address is
   enough to take over their account here. With no password, signing in through an already-attached
   provider is the only proof of ownership available.
2. **Only a verified address counts.** Discord reports `verified`; the provider package maps no such
   claim, so the raw field is mapped in startup. An unverified address is treated as no address at all,
   not as a weaker hint — otherwise rule 1 is bypassed at the provider.
3. **Email is a hint, never a key.** Addresses change and get reused; identity stays
   `(provider, subject key)`.

Removing the last login is refused as well: with no password it would leave an account nobody can ever
enter again.

User names are built from the provider identity rather than from a person's chosen name, because
Identity needs them unique while display names collide and change. Display names are cached labels,
refreshed on each sign-in.

## net10 and the dependency refresh

`net6.0` was out of support and `MongoDB.Driver` 2.16.1 carried `GHSA-7j9m-j397-g4wx` (patched in
2.19.0). Both were already listed as known problems, and Identity needs current packages, so they were
paid off first and on their own, where a break is attributable.

The target framework moved into `Directory.Build.props`: eight copies of one line is eight chances for
one to drift.

`AspNetCore.Identity.Mongo` has no net10 build, so it runs on its net9 assets with Identity 10.x
unified above it. That was checked rather than assumed: `Microsoft.Extensions.Identity.Stores` 9 → 10 is
purely additive, and the one breaking change in `Identity.Core` is a constructor *parameter rename* on
`UserLoginInfo`, which breaks named arguments in source and nothing at runtime.

### Two things the tests caught

**Guid serialization was registered in the wrong assembly.** Driver 3 dropped the legacy representation
modes and leaves the default `Unspecified`, which refuses to serialize a `Guid` at all. The module
initializer sat next to the Mongo infrastructure, which a caller using only the Identity entities never
loads — so it never ran. It now lives in the assembly that declares the documents, where the trigger is
self-enforcing, and uses `RegisterSerializer` rather than `TryRegisterSerializer` so a conflict fails
loudly instead of silently storing a different byte layout.

**Scope `Own` with no account would have read the whole guild.** A null account id passed to the
repository is not "my records" but "no filter". It now returns nothing, because an actor with no account
owns nothing.

Both are the kind of defect that compiles, reads correctly, and is only found by running the thing.

## Still out of scope

- **Battle.net and Google** — the shape accepts them; no provider is wired yet.
- **The browser sign-in flow itself** is not covered by tests and has not been exercised against a real
  Discord application. The store-level behaviour it rests on is.
- **Role management API** — a guild is still configured by writing to MongoDB.
