# Deviations from DESIGN.md (SpecializedDisplay.Y70ti)

Tracks departures from `docs/DESIGN.md` §4. Both items below are behavior-neutral.

## 1. `CreateOptions` sets `RefreshHz = NativeRefreshHz`

DESIGN §4's shown initializer is `new AcquireOptions { ModeSelector = NativeMode, Rotation = …, Log = … }`
(resilience knobs at defaults). `CreateOptions` additionally sets `RefreshHz = NativeRefreshHz`. This is
behaviorally identical to leaving it unset — the core `AcquireOptions.RefreshHz` default is already
`60.0` — but it makes the declared `NativeRefreshHz` constant load-bearing and states the panel's pacing
target explicitly. All other resilience knobs are left at the core (hardware-tuned) defaults, as
specified.

## 2. `CreateTouchSource` fully-qualifies `SpecializedDisplay.Touch.LogLevel`

The profile references both `SpecializedDisplay` and `SpecializedDisplay.Touch`, which each define a
`LogLevel` enum. The touch source's log sink uses the Touch one; its type is written as
`Action<SpecializedDisplay.Touch.LogLevel, string>?` in the `CreateTouchSource` signature to resolve the
ambiguity (a `using LogLevel = …` alias did not override the namespace-imported name). No public-API
impact — the parameter type is exactly what `RawTouchSource` expects.
