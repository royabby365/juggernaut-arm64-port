# Remaining semantic fixes (2 sites — requires human/Editor decision)

These could NOT be auto-resolved because no matching type exists in the project.
`character` here is the GameObject variable, not a component type.

## GetComponent("character") — 2 sites
UnityScript form `GetComponent("character")` on a GameObject. In modern Unity this is
likely `GetComponent<CharacterParameters>()` (the component on that GameObject) or the
script's own MonoBehaviour. Resolve by checking what `character` GameObject actually holds.

- Assembly-UnityScript/assets.cs:137
- Assembly-UnityScript/assets.cs:145
