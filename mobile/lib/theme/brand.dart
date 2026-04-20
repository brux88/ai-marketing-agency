import 'package:flutter/material.dart';

/// wepostai brand palette — mirrors frontend/src/app/globals.css tokens.
/// Hex values are sRGB approximations of the original oklch() tokens.
class BrandColors {
  static const Color ink = Color(0xFF222222);
  static const Color ink2 = Color(0xFF363636);
  static const Color paper = Color(0xFFFBFAF5);
  static const Color paper2 = Color(0xFFF2EFE8);
  static const Color line = Color(0xFFE4E1DA);
  static const Color mutedInk = Color(0xFF898780);
  static const Color lime = Color(0xFFBFE836);
  static const Color limeDeep = Color(0xFF86B224);
  static const Color terra = Color(0xFFC07F4E);
  static const Color terraSoft = Color(0xFFECDDCA);
  static const Color error = Color(0xFFCD3A3A);
}

class BrandTheme {
  static ThemeData light() {
    const cs = ColorScheme(
      brightness: Brightness.light,
      primary: BrandColors.ink,
      onPrimary: BrandColors.paper,
      primaryContainer: BrandColors.paper2,
      onPrimaryContainer: BrandColors.ink,
      secondary: BrandColors.terra,
      onSecondary: BrandColors.paper,
      secondaryContainer: BrandColors.terraSoft,
      onSecondaryContainer: BrandColors.ink,
      tertiary: BrandColors.limeDeep,
      onTertiary: BrandColors.ink,
      tertiaryContainer: BrandColors.lime,
      onTertiaryContainer: BrandColors.ink,
      error: BrandColors.error,
      onError: BrandColors.paper,
      errorContainer: Color(0xFFFBE5E5),
      onErrorContainer: BrandColors.error,
      surface: BrandColors.paper,
      onSurface: BrandColors.ink,
      surfaceContainerLowest: BrandColors.paper,
      surfaceContainerLow: BrandColors.paper2,
      surfaceContainer: BrandColors.paper2,
      surfaceContainerHigh: BrandColors.paper2,
      surfaceContainerHighest: BrandColors.paper2,
      surfaceTint: BrandColors.ink,
      outline: BrandColors.line,
      outlineVariant: BrandColors.line,
      onSurfaceVariant: BrandColors.mutedInk,
      inverseSurface: BrandColors.ink,
      onInverseSurface: BrandColors.paper,
      inversePrimary: BrandColors.lime,
      scrim: Colors.black54,
      shadow: Colors.black12,
    );

    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.light,
      colorScheme: cs,
      scaffoldBackgroundColor: BrandColors.paper,
      canvasColor: BrandColors.paper,
      dividerColor: BrandColors.line,
      appBarTheme: const AppBarTheme(
        backgroundColor: BrandColors.paper,
        foregroundColor: BrandColors.ink,
        elevation: 0,
        scrolledUnderElevation: 0,
        centerTitle: true,
        titleTextStyle: TextStyle(
          color: BrandColors.ink,
          fontSize: 18,
          fontWeight: FontWeight.w600,
          letterSpacing: -0.2,
        ),
      ),
      cardTheme: CardThemeData(
        color: BrandColors.paper,
        elevation: 0,
        shape: RoundedRectangleBorder(
          side: const BorderSide(color: BrandColors.line),
          borderRadius: BorderRadius.circular(12),
        ),
        margin: EdgeInsets.zero,
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: BrandColors.ink,
          foregroundColor: BrandColors.paper,
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10)),
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
          textStyle: const TextStyle(
              fontWeight: FontWeight.w600, letterSpacing: -0.1),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: BrandColors.ink,
          side: const BorderSide(color: BrandColors.ink),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10)),
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
          textStyle: const TextStyle(fontWeight: FontWeight.w600),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: BrandColors.ink,
          textStyle: const TextStyle(fontWeight: FontWeight.w600),
        ),
      ),
      floatingActionButtonTheme: const FloatingActionButtonThemeData(
        backgroundColor: BrandColors.ink,
        foregroundColor: BrandColors.paper,
        elevation: 2,
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: BrandColors.paper2,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: BrandColors.line),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: BrandColors.line),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: BrandColors.ink, width: 1.5),
        ),
        labelStyle: const TextStyle(color: BrandColors.mutedInk),
        hintStyle: const TextStyle(color: BrandColors.mutedInk),
      ),
      chipTheme: ChipThemeData(
        backgroundColor: BrandColors.paper2,
        selectedColor: BrandColors.lime,
        disabledColor: BrandColors.paper2,
        labelStyle: const TextStyle(color: BrandColors.ink),
        side: const BorderSide(color: BrandColors.line),
        shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8)),
      ),
      tabBarTheme: const TabBarThemeData(
        labelColor: BrandColors.ink,
        unselectedLabelColor: BrandColors.mutedInk,
        indicatorColor: BrandColors.ink,
        indicatorSize: TabBarIndicatorSize.label,
        labelStyle: TextStyle(
            fontWeight: FontWeight.w600, letterSpacing: -0.1),
      ),
      bottomNavigationBarTheme: const BottomNavigationBarThemeData(
        backgroundColor: BrandColors.paper,
        selectedItemColor: BrandColors.ink,
        unselectedItemColor: BrandColors.mutedInk,
        type: BottomNavigationBarType.fixed,
      ),
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: BrandColors.paper,
        indicatorColor: BrandColors.lime,
        iconTheme: WidgetStateProperty.all(
            const IconThemeData(color: BrandColors.ink)),
        labelTextStyle: WidgetStateProperty.all(
          const TextStyle(
              color: BrandColors.ink, fontWeight: FontWeight.w600),
        ),
      ),
      switchTheme: SwitchThemeData(
        thumbColor: WidgetStateProperty.resolveWith((s) =>
            s.contains(WidgetState.selected)
                ? BrandColors.paper
                : BrandColors.paper),
        trackColor: WidgetStateProperty.resolveWith((s) =>
            s.contains(WidgetState.selected)
                ? BrandColors.ink
                : BrandColors.line),
        trackOutlineColor: WidgetStateProperty.all(BrandColors.line),
      ),
      dialogTheme: DialogThemeData(
        backgroundColor: BrandColors.paper,
        shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14)),
      ),
      snackBarTheme: const SnackBarThemeData(
        backgroundColor: BrandColors.ink,
        contentTextStyle: TextStyle(color: BrandColors.paper),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }
}

