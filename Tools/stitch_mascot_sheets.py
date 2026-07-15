"""Stitch VX mascot walk/defeat/revive sheets with shared ground + scale."""
from __future__ import annotations

from collections import deque
from pathlib import Path

from PIL import Image

ROOT = Path(r"D:\Extra\Monster\Assets\Common\Sprites\Characters\Wizard")
CELL = 512
GROUND = CELL - 20  # opaque feet sit on this Y (exclusive bottom pad)
TARGET_H = 420  # normalize character height into cell


def punch_black(im: Image.Image, thresh: int = 36) -> Image.Image:
    im = im.convert("RGBA")
    px = im.load()
    w, h = im.size
    visited = [[False] * w for _ in range(h)]
    q = deque()
    for x, y in ((0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)):
        q.append((x, y))
        visited[y][x] = True
    while q:
        x, y = q.popleft()
        r, g, b, a = px[x, y]
        if r <= thresh and g <= thresh and b <= thresh and a > 0:
            px[x, y] = (0, 0, 0, 0)
            for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                if 0 <= nx < w and 0 <= ny < h and not visited[ny][nx]:
                    visited[ny][nx] = True
                    nr, ng, nb, na = px[nx][ny] if False else px[nx, ny]
                    if nr <= thresh and ng <= thresh and nb <= thresh and na > 0:
                        q.append((nx, ny))
    return im


def fit_cell(im: Image.Image, size: int = CELL, ground: int = GROUND, target_h: int = TARGET_H) -> Image.Image:
    im = punch_black(im)
    bbox = im.getbbox()
    if not bbox:
        return Image.new("RGBA", (size, size), (0, 0, 0, 0))
    im = im.crop(bbox)
    # scale by height so all frames share similar vertical size
    scale = target_h / im.height
    # keep within cell width with padding
    max_w = size - 40
    if int(im.width * scale) > max_w:
        scale = max_w / im.width
    nw = max(1, int(im.width * scale))
    nh = max(1, int(im.height * scale))
    im = im.resize((nw, nh), Image.Resampling.LANCZOS)
    cell = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    x = (size - nw) // 2
    y = ground - nh
    if y < 8:
        y = 8
    cell.paste(im, (x, y), im)
    return cell


def stitch(frames: list[Path], out: Path, n_expected: int | None = None) -> Image.Image:
    cells = [fit_cell(Image.open(p)) for p in frames]
    if n_expected is not None and len(cells) != n_expected:
        raise SystemExit(f"{out.name}: expected {n_expected} frames, got {len(cells)}")
    sheet = Image.new("RGBA", (CELL * len(cells), CELL), (0, 0, 0, 0))
    bottoms = []
    heights = []
    for i, c in enumerate(cells):
        sheet.paste(c, (i * CELL, 0), c)
        bb = c.getbbox()
        if bb:
            bottoms.append(bb[3])
            heights.append(bb[3] - bb[1])
    sheet.save(out)
    print(f"{out.name}: {sheet.size} bottoms={bottoms} heights={heights}")
    if bottoms and (max(bottoms) - min(bottoms) > 24):
        print(f"  WARN ground drift {max(bottoms) - min(bottoms)}px")
    if heights and (max(heights) - min(heights) > 80):
        print(f"  WARN height drift {max(heights) - min(heights)}px")
    return sheet


def punch_and_fit_raw(im: Image.Image, target_h: int = TARGET_H) -> Image.Image:
    im = punch_black(im)
    bbox = im.getbbox()
    if not bbox:
        raise SystemExit("empty sprite after punch")
    im = im.crop(bbox)
    scale = target_h / im.height
    max_w = CELL - 48
    if int(im.width * scale) > max_w:
        scale = max_w / im.width
    nw = max(1, int(im.width * scale))
    nh = max(1, int(im.height * scale))
    return im.resize((nw, nh), Image.Resampling.LANCZOS)


