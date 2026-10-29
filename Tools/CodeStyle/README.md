# Owned C# conventions

Run from the repository root with .NET SDK 10:

```powershell
dotnet run --project Tools/CodeStyle -- --check
dotnet run --project Tools/CodeStyle -- --fix
```

The default is check-only and exits with code 1 when source differs from the conventions. The tool references Roslyn bundled with the SDK; it has no NuGet package dependencies and never produces a Unity player build.

Scope: owned runtime scripts, owned Editor scripts, architecture checks and this tool's sources. Generated bin/obj files and third-party assets/packages are excluded.

Rules: member-kind/access/static/readonly ordering, Unity lifecycle callback priority and phase order, stable order for equal keys, explicit `== false` instead of unary logical negation, control-flow separation without blank lines against enclosing block boundaries, member separation and long expression wrapping. Serialized fields retain their relative order inside Inspector groups. `.editorconfig` supplies editor defaults. Existing small immutable palette/hash caches and framework-required static entry points are intentional; persistent/domain services are instances.

The checker implements the user's callback exception rather than blindly applying stock SA1202 to Unity methods. It uses syntax trees, preserving string literals and expression semantics.
