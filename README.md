# Tem Tong

Tem Tong is a short iOS puzzle. One question is on screen at a time. The first ones are plain. The later ones turn on a careful reading. After every answer, the app says why.

There are ten hand-written questions. The pictures were drawn for this game and rendered with [Manim Community](https://github.com/ManimCommunity/manim) (MIT), then bundled in the app. No stock images and no generated art.

SwapTube was not used. That renderer needs a CUDA or HIP GPU, and the machine that made these pictures does not have one.

## Run it in the Simulator

You need a Mac with Xcode 15 or newer. This project was prepared on a machine without Xcode, so the Simulator was not launched here.

Open the project:

```bash
open TemTong.xcodeproj
```

Choose an iPhone simulator, such as iPhone 16, then press Run (⌘R).

The first time, Xcode may ask you to pick a development team under Signing & Capabilities. A free personal team is enough for the Simulator. The app is not set up for the App Store, and it should not be submitted there.

From the command line, pick a simulator name from `xcrun simctl list devices available`, then:

```bash
xcodebuild \
  -scheme TemTong \
  -destination 'platform=iOS Simulator,name=iPhone 16' \
  -configuration Debug \
  build
```

## Render the pictures again

Install [Manim Community](https://github.com/ManimCommunity/manim) and ffmpeg, then from this directory:

```bash
./animations/render.sh
```

The scene source is `animations/trials.py`. The script writes the videos and stills into `TemTong/Media/`. Those files are already in the app, so running Tem Tong does not require Manim.
