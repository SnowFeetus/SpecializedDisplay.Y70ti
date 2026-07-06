# HYTE Y70ti device notes

The hardware-specific facts this profile encodes, migrated from the Y70ti HWMonitor
`docs/display-findings.md` and `docs/touch-findings.md`. The `SpecializedDisplay.NET` /
`SpecializedDisplay.Touch` libraries stay device-agnostic; every Y70ti constant lives here.

## Display target identity (never touch the BenQ)

- **Y70ti target:** adapter LUID `HighPart=0, LowPart=0x00012041`, `AdapterRelativeId=4353`,
  `StableMonitorId` starts `RTK409AW09C23CM024L_...`.
- **DisplayMonitor correlation:** `DeviceId` contains `RTK409A`; `DisplayName = "HYTE Y70ti"`;
  `NativeResolutionInRawPixels = 2560x682`; `DisplayAdapterId = (0:12041)`;
  `DisplayAdapterTargetId = 4353`.
- **BenQ — DO NOT ACQUIRE:** `AdapterRelativeId=4357`, `DisplayName="XL2586X+"`, native `1920x1080`.

`Y70tiDisplay.Selector` reproduces the renderer's two-phase match exactly: phase 1 takes a monitor whose
`DeviceId` contains `RTK409A` OR whose `DisplayName` contains `HYTE`/`Y70` (last match wins); phase 2
falls back to a target whose `StableMonitorId` contains `RTK409A`. Neither phase can match the BenQ.

## Acquire specifics

- Must use `DisplayManager.Create(DisplayManagerOptions.EnforceSourceOwnership)` (NOT `None`) and
  `TryAcquireTargetsAndCreateEmptyState` (NOT `ReadCurrentState`) to build a source path via
  `ConnectTarget`. This is handled inside `SpecializedDisplay` core; the profile just supplies the
  selector and options.
- `DisplayDevice` has no `RenderAdapterId` in this SDK — the render adapter LUID (`0x12041`) comes from
  `target.Adapter.Id`.

## Native modes (use the 1:1 src=tgt ones)

- `src=2560x682 tgt=2560x682 @ 60.001 Hz` is available in several `DirectXPixelFormat`s. The profile
  pins `B8G8R8A8UIntNormalized` via `Y70tiDisplay.NativeMode`.
- Scaled modes exist where `src < 2560` and the RTK409A scaler upscales to `tgt=2560x682`
  (e.g. `src=1176x664 -> tgt=2560x682`). **Avoid** — always pick a 1:1 `src=2560x682` mode.
  `ModeSelectors.NativeOneToOne` enforces src==tgt==2560x682, so scaled modes are rejected.

## Orientation (LOCKED)

The framebuffer/scanout is **landscape 2560 wide x 682 tall**. The panel is physically mounted vertical,
so the renderer authors a logical **682w x 2560h** portrait canvas and applies one 90°/270° root
transform. **`--rot 90`, no flip = upright & correct** (user-confirmed on-panel 2026-07-03).
`Y70tiDisplay.DefaultRotation = 90`.

## Touch — ILITEK digitizer

- **Identity:** `VID_0x222A / PID_0x0001`, COL01. Device path:
  `\\?\hid#vid_222a&pid_0001&col01#a&1588b901&0&0000#{4d1e55b2-...}`. Manufacturer `ILITEK`, product
  `ILITEK-TOUCH`, `InputReportByteLength = 64`, single report ID = 4.
- **Axes:** X (`0x01/0x30`) and Y (`0x01/0x31`) both 16-bit logical `[0..9600]`. 10 finger collections
  (10-point). `ContactId 0x0D/0x51`, `ContactCount 0x0D/0x54` (at TLC 0), `TipSwitch 0x0D/0x42` present.

### Corner calibration (physical corner -> raw), from the 4 corner-holds

| Physical corner | raw X | raw Y |
|---|---|---|
| Top-left     | 9464 | 225  |
| Top-right    | 9377 | 9499 |
| Bottom-right | 274  | 9505 |
| Bottom-left  | 253  | 264  |

Consistent result: **physical vertical = raw X (top≈9464 → bottom≈253, INVERTED)**; **physical
horizontal = raw Y (left≈245 → right≈9499)**. Active bounds ≈ `rawX[253..9464]`, `rawY[245..9499]`.
The descriptor's physical-mm metadata is nominal — ignore it. These are exactly
`Y70tiTouch.CalibrationModel` (`HorizontalFromRawY=true`, `InvertVertical=true`).

Touch auto-follows the display `--rot/--flip`: the calibration delta is
`baseline(rot90) * inverse(display)`, computed by the same core `DisplayTransform` the session uses, so
there is no separate touch flag and the two cannot drift. rot90↔rot270 is an exact 180° relationship.

## TouchGate history

`HKLM\SOFTWARE\Microsoft\Wisp\Touch\TouchGate` was `1` (pre-existing, non-default). Our Raw Input read
does NOT depend on it. **Set `TouchGate = 0` on 2026-07-03** (elevated, user-approved UAC) to stop
touch→mouse promotion onto the BenQ desktop (a panel touch had nearly closed a terminal on the primary
display). Effective after the next sign-out/reboot. Revert by setting it back to `1` (or deleting the
value). Provisioning owns applying it; `TouchGate.EnsureDisabled` is best-effort otherwise.
