import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:wepostai_mobile/main.dart';

void main() {
  testWidgets('App boots and shows login or loader', (WidgetTester tester) async {
    await tester.pumpWidget(const WePosteAIApp());
    await tester.pump();
    expect(find.byType(MaterialApp), findsOneWidget);
  });
}
