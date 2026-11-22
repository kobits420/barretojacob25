import AVFoundation
import SwiftUI
import UIKit

struct TrialPicture: View {
    let videoName: String
    let stillName: String
    let descriptionText: String

    @Environment(\.accessibilityReduceMotion) private var reduceMotion
    @State private var player: AVPlayer?
    @State private var showVideo = false

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            ZStack {
                Theme.paper
                stillView
                if showVideo, let player {
                    PlayerLayerView(player: player)
                }
            }
            .aspectRatio(16.0 / 9.0, contentMode: .fit)
            .frame(maxWidth: .infinity)
            .clipped()
            .overlay(Rectangle().stroke(Theme.line, lineWidth: 1))
            .accessibilityElement(children: .ignore)
            .accessibilityLabel(descriptionText)

            if player != nil {
                Button("Replay picture", action: replay)
                    .buttonStyle(.plain)
                    .font(.caption.weight(.semibold))
                    .textCase(.uppercase)
                    .tracking(1.1)
                    .foregroundStyle(Theme.brassDeep)
            }
        }
        .onAppear(perform: prepare)
        .onChange(of: videoName) { _, _ in
            prepare()
        }
        .onDisappear {
            player?.pause()
        }
    }

    @ViewBuilder
    private var stillView: some View {
        if let image = Self.loadImage(named: stillName) {
            Image(uiImage: image)
                .resizable()
                .scaledToFit()
        } else {
            VStack(spacing: 8) {
                TongMark()
                Text("Picture unavailable")
                    .font(.caption)
                    .foregroundStyle(Theme.brassDeep)
            }
            .frame(maxWidth: .infinity, maxHeight: .infinity)
        }
    }

    private func prepare() {
        player?.pause()
        showVideo = false
        guard let url = Self.bundleURL(name: videoName, ext: "mp4") else {
            player = nil
            return
        }
        let next = AVPlayer(url: url)
        next.isMuted = true
        next.actionAtItemEnd = .pause
        player = next
        if !reduceMotion {
            showVideo = true
            play(next)
        }
    }

    private func replay() {
        guard let player else { return }
        showVideo = true
        play(player)
    }

    private func play(_ player: AVPlayer) {
        DispatchQueue.main.async {
            player.seek(to: .zero)
            player.play()
        }
    }

    private static func bundleURL(name: String, ext: String) -> URL? {
        Bundle.main.url(forResource: name, withExtension: ext)
    }

    private static func loadImage(named name: String) -> UIImage? {
        guard let url = bundleURL(name: name, ext: "png"),
              let data = try? Data(contentsOf: url) else {
            return nil
        }
        return UIImage(data: data)
    }
}

private struct PlayerLayerView: UIViewRepresentable {
    let player: AVPlayer

    func makeUIView(context: Context) -> PlayerHost {
        let view = PlayerHost()
        view.playerLayer.player = player
        return view
    }

    func updateUIView(_ uiView: PlayerHost, context: Context) {
        if uiView.playerLayer.player !== player {
            uiView.playerLayer.player = player
        }
    }
}

private final class PlayerHost: UIView {
    override static var layerClass: AnyClass { AVPlayerLayer.self }

    var playerLayer: AVPlayerLayer { layer as! AVPlayerLayer }

    override init(frame: CGRect) {
        super.init(frame: frame)
        backgroundColor = .clear
        isOpaque = false
        playerLayer.videoGravity = .resizeAspect
        playerLayer.backgroundColor = UIColor.clear.cgColor
    }

    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
}
