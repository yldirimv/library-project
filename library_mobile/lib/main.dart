import 'package:flutter/material.dart';
import 'screens/login_screen.dart';

void main() => runApp(const LibraryApp());

class LibraryApp extends StatelessWidget {
  const LibraryApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Kütüphane',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF1A4D3E)),
        useMaterial3: true,
      ),
      home: const LoginScreen(),
    );
  }
}