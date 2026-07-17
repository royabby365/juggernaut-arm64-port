# Semantic fixes — ALL RESOLVED

As of the final pass, every auto-resolvable UnityScript/Unity-4.x semantic site has
been converted:
- 16× `GetComponent("typename")` → `GetComponent<TypeName>()`
- 36× bare UnityScript globals → `GetComponent<T>()`
- 2× `GetComponent("character")` → `GetComponent<character_parameters>()`
  (resolved 2026-07-17: `character` was the GameObject var; the adjacent code uses
  `character_parameters`, so that is the correct component type)

No semantic sites remain outstanding. Any further build errors will come from:
1. Unity 4.x→2021 API gaps the decompiler masked (e.g. WWW usage still present).
2. Asset reimport (original `assets/bin/Data/` not yet in the project).
3. Type-name casing mismatches Unity's compiler rejects (decompile artifacts).
