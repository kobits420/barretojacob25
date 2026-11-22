"""Original pictures for Tem Tong, rendered with Manim.

These scenes illustrate the questions. They do not print the answer,
except the box-reveal scene, which plays only after a choice is made.
"""

from manim import *

PAPER = "#f4efe4"
INK = "#1c1612"
BRASS = "#c4923a"
BRASS_DEEP = "#6e4e28"
SEAL = "#8d3030"
CARD = "#efe6d4"

config.pixel_width = 960
config.pixel_height = 540
config.frame_rate = 24
config.background_color = PAPER


def T(text, size=36, color=INK, font="Liberation Sans"):
    return Text(text, font=font, font_size=size, color=color)


def brass_bar(width, height=0.46):
    return RoundedRectangle(
        width=width,
        height=height,
        corner_radius=min(0.1, height / 2),
        fill_color=BRASS,
        fill_opacity=1,
        stroke_color=BRASS_DEEP,
        stroke_width=2,
    )


def ruled(scene):
    top = Rectangle(
        width=config.frame_width + 0.2,
        height=0.06,
        fill_color=INK,
        fill_opacity=1,
        stroke_width=0,
    )
    top.to_edge(UP, buff=0)
    bottom = Rectangle(
        width=config.frame_width + 0.2,
        height=0.16,
        fill_color=BRASS,
        fill_opacity=1,
        stroke_width=0,
    )
    bottom.to_edge(DOWN, buff=0)
    scene.add(top, bottom)


def person():
    head = Circle(
        radius=0.22,
        color=INK,
        fill_color=INK,
        fill_opacity=1,
        stroke_width=0,
    )
    torso = RoundedRectangle(
        width=0.5,
        height=0.56,
        corner_radius=0.16,
        color=INK,
        fill_color=INK,
        fill_opacity=1,
        stroke_width=0,
    )
    torso.next_to(head, DOWN, buff=0.05)
    leg = RoundedRectangle(
        width=0.14,
        height=0.36,
        corner_radius=0.04,
        color=INK,
        fill_color=INK,
        fill_opacity=1,
        stroke_width=0,
    )
    legs = VGroup(leg, leg.copy()).arrange(RIGHT, buff=0.1)
    legs.next_to(torso, DOWN, buff=0.04)
    return VGroup(head, torso, legs)


def cup():
    body = RoundedRectangle(
        width=0.56,
        height=0.7,
        corner_radius=0.1,
        fill_color=BRASS,
        fill_opacity=1,
        stroke_width=0,
    )
    rim = Ellipse(
        width=0.64,
        height=0.18,
        fill_color="#f7f1e4",
        fill_opacity=1,
        stroke_color=BRASS_DEEP,
        stroke_width=1.5,
    )
    rim.move_to(body.get_top() + DOWN * 0.01)
    handle = Arc(
        radius=0.18,
        start_angle=-PI / 2,
        angle=PI,
        color=BRASS_DEEP,
        stroke_width=3.5,
    )
    handle.next_to(body, RIGHT, buff=-0.04)
    return VGroup(body, rim, handle)


def month_card(name):
    card = RoundedRectangle(
        width=1.7,
        height=0.92,
        corner_radius=0.08,
        fill_color=CARD,
        fill_opacity=1,
        stroke_color=INK,
        stroke_width=1.75,
    )
    label = T(name, size=24)
    label.move_to(card.get_center())
    return VGroup(card, label)


def number_card(name):
    card = RoundedRectangle(
        width=1.7,
        height=2.15,
        corner_radius=0.1,
        fill_color=CARD,
        fill_opacity=1,
        stroke_color=INK,
        stroke_width=2,
    )
    label = T(name, size=56, font="Liberation Serif")
    label.move_to(card.get_center())
    return VGroup(card, label)


