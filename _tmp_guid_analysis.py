import os
import re
from pathlib import Path

ROOT = Path("D:/Extra/Monster")
SEARCH_DIRS = ["Assets/Common/Scripts", "Assets/Platform"]
ASSET_EXTS = {".prefab", ".unity", ".asset", ".controller", ".playable", ".overrideController"}
SKIP_PARTS = ("Firebase", "GooglePlayGames", "ExternalDependencyManager", "TextMesh Pro", "Samples")


def rel(p: Path) -> str:
    return str(p.relative_to(ROOT)).replace("\\", "/")


def read_guid(meta_path: Path):
    if not meta_path.exists():
        return None
    text = meta_path.read_text(encoding="utf-8", errors="ignore")
    m = re.search(r"^guid:\s*([0-9a-f]+)", text, re.MULTILINE)
    return m.group(1) if m else None


# Index all asset files content once
asset_files = []
for root, dirs, files in os.walk(ROOT / "Assets"):
    rp = root.replace("\\", "/")
    if any(x in rp for x in SKIP_PARTS):
        dirs[:] = []
        continue
    for name in files:
        ext = os.path.splitext(name)[1].lower()
        if ext in ASSET_EXTS:
            asset_files.append(Path(root) / name)

runtime_scripts = []
for sd in SEARCH_DIRS:
    base = ROOT / sd
    if not base.exists():
        continue
    for p in base.rglob("*.cs"):
        r = rel(p)
        if "/Editor/" in r:
            continue
        runtime_scripts.append(r)

# Prioritize zero-ref candidates from first pass
ZERO_CANDIDATES = [
    "Assets/Common/Scripts/UI/SafeAreaManager.cs",
    "Assets/Common/Scripts/UI/CustomScrollView.cs",
    "Assets/Common/Scripts/UI/CurrencyScreenIncicatorBehavior.cs",
    "Assets/Common/Scripts/UI/ScalerHelper.cs",
    "Assets/Common/Scripts/UI/UITimer.cs",
    "Assets/Common/Scripts/Player/CharacterBehavior.cs",
    "Assets/Common/Scripts/Player/FollowPlayerBehavior.cs",
    "Assets/Common/Scripts/VXMonster/Gameplay/AndroidQualityBootstrap.cs",
    "Assets/Common/Scripts/VXMonster/UI/VxMetaMenuRow.cs",
    "Assets/Common/Scripts/VXMonster/UI/VxMinTouchTarget.cs",
    "Assets/Common/Scripts/VXMonster/UI/VxSafeAreaPadding.cs",
    "Assets/Common/Scripts/Upgrades/UpgradeTierLimits.cs",
    "Assets/Common/Scripts/Wave/Camera/CameraTrack.cs",
    "Assets/Common/Scripts/Wave/Rush Wave/ContinuousRushWave.cs",
    "Assets/Common/Scripts/Wave/Rush Wave/MaintainRushWave.cs",
    "Assets/Platform/iOS/IOSAdServiceStub.cs",
    "Assets/Platform/iOS/IOSAttConsentStub.cs",
    "Assets/Platform/iOS/IOSGameCenterStub.cs",
    "Assets/Common/Scripts/Drop/Gem/BombDropBehavior.cs",
    "Assets/Common/Scripts/Drop/Gem/ChestDropBehavior.cs",
    "Assets/Common/Scripts/Drop/Gem/CoinDropBehavior.cs",
    "Assets/Common/Scripts/Drop/Gem/FoodDropBehavior.cs",
    "Assets/Common/Scripts/Drop/Gem/GemBehavior.cs",
    "Assets/Common/Scripts/Drop/Gem/MagnetDropBehavior.cs",
    "Assets/Common/Scripts/Enemies/EnemyBellBehavior.cs",
    "Assets/Common/Scripts/Abilities/Behaviors/Active Abilities/FireBall/FireballAbilityBehavior.cs",
]

print("GUID_ASSET_REFERENCES")
for script_rel in runtime_scripts:
    guid = read_guid(ROOT / (script_rel + ".meta"))
    if not guid:
        continue
    hits = []
    for af in asset_files:
        try:
            content = af.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        if guid in content:
            hits.append(rel(af))
    if hits:
        cls = Path(script_rel).stem
        print(f"{cls}|{script_rel}|guid={guid}|asset_hits={len(hits)}")
        for h in hits[:5]:
            print(f"  {h}")
        if len(hits) > 5:
            print(f"  ... +{len(hits)-5} more")

print("\nZERO_CANDIDATE_GUID_CHECK")
for script_rel in ZERO_CANDIDATES:
    guid = read_guid(ROOT / (script_rel + ".meta"))
    hits = []
    if guid:
        for af in asset_files:
            try:
                content = af.read_text(encoding="utf-8", errors="ignore")
            except OSError:
                continue
            if guid in content:
                hits.append(rel(af))
    cls = Path(script_rel).stem
    print(f"{cls}|guid={guid}|asset_hits={len(hits)}|samples={';'.join(hits[:3])}")

# Ability prefab wiring check
print("\nABILITY_PREFAB_WIRING")
ability_data_dir = ROOT / "Assets/Common/Scriptables/Abilities"
if ability_data_dir.exists():
    for asset in ability_data_dir.rglob("*.asset"):
        text = asset.read_text(encoding="utf-8", errors="ignore")
        if "prefab:" in text:
            print(rel(asset))
