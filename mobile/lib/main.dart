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
      child: const AiMarketingApp(),
    ),
  );
}

class AiMarketingApp extends StatelessWidget {
  const AiMarketingApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'AI Marketing Agency',
      debugShowCheckedModeBanner: false,
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
              ? const HomeShell()
              : const LoginScreen();
        },
      ),
    );
  }
}
