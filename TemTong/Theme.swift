import SwiftUI

enum Theme {
    static let ink = Color(red: 0.110, green: 0.086, blue: 0.071)
    static let paper = Color(red: 0.957, green: 0.937, blue: 0.894)
    static let card = Color(red: 0.937, green: 0.902, blue: 0.831)
    static let surround = Color(red: 0.102, green: 0.078, blue: 0.063)
    static let brass = Color(red: 0.663, green: 0.471, blue: 0.196)
    static let brassDeep = Color(red: 0.431, green: 0.306, blue: 0.157)
    static let brassBright = Color(red: 0.843, green: 0.710, blue: 0.416)
    static let seal = Color(red: 0.553, green: 0.188, blue: 0.188)
    static let fair = Color(red: 0.122, green: 0.361, blue: 0.290)
    static let fairWash = Color(red: 0.894, green: 0.941, blue: 0.918)
    static let missWash = Color(red: 0.973, green: 0.902, blue: 0.886)
    static let line = Color.black.opacity(0.16)

    static let credit = "Pictures rendered with Manim Community (MIT). No stock images."
}

struct TongMark: View {
    var body: some View {
        ZStack {
            Circle().stroke(Theme.brass, lineWidth: 2)
            Capsule().fill(Theme.brass).frame(width: 28, height: 4)
        }
        .frame(width: 52, height: 52)
        .accessibilityHidden(true)
    }
}

struct RisingBars: View {
    private let heights: [CGFloat] = [28, 48, 64, 36, 56]

    var body: some View {
        HStack(alignment: .bottom, spacing: 8) {
            ForEach(Array(heights.enumerated()), id: \.offset) { _, height in
                Rectangle()
                    .fill(Theme.brass)
                    .frame(width: 14, height: height)
            }
        }
        .accessibilityHidden(true)
        .padding(.vertical, 8)
    }
}

struct InkButton: View {
    let title: String
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            Text(title)
                .font(.body.weight(.semibold))
                .foregroundStyle(Theme.paper)
                .frame(maxWidth: .infinity, minHeight: 52)
                .background(Theme.ink)
        }
        .buttonStyle(.plain)
        .padding(.top, 8)
    }
}
