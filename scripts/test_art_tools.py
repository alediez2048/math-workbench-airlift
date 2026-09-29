import os
import tempfile
import unittest

from PIL import Image

import art_tools


def opened(path):
    with Image.open(path) as im:
        im.load()
        return im.copy()


class FitCoverTests(unittest.TestCase):
    def setUp(self):
        self.dir = tempfile.mkdtemp()

    def path(self, name):
        return os.path.join(self.dir, name)

    def test_wide_target_from_square_rgba_is_exact_size_and_opaque(self):
        Image.new('RGBA', (1024, 1024), (255, 168, 78, 0)).save(self.path('src.png'))
        art_tools.fit_cover(self.path('src.png'), self.path('out.png'), 584, 166)
        out = opened(self.path('out.png'))
        self.assertEqual(out.size, (584, 166))
        self.assertEqual(out.mode, 'RGB')

    def test_portrait_and_tiny_sources_still_fill_the_frame(self):
        Image.new('RGB', (90, 300), (88, 222, 190)).save(self.path('tall.png'))
        art_tools.fit_cover(self.path('tall.png'), self.path('out.png'), 584, 166)
        self.assertEqual(opened(self.path('out.png')).size, (584, 166))

    def test_focus_y_picks_the_band(self):
        src = Image.new('RGB', (1600, 900), (0, 0, 0))
        src.paste((255, 255, 255), (0, 0, 1600, 300))   # white top third
        src.save(self.path('src.png'))
        art_tools.fit_cover(self.path('src.png'), self.path('top.png'), 584, 166, focus_y=0.0)
        art_tools.fit_cover(self.path('src.png'), self.path('bottom.png'), 584, 166, focus_y=1.0)
        self.assertGreater(opened(self.path('top.png')).getpixel((292, 10))[0], 200)
        self.assertLess(opened(self.path('bottom.png')).getpixel((292, 150))[0], 50)


class SeamTests(unittest.TestCase):
    def setUp(self):
        self.dir = tempfile.mkdtemp()

    def test_seam_error_is_zero_for_matching_edges_and_high_for_mismatch(self):
        same = os.path.join(self.dir, 'same.png')
        Image.new('RGB', (256, 128), (32, 35, 68)).save(same)
        self.assertEqual(art_tools.seam_error(same), 0.0)
        split = Image.new('RGB', (256, 128), (0, 0, 0))
        split.paste((255, 255, 255), (128, 0, 256, 128))
        path = os.path.join(self.dir, 'split.png')
        split.save(path)
        self.assertGreater(art_tools.seam_error(path), 200)

    def test_make_seamless_brings_the_seam_under_tolerance_and_keeps_size(self):
        split = Image.new('RGB', (512, 256), (0, 0, 0))
        split.paste((255, 255, 255), (256, 0, 512, 256))
        src = os.path.join(self.dir, 'split.png')
        dst = os.path.join(self.dir, 'wrapped.png')
        split.save(src)
        art_tools.make_seamless(src, dst, band=64)
        self.assertEqual(opened(dst).size, (512, 256))
        self.assertLess(art_tools.seam_error(dst), 8.0)

    def test_make_seamless_does_not_mirror_one_edge_into_the_other(self):
        # A red dot just inside the left edge must not reappear near the right edge (a mirror-crossfade ghost).
        img = Image.new('RGB', (512, 256), (90, 90, 90))
        img.paste((255, 0, 0), (10, 100, 16, 106))
        src = os.path.join(self.dir, 'dot.png')
        dst = os.path.join(self.dir, 'dot-wrapped.png')
        img.save(src)
        art_tools.make_seamless(src, dst, band=64)
        out = opened(dst)
        right_band_red = max(out.getpixel((x, y))[0] - out.getpixel((x, y))[1] for x in range(512 - 64, 512) for y in range(96, 110))
        self.assertLess(right_band_red, 20)

class DetailMapTests(unittest.TestCase):
    def setUp(self):
        self.dir = tempfile.mkdtemp()
        noisy = Image.new('RGB', (700, 640))
        noisy.putdata([((x * 37 + y * 11) % 256, (x * 5) % 256, (y * 13) % 256) for y in range(640) for x in range(700)])
        self.src = os.path.join(self.dir, 'noisy.png')
        noisy.save(self.src)
        self.dst = os.path.join(self.dir, 'detail.png')
        art_tools.detail_map(self.src, self.dst, size=256, mean=0.92, spread=0.08)
        self.out = opened(self.dst)

    def test_is_a_square_grey_map_of_the_requested_size(self):
        self.assertEqual(self.out.size, (256, 256))
        self.assertEqual(self.out.mode, 'RGB')
        self.assertTrue(all(r == g == b for r, g, b in self.out.getdata()))

    def test_stays_pale_and_low_contrast_so_it_never_darkens_the_palette(self):
        values = [p[0] for p in self.out.getdata()]
        self.assertAlmostEqual(sum(values) / len(values) / 255, 0.92, delta=0.02)
        self.assertGreaterEqual(min(values), round((0.92 - 0.08) * 255) - 2)
        self.assertLessEqual(max(values), 255)

    def test_tiles_in_both_directions(self):
        self.assertLess(art_tools.seam_error(self.dst), 8.0)
        rotated = os.path.join(self.dir, 'rot.png')
        self.out.transpose(Image.Transpose.ROTATE_90).save(rotated)
        self.assertLess(art_tools.seam_error(rotated), 8.0)

class TintToMeanTests(unittest.TestCase):
    def test_trims_the_border_and_lands_on_the_target_average_colour(self):
        d = tempfile.mkdtemp()
        img = Image.new('RGB', (400, 250), (40, 200, 60))
        img.paste((255, 255, 0), (0, 0, 400, 8))            # a border stripe that must be trimmed away
        src, dst = os.path.join(d, 's.png'), os.path.join(d, 'd.png')
        img.save(src)
        art_tools.tint_to_mean(src, dst, (0.49, 0.77, 0.40), width=320, height=200, trim=0.05)
        out = opened(dst)
        self.assertEqual(out.size, (320, 200))
        px = list(out.getdata())
        mean = [sum(p[i] for p in px) / len(px) / 255 for i in range(3)]
        for got, want in zip(mean, (0.49, 0.77, 0.40)):
            self.assertAlmostEqual(got, want, delta=0.02)
        self.assertNotIn((255, 255, 0), px[:320])

if __name__ == '__main__':
    unittest.main()
