import 'package:flutter/material.dart';
import '../services/checkin_service.dart';
import 'scanner_screen.dart';
import '../services/auth_service.dart';
import 'login_screen.dart';
import '../services/data_service.dart';

class QrScreen extends StatefulWidget {
  const QrScreen({super.key});

  @override
  State<QrScreen> createState() => _QrScreenState();
}

class _QrScreenState extends State<QrScreen> {
  bool _busy = false;
  String? _message;
  bool _success = false;

  Future<void> _scanAndProcess(String action) async {
    // 1) Kamerayı aç, token'ı bekle
    final token = await Navigator.of(context).push<String>(
        MaterialPageRoute(builder: (_) => const ScannerScreen()));

    if (token == null || !mounted) return; // kullanıcı vazgeçti

    // 2) API'ye gönder
    setState(() { _busy = true; _message = null; });
    final (ok, message) = await CheckInService.process(token, action);

    if (!mounted) return;
    setState(() { _busy = false; _message = message; _success = ok; });
  }

  Future<void> _reportNoise() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Gürültü İhbarı'),
        content: const Text(
            'Bulunduğunuz bölgedeki gürültüyü görevlilere bildirmek istiyor musunuz?'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('Vazgeç')),
          FilledButton(
              onPressed: () => Navigator.pop(ctx, true),
              child: const Text('Evet, Bildir')),
        ],
      ),
    );

    if (confirmed != true || !mounted) return;

    setState(() { _busy = true; _message = null; });
    final (ok, message) = await DataService.reportNoise();

    if (!mounted) return;
    setState(() { _busy = false; _message = message; _success = ok; });
  }


  Widget _actionButton(String label, IconData icon, String action, Color color) {
    return SizedBox(
      width: double.infinity,
      height: 64,
      child: FilledButton.icon(
        onPressed: _busy ? null : () => _scanAndProcess(action),
        icon: Icon(icon, size: 28),
        label: Text(label, style: const TextStyle(fontSize: 18)),
        style: FilledButton.styleFrom(backgroundColor: color),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('QR İşlemleri'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Çıkış Yap',
            onPressed: () async {
              await AuthService.logout();
              if (context.mounted) {
                Navigator.of(context).pushAndRemoveUntil(
                  MaterialPageRoute(builder: (_) => const LoginScreen()),
                  (route) => false,
                );
              }
            },
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          children: [
            const Text(
                'Yapmak istediğiniz işlemi seçin,\nardından girişteki QR kodu okutun',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey)),
            const SizedBox(height: 24),
            _actionButton('Giriş Yap', Icons.login, 'CheckIn', const Color(0xFF1A4D3E)),
            const SizedBox(height: 12),
            _actionButton('Mola Ver', Icons.coffee, 'BreakStart', Colors.blue),
            const SizedBox(height: 12),
            _actionButton('Mola Bitir', Icons.keyboard_return, 'BreakEnd', Colors.blueGrey),
            const SizedBox(height: 12),
            _actionButton('Çıkış Yap', Icons.logout, 'CheckOut', Colors.brown),
            const SizedBox(height: 24),

            if (_busy) const CircularProgressIndicator(),

            if (_message != null)
              Card(
                color: _success ? Colors.green.shade50 : Colors.red.shade50,
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      Icon(_success ? Icons.check_circle : Icons.error,
                          color: _success ? Colors.green : Colors.red),
                      const SizedBox(width: 12),
                      Expanded(child: Text(_message!)),
                    ],
                  ),
                ),
              ),

            const SizedBox(height: 24),

            // Gürültü ihbar — Column'un DOĞRUDAN çocuğu, kartın dışında
            OutlinedButton.icon(
              onPressed: _busy ? null : _reportNoise,
              icon: const Icon(Icons.volume_up, color: Colors.red),
              label: const Text('Gürültü İhbar Et',
                  style: TextStyle(color: Colors.red)),
              style: OutlinedButton.styleFrom(
                side: const BorderSide(color: Colors.red),
                minimumSize: const Size(double.infinity, 48),
              ),
            ),
          ],
        ),
      ),
    );
  }
}