/// "wepost" in normal weight + "ai.com" italic muted — the wepostai wordmark.
class BrandWordmark extends StatelessWidget {
  final double fontSize;
  final Color? color;
  final Color? mutedColor;
  const BrandWordmark({
    super.key,
    this.fontSize = 28,
    this.color,
    this.mutedColor,
  });

  @override
  Widget build(BuildContext context) {
    final base = color ?? BrandColors.ink;
    final muted = mutedColor ?? BrandColors.mutedInk;
    return RichText(
      text: TextSpan(
        style: TextStyle(
          fontSize: fontSize,
          color: base,
          fontWeight: FontWeight.w500,
          letterSpacing: -0.8,
          height: 1,
        ),
        children: [
          const TextSpan(text: 'wepost'),
          TextSpan(
            text: 'ai.com',
            style: TextStyle(
              color: muted,
              fontStyle: FontStyle.italic,
            ),
          ),
        ],
      ),
    );
  }
}

/// Square brand mark — ink rounded square with a lime cursor dot.
/// Stand-in for the SVG BrandMark used on web.
class BrandMarkIcon extends StatelessWidget {
  final double size;
  final bool inverted;
  const BrandMarkIcon({super.key, this.size = 40, this.inverted = false});

  @override
  Widget build(BuildContext context) {
    final bg = inverted ? BrandColors.paper : BrandColors.ink;
    final fg = inverted ? BrandColors.ink : BrandColors.paper;
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(size * 0.22),
      ),
      child: Stack(
        alignment: Alignment.center,
        children: [
          Text(
            'w',
            style: TextStyle(
              color: fg,
              fontSize: size * 0.58,
              fontWeight: FontWeight.w500,
              height: 1,
              letterSpacing: -1,
            ),
          ),
          Positioned(
            right: size * 0.15,
            bottom: size * 0.2,
            child: Container(
              width: size * 0.1,
              height: size * 0.1,
              decoration: const BoxDecoration(
                color: BrandColors.lime,
                shape: BoxShape.circle,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
