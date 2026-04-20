import 'package:flutter/material.dart';
import 'package:flutter_native_splash/flutter_native_splash.dart';
import 'package:provider/provider.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'api/api_client.dart';
import 'services/app_state.dart';
import 'services/notification_service.dart';
import 'screens/login_screen.dart';
import 'screens/home_shell.dart';
import 'theme/brand.dart';

void main() async {
  final binding = WidgetsFlutterBinding.ensureInitialized();
  FlutterNativeSplash.preserve(widgetsBinding: binding);

  await initializeDateFormatting('it_IT', null);
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

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'wepostai',
      debugShowCheckedModeBanner: false,
      theme: BrandTheme.light(),
      home: FutureBuilder<String?>(
        future: ApiClient.token,
        builder: (_, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Scaffold(
                body: Center(child: CircularProgressIndicator()));
          }
          FlutterNativeSplash.remove();
          if (snap.data != null) {
            // Already logged in: (re)register FCM token with backend.
            NotificationService().registerAfterLogin();
            return const HomeShell();
          }
          return const LoginScreen();
        },
      ),
    );
  }
}
