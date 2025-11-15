# Pressure Emulation

Tool for consistent testing of pen pressure.

## Pressure Emulation:

Changes the tablet's pressure resolution.

**Pressure Resolution:** The pressure resolution to emulate (must be lower than the tablet's max).

**Ignore Zero Level:** Start pressure emulation range at the first applicable non-zero level. This can help ensure multiple pressure lines start at the same point and avoid looking like there is a higher initial activation force when emulating small pressure ranges.

## Pressure Emulation Line:

Changes the tablet's pressure resolution and automatically draws a line when pressure is detected.

**Pressure Resolution:** The pressure resolution to emulate (must be lower than the tablet's max).

**Line Length:** The length in pixels to draw the line.

**Line Offset:** The length in pixels to continue drawing after max pressure is reached.

**Pressure Deadzone:** Adds a pressure deadzone at the set pressure percent (match this value to your Tip Threshold in the Pen Settings tab).

**Continuous Mode:** Repeats the line drawing after applying the specified offsets and divisors.

**Continuous Mode X Offset:** The length in pixels to offset the line in the X axis every repeat.

**Continuous Mode Y Offset:** The length in pixels to offset the line in the Y axis every repeat.

**Continuous Mode Pressure Divisor:** The number to divide the pressure resolution by every repeat.

**Minimum Pressure Resolution:** The minimum pressure resolution to emulate.

**Ignore Zero Level:** Start pressure emulation range at the first applicable non-zero level. This can help ensure multiple pressure lines start at the same point and avoid looking like there is a higher initial activation force when emulating small pressure ranges.
