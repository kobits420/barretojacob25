import SwiftUI

struct ContentView: View {
    @StateObject private var session = GameSession()

    var body: some View {
        ZStack {
            Theme.surround.ignoresSafeArea()
            ScrollView {
                VStack(alignment: .leading, spacing: 0) {
                    VStack(alignment: .leading, spacing: 16) {
                        switch session.phase {
                        case .start:
                            StartView { session.begin() }
                        case .question:
                            QuestionView(session: session)
                        case .result:
                            ResultView(session: session)
                        }
                    }
                    .padding(22)
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .background(Theme.paper)
                    .foregroundStyle(Theme.ink)
                    .padding(6)
                    .background(Theme.brass.opacity(0.32))

                    Text(Theme.credit)
                        .font(.caption2)
                        .foregroundStyle(Theme.brassBright)
                        .multilineTextAlignment(.center)
                        .frame(maxWidth: .infinity)
                        .padding(.top, 12)
                }
            }
            .frame(maxWidth: 640)
            .frame(maxWidth: .infinity)
            .padding(.horizontal, 16)
            .padding(.vertical, 12)
        }
        .preferredColorScheme(.dark)
    }
}

private struct StartView: View {
    let begin: () -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: 14) {
            TongMark()
            Text("Tem Tong")
                .font(.system(.largeTitle, design: .serif))
                .accessibilityAddTraits(.isHeader)
            Text("Ten questions, one at a time. The first are plain. The later ones depend on a careful reading. After you answer, you see why.")
                .font(.body)
                .fixedSize(horizontal: false, vertical: true)
            RisingBars()
            InkButton(title: "Begin", action: begin)
        }
    }
}

private struct QuestionView: View {
    @ObservedObject var session: GameSession

    var body: some View {
        let trial = session.trial
        VStack(alignment: .leading, spacing: 14) {
            HStack(alignment: .firstTextBaseline) {
                Text("Tem Tong")
                    .font(.subheadline.weight(.semibold))
                    .tracking(1.4)
                    .textCase(.uppercase)
                Spacer()
                Text("\(session.score) correct")
                    .font(.caption.weight(.semibold))
                    .tracking(0.8)
                    .textCase(.uppercase)
                    .foregroundStyle(Theme.brassDeep)
            }
            Text("Question \(session.index + 1) of \(Trial.opening.count)  ·  \(trial.title)")
                .font(.caption.weight(.semibold))
                .tracking(0.8)
                .textCase(.uppercase)
                .foregroundStyle(Theme.brassDeep)
            progress
            TrialPicture(
                videoName: session.pictureVideoName,
                stillName: session.pictureStillName,
                descriptionText: session.pictureDescription
            )
            .id(session.pictureVideoName + session.pictureStillName)
            if let setup = trial.setup {
                Text(setup)
                    .font(.body)
                    .fixedSize(horizontal: false, vertical: true)
            }
            if !trial.notes.isEmpty {
                VStack(alignment: .leading, spacing: 6) {
                    ForEach(trial.notes, id: \.self) { note in
                        Text(note)
                            .font(.body)
                            .fixedSize(horizontal: false, vertical: true)
                    }
                }
            }
            Text(trial.question)
                .font(.system(.title3, design: .serif))
                .fixedSize(horizontal: false, vertical: true)
                .accessibilityAddTraits(.isHeader)
            LazyVGrid(
                columns: [GridItem(.adaptive(minimum: 200), spacing: 12)],
                spacing: 12
            ) {
                ForEach(trial.choices) { choice in
                    choiceButton(choice)
                }
            }
            if session.hasAnswered {
                feedback
            }
        }
    }

    private var progress: some View {
        HStack(spacing: 5) {
            ForEach(0..<Trial.opening.count, id: \.self) { slot in
                Rectangle()
                    .fill(pipFill(slot))
                    .overlay(Rectangle().stroke(pipStroke(slot), lineWidth: 1))
                    .frame(height: 8)
            }
        }
        .accessibilityHidden(true)
    }

    private func pipFill(_ slot: Int) -> Color {
        if slot < session.index || (slot == session.index && session.hasAnswered) {
            return Theme.brass
        }
        if slot == session.index {
            return Theme.ink
        }
        return Color.clear
    }

    private func pipStroke(_ slot: Int) -> Color {
        if slot < session.index || (slot == session.index && session.hasAnswered) {
            return Theme.brass
        }
        if slot == session.index {
            return Theme.ink
        }
        return Theme.brass
    }

    private func choiceButton(_ choice: Choice) -> some View {
        let showCorrect = session.hasAnswered && choice.isCorrect
        let showPick = session.hasAnswered && choice.id == session.pickedID && !choice.isCorrect
        return Button {
            session.choose(choice)
        } label: {
            HStack(alignment: .center, spacing: 12) {
                Text(choice.label)
                    .font(.body)
                    .multilineTextAlignment(.leading)
                    .fixedSize(horizontal: false, vertical: true)
                Spacer(minLength: 8)
                if showCorrect {
                    Text("Correct")
                        .font(.caption2.weight(.semibold))
                        .textCase(.uppercase)
                        .tracking(0.6)
                } else if showPick {
                    Text("Your pick")
                        .font(.caption2.weight(.semibold))
                        .textCase(.uppercase)
                        .tracking(0.6)
                }
            }
            .padding(.horizontal, 14)
            .padding(.vertical, 12)
            .frame(maxWidth: .infinity, minHeight: 52, alignment: .leading)
            .background(showCorrect ? Theme.fairWash : (showPick ? Theme.missWash : Theme.card))
            .overlay(
                Rectangle().stroke(
                    showCorrect ? Theme.fair : (showPick ? Theme.seal : Theme.ink),
                    lineWidth: 1.5
                )
            )
            .foregroundStyle(showCorrect ? Theme.fair : Theme.ink)
        }
        .buttonStyle(.plain)
        .allowsHitTesting(!session.hasAnswered)
        .accessibilityAddTraits(showCorrect ? .isSelected : [])
    }

    private var feedback: some View {
        VStack(alignment: .leading, spacing: 8) {
            Text(session.pickedIsCorrect ? "That holds." : "Not this time.")
                .font(.system(.title3, design: .serif))
                .foregroundStyle(session.pickedIsCorrect ? Theme.fair : Theme.seal)
            Text(session.trial.explanation)
                .font(.body)
                .fixedSize(horizontal: false, vertical: true)
            InkButton(
                title: session.isLast ? "See the record" : "Next question",
                action: { session.advance() }
            )
        }
        .padding(.top, 4)
        .accessibilityElement(children: .contain)
    }
}

private struct ResultView: View {
    @ObservedObject var session: GameSession

    var body: some View {
        let verdict = Verdict.make(score: session.score)
        VStack(alignment: .leading, spacing: 12) {
            Text("Tem Tong")
                .font(.subheadline.weight(.semibold))
                .tracking(1.4)
                .textCase(.uppercase)
            Text("Record")
                .font(.caption.weight(.semibold))
                .tracking(0.8)
                .textCase(.uppercase)
                .foregroundStyle(Theme.brassDeep)
            Text("\(session.score) / \(Trial.opening.count)")
                .font(.system(.title, design: .serif))
            Text(verdict.name)
                .font(.system(.largeTitle, design: .serif))
                .accessibilityAddTraits(.isHeader)
            Text(verdict.detail)
                .font(.body)
                .fixedSize(horizontal: false, vertical: true)
            InkButton(title: "Start again", action: session.begin)
        }
    }
}

#Preview {
    ContentView()
}
