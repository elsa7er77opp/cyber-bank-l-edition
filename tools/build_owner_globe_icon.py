from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "src" / "CyberBank.LEdition.Desktop" / "Assets"
SOURCE = ASSETS / "l-edition-globe-l-clean-transparent.png"
DESTINATION = ASSETS / "l-edition-icon.ico"


def main() -> None:
    with Image.open(SOURCE) as source:
        icon = source.convert("RGBA")
        icon.save(
            DESTINATION,
            sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)],
        )
    print(DESTINATION)


if __name__ == "__main__":
    main()
