import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:badges/badges.dart' as badges;
import '../services/app_state.dart';
import 'agencies_screen.dart';
import 'notifications_screen.dart';
import 'jobs_screen.dart';
import 'profile_screen.dart';

class HomeShell extends StatefulWidget {
  const HomeShell({super.key});
  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int _currentIndex = 0;

  final _pages = const <Widget>[
    AgenciesScreen(),
    NotificationsScreen(),
    JobsScreen(),
    ProfileScreen(),
  ];

  @override
  void initState() {
    super.initState();
    // Fetch unread notification count on startup
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<AppState>().fetchUnreadCount();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(
        index: _currentIndex,
        children: _pages,
      ),
      bottomNavigationBar: Consumer<AppState>(
        builder: (context, appState, _) {
          return NavigationBar(
            selectedIndex: _currentIndex,
            onDestinationSelected: (i) => setState(() => _currentIndex = i),
            destinations: [
              const NavigationDestination(
                icon: Icon(Icons.business_outlined),
                selectedIcon: Icon(Icons.business),
                label: 'Agenzie',
              ),
              NavigationDestination(
                icon: appState.unreadNotificationCount > 0
                    ? badges.Badge(
                        badgeContent: Text(
                          appState.unreadNotificationCount > 99
                              ? '99+'
                              : '${appState.unreadNotificationCount}',
                          style: const TextStyle(
                              color: Colors.white, fontSize: 10),
                        ),
                        child: const Icon(Icons.notifications_outlined),
                      )
                    : const Icon(Icons.notifications_outlined),
                selectedIcon: appState.unreadNotificationCount > 0
                    ? badges.Badge(
                        badgeContent: Text(
                          appState.unreadNotificationCount > 99
                              ? '99+'
                              : '${appState.unreadNotificationCount}',
                          style: const TextStyle(
                              color: Colors.white, fontSize: 10),
                        ),
                        child: const Icon(Icons.notifications),
                      )
                    : const Icon(Icons.notifications),
                label: 'Notifiche',
              ),
              const NavigationDestination(
                icon: Icon(Icons.work_outline),
                selectedIcon: Icon(Icons.work),
                label: 'Jobs',
              ),
              const NavigationDestination(
                icon: Icon(Icons.person_outline),
                selectedIcon: Icon(Icons.person),
                label: 'Profilo',
              ),
            ],
          );
        },
      ),
    );
  }
}