def stitch_calm_walk_from_base(base_path: Path, out: Path) -> None:
    """Upright squash-stretch bob (original walk language) — no sprint lean / airborne."""
    base = punch_and_fit_raw(Image.open(base_path))
    # ~3% height variance, tiny sway — feet always on GROUND
    keys = [
        (1.000, 1.000, 0),
        (1.012, 0.985, -2),
        (1.025, 0.970, 0),
        (1.012, 0.985, 2),
        (1.000, 1.000, 0),
    ]
    sheet = Image.new("RGBA", (CELL * 5, CELL), (0, 0, 0, 0))
    bottoms, heights = [], []
    for i, (sx, sy, dx) in enumerate(keys):
        nw = max(1, int(base.width * sx))
        nh = max(1, int(base.height * sy))
        fr = base.resize((nw, nh), Image.Resampling.LANCZOS)
        cell = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
        cell.paste(fr, ((CELL - nw) // 2 + dx, GROUND - nh), fr)
        sheet.paste(cell, (i * CELL, 0), cell)
        bb = cell.getbbox()
        if bb:
            bottoms.append(bb[3])
            heights.append(bb[3] - bb[1])
    sheet.save(out)
    print(f"{out.name}: {sheet.size} bottoms={bottoms} heights={heights}")


def stitch_legged_walk(frames: list[Path], out: Path) -> None:
    """Real leg cycle, shared body scale + locked ground (no floating / size-pop)."""
    punched = []
    for p in frames:
        im = punch_black(Image.open(p))
        bb = im.getbbox()
        if not bb:
            raise SystemExit(f"empty {p}")
        punched.append(im.crop(bb))

    # Same vertical scale for all: fit tallest frame into TARGET_H
    max_h = max(im.height for im in punched)
    scale = TARGET_H / max_h
    max_w = CELL - 40
    widest = max(im.width for im in punched)
    if widest * scale > max_w:
        scale = max_w / widest

    sheet = Image.new("RGBA", (CELL * len(punched), CELL), (0, 0, 0, 0))
    bottoms, heights, cens = [], [], []
    for i, im in enumerate(punched):
        nw = max(1, int(im.width * scale))
        nh = max(1, int(im.height * scale))
        fr = im.resize((nw, nh), Image.Resampling.LANCZOS)
        cell = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
        x = (CELL - nw) // 2
        y = GROUND - nh
        if y < 8:
            y = 8
        cell.paste(fr, (x, y), fr)
        sheet.paste(cell, (i * CELL, 0), cell)
        bb = cell.getbbox()
        if bb:
            bottoms.append(bb[3])
            heights.append(bb[3] - bb[1])
            # centroid y for jump diagnostics
            px = cell.load()
            sy = n = 0
            for yy in range(bb[1], bb[3]):
                for xx in range(bb[0], bb[2]):
                    if px[xx, yy][3] > 20:
                        sy += yy
                        n += 1
            cens.append(sy / n if n else 0)
    sheet.save(out)
    print(f"{out.name}: {sheet.size} bottoms={bottoms} heights={heights}")
    print(f"  centroids_y={[round(c,1) for c in cens]} drift={max(cens)-min(cens):.1f}")
    if bottoms and max(bottoms) - min(bottoms) > 8:
        print(f"  WARN ground drift {max(bottoms)-min(bottoms)}px")


def main() -> None:
    defeat_frames = [ROOT / f"_d{i}.png" for i in range(5)]
    walk_frames = [ROOT / f"_w{i}.png" for i in range(5)]
    for p in defeat_frames:
        if not p.exists():
            raise SystemExit(f"missing {p}")

    # Prefer posed leg frames; fall back to calm bob only if missing
    if all(p.exists() for p in walk_frames):
        stitch_legged_walk(walk_frames, ROOT / "wizard_walk_new.png")
    else:
        walk_base = ROOT / "_walk_base.png"
        if not walk_base.exists():
            raise SystemExit("missing walk frames and _walk_base.png")
        stitch_calm_walk_from_base(walk_base, ROOT / "wizard_walk_new.png")

    stitch(defeat_frames, ROOT / "wizard_defeat.png", 5)

    # Revive = reverse of defeat (dead → alive)
    revive_frames = list(reversed(defeat_frames))
    stitch(revive_frames, ROOT / "wizard_revive.png", 5)

    # Idle from upright base if present
    idle_src = ROOT / "_walk_base.png"
    if not idle_src.exists():
        idle_src = walk_frames[0]
    idle_base = fit_cell(Image.open(idle_src))
    idle = Image.new("RGBA", (CELL * 4, CELL), (0, 0, 0, 0))
    for i, dy in enumerate((0, -6, 0, 6)):
        frame = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
        frame.paste(idle_base, (0, dy), idle_base)
        idle.paste(frame, (i * CELL, 0), frame)
    idle_path = ROOT / "wizard_idle.png"
    idle.save(idle_path)
    print(f"{idle_path.name}: {idle.size}")


if __name__ == "__main__":
    main()
