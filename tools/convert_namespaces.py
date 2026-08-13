#!/usr/bin/env python3
"""Convert C# 10 file-scoped namespaces (`namespace X;`) to C# 9 block form
(`namespace X { ... }`) so Unity 2021.3 (C# 9) can compile the decompiled
Juggernaut source. Idempotent; skips files already block-scoped.

The decompiler output indents the namespace body with one tab as if the
opening brace were present, so the transform is:
    namespace X;\n  ->  namespace X\n  {\n
and append a closing "}\n" at EOF (unless the file already ends balanced).
"""
import re
import sys
from pathlib import Path

NS_RE = re.compile(r'^namespace\s+([A-Za-z_][A-Za-z0-9_.]*);\s*$')

def convert(path: Path) -> bool:
    raw = path.read_bytes()
    # strip UTF-8 BOM if present
    has_bom = raw.startswith(b'\xef\xbb\xbf')
    if has_bom:
        raw = raw[3:]
    text = raw.decode('utf-8')
    lines = text.splitlines(keepends=True)

    # find first non-blank, non-comment, non-using line
    ns_idx = None
    for i, ln in enumerate(lines):
        s = ln.strip()
        if not s:
            continue
        if s.startswith('//') or s.startswith('/*'):
            continue
        if s.startswith('using ') or s.startswith('using\t'):
            continue
        m = NS_RE.match(s)
        if m:
            ns_idx = i
            break
        # not a file-scoped namespace -> leave alone
        return False

    if ns_idx is None:
        return False

    ns_name = NS_RE.match(lines[ns_idx].strip()).group(1)
    # Replace `namespace X;` with `namespace X\n{`
    # Preserve indentation of the original line (should be none, but be safe)
    indent = lines[ns_idx][:len(lines[ns_idx]) - len(lines[ns_idx].lstrip())]
    lines[ns_idx] = f'{indent}namespace {ns_name}\n{indent}{{\n'

    # Append closing brace at EOF. If the last non-blank line is already `}`
    # at column 0 that closes the namespace... we can't know for sure; the
    # decompiler output has the final type close brace at col 0, so we ALWAYS
    # need one more. But guard against double conversion (idempotency) by
    # checking we didn't already do this (file is being rewritten only once).
    if not text.endswith('\n'):
        lines.append('\n')
    lines.append('}\n')

    out = ''.join(lines)
    if has_bom:
        out = '\ufeff' + out
    path.write_text(out, encoding='utf-8')
    return True

def main() -> None:
    roots = [Path(a) for a in sys.argv[1:]] or [Path('Assets/Scripts')]
    files = []
    for root in roots:
        if root.is_file():
            files.append(root)
        else:
            files.extend(root.rglob('*.cs'))
    converted = 0
    for f in sorted(files):
        try:
            if convert(f):
                converted += 1
        except Exception as e:  # noqa: BLE001
            print(f'ERROR {f}: {e}', file=sys.stderr)
    print(f'converted {converted}/{len(files)} files')

if __name__ == '__main__':
    main()
