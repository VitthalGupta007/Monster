import os
import re
from pathlib import Path

ROOT = Path("D:/Extra/Monster")
SEARCH_DIRS = ["Assets/Common/Scripts", "Assets/Platform"]
ASSET_EXTS = {".cs", ".prefab", ".unity", ".asset", ".controller", ".playable", ".overrideController"}
SKIP_PARTS = ("Firebase", "GooglePlayGames", "ExternalDependencyManager", "TextMesh Pro", "Samples")


def should_skip(path_str: str) -> bool:
    return any(part in path_str for part in SKIP_PARTS)


all_files = []
for root, dirs, files in os.walk(ROOT / "Assets"):
    rp = root.replace("\\", "/")
    if should_skip(rp):
        dirs[:] = []
        continue
    for name in files:
        ext = os.path.splitext(name)[1].lower()
        if ext in ASSET_EXTS:
            all_files.append(Path(root) / name)

runtime_scripts = []
editor_scripts = []
for sd in SEARCH_DIRS:
    base = ROOT / sd
    if not base.exists():
        continue
    for p in base.rglob("*.cs"):
        rel = str(p.relative_to(ROOT)).replace("\\", "/")
        if "/Editor/" in rel:
            editor_scripts.append(rel)
        else:
            runtime_scripts.append(rel)


def extract_types(path: str):
    text = (ROOT / path).read_text(encoding="utf-8", errors="ignore")
    classes = re.findall(
        r"(?:public|internal|private|protected|abstract|sealed|static|partial|\s)*\s*class\s+(\w+)",
        text,
    )
    structs = re.findall(
        r"(?:public|internal|private|protected|abstract|sealed|static|partial|\s)*\s*struct\s+(\w+)",
        text,
    )
    enums = re.findall(
        r"(?:public|internal|private|protected|\s)*\s*enum\s+(\w+)",
        text,
    )
    return list(dict.fromkeys(classes + structs + enums)), text


def count_refs(name: str, self_path: str):
    pattern = re.compile(r"\b" + re.escape(name) + r"\b")
    cs_refs = []
    asset_refs = []
    for fp in all_files:
        rel = str(fp.relative_to(ROOT)).replace("\\", "/")
        if rel == self_path:
            continue
        try:
            content = fp.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        if not pattern.search(content):
            continue
        if fp.suffix.lower() == ".cs":
            cs_refs.append(rel)
        else:
            asset_refs.append(rel)
    return cs_refs, asset_refs


results = []
for rel in sorted(runtime_scripts):
    types, text = extract_types(rel)
    if not types:
        results.append({"path": rel, "class": None, "status": "NO_CLASS_FOUND"})
        continue
    primary = types[0]
    cs_refs, asset_refs = count_refs(primary, rel)
    total = len(cs_refs) + len(asset_refs)
    if total == 0:
        status = "ZERO_REFS"
    elif total <= 2:
        status = "LOW_REFS"
    else:
        status = "REFERENCED"
    results.append(
        {
            "path": rel,
            "class": primary,
            "all_types": types,
            "obsolete": "[Obsolete" in text,
            "cs_ref_count": len(cs_refs),
            "asset_ref_count": len(asset_refs),
            "cs_refs_sample": cs_refs[:8],
            "asset_refs_sample": asset_refs[:8],
            "status": status,
        }
    )

zero = [r for r in results if r.get("status") == "ZERO_REFS"]
low = [r for r in results if r.get("status") == "LOW_REFS"]
obs = [r for r in results if r.get("obsolete")]

print("RUNTIME_COUNT", len(runtime_scripts))
print("EDITOR_COUNT", len(editor_scripts))
print("ZERO_REFS", len(zero))
print("LOW_REFS", len(low))
print("OBSOLETE", len(obs))
print("---ZERO---")
for r in zero:
    print(
        f"{r['class']}|{r['path']}|cs={r['cs_ref_count']}|asset={r['asset_ref_count']}"
    )
print("---LOW---")
for r in low:
    print(
        f"{r['class']}|{r['path']}|cs={r['cs_ref_count']}|asset={r['asset_ref_count']}"
    )
    if r["cs_refs_sample"]:
        print("  cs:", ";".join(r["cs_refs_sample"][:3]))
    if r["asset_refs_sample"]:
        print("  asset:", ";".join(r["asset_refs_sample"][:3]))
print("---OBSOLETE---")
for r in obs:
    print(
        f"{r['class']}|{r['path']}|cs={r['cs_ref_count']}|asset={r['asset_ref_count']}"
    )
