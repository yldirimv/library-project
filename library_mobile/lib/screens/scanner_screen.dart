import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';

class ScannerScreen extends StatefulWidget {
  const ScannerScreen({super.key});

  @override
  State<ScannerScreen> createState() => _ScannerScreenState();
}

class _ScannerScreenState extends State<ScannerScreen> {
  bool _handled = false;

  @override
  Widget build(BuildContext context) {
    final scanSize = MediaQuery.of(context).size.width * 0.7;

    return Scaffold(
      backgroundColor: Colors.black,
      appBar: AppBar(
        title: const Text('QR Kodu Okutun'),
        backgroundColor: Colors.transparent,
        foregroundColor: Colors.white,
      ),
      extendBodyBehindAppBar: true,
      body: Stack(
        alignment: Alignment.center,
        children: [
          // 1) Kamera — tam ekran
          MobileScanner(
            onDetect: (capture) {
              if (_handled) return;
              final value = capture.barcodes.firstOrNull?.rawValue;
              if (value != null) {
                _handled = true;
                Navigator.of(context).pop(value);
              }
            },
          ),

          // 2) Karartma maskesi: ortası delik siyah katman
          ColorFiltered(
            colorFilter: ColorFilter.mode(
                Colors.black.withOpacity(0.55), BlendMode.srcOut),
            child: Stack(
              alignment: Alignment.center,
              children: [
                Container(
                  decoration: const BoxDecoration(
                      color: Colors.black,
                      backgroundBlendMode: BlendMode.dstOut),
                ),
                // Delik: tarama alanı
                Container(
                  width: scanSize,
                  height: scanSize,
                  decoration: BoxDecoration(
                    color: Colors.black,
                    borderRadius: BorderRadius.circular(24),
                  ),
                ),
              ],
            ),
          ),

          // 3) Köşe çerçeveleri
          SizedBox(
            width: scanSize,
            height: scanSize,
            child: CustomPaint(painter: _CornerPainter()),
          ),

          // 4) Yönlendirme metni
          Positioned(
            bottom: 80,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
              decoration: BoxDecoration(
                color: Colors.black54,
                borderRadius: BorderRadius.circular(20),
              ),
              child: const Text(
                'QR kodu çerçevenin içine hizalayın',
                style: TextStyle(color: Colors.white),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Dört köşeye L şeklinde altın çizgiler çizer
class _CornerPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = const Color(0xFFD4A24E) // web'deki accent altını
      ..strokeWidth = 5
      ..strokeCap = StrokeCap.round
      ..style = PaintingStyle.stroke;

    const len = 32.0;   // köşe çizgisi uzunluğu
    const r = 24.0;     // köşe yumuşaklığı

    final path = Path()
      // sol üst
      ..moveTo(0, len)..lineTo(0, r)..quadraticBezierTo(0, 0, r, 0)..lineTo(len, 0)
      // sağ üst
      ..moveTo(size.width - len, 0)..lineTo(size.width - r, 0)
      ..quadraticBezierTo(size.width, 0, size.width, r)..lineTo(size.width, len)
      // sağ alt
      ..moveTo(size.width, size.height - len)..lineTo(size.width, size.height - r)
      ..quadraticBezierTo(size.width, size.height, size.width - r, size.height)
      ..lineTo(size.width - len, size.height)
      // sol alt
      ..moveTo(len, size.height)..lineTo(r, size.height)
      ..quadraticBezierTo(0, size.height, 0, size.height - r)
      ..lineTo(0, size.height - len);

    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}