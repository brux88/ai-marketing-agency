import 'package:flutter/material.dart';
import 'brand_colors.dart';
import 'brand_mark.dart';

/// weposteai.com logo widget: mark + wordmark.
class BrandLogo extends StatelessWidget {
  const BrandLogo({
    super.key,
    this.size = 24,
    this.inverted = false,
  });

  final double size;
  final bool inverted;

  @override
  Widget build(BuildContext context) {
    final inkColor = inverted ? BrandColors.paper : BrandColors.ink;
    final mutedColor = inverted
        ? BrandColors.paper.withAlpha(140)
        : BrandColors.mutedInk;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        BrandMark(
          size: size,
          strokeColor: inkColor,
          cursorColor: inverted ? BrandColors.lime : BrandColors.limeDeep,
        ),
        SizedBox(width: size * 0.28),
        RichText(
          text: TextSpan(
            style: TextStyle(
              fontSize: size * 0.85,
              fontWeight: FontWeight.w600,
              letterSpacing: -0.5,
              color: inkColor,
            ),
            children: [
              const TextSpan(text: 'weposte'),
              TextSpan(
                text: 'ai.com',
                style: TextStyle(
                  fontStyle: FontStyle.italic,
                  color: mutedColor,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
