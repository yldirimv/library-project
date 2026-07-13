import 'package:flutter/material.dart';
import '../models/reservation.dart';
import '../services/data_service.dart';

class ReservationsScreen extends StatelessWidget {
  const ReservationsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Rezervasyonlarım')),
      body: FutureBuilder<List<Reservation>>(
        future: DataService.getReservations(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return const Center(child: Text('Veri alınamadı'));
          }
          final items = snapshot.data ?? [];
          if (items.isEmpty) {
            return const Center(child: Text('Henüz rezervasyonunuz yok'));
          }
          return ListView.builder(
            itemCount: items.length,
            itemBuilder: (context, i) {
              final r = items[i];
              return Card(
                margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                child: ListTile(
                  leading: CircleAvatar(child: Text(r.seatNumber.split('-').last)),
                  title: Text('Koltuk ${r.seatNumber}'),
                  subtitle: Text(
                    '${r.start.day}.${r.start.month}.${r.start.year}  '
                    '${r.start.hour.toString().padLeft(2, '0')}:${r.start.minute.toString().padLeft(2, '0')}'
                    ' - ${r.end.hour.toString().padLeft(2, '0')}:${r.end.minute.toString().padLeft(2, '0')}',
                  ),
                  trailing: _statusChip(r.status),
                ),
              );
            },
          );
        },
      ),
    );
  }

  Widget _statusChip(String status) {
    final map = {
      'Pending':   ('Bekliyor', Colors.grey),
      'Active':    ('İçeride', Colors.red),
      'OnBreak':   ('Molada', Colors.blue),
      'Completed': ('Tamamlandı', Colors.green),
      'Cancelled': ('İptal', Colors.black38),
      'NoShow':    ('Gelmedi', Colors.orange),
    };
    final (label, color) = map[status] ?? (status, Colors.grey);
    return Chip(
      label: Text(label, style: const TextStyle(color: Colors.white, fontSize: 12)),
      backgroundColor: color,
      padding: EdgeInsets.zero,
    );
  }
}