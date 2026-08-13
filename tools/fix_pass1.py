#!/usr/bin/env python3
"""Batch fix decompiler artifacts for Unity 2021.3 compile.

Pass 1 (mechanical, safe):
  1. CS0617: strip `, IsRequired = ...` from [ProtoMember(N, ...)] attrs
  2. CS0104: fully-qualify `Tuple<...>` -> `System.Tuple<...>` when Yuval.Collections in scope
"""
import re
import sys
from pathlib import Path

ROOT = Path('/home/rabby/juggernaut-arm64-port')

ISREQ_RE = re.compile(r'(\[ProtoMember\(\s*\d+\s*,\s*)IsRequired\s*=\s*(?:true|false)\s*\)')
TUPLE_RE = re.compile(r'\bTuple<')

def fix_file(path: Path) -> bool:
    text = path.read_text(encoding='utf-8')
    orig = text
    text = ISREQ_RE.sub(r'\1)', text)
    text = TUPLE_RE.sub('System.Tuple<', text)
    return text != orig

def main() -> None:
    files = sorted(ROOT.rglob('*.cs'))
    changed = 0
    for f in files:
        if fix_file(f):
            changed += 1
            print(f'fixed {f.relative_to(ROOT)}')
    print(f'{changed}/{len(files)} files changed (pass 1)')

if __name__ == '__main__':
    main()