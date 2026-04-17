import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'api/api_client.dart';
import 'services/app_state.dart';
import 'services/notification_service.dart';
import 'screens/login_screen.dart';
import 'screens/home_shell.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize local notifications (FCM requires Firebase config files)
  await NotificationService().initialize();

  runApp(
    ChangeNotifierProvider(
      create: (_) => AppState(),
      child: const WePostAIApp(),
    ),
  );
}

class WePostAIApp extends StatelessWidget {
  const WePostAIApp({super.key});

  static const Color brandIndigo = Color(0xFF6366F1);
  static const Color brandPurple = Color(0xFF8B5CF6);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'WePostAI',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: brandIndigo,
          primary: brandIndigo,
        ),
        useMaterial3: true,
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
        ),
      ),
      home: FutureBuilder<String?>(
        future: ApiClient.token,
        builder: (_, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Scaffold(
                body: Center(child: CircularProgressIndicator()));
          }
          return snap.data != null
              ? const HomeShell()
              : const LoginScreen();
        },
      ),
    );
  }
}
