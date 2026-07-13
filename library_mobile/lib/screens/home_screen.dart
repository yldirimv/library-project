import 'package:flutter/material.dart';
import 'qr_screen.dart';
import 'reservations_screen.dart';
import 'loans_screen.dart';
import 'announcements_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _index = 0;

  static const _screens = [
    QrScreen(),
    ReservationsScreen(),
    LoansScreen(),
    AnnouncementsScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _screens[_index],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (i) => setState(() => _index = i),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.qr_code_scanner), label: 'QR'),
          NavigationDestination(icon: Icon(Icons.event_seat), label: 'Rezervasyon'),
          NavigationDestination(icon: Icon(Icons.menu_book), label: 'Kitaplarım'),
          NavigationDestination(icon: Icon(Icons.campaign), label: 'Duyurular'),
        ],
      ),
    );
  }
}