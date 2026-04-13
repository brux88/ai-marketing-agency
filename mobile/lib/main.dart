import 'package:flutter/material.dart';
import 'api/api_client.dart';
import 'screens/login_screen.dart';
import 'screens/agencies_screen.dart';

void main() {
  runApp(const AiMarketingApp());
}

class AiMarketingApp extends StatelessWidget {
  const AiMarketingApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'AI Marketing Agency',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.indigo),
        useMaterial3: true,
      ),
      home: FutureBuilder<String?>(
        future: ApiClient.token,
        builder: (_, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Scaffold(
                body: Center(child: CircularProgressIndicator()));
          }
          return snap.data != null
              ? const AgenciesScreen()
              : const LoginScreen();
        },
      ),
    );
  }
}
