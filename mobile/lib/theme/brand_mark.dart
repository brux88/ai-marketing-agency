import 'package:flutter/material.dart';
import 'brand_colors.dart';

/// The weposteai.com brand mark: speech bubble with blinking cursor.
class BrandMark extends StatelessWidget {
  const BrandMark({
    super.key,
    this.size = 28,
    this.strokeColor,
    this.cursorColor,
  });

  final double size;
  final Color? strokeColor;
  final Color? cursorColor;

  @override
  Widget build(BuildContext context) {
    final stroke = strokeColor ?? Theme.of(context).colorScheme.onSurface;
    final cursor = cursorColor ?? BrandColors.limeDeep;

    return CustomPaint(
      size: Size(size, size),
      painter: _BrandMarkPainter(strokeColor: stroke, cursorColor: cursor),
    );
  }
}

class _BrandMarkPainter extends CustomPainter {
  _BrandMarkPainter({required this.strokeColor, required this.cursorColor});

  final Color strokeColor;
  final Color cursorColor;

  @override
  void paint(Canvas canvas, Size size) {
    final scale = size.width / 120;
    canvas.save();
    canvas.scale(scale);

    // Speech bubble
    final bubblePaint = Paint()
      ..color = strokeColor
      ..style = PaintingStyle.stroke
      ..strokeWidth = 8
      ..strokeJoin = StrokeJoin.round;

    final path = Path()
      ..moveTo(16, 20)
      ..lineTo(104, 20)
      ..arcToPoint(const Offset(112, 28), radius: const Radius.circular(8))
      ..lineTo(112, 80)
      ..arcToPoint(const Offset(104, 88), radius: const Radius.circular(8))
      ..lineTo(62, 88)
      ..lineTo(44, 106)
      ..lineTo(44, 88)
      ..lineTo(16, 88)
      ..arcToPoint(const Offset(8, 80), radius: const Radius.circular(8))
      ..lineTo(8, 28)
      ..arcToPoint(const Offset(16, 20), radius: const Radius.circular(8))
      ..close();

    canvas.drawPath(path, bubblePaint);

    // Cursor
    final cursorPaint = Paint()..color = cursorColor;
    canvas.drawRect(const Rect.fromLTWH(56, 34, 8, 40), cursorPaint);

    canvas.restore();
  }

  @override
  bool shouldRepaint(_BrandMarkPainter oldDelegate) =>
      oldDelegate.strokeColor != strokeColor ||
      oldDelegate.cursorColor != cursorColor;
}
