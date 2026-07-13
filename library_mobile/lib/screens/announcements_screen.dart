import 'package:flutter/material.dart';
import '../models/announcement.dart';
import '../services/data_service.dart';

class AnnouncementsScreen extends StatelessWidget {
  const AnnouncementsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Duyurular')),
      body: FutureBuilder<List<Announcement>>(
        future: DataService.getAnnouncements(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return const Center(child: Text('Veri alınamadı'));
          }
          final items = snapshot.data ?? [];
          if (items.isEmpty) {
            return const Center(child: Text('Henüz duyuru yok'));
          }
          return ListView.builder(
            itemCount: items.length,
            itemBuilder: (context, i) {
              final a = items[i];
              return Card(
                margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          const Icon(Icons.campaign, color: Color(0xFF1A4D3E)),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(a.title,
                                style: const TextStyle(
                                    fontWeight: FontWeight.bold, fontSize: 16)),
                          ),
                          Text('${a.date.day}.${a.date.month}.${a.date.year}',
                              style: const TextStyle(
                                  color: Colors.grey, fontSize: 12)),
                        ],
                      ),
                      const SizedBox(height: 8),
                      Text(a.content),
                    ],
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}