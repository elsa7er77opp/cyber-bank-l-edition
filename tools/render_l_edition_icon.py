from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
OUTPUT_DIRECTORY = ROOT / "src" / "CyberBank.LEdition.Desktop" / "Assets"
PNG_PATH = OUTPUT_DIRECTORY / "l-edition-icon-preview.png"
ICO_PATH = OUTPUT_DIRECTORY / "l-edition-icon.ico"


def scale(value: float, size: int) -> int:
    return round(value * size / 512)


def points(values: list[tuple[float, float]], size: int) -> list[tuple[int, int]]:
    return [(scale(x, size), scale(y, size)) for x, y in values]


def draw_icon(size: int) -> Image.Image:
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas)

    ink = (20, 36, 59, 255)
    bone = (245, 239, 227, 255)
    fold = (221, 229, 235, 255)
    vermilion = (217, 77, 51, 255)
    cobalt = (36, 88, 211, 255)

    draw.polygon(
        points([(84, 54), (316, 54), (432, 170), (432, 458), (84, 458)], size),
        fill=ink,
    )
    draw.polygon(
        points([(108, 78), (305, 78), (408, 181), (408, 434), (108, 434)], size),
        fill=bone,
    )
    draw.polygon(
        points([(305, 78), (408, 181), (305, 181)], size),
        fill=fold,
    )

    draw.line(
        points([(174, 136), (174, 346), (316, 346)], size),
        fill=ink,
        width=scale(29, size),
        joint="curve",
    )
    draw.line(
        points([(138, 122), (138, 378), (322, 378)], size),
        fill=vermilion,
        width=scale(17, size),
        joint="curve",
    )
    draw.line(
        points([(138, 96), (336, 96)], size),
        fill=vermilion,
        width=scale(8, size),
    )
    draw.line(
        points([(350, 202), (350, 314)], size),
        fill=cobalt,
        width=scale(14, size),
    )
    draw.rectangle(
        (scale(329, size), scale(332, size), scale(371, size), scale(374, size)),
        fill=cobalt,
    )
    draw.line(
        points([(350, 342), (350, 365)], size),
        fill=bone,
        width=scale(4, size),
    )

    return canvas


def main() -> None:
    OUTPUT_DIRECTORY.mkdir(parents=True, exist_ok=True)
    preview = draw_icon(512)
    preview.save(PNG_PATH)
    preview.save(
        ICO_PATH,
        sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)],
    )
    print(PNG_PATH)
    print(ICO_PATH)


if __name__ == "__main__":
    main()
