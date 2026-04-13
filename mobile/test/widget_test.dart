import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:ai_marketing_mobile/main.dart';

void main() {
  testWidgets('App boots and shows login or loader', (WidgetTester tester) async {
    await tester.pumpWidget(const AiMarketingApp());
    await tester.pump();
    expect(find.byType(MaterialApp), findsOneWidget);
  });
}
