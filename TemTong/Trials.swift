import Foundation

/// Hand-written questions for Tem Tong. Each question has one correct choice.
struct Choice: Identifiable, Equatable {
    let id: String
    let label: String
    let isCorrect: Bool
}

struct Trial: Identifiable, Equatable {
    let id: Int
    let title: String
    let videoName: String
    let stillName: String
    let stillDescription: String
    let setup: String?
    let notes: [String]
    let question: String
    let choices: [Choice]
    let explanation: String
    let revealVideoName: String?
    let revealStillName: String?
    let revealDescription: String?

    static let opening: [Trial] = [
        Trial(
            id: 1,
            title: "The Sum",
            videoName: "trial-01",
            stillName: "trial-01",
            stillDescription: "Three brass bars, a plus sign, four brass bars, and a question mark.",
            setup: nil,
            notes: [],
            question: "What is 3 + 4?",
            choices: [
                Choice(id: "a", label: "12", isCorrect: false),
                Choice(id: "b", label: "7", isCorrect: true),
                Choice(id: "c", label: "34", isCorrect: false),
                Choice(id: "d", label: "1", isCorrect: false),
            ],
            explanation: "The answer is 7. Three plus four is seven. 12 is three times four, and 34 pastes the two digits together.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 2,
            title: "The Gallery",
            videoName: "trial-02",
            stillName: "trial-02",
            stillDescription: "Five person-shaped figures in a row. Each figure has one head and one body.",
            setup: nil,
            notes: [],
            question: "How many people are in the gallery?",
            choices: [
                Choice(id: "a", label: "4", isCorrect: false),
                Choice(id: "b", label: "6", isCorrect: false),
                Choice(id: "c", label: "5", isCorrect: true),
                Choice(id: "d", label: "10", isCorrect: false),
            ],
            explanation: "The answer is 5. There are five figures. A head and a body belong to the same person, so counting both would double the gallery.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 3,
            title: "The Longer Bar",
            videoName: "trial-03",
            stillName: "trial-03",
            stillDescription: "Two horizontal bars that start on the same left line. The lower bar reaches much farther right than the upper bar.",
            setup: nil,
            notes: [],
            question: "Which bar is longer?",
            choices: [
                Choice(id: "a", label: "The upper bar", isCorrect: false),
                Choice(id: "b", label: "They are the same length", isCorrect: false),
                Choice(id: "c", label: "The lower bar", isCorrect: true),
                Choice(id: "d", label: "You cannot tell from the picture", isCorrect: false),
            ],
            explanation: "The answer is the lower bar. Both bars start at the same left line and have the same thickness. The lower bar ends much farther to the right. Nothing in the picture is drawn to trick the eye.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 4,
            title: "The Order",
            videoName: "trial-04",
            stillName: "trial-04",
            stillDescription: "Eight cups lined up on a counter.",
            setup: nil,
            notes: [],
            question: "Jacob orders 8 coffees for the jury and drinks 1 of them. How many coffees did Jacob order?",
            choices: [
                Choice(id: "a", label: "7", isCorrect: false),
                Choice(id: "b", label: "1", isCorrect: false),
                Choice(id: "c", label: "8", isCorrect: true),
                Choice(id: "d", label: "9", isCorrect: false),
            ],
            explanation: "The answer is 8. The question asks how many he ordered. Drinking 1 changes how many are left, which is a different question. 7 would be a remainder, and 1 is the cup he drank.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 5,
            title: "The Cuts",
            videoName: "trial-05",
            stillName: "trial-05",
            stillDescription: "A brass bar in three separated pieces, with a red mark in each of the two gaps.",
            setup: nil,
            notes: [],
            question: "A clerk cuts one bar straight through, then cuts one of those pieces straight through again. How many pieces are there at the end?",
            choices: [
                Choice(id: "a", label: "2", isCorrect: false),
                Choice(id: "b", label: "4", isCorrect: false),
                Choice(id: "c", label: "3", isCorrect: true),
                Choice(id: "d", label: "1", isCorrect: false),
            ],
            explanation: "The answer is 3. The first cut turns one bar into two pieces. The second cut divides one of those and adds one piece. Two cuts produce three pieces. 2 counts the cuts, not the pieces.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 6,
            title: "The Calendar",
            videoName: "trial-06",
            stillName: "trial-06",
            stillDescription: "Twelve month cards, January through December, all the same size.",
            setup: nil,
            notes: [],
            question: "How many months have at least 28 days?",
            choices: [
                Choice(id: "a", label: "1", isCorrect: false),
                Choice(id: "b", label: "12", isCorrect: true),
                Choice(id: "c", label: "6", isCorrect: false),
                Choice(id: "d", label: "2", isCorrect: false),
            ],
            explanation: "The answer is 12. Every month has 28 days or more. February has 28 or 29, and the others have 30 or 31. The question is not how many months have only 28 days.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 7,
            title: "The Spelling",
            videoName: "trial-07",
            stillName: "trial-07",
            stillDescription: "A triangle drawn with three sides, and a blank line underneath it.",
            setup: nil,
            notes: [],
            question: "The form says to spell the answer and not to write a digit. How many sides does a triangle have?",
            choices: [
                Choice(id: "a", label: "3", isCorrect: false),
                Choice(id: "b", label: "four", isCorrect: false),
                Choice(id: "c", label: "three", isCorrect: true),
                Choice(id: "d", label: "third", isCorrect: false),
            ],
            explanation: "The answer is three. The form asked for the spelling. 3 is the digit it told you not to use. A triangle has three sides, not four.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 8,
            title: "The Docket",
            videoName: "trial-08",
            stillName: "trial-08",
            stillDescription: "Four cards showing the even numbers 0, 2, 4, and 8.",
            setup: nil,
            notes: [],
            question: "Which of these even numbers is a prime number?",
            choices: [
                Choice(id: "a", label: "4", isCorrect: false),
                Choice(id: "b", label: "0", isCorrect: false),
                Choice(id: "c", label: "8", isCorrect: false),
                Choice(id: "d", label: "2", isCorrect: true),
            ],
            explanation: "The answer is 2. A prime has exactly two distinct positive divisors: 1 and itself. 2 is even, and it is prime. 0 is not a positive prime. 4 and 8 can be divided by 2, so they are not prime.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 9,
            title: "The Half",
            videoName: "trial-09",
            stillName: "trial-09",
            stillDescription: "A stack of pages labeled 30 beside a division sign and the fraction 1/2.",
            setup: nil,
            notes: [],
            question: "A stack holds 30 pages. Divide that stack by one half (1/2), then add 10 pages. How many pages are there?",
            choices: [
                Choice(id: "a", label: "25", isCorrect: false),
                Choice(id: "b", label: "15", isCorrect: false),
                Choice(id: "c", label: "70", isCorrect: true),
                Choice(id: "d", label: "20", isCorrect: false),
            ],
            explanation: "The answer is 70. Dividing by one half doubles the stack: 30 ÷ 1/2 = 60. Then 60 + 10 = 70. The answer 25 comes from dividing by 2, which is cutting the stack in half, not dividing by 1/2.",
            revealVideoName: nil,
            revealStillName: nil,
            revealDescription: nil
        ),
        Trial(
            id: 10,
            title: "The Three Boxes",
            videoName: "trial-10",
            stillName: "trial-10",
            stillDescription: "Three closed boxes labeled A, B, and C.",
            setup: "Three boxes each have one label. Exactly one label is true. The other two are false. The prize is in exactly one box.",
            notes: [
                "Box A: “The prize is in this box.”",
                "Box B: “The prize is not in this box.”",
                "Box C: “The prize is not in box A.”",
            ],
            question: "Which box holds the prize?",
            choices: [
                Choice(id: "a", label: "Box A", isCorrect: false),
                Choice(id: "b", label: "Box C", isCorrect: false),
                Choice(id: "c", label: "No box fits these rules", isCorrect: false),
                Choice(id: "d", label: "Box B", isCorrect: true),
            ],
            explanation: "The answer is box B. If the prize were in A, both A and B would be true, because the prize would not be in B. If it were in C, both B and C would be true. If it is in B, A is false, B is false, and C is true. That is the only case with exactly one true label.",
            revealVideoName: "trial-10-reveal",
            revealStillName: "trial-10-reveal",
            revealDescription: "Box B is open with a brass star inside. Boxes A and C are marked with a cross."
        ),
    ]
}

struct Verdict: Equatable {
    let name: String
    let detail: String

    static func make(score: Int) -> Verdict {
        switch score {
        case 10:
            return Verdict(
                name: "Perfect score",
                detail: "A clean record. Every question answered."
            )
        case 8...9:
            return Verdict(
                name: "Sharp",
                detail: "The tricks caught you once or twice. The rest held."
            )
        case 6...7:
            return Verdict(
                name: "Careful",
                detail: "A solid showing. A few questions were dressed to mislead."
            )
        case 4...5:
            return Verdict(
                name: "Halfway",
                detail: "The early questions are yours. The later ones still slip."
            )
        case 2...3:
            return Verdict(
                name: "Warming up",
                detail: "You stayed with it. The wording did most of the work."
            )
        default:
            return Verdict(
                name: "Read it twice",
                detail: "Start again. Read each question once more than feels necessary."
            )
        }
    }
}
