# Arrow Operator: low-poly sci-fi / proposed v1

Owner: Jeff. Based on the J-marked **STYLE GUIDE: low-poly sci-fi** note.
Everything below is a proposed implementation of that direction for team review.

| Role | Color | Form / additional cue |
| --- | --- | --- |
| Background | `#0B1220` | Dark navy, uncluttered |
| Environment | `#18263A` | Simple grid and large planar surfaces |
| Player / main text | `#EAF4FF` | High-contrast light silhouette |
| Speed pickup | `#43E5E0` | Cube; SPEED label and countdown |
| Direction pickup | `#63A7FF` | Diamond; explicit direction label (+X in demo) |
| Shield pickup / active player | `#B39AFF` | Orb; SHIELD countdown |
| Time pickup | `#FFD166` | Short cylinder; +10s label |
| Hazard | `#FF6473` | Solid wall with clearly readable boundary |

Use simple low-poly forms, flat surfaces, and limited detail. Distinguish effects by shape and
text as well as hue. Reserve the brightest colors for the player, objectives, and active effects.
Prefer flat or restrained emissive materials; avoid bloom that obscures collision boundaries.
The demo uses unlit colors to remain readable without extra lights or post-processing.

UI: plain sans-serif type, large timer, short status labels, visible remaining duration. Keep
instructions outside the arena. Use consistent spacing and ensure the selected menu option
can be identified by its shape or marker, not just color. Confirm legibility on the class display.

The sandbox includes procedural primitives and runtime materials as a first visual sample.
Final arrow model, environment assets, audio, and team-wide menu restyling are not supplied
by this prototype. The adjacent unassigned `assets?` note is not treated as a committed J task.