def build_boxes():
    specs = [
        ("A", "The prize is", "in this box."),
        ("B", "The prize is", "not in this box."),
        ("C", "The prize is", "not in box A."),
    ]
    parts = []
    group = VGroup()
    for i, (letter, line_a, line_b) in enumerate(specs):
        x = (i - 1) * 4.4
        body = RoundedRectangle(
            width=3.7,
            height=2.85,
            corner_radius=0.1,
            fill_color=CARD,
            fill_opacity=1,
            stroke_color=INK,
            stroke_width=2.5,
        )
        body.move_to([x, 0.05, 0])
        cap = Rectangle(
            width=3.7,
            height=0.16,
            fill_color=BRASS,
            fill_opacity=1,
            stroke_width=0,
        )
        cap.move_to(body.get_top() + DOWN * 0.45)
        letter_t = T(letter, size=54, color=SEAL, font="Liberation Serif")
        letter_t.move_to(body.get_center() + UP * 0.55)
        words = VGroup(T(line_a, size=22), T(line_b, size=22)).arrange(DOWN, buff=0.08)
        words.move_to(body.get_center() + DOWN * 0.55)
        piece = VGroup(body, cap, letter_t, words)
        group.add(piece)
        parts.append(
            {
                "piece": piece,
                "body": body,
                "letter": letter_t,
                "words": words,
            }
        )
    return group, parts


