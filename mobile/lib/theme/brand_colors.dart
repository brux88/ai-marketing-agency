import 'package:flutter/material.dart';

/// weposteai.com brand tokens for Flutter.
/// Mirrors the CSS tokens from the web frontend handoff.
class BrandColors {
  BrandColors._();

  // Core
  static const Color ink = Color(0xFF0A0A0B);
  static const Color ink2 = Color(0xFF18181B);
  static const Color paper = Color(0xFFFAFAF7);
  static const Color paper2 = Color(0xFFF2F1EC);
  static const Color line = Color(0xFFE4E2DB);
  static const Color mutedInk = Color(0xFF6B6A64);

  // Accent
  static const Color lime = Color(0xFFB8E63E);
  static const Color limeDeep = Color(0xFF7BBF1A);
  static const Color terra = Color(0xFFC28B4E);
  static const Color terraSoft = Color(0xFFE8D5BF);

  // Semantic
  static const Color destructive = Color(0xFFEF4444);

  /// Light theme color scheme
  static ColorScheme get lightScheme => const ColorScheme(
        brightness: Brightness.light,
        primary: ink,
        onPrimary: paper,
        secondary: paper2,
        onSecondary: ink,
        surface: paper,
        onSurface: ink,
        error: destructive,
        onError: paper,
        surfaceContainerHighest: paper2,
        outline: line,
        outlineVariant: line,
        onSurfaceVariant: mutedInk,
      );

  /// Dark theme color scheme
  static ColorScheme get darkScheme => const ColorScheme(
        brightness: Brightness.dark,
        primary: paper,
        onPrimary: ink,
        secondary: ink2,
        onSecondary: paper,
        surface: ink,
        onSurface: paper,
        error: destructive,
        onError: paper,
        surfaceContainerHighest: ink2,
        outline: Color(0xFF3A3A3F),
        outlineVariant: Color(0xFF3A3A3F),
        onSurfaceVariant: Color(0xFF9E9E96),
      );
}
