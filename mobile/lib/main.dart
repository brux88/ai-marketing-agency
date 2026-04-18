import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'api/api_client.dart';
import 'services/app_state.dart';
import 'services/notification_service.dart';
import 'screens/login_screen.dart';
import 'screens/home_shell.dart';
import 'theme/brand_colors.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize local notifications (FCM requires Firebase config files)
  await NotificationService().initialize();

  runApp(
    ChangeNotifierProvider(
      create: (_) => AppState(),
      child: const WePosteAIApp(),
    ),
  );
}

class WePosteAIApp extends StatelessWidget {
  const WePosteAIApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'weposteai.com',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: BrandColors.lightScheme,
        useMaterial3: true,
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
        ),
      ),
      darkTheme: ThemeData(
        colorScheme: BrandColors.darkScheme,
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
