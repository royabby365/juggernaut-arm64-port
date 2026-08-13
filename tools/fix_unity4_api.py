#!/usr/bin/env python3
"""Fix decompiler artifacts in the Juggernaut Unity 4 -> 2021 port:

1. `GetComponent<X>()ident` -> `GetComponent<X>().ident` (missing dot)
   e.g. GetComponent<Renderer>()materials.Length -> GetComponent<Renderer>().materials.Length
2. Legacy Unity 4 component quick-accessors removed in Unity 5+/2021:
       .renderer   -> .GetComponent<Renderer>()
       .camera     -> .GetComponent<Camera>()
       .animation  -> .GetComponent<Animation>()
   Applied to member access expressions (not type names).
"""
import re
import sys
from pathlib import Path

GETCOMP_DOT = re.compile(
    r'GetComponent<([A-Za-z_][\w.]*)>\(\)([A-Za-z_]\w*)',
)
# legacy accessor -> replacement component type
LEGACY = {
    'renderer': 'Renderer',
    'camera': 'Camera',
    'animation': 'Animation',
}
# receiver of member access: allow identifier chains (a.b.c) — the accessor
# applies to the last segment. We match `.renderer` preceded by an ident chain.
LEGACY_RE = re.compile(r'([A-Za-z_][\w.]*?)\.(renderer|camera|animation)\b')

def fix(text: str) -> str:
    out = GETCOMP_DOT.sub(r'GetComponent<\1>().\2', text)

    def legacy_sub(m: re.Match) -> str:
        receiver, accessor = m.group(1), m.group(2)
        comp_type = LEGACY[accessor]
        return f'{receiver}.GetComponent<{comp_type}>()'

    out = LEGACY_RE.sub(legacy_sub, out)
    return out

def main() -> None:
    roots = [Path(a) for a in sys.argv[1:]] or [Path('Assets/Scripts')]
    files = []
    for root in roots:
        if root.is_file():
            files.append(root)
        else:
            files.extend(root.rglob('*.cs'))
    changed = 0
    for f in sorted(files):
        raw = f.read_bytes()
        has_bom = raw.startswith(b'\xef\xbb\xbf')
        if has_bom:
            raw = raw[3:]
        text = raw.decode('utf-8')
        new = fix(text)
        if new != text:
            out = ('\ufeff' if has_bom else '') + new
            f.write_bytes(out.encode('utf-8'))
            changed += 1
    print(f'fixed {changed}/{len(files)} files')

if __name__ == '__main__':
    main()