class Trial01(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        left = VGroup(*[brass_bar(0.36, 1.55) for _ in range(3)]).arrange(RIGHT, buff=0.14)
        right = VGroup(*[brass_bar(0.36, 1.55) for _ in range(4)]).arrange(RIGHT, buff=0.14)
        plus = T("+", size=68)
        eq = T("=", size=68)
        mark = T("?", size=78, color=SEAL, font="Liberation Serif")
        row = VGroup(left, plus, right, eq, mark).arrange(RIGHT, buff=0.42)
        row.move_to(ORIGIN)
        self.play(
            LaggedStart(*[GrowFromEdge(bar, DOWN) for bar in left], lag_ratio=0.2),
            run_time=0.9,
        )
        self.play(FadeIn(plus, scale=0.7), run_time=0.3)
        self.play(
            LaggedStart(*[GrowFromEdge(bar, DOWN) for bar in right], lag_ratio=0.15),
            run_time=1.0,
        )
        self.play(FadeIn(eq), FadeIn(mark, shift=UP * 0.12), run_time=0.45)
        self.wait(0.8)


class Trial02(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        people = VGroup(*[person() for _ in range(5)])
        people.arrange(RIGHT, buff=0.55)
        people.move_to(UP * 0.25)
        floor = Line(
            people.get_left() + LEFT * 0.45,
            people.get_right() + RIGHT * 0.45,
            color=INK,
            stroke_width=3,
        )
        floor.next_to(people, DOWN, buff=0.06)
        self.play(Create(floor), run_time=0.35)
        self.play(
            LaggedStart(*[FadeIn(p, shift=UP * 0.25) for p in people], lag_ratio=0.18),
            run_time=1.8,
        )
        self.wait(0.8)


class Trial03(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        origin_x = -3.25
        spine = Line(
            [origin_x, 1.7, 0],
            [origin_x, -1.7, 0],
            color=INK,
            stroke_width=3,
        )
        upper = brass_bar(3.15, 0.52)
        upper.move_to([origin_x + 3.15 / 2, 0.85, 0])
        lower = brass_bar(6.35, 0.52)
        lower.move_to([origin_x + 6.35 / 2, -0.75, 0])
        upper_end = Line(UP * 0.38, DOWN * 0.38, color=INK, stroke_width=3).move_to(
            upper.get_right()
        )
        lower_end = Line(UP * 0.38, DOWN * 0.38, color=INK, stroke_width=3).move_to(
            lower.get_right()
        )
        self.play(Create(spine), run_time=0.25)
        self.play(
            GrowFromEdge(upper, LEFT),
            GrowFromEdge(lower, LEFT),
            run_time=1.05,
        )
        self.play(FadeIn(upper_end), FadeIn(lower_end), run_time=0.3)
        self.wait(0.7)


class Trial04(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        cups = VGroup(*[cup() for _ in range(8)])
        cups.arrange(RIGHT, buff=0.28)
        cups.move_to(UP * 0.35)
        counter = RoundedRectangle(
            width=cups.width + 0.8,
            height=0.18,
            corner_radius=0.04,
            fill_color=BRASS_DEEP,
            fill_opacity=1,
            stroke_width=0,
        )
        counter.next_to(cups, DOWN, buff=0.12)
        self.play(FadeIn(counter, shift=UP * 0.1), run_time=0.3)
        self.play(
            LaggedStart(*[FadeIn(c, shift=UP * 0.35) for c in cups], lag_ratio=0.1),
            run_time=1.8,
        )
        self.wait(0.7)


class Trial05(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        whole = brass_bar(6.15, 0.62)
        self.play(FadeIn(whole, shift=UP * 0.12), run_time=0.45)
        self.wait(0.25)

        cut_a = Line(UP * 0.5, DOWN * 0.5, color=SEAL, stroke_width=5).move_to(LEFT * 1.02)
        self.play(Create(cut_a), run_time=0.25)
        left = brass_bar(1.9, 0.62).move_to(LEFT * 2.05)
        rest = brass_bar(3.95, 0.62).move_to(RIGHT * 1.05)
        self.play(FadeOut(whole), FadeOut(cut_a), FadeIn(left), FadeIn(rest), run_time=0.25)
        self.play(
            left.animate.move_to(LEFT * 2.3),
            rest.animate.move_to(RIGHT * 1.15),
            run_time=0.4,
        )

        cut_b = Line(UP * 0.5, DOWN * 0.5, color=SEAL, stroke_width=5).move_to(
            rest.get_center()
        )
        self.play(Create(cut_b), run_time=0.25)
        mid = brass_bar(1.85, 0.62).move_to(rest.get_center() + LEFT * 0.95)
        right = brass_bar(1.85, 0.62).move_to(rest.get_center() + RIGHT * 0.95)
        self.play(FadeOut(rest), FadeOut(cut_b), FadeIn(mid), FadeIn(right), run_time=0.25)
        self.play(
            left.animate.move_to(LEFT * 2.55),
            mid.animate.move_to(ORIGIN),
            right.animate.move_to(RIGHT * 2.55),
            run_time=0.45,
        )
        gap_a = Line(UP * 0.42, DOWN * 0.42, color=SEAL, stroke_width=4).move_to(
            (left.get_right() + mid.get_left()) / 2
        )
        gap_b = Line(UP * 0.42, DOWN * 0.42, color=SEAL, stroke_width=4).move_to(
            (mid.get_right() + right.get_left()) / 2
        )
        self.play(FadeIn(gap_a), FadeIn(gap_b), run_time=0.3)
        self.wait(0.75)


class Trial06(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        names = [
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec",
        ]
        cards = VGroup(*[month_card(name) for name in names])
        cards.arrange_in_grid(rows=3, cols=4, buff=(0.32, 0.28))
        cards.move_to(ORIGIN)
        self.play(
            LaggedStart(*[FadeIn(card, shift=UP * 0.12) for card in cards], lag_ratio=0.07),
            run_time=2.0,
        )
        self.wait(0.7)


class Trial07(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        top = UP * 1.45
        left = LEFT * 1.7 + DOWN * 1.15
        right = RIGHT * 1.7 + DOWN * 1.15
        sides = VGroup(
            Line(top, left, color=INK, stroke_width=7),
            Line(left, right, color=INK, stroke_width=7),
            Line(right, top, color=INK, stroke_width=7),
        )
        sides.shift(UP * 0.35)
        for side in sides:
            self.play(Create(side), run_time=0.4)
        blank = Line(LEFT * 1.7, RIGHT * 1.7, color=BRASS_DEEP, stroke_width=3.5)
        blank.next_to(sides, DOWN, buff=0.48)
        self.play(Create(blank), run_time=0.4)
        self.wait(0.7)


class Trial08(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        cards = VGroup(*[number_card(n) for n in ("0", "2", "4", "8")])
        cards.arrange(RIGHT, buff=0.38)
        cards.move_to(ORIGIN)
        self.play(
            LaggedStart(*[FadeIn(card, shift=UP * 0.2) for card in cards], lag_ratio=0.16),
            run_time=1.5,
        )
        self.wait(0.8)


class Trial09(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        pages = VGroup()
        for i in range(8):
            page = RoundedRectangle(
                width=2.35,
                height=0.16,
                corner_radius=0.03,
                fill_color="#fffaf1" if i % 2 == 0 else "#e4d8c2",
                fill_opacity=1,
                stroke_color=BRASS_DEEP,
                stroke_width=1,
            )
            page.shift(UP * i * 0.11)
            pages.add(page)
        pages.move_to(LEFT * 3.15 + DOWN * 0.35)
        count = T("30", size=48, font="Liberation Serif")
        count.next_to(pages, UP, buff=0.28)
        sign = VGroup(
            Line(LEFT * 0.42, RIGHT * 0.42, color=INK, stroke_width=7),
            Dot(UP * 0.32, radius=0.075, color=INK),
            Dot(DOWN * 0.32, radius=0.075, color=INK),
        )
        numerator = T("1", size=44, font="Liberation Serif")
        denominator = T("2", size=44, font="Liberation Serif")
        bar = Line(LEFT * 0.32, RIGHT * 0.32, color=INK, stroke_width=3.5)
        fraction = VGroup(numerator, bar, denominator).arrange(DOWN, buff=0.07)
        operation = VGroup(sign, fraction).arrange(RIGHT, buff=0.32)
        operation.move_to(RIGHT * 2.15)
        self.play(
            LaggedStart(
                *[FadeIn(page, shift=UP * 0.05) for page in pages],
                lag_ratio=0.08,
            ),
            run_time=1.0,
        )
        self.play(FadeIn(count, shift=UP * 0.08), run_time=0.35)
        self.play(FadeIn(operation, scale=0.85), run_time=0.55)
        self.wait(0.75)


class Trial10(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        group, _parts = build_boxes()
        self.play(
            LaggedStart(*[FadeIn(piece, shift=UP * 0.18) for piece in group], lag_ratio=0.18),
            run_time=1.5,
        )
        self.wait(0.8)


class Trial10Reveal(Scene):
    def construct(self):
        self.camera.background_color = PAPER
        ruled(self)
        _group, parts = build_boxes()
        self.add(*[part["piece"] for part in parts])
        star = RegularPolygram(
            5,
            radius=0.48,
            start_angle=PI / 2,
            fill_color=BRASS,
            fill_opacity=1,
            stroke_color=BRASS_DEEP,
            stroke_width=2,
        )
        star.next_to(parts[1]["body"], DOWN, buff=0.28)
        marks = VGroup()
        for index in (0, 2):
            corner = parts[index]["body"].get_corner(UR) + LEFT * 0.38 + DOWN * 0.38
            mark = VGroup(
                Line(
                    LEFT * 0.16 + UP * 0.16,
                    RIGHT * 0.16 + DOWN * 0.16,
                    color=SEAL,
                    stroke_width=5,
                ),
                Line(
                    RIGHT * 0.16 + UP * 0.16,
                    LEFT * 0.16 + DOWN * 0.16,
                    color=SEAL,
                    stroke_width=5,
                ),
            )
            mark.move_to(corner)
            marks.add(mark)
        self.play(FadeIn(star, scale=0.4), FadeIn(marks), run_time=0.6)
        self.wait(0.9)
