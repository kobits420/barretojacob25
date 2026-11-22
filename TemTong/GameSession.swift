import Combine
import Foundation

@MainActor
final class GameSession: ObservableObject {
    enum Phase {
        case start
        case question
        case result
    }

    @Published private(set) var phase: Phase = .start
    @Published private(set) var index = 0
    @Published private(set) var score = 0
    @Published private(set) var hasAnswered = false
    @Published private(set) var pickedID: String?

    var trial: Trial { Trial.opening[index] }
    var isLast: Bool { index == Trial.opening.count - 1 }

    var pickedIsCorrect: Bool {
        trial.choices.first { $0.id == pickedID }?.isCorrect ?? false
    }

    var pictureVideoName: String {
        if hasAnswered, let name = trial.revealVideoName { return name }
        return trial.videoName
    }

    var pictureStillName: String {
        if hasAnswered, let name = trial.revealStillName { return name }
        return trial.stillName
    }

    var pictureDescription: String {
        if hasAnswered, let text = trial.revealDescription { return text }
        return trial.stillDescription
    }

    func begin() {
        index = 0
        score = 0
        hasAnswered = false
        pickedID = nil
        phase = .question
    }

    func choose(_ choice: Choice) {
        guard phase == .question, !hasAnswered else { return }
        hasAnswered = true
        pickedID = choice.id
        if choice.isCorrect {
            score += 1
        }
    }

    func advance() {
        guard phase == .question, hasAnswered else { return }
        if isLast {
            phase = .result
        } else {
            index += 1
            hasAnswered = false
            pickedID = nil
        }
    }
